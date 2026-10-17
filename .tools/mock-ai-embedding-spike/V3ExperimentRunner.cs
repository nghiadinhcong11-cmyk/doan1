using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MockAiEmbeddingSpike;

public static class V3ExperimentRunner
{
    private static readonly CapabilityContract Revenue = new("REVENUE_QUERY", "get_revenue", new[] { "today", "month", "year" }, new[] { "revenue" }, "admin|manager", "Read", "userContext.BranchId");
    private static readonly CapabilityContract Summary = new("BUSINESS_SUMMARY", "get_business_summary", new[] { "today", "yesterday", "this_week", "last_week", "this_month", "last_month" }, new[] { "revenue", "order_count", "aov", "expenses", "estimated_cogs", "estimated_profit", "top_products" }, "admin|manager", "Read", "userContext.BranchId");

    public static async Task<int> RunAsync(string[] args)
    {
        var root = GetOption(args, "--artifact-root") ?? "artifacts";
        var exp = GetOption(args, "--experiment-root") ?? Path.Combine("experiments", "manager-revenue-business-summary-v3");
        var historical = GetOption(args, "--historical-root") ?? Path.Combine("experiments", "manager-revenue-business-summary-v2");
        var output = GetOption(args, "--output") ?? Path.Combine(root, "manager-revenue-business-summary-v3-result.json");
        var holdoutRoot = GetOption(args, "--holdout-root");
        var adjudicationRoot = GetOption(args, "--adjudication-root");
        var model = Path.Combine(root, "onnx", "model_O4.onnx");
        var tokenizer = Path.Combine(root, "onnx", "tokenizer.json");
        var reference = Load<ReferenceRow>(Path.Combine(exp, "reference.json"));
        var development = Load<CaseRow>(Path.Combine(exp, "development.json"));
        var test = Load<CaseRow>(Path.Combine(exp, "test.json"));
        var boundary = Load<BoundaryRow>(Path.Combine(exp, "boundary.json"));
        var oldTest = Load<LegacyCaseRow>(Path.Combine(historical, "test.json"))
            .Select(x => new CaseRow(x.Id, x.Text, x.Intent, x.ExpectedDecision, "HISTORICAL", "unknown", Array.Empty<string>())).ToArray();
        var adjudication = adjudicationRoot is not null && File.Exists(Path.Combine(adjudicationRoot, "historical-adjudication.json"))
            ? Load<AdjudicationRow>(Path.Combine(adjudicationRoot, "historical-adjudication.json")).ToDictionary(x => x.Id)
            : new Dictionary<string, AdjudicationRow>();
        var adjudicatedTest = oldTest.Select(x => adjudication.TryGetValue(x.Id, out var a)
            ? new CaseRow(x.Id, x.Text, a.Intent, a.ExpectedDecision, a.Composition, a.Period, a.RequiredOutputs)
            : x).ToArray();
        var holdout = holdoutRoot is not null && File.Exists(Path.Combine(holdoutRoot, "holdout.json"))
            ? Load<CaseRow>(Path.Combine(holdoutRoot, "holdout.json")) : Array.Empty<CaseRow>();
        EnsureDisjoint(reference.Select(x => x.Id), development.Select(x => x.Id), test.Select(x => x.Id));

        using var adapter = new SentenceEmbeddingAdapter(new SentenceEmbeddingOptions(model, tokenizer));
        var texts = reference.Select(x => x.Text).Concat(boundary.Select(x => x.Text)).Concat(development.Select(x => x.Text)).Concat(test.Select(x => x.Text)).Concat(oldTest.Select(x => x.Text)).Concat(holdout.Select(x => x.Text)).ToArray();
        var allVectors = await adapter.EmbedBatchAsync(texts);
        var cursor = 0;
        var refVectors = allVectors[cursor..(cursor += reference.Length)];
        var boundaryVectors = allVectors[cursor..(cursor += boundary.Length)];
        var devVectors = allVectors[cursor..(cursor += development.Length)];
        var testVectors = allVectors[cursor..(cursor += test.Length)];
        var oldVectors = allVectors[cursor..];
        var holdoutVectors = holdout.Length == 0 ? Array.Empty<float[]>() : oldVectors[^holdout.Length..];
        if (holdout.Length > 0) oldVectors = oldVectors[..^holdout.Length];

        var candidates = reference.Select((r, i) => new Example(r.Id, r.Intent, r.Text, refVectors[i])).ToArray();
        var selected = Calibrate(development, devVectors, candidates, boundaryVectors, boundary);
        var devRows = Evaluate(development, devVectors, candidates, boundaryVectors, boundary, selected);
        var testRows = Evaluate(test, testVectors, candidates, boundaryVectors, boundary, selected);
        var oldRows = Evaluate(oldTest, oldVectors, candidates, boundaryVectors, boundary, selected);
        var adjudicatedRows = Evaluate(adjudicatedTest, oldVectors, candidates, boundaryVectors, boundary, selected);
        var holdoutRows = Evaluate(holdout, holdoutVectors, candidates, boundaryVectors, boundary, selected);
        var result = new
        {
            experiment = "manager-revenue-business-summary-v3",
            role = "manager",
            contracts = new[] { Revenue, Summary },
            scoring = new { selected.Aggregation, selected.MinimumScore, selected.MinimumMargin, boundaryGap = selected.MinimumBoundaryGap },
            development = Metrics(devRows),
            test = Metrics(testRows),
            historicalRegression = Metrics(oldRows),
            adjudicatedHistoricalRegression = Metrics(adjudicatedRows),
            holdout = Metrics(holdoutRows),
            rows = new { development = devRows, test = testRows, historical = oldRows, adjudicatedHistorical = adjudicatedRows, holdout = holdoutRows }
        };
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(output))!);
        await File.WriteAllTextAsync(output, JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping }));
        Console.WriteLine($"CONFIG|aggregation={selected.Aggregation}|score={selected.MinimumScore.ToString(CultureInfo.InvariantCulture)}|margin={selected.MinimumMargin.ToString(CultureInfo.InvariantCulture)}|boundaryGap={selected.MinimumBoundaryGap.ToString(CultureInfo.InvariantCulture)}");
        Print("DEV", devRows); Print("TEST", testRows); Print("HIST", oldRows); Print("ADJHIST", adjudicatedRows);
        if (holdout.Length > 0) Print("HOLDOUT", holdoutRows);
        Console.WriteLine($"RESULT_JSON={output}");
        return 0;
    }

    private static Policy Calibrate(IReadOnlyList<CaseRow> rows, float[][] vectors, IReadOnlyList<Example> refs, float[][] boundaryVectors, IReadOnlyList<BoundaryRow> boundary)
    {
        Policy? best = null;
        foreach (var aggregation in new[] { "Max", "TopTwoMean" })
        for (var score = 0.75; score <= 0.9001; score += 0.01)
        for (var margin = 0.0; margin <= 0.0301; margin += 0.005)
        {
            var p = new Policy(aggregation, Math.Round(score, 3), Math.Round(margin, 3), 0);
            var result = Evaluate(rows, vectors, refs, boundaryVectors, boundary, p);
            var candidate = p with { Correct = result.Count(x => x.Pass), FalseAccepts = result.Count(x => x.ExpectedDecision != "ACCEPT" && x.Decision == "ACCEPT"), WrongAccepts = result.Count(x => x.ExpectedDecision == "ACCEPT" && x.Decision == "ACCEPT" && x.ResolvedIntent != x.ExpectedIntent) };
            if (best is null || candidate.CompareTo(best) > 0) best = candidate;
        }
        return best!;
    }

    private static List<ResultRow> Evaluate(IReadOnlyList<CaseRow> rows, float[][] vectors, IReadOnlyList<Example> refs, float[][] boundaryVectors, IReadOnlyList<BoundaryRow> boundary, Policy policy)
        => rows.Select((r, i) => Decide(r, vectors[i], refs, boundaryVectors, boundary, policy)).ToList();

    private static ResultRow Decide(CaseRow row, float[] vector, IReadOnlyList<Example> refs, float[][] boundaryVectors, IReadOnlyList<BoundaryRow> boundary, Policy policy)
    {
        var ranked = refs.GroupBy(x => x.Intent).Select(g =>
        {
            var evidence = g.Select(x => new Evidence(x.Id, x.Text, Cosine(vector, x.Vector))).OrderByDescending(x => x.Score).ToArray();
            var score = policy.Aggregation == "TopTwoMean" ? evidence.Take(2).Average(x => x.Score) : evidence[0].Score;
            return new Candidate(g.Key, score, evidence);
        }).OrderByDescending(x => x.Score).ToArray();
        var top = ranked[0]; var second = ranked.ElementAtOrDefault(1); var semanticMargin = second is null ? top.Score : top.Score - second.Score;
        var boundaryTop = boundary.Select((b, i) => new BoundaryEvidence(b.Category, b.Text, Cosine(vector, boundaryVectors[i]))).OrderByDescending(x => x.Score).First();
        var period = ParsePeriod(row.Text);
        var outputs = ParseOutputs(row.Text);
        var normalizedText = Fold(row.Text);
        var negated = IsNegated(normalizedText);
        var resolved = top.Intent;
        var composition = Composition(outputs, row.Text);
        var reason = "ReadyForBackendChecks";
        var decision = "ACCEPT";
        var capability = Contract(top.Intent);
        var topSupports = capability is not null && period.Value is not null && SupportsPeriod(capability, period.Value) && outputs.All(x => capability.Outputs.Contains(x));
        // A revenue-only request is more specific than the summary contract when both semantic candidates overlap.
        if (top.Intent == "BUSINESS_SUMMARY" && outputs.SetEquals(new[] { "revenue" }) && period.Value is not null && SupportsPeriod(Revenue, period.Value))
        { resolved = Revenue.Intent; capability = Revenue; topSupports = true; reason = "NarrowerRevenueCapability"; }
        var unsupportedOutputs = outputs.Where(x => !Revenue.Outputs.Contains(x) && !Summary.Outputs.Contains(x)).ToArray();
        if (IsClearlyOutOfScope(row.Text)) { decision = "UNSUPPORTED"; reason = "OutOfScopeCapability"; }
        else if (unsupportedOutputs.Length > 0 && outputs.Any(x => Revenue.Outputs.Contains(x) || Summary.Outputs.Contains(x)))
        { decision = "CLARIFY"; reason = "MultipleIndependentCapabilities"; }
        else if (unsupportedOutputs.Length > 0)
        { decision = "UNSUPPORTED"; reason = "UnsupportedCapability"; }
        else if (negated) { decision = "CLARIFY"; reason = "NegatedRequest"; }
        else if (IsAmbiguousSubject(row.Text)) { decision = "CLARIFY"; reason = "AmbiguousSubject"; }
        else if (period.Kind is "missing" or "unsupported") { decision = "CLARIFY"; reason = period.Kind == "missing" ? "MissingPeriod" : "UnsupportedPeriod"; }
        else if (composition == "MULTI_TOOL_COMPOUND") { decision = "CLARIFY"; reason = "MultipleIndependentCapabilities"; }
        else if (top.Score < policy.MinimumScore) { decision = "UNSUPPORTED"; reason = "LowSimilarity"; }
        else if (semanticMargin < policy.MinimumMargin) { decision = "CLARIFY"; reason = "CloseIntentCandidates"; }
        else if (boundaryTop.Score - top.Score >= policy.MinimumBoundaryGap && boundaryTop.Score > top.Score) { decision = "UNSUPPORTED"; reason = "BoundaryCompetition"; }
        else if (!topSupports) { decision = "CLARIFY"; reason = "CapabilityContractMismatch"; }
        var pass = row.ExpectedDecision == decision && (decision != "ACCEPT" || resolved == row.ExpectedIntent);
        return new ResultRow(row.Id, row.Text, row.ExpectedIntent, row.ExpectedDecision, top.Intent, resolved, top.Score, second?.Score ?? 0, semanticMargin, boundaryTop.Category, boundaryTop.Score, period.Value ?? period.Kind, outputs.OrderBy(x => x).ToArray(), capability?.Tool ?? "NONE", capability?.Periods ?? Array.Empty<string>(), capability?.Outputs ?? Array.Empty<string>(), composition, decision, reason, pass, top.Evidence, second is null ? Array.Empty<Evidence>() : second.Evidence);
    }

    private static PeriodResult ParsePeriod(string text)
    {
        var t = Fold(text);
        if (Regex.IsMatch(t, @"tuan sau|thang sau|nam sau|hom kia|ngay mai")) return new("unsupported", null);
        if (Regex.IsMatch(t, @"hom nay|ngay hien tai")) return new("supported", "today");
        if (Regex.IsMatch(t, @"hom qua")) return new("supported", "yesterday");
        if (Regex.IsMatch(t, @"tuan nay|tuan hien tai")) return new("supported", "this_week");
        if (Regex.IsMatch(t, @"thang nay|thang hien tai")) return new("supported", "this_month");
        if (Regex.IsMatch(t, @"nam nay")) return new("supported", "year");
        return new("missing", null);
    }

    private static HashSet<string> ParseOutputs(string text)
    {
        var t = Fold(text); var o = new HashSet<string>();
        if (Regex.IsMatch(t, @"doanh thu|doanh so|ban hang|thu duoc|thu ve|kiem duoc")) o.Add("revenue");
        if (Regex.IsMatch(t, @"so don|don hang|luong don")) o.Add("order_count");
        if (Regex.IsMatch(t, @"aov|gia tri don|don trung binh")) o.Add("aov");
        if (Regex.IsMatch(t, @"chi phi|expense")) o.Add("expenses");
        if (Regex.IsMatch(t, @"gia von|cogs")) o.Add("estimated_cogs");
        if (Regex.IsMatch(t, @"loi nhuan|profit")) o.Add("estimated_profit");
        if (Regex.IsMatch(t, @"top mon|mon ban chay|san pham ban chay")) o.Add("top_products");
        if (Regex.IsMatch(t, @"thuc don|menu|danh sach mon|mon an|gia mon")) o.Add("menu");
        if (Regex.IsMatch(t, @"ton kho|nguyen lieu")) o.Add("inventory");
        if (Regex.IsMatch(t, @"ca lam|lich ca")) o.Add("shift");
        if (Regex.IsMatch(t, @"trang thai")) o.Add("order_status");
        if (Regex.IsMatch(t, @"kpi|tong quan|tom tat|tinh hinh|bao cao")) o.UnionWith(Summary.Outputs);
        return o;
    }

    private static string Composition(HashSet<string> outputs, string text)
    {
        var supportedByRevenue = outputs.All(x => Revenue.Outputs.Contains(x));
        var supportedBySummary = outputs.All(x => Summary.Outputs.Contains(x));
        var clauses = Regex.Split(Fold(text), @"\s*(?:,|;|\bva\b|\broi\b)\s*", RegexOptions.IgnoreCase).Count(x => !string.IsNullOrWhiteSpace(x));
        if (!supportedByRevenue && !supportedBySummary && clauses > 1) return "MULTI_TOOL_COMPOUND";
        if (supportedBySummary && outputs.Count > 1) return "SINGLE_TOOL_MULTI_FIELD";
        return supportedByRevenue || supportedBySummary ? "SINGLE_INTENT" : "OUT_OF_SCOPE";
    }

    private static string Fold(string text)
    {
        var mapped = text.Replace('\u0111', 'd').Replace('\u0110', 'D');
        return new string(mapped.Normalize(System.Text.NormalizationForm.FormD).Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark).ToArray()).ToLowerInvariant();
    }
    private static bool IsNegated(string normalizedText)
    {
        // Any explicit negation makes automatic execution unsafe; clause-level policy may clarify later.
        return Regex.IsMatch(normalizedText, @"(^|\s)(khong|dung)(?=\s|$|[,;.!?])");
    }
    private static bool SupportsPeriod(CapabilityContract contract, string period) => contract == Revenue && period == "this_month" ? contract.Periods.Contains("month") : contract.Periods.Contains(period);
    private static bool IsAmbiguousSubject(string text)
    {
        var t = Fold(text);
        return Regex.IsMatch(t, @"\b(tinh hinh|hoat dong)\b") && !Regex.IsMatch(t, @"doanh thu|doanh so|kpi|chi so|so don|tong quan|tong hop|tom tat|chi phi|loi nhuan|bao cao");
    }
    private static bool IsClearlyOutOfScope(string text)
    {
        var t = Fold(text);
        return Regex.IsMatch(t, @"thuc don|menu|mo cua|gio phuc vu|phuc vu den|diem thanh vien|loyalty|ca lam|lich ca|ton kho|nguyen lieu|cap nhat trang thai|trang thai don|xep hang mon|mon ban chay|don dang cho|don cho xu ly");
    }

    private static CapabilityContract? Contract(string intent) => intent == Revenue.Intent ? Revenue : intent == Summary.Intent ? Summary : null;
    private static double Cosine(IReadOnlyList<float> a, IReadOnlyList<float> b) => a.Zip(b, (x, y) => x * y).Sum();
    private static void Print(string label, IReadOnlyList<ResultRow> rows) { foreach (var r in rows) Console.WriteLine($"{label}|{r.Id}|expected={r.ExpectedDecision}|predicted={r.PredictedIntent}|resolved={r.ResolvedIntent}|decision={r.Decision}|period={r.ParsedPeriod}|composition={r.Composition}|{(r.Pass ? "PASS" : "FAIL")}"); }
    private static object Metrics(IReadOnlyList<ResultRow> rows)
    {
        var inScope = rows.Where(x => x.ExpectedDecision == "ACCEPT").ToArray();
        var ood = rows.Where(x => x.ExpectedDecision != "ACCEPT").ToArray();
        var explicitOod = rows.Where(x => x.ExpectedIntent == "OUT_OF_SCOPE").ToArray();
        var semanticCandidateCorrect = inScope.Count(x => x.PredictedIntent == x.ExpectedIntent);
        var finalIntentCorrect = inScope.Count(x => x.Decision == "ACCEPT" && x.ResolvedIntent == x.ExpectedIntent);
        var capabilityCompliant = rows.Count(x => x.Decision == "ACCEPT" && x.ParsedPeriod is not "unsupported" and not "missing" && x.RequiredOutputs.All(o => x.SupportedOutputs.Contains(o)));
        return new
        {
            total = rows.Count,
            inScopeTotal = inScope.Length,
            inScopeAccepted = inScope.Count(x => x.Decision == "ACCEPT"),
            inScopeAcceptanceRate = inScope.Length == 0 ? 0 : Math.Round((double)inScope.Count(x => x.Decision == "ACCEPT") / inScope.Length, 4),
            semanticCandidateCorrect,
            finalIntentCorrect,
            finalDecisionCorrect = rows.Count(x => x.Pass),
            safeAbstention = ood.Count(x => x.Decision != "ACCEPT"),
            correctAbstention = ood.Count(x => x.Decision != "ACCEPT"),
            oodTotal = explicitOod.Length,
            oodRejection = explicitOod.Count(x => x.Decision != "ACCEPT"),
            oodRejectionRate = explicitOod.Length == 0 ? 0 : Math.Round((double)explicitOod.Count(x => x.Decision != "ACCEPT") / explicitOod.Length, 4),
            falseAcceptance = ood.Count(x => x.Decision == "ACCEPT"),
            invalidPeriodBlocked = rows.Count(x => x.ParsedPeriod == "unsupported" && x.Decision != "ACCEPT"),
            capabilityCompliantAccepts = capabilityCompliant,
            toolRoutingEligible = capabilityCompliant,
            singleToolMultiField = rows.Count(x => x.Composition == "SINGLE_TOOL_MULTI_FIELD" && x.Decision == "ACCEPT"),
            multiToolClarification = rows.Count(x => x.Composition == "MULTI_TOOL_COMPOUND" && x.Decision == "CLARIFY"),
            pass = rows.Count(x => x.Pass)
        };
    }
    private static T[] Load<T>(string path, bool allowLegacy = false) => JsonSerializer.Deserialize<T[]>(File.ReadAllText(path), new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? throw new InvalidDataException(path);
    private static void EnsureDisjoint(IEnumerable<string> a, IEnumerable<string> b, IEnumerable<string> c) { var ids = a.Concat(b).Concat(c).ToArray(); if (ids.Length != ids.Distinct().Count()) throw new InvalidDataException("Dataset IDs overlap"); }
    private static string? GetOption(string[] args, string name) { var i = Array.FindIndex(args, x => x.Equals(name, StringComparison.OrdinalIgnoreCase)); return i >= 0 && i + 1 < args.Length ? args[i + 1] : null; }

    private sealed record ReferenceRow(string Id, string Intent, string Text);
    private sealed record BoundaryRow(string Id, string Category, string Text);
    private sealed record CaseRow(string Id, string Text, string ExpectedIntent, string ExpectedDecision, string ExpectedComposition, string ExpectedPeriod, string[] RequiredOutputs);
    private sealed record LegacyCaseRow(string Id, string Intent, string ExpectedDecision, string Text);
    private sealed record AdjudicationRow(string Id, string Intent, string ExpectedDecision, string CapabilityClassification, string Period, string[] RequiredOutputs, string Composition, string AdjudicationReason);
    private sealed record Example(string Id, string Intent, string Text, float[] Vector);
    private sealed record Candidate(string Intent, double Score, IReadOnlyList<Evidence> Evidence);
    private sealed record Evidence(string ExampleId, string ReferenceText, double Score);
    private sealed record BoundaryEvidence(string Category, string ReferenceText, double Score);
    private sealed record CapabilityContract(string Intent, string Tool, string[] Periods, string[] Outputs, string Role, string Risk, string BranchScope);
    private sealed record PeriodResult(string Kind, string? Value);
    private sealed record Policy(string Aggregation, double MinimumScore, double MinimumMargin, double MinimumBoundaryGap, int Correct = 0, int FalseAccepts = 0, int WrongAccepts = 0) : IComparable<Policy>
    { public int CompareTo(Policy? other) => other is null ? 1 : (Correct, -FalseAccepts, -WrongAccepts, -MinimumScore, -MinimumMargin).CompareTo((other.Correct, -other.FalseAccepts, -other.WrongAccepts, -other.MinimumScore, -other.MinimumMargin)); }
    private sealed record ResultRow(string Id, string Text, string ExpectedIntent, string ExpectedDecision, string PredictedIntent, string ResolvedIntent, double TopScore, double SecondScore, double Margin, string BoundaryCategory, double BoundaryScore, string ParsedPeriod, string[] RequiredOutputs, string Tool, string[] SupportedPeriods, string[] SupportedOutputs, string Composition, string Decision, string Reason, bool Pass, IReadOnlyList<Evidence> TopEvidence, IReadOnlyList<Evidence> SecondEvidence);
}
