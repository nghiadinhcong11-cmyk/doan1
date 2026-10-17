using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MockAiEmbeddingSpike;

public static class OodAbstentionExperimentRunner
{
    public static async Task<int> RunAsync(string[] args)
    {
        var root = GetOption(args, "--artifact-root") ?? "artifacts";
        var experimentRoot = GetOption(args, "--experiment-root") ?? Path.Combine("experiments", "manager-revenue-business-summary-v2");
        var historicalRoot = GetOption(args, "--historical-root") ?? Path.Combine("experiments", "manager-revenue-business-summary-v1");
        var output = GetOption(args, "--output") ?? Path.Combine(root, "manager-revenue-business-summary-v2-result.json");
        var modelPath = Path.Combine(root, "onnx", "model_O4.onnx");
        var tokenizerPath = Path.Combine(root, "onnx", "tokenizer.json");

        var reference = Load<ReferenceRow>(Path.Combine(experimentRoot, "reference.json"));
        var boundaries = Load<BoundaryRow>(Path.Combine(experimentRoot, "boundary.json"));
        var development = Load<CaseRow>(Path.Combine(experimentRoot, "development.json"));
        var test = Load<CaseRow>(Path.Combine(experimentRoot, "test.json"));
        var historical = Load<CaseRow>(Path.Combine(historicalRoot, "test.json"));
        EnsureDisjoint(reference.Select(x => x.Id), development.Select(x => x.Id), test.Select(x => x.Id));

        using var adapter = new SentenceEmbeddingAdapter(new SentenceEmbeddingOptions(modelPath, tokenizerPath, "query: ", 512, 1, 8));
        var refTexts = reference.Select(x => x.Text).ToArray();
        var boundaryTexts = boundaries.Select(x => x.Text).ToArray();
        var allTexts = refTexts.Concat(boundaryTexts).Concat(development.Select(x => x.Text)).Concat(test.Select(x => x.Text)).Concat(historical.Select(x => x.Text)).ToArray();
        var vectors = await adapter.EmbedBatchAsync(allTexts);
        var at = 0;
        var refVectors = vectors[at..(at + refTexts.Length)]; at += refTexts.Length;
        var boundaryVectors = vectors[at..(at + boundaryTexts.Length)]; at += boundaryTexts.Length;
        var devVectors = vectors[at..(at + development.Length)]; at += development.Length;
        var testVectors = vectors[at..(at + test.Length)]; at += test.Length;
        var historicalVectors = vectors[at..(at + historical.Length)];
        var examples = reference.Select(x => new IntentExample(x.Id, x.Intent, x.Text, x.Intent == "REVENUE_QUERY" ? "get_revenue" : "get_business_summary", "manager")).ToArray();
        var scorer = new IntentScorer(examples, reference.Select((x, i) => (x.Id, refVectors[i])).ToDictionary(x => x.Id, x => x.Item2));

        var selected = Calibrate(development, devVectors, scorer, boundaryVectors, boundaries);
        var newRows = await Evaluate(test, testVectors, scorer, adapter, boundaryVectors, boundaries, selected);
        var historicalRows = await Evaluate(historical, historicalVectors, scorer, adapter, boundaryVectors, boundaries, selected);
        var result = new
        {
            experiment = "manager-revenue-business-summary-v2-ood-abstention",
            role = "manager",
            policy = new { selected.MinimumScore, selected.MinimumBoundaryGap, selected.MinimumIntentMargin, negation = "generic clause-level negation", multiIntent = "independent clause scoring", missingPeriod = "clarify", boundaryAnchors = boundaries.Length },
            development = Metrics(development, CalibrateRows(development, devVectors, scorer, boundaryVectors, boundaries, selected)),
            test = Metrics(test, newRows),
            historicalRegression = Metrics(historical, historicalRows),
            data = new { referenceCount = reference.Length, boundaryCount = boundaries.Length, developmentCount = development.Length, testCount = test.Length, historicalCount = historical.Length },
            rows = new { newTest = newRows, historical = historicalRows }
        };
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(output))!);
        await File.WriteAllTextAsync(output, JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping }));
        Console.WriteLine($"THRESHOLD|score={selected.MinimumScore.ToString(CultureInfo.InvariantCulture)}|boundaryGap={selected.MinimumBoundaryGap.ToString(CultureInfo.InvariantCulture)}");
        Print("DEVELOPMENT", development, CalibrateRows(development, devVectors, scorer, boundaryVectors, boundaries, selected));
        Print("NEW_TEST", test, newRows);
        Print("HISTORICAL", historical, historicalRows);
        Console.WriteLine($"RESULT_JSON={output}");
        return 0;
    }

    private static ThresholdChoice Calibrate(IReadOnlyList<CaseRow> rows, float[][] vectors, IntentScorer scorer, float[][] boundaryVectors, IReadOnlyList<BoundaryRow> boundaries)
    {
        ThresholdChoice? best = null;
        for (var score = 0.80; score <= 0.950001; score += 0.005)
        for (var gap = 0.00; gap <= 0.250001; gap += 0.005)
        for (var margin = 0.00; margin <= 0.060001; margin += 0.005)
        {
            var choice = new ThresholdChoice(Math.Round(score, 3), Math.Round(gap, 3), Math.Round(margin, 3));
            var evaluated = CalibrateRows(rows, vectors, scorer, boundaryVectors, boundaries, choice);
            choice = choice with { Correct = evaluated.Count(x => x.Pass), FalseAccepts = evaluated.Count(x => x.ExpectedDecision == "UNSUPPORTED" && x.Decision == "ACCEPT"), ClarifyMisses = evaluated.Count(x => x.ExpectedDecision == "CLARIFY" && x.Decision != "CLARIFY") };
            if (best is null || choice.CompareTo(best) > 0) best = choice;
        }
        return best!;
    }

    private static List<ResultRow> CalibrateRows(IReadOnlyList<CaseRow> rows, float[][] vectors, IntentScorer scorer, float[][] boundaryVectors, IReadOnlyList<BoundaryRow> boundaries, ThresholdChoice policy)
        => rows.Select((row, i) => Decide(row, vectors[i], scorer, boundaryVectors, boundaries, policy)).ToList();

    private static async Task<List<ResultRow>> Evaluate(IReadOnlyList<CaseRow> rows, float[][] vectors, IntentScorer scorer, SentenceEmbeddingAdapter adapter, float[][] boundaryVectors, IReadOnlyList<BoundaryRow> boundaries, ThresholdChoice policy)
    {
        var output = new List<ResultRow>();
        for (var i = 0; i < rows.Count; i++)
            output.Add(await DecideAsync(rows[i], vectors[i], scorer, adapter, boundaryVectors, boundaries, policy));
        return output;
    }

    private static async Task<ResultRow> DecideAsync(CaseRow row, float[] vector, IntentScorer scorer, SentenceEmbeddingAdapter adapter, float[][] boundaryVectors, IReadOnlyList<BoundaryRow> boundaries, ThresholdChoice policy)
    {
        var result = Decide(row, vector, scorer, boundaryVectors, boundaries, policy);
        var clauses = SplitClauses(row.Text);
        if (clauses.Count > 1 && result.Decision != "UNSUPPORTED")
        {
            var clauseVectors = await adapter.EmbedBatchAsync(clauses);
            var clauseCandidates = clauseVectors.Select(v => scorer.Score(v, policy.MinimumScore, 0).Top).ToArray();
            var intents = clauseCandidates
                .Where(x => x is not null && x.Score >= policy.MinimumScore).Select(x => x!.IntentId).Distinct().ToArray();
            var explicitRequestClauses = clauses.Count(HasExplicitRequestVerb);
            if (explicitRequestClauses >= 2)
                result = result with { Decision = "CLARIFY", Pass = row.ExpectedDecision == "CLARIFY", Reason = "MultipleIntents" };
        }
        return result;
    }

    private static ResultRow Decide(CaseRow row, float[] vector, IntentScorer scorer, float[][] boundaryVectors, IReadOnlyList<BoundaryRow> boundaries, ThresholdChoice policy)
    {
        var semantic = scorer.Score(vector, 0, 0);
        var top = semantic.Top?.Score ?? 0;
        var second = semantic.Second?.Score ?? 0;
        var boundary = boundaryVectors.Select((v, i) => (Score: Cosine(vector, v), Category: boundaries[i].Category)).OrderByDescending(x => x.Score).First();
        var normalized = row.Text.Normalize().ToLowerInvariant();
        string decision;
        string reason;
        if (Negated(normalized)) { decision = "CLARIFY"; reason = "NegatedRequest"; }
        else if (top < policy.MinimumScore) { decision = "UNSUPPORTED"; reason = "LowSimilarity"; }
        else if (top - boundary.Score < policy.MinimumBoundaryGap) { decision = "UNSUPPORTED"; reason = "BoundaryCompetition"; }
        else if (semantic.Margin < policy.MinimumIntentMargin) { decision = "CLARIFY"; reason = "CloseIntentCandidates"; }
        else if (!HasPeriod(normalized)) { decision = "CLARIFY"; reason = "MissingOrAmbiguousPeriod"; }
        else if (IsVague(normalized)) { decision = "CLARIFY"; reason = "AmbiguousSubject"; }
        else { decision = "ACCEPT"; reason = "ReadyForBackendChecks"; }
        return new ResultRow(row.Id, row.Text, row.Intent, row.ExpectedDecision, semantic.Top?.IntentId ?? "NONE", top, second, top - second, boundary.Category, boundary.Score, decision, reason, decision == row.ExpectedDecision);
    }

    private static bool Negated(string text) => Regex.IsMatch(text, @"\b(không|đừng|chớ)\b", RegexOptions.CultureInvariant);
    private static bool HasPeriod(string text) => Regex.IsMatch(text, @"\b(hôm nay|hôm qua|ngày hiện tại|tháng này|tháng hiện tại|tuần này|tuần hiện tại|năm nay)\b", RegexOptions.CultureInvariant);
    private static bool IsVague(string text) =>
        (text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length <= 6 && !Regex.IsMatch(text, @"\b(doanh thu|doanh số|tổng quan|báo cáo|kpi|chỉ số|hoạt động|kinh doanh|bán hàng)\b", RegexOptions.CultureInvariant))
        || (Regex.IsMatch(text, @"\b(thế nào|ra sao)\b", RegexOptions.CultureInvariant) && !Regex.IsMatch(text, @"\b(doanh thu|doanh số|tổng quan|báo cáo|kpi|chỉ số|số đơn)\b", RegexOptions.CultureInvariant));
    private static bool HasExplicitRequestVerb(string text) => Regex.IsMatch(text.Trim(), @"^(xem|cho|hãy|báo cáo|tổng hợp|tra|kiểm tra|cần|muốn)\b", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
    private static IReadOnlyList<string> SplitClauses(string text) => Regex.Split(text, @"\s*(?:,|;|\bvà\b|\brồi\b)\s*", RegexOptions.CultureInvariant).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
    private static double Cosine(IReadOnlyList<float> a, IReadOnlyList<float> b) => a.Zip(b, (x, y) => x * y).Sum();

    private static object Metrics(IReadOnlyList<CaseRow> input, IReadOnlyList<ResultRow> rows) => new
    {
        total = rows.Count,
        inScope = input.Count(x => x.ExpectedDecision == "ACCEPT"),
        inScopeCorrect = rows.Count(x => x.ExpectedDecision == "ACCEPT" && x.Decision == "ACCEPT" && x.PredictedIntent == x.ExpectedIntent),
        falseAccepts = rows.Count(x => x.ExpectedDecision != "ACCEPT" && x.Decision == "ACCEPT"),
        correctRejections = rows.Count(x => x.ExpectedDecision == "UNSUPPORTED" && x.Decision == "UNSUPPORTED"),
        unsupportedCount = input.Count(x => x.ExpectedDecision == "UNSUPPORTED"),
        clarificationCorrect = rows.Count(x => x.ExpectedDecision == "CLARIFY" && x.Decision == "CLARIFY"),
        clarificationCount = input.Count(x => x.ExpectedDecision == "CLARIFY"),
        pass = rows.Count(x => x.Pass),
        confusion = rows.Where(x => x.ExpectedDecision == "ACCEPT").GroupBy(x => $"{x.ExpectedIntent}->{x.PredictedIntent}").ToDictionary(x => x.Key, x => x.Count())
    };

    private static void Print(string name, IReadOnlyList<CaseRow> input, IReadOnlyList<ResultRow> rows)
    { foreach (var row in rows) Console.WriteLine($"{name}|{row.Id}|expected={row.ExpectedDecision}|predicted={row.PredictedIntent}|top={row.TopScore:F4}|boundary={row.BoundaryScore:F4}|decision={row.Decision}|reason={row.Reason}|{(row.Pass ? "PASS" : "FAIL")}"); }
    private static T[] Load<T>(string path) => JsonSerializer.Deserialize<T[]>(File.ReadAllText(path, System.Text.Encoding.UTF8), new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? throw new InvalidDataException(path);
    private static void EnsureDisjoint(IEnumerable<string> a, IEnumerable<string> b, IEnumerable<string> c) { var all = a.Concat(b).Concat(c).ToArray(); if (all.Length != all.Distinct().Count()) throw new InvalidDataException("Dataset IDs overlap."); }
    private static string? GetOption(string[] args, string name) { var i = Array.FindIndex(args, x => string.Equals(x, name, StringComparison.OrdinalIgnoreCase)); return i >= 0 && i + 1 < args.Length ? args[i + 1] : null; }

    private sealed record ReferenceRow(string Id, string Intent, string Text);
    private sealed record BoundaryRow(string Id, string Category, string Text);
    private sealed record CaseRow(string Id, string Intent, string ExpectedDecision, string Text);
    private sealed record ThresholdChoice(double MinimumScore, double MinimumBoundaryGap, double MinimumIntentMargin, int Correct = 0, int FalseAccepts = 0, int ClarifyMisses = 0) : IComparable<ThresholdChoice>
    { public int CompareTo(ThresholdChoice? o) => o is null ? 1 : (Correct, -FalseAccepts, -ClarifyMisses, -MinimumScore, -MinimumBoundaryGap, -MinimumIntentMargin).CompareTo((o.Correct, -o.FalseAccepts, -o.ClarifyMisses, -o.MinimumScore, -o.MinimumBoundaryGap, -o.MinimumIntentMargin)); }
    private sealed record ResultRow(string Id, string Text, string ExpectedIntent, string ExpectedDecision, string PredictedIntent, double TopScore, double SecondScore, double Margin, string BoundaryCategory, double BoundaryScore, string Decision, string Reason, bool Pass);
}
