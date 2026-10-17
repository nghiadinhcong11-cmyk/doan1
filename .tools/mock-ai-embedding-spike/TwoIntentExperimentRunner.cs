using System.Globalization;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MockAiEmbeddingSpike;

public sealed record TwoIntentCase(string Id, string Intent, string ExpectedDecision, string Text);
public sealed record TwoIntentReference(string Id, string Intent, string Text);

/// <summary>
/// Offline two-intent evaluation. It only embeds and scores frozen files; it does not
/// call the API, permissions, tools, database, or keyword recognizers.
/// </summary>
public static class TwoIntentExperimentRunner
{
    private const string Revenue = "REVENUE_QUERY";
    private const string Summary = "BUSINESS_SUMMARY";

    public static async Task<int> RunAsync(string[] args)
    {
        var root = Option(args, "--artifact-root", "artifacts");
        var experimentRoot = Option(args, "--experiment-root", "experiments/manager-revenue-business-summary-v1");
        var output = Option(args, "--output", Path.Combine(experimentRoot, "result.json"));
        var modelPath = Path.Combine(root, "onnx", "model_O4.onnx");
        var tokenizerPath = Path.Combine(root, "onnx", "tokenizer.json");
        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, WriteIndented = true };

        var references = await ReadAsync<TwoIntentReference>(Path.Combine(experimentRoot, "reference.json"), jsonOptions);
        var development = await ReadAsync<TwoIntentCase>(Path.Combine(experimentRoot, "development.json"), jsonOptions);
        var test = await ReadAsync<TwoIntentCase>(Path.Combine(experimentRoot, "test.json"), jsonOptions);
        ValidateData(references, development, test);

        var initWatch = Stopwatch.StartNew();
        using var adapter = new SentenceEmbeddingAdapter(new SentenceEmbeddingOptions(modelPath, tokenizerPath, QueueCapacity: 2));
        initWatch.Stop();
        var embeddingWatch = Stopwatch.StartNew();
        var referenceVectors = await adapter.EmbedBatchAsync(references.Select(x => x.Text).ToArray());
        var examples = references.Select(x => new IntentExample(
            x.Id,
            x.Intent,
            x.Text,
            x.Intent == Revenue ? "get_revenue" : "get_business_summary",
            "manager")).ToArray();
        var cache = references.Select((x, i) => (x.Id, Vector: referenceVectors[i]))
            .ToDictionary(x => x.Id, x => x.Vector, StringComparer.Ordinal);
        var scorer = new IntentScorer(examples, cache, IntentScoreAggregation.Max);

        var developmentVectors = await adapter.EmbedBatchAsync(development.Select(x => x.Text).ToArray());
        var selected = SelectThreshold(scorer, development, developmentVectors);
        var testVectors = await adapter.EmbedBatchAsync(test.Select(x => x.Text).ToArray());
        embeddingWatch.Stop();
        var developmentRows = Evaluate(scorer, development, developmentVectors, selected.MinimumScore, selected.MinimumMargin);
        var testRows = Evaluate(scorer, test, testVectors, selected.MinimumScore, selected.MinimumMargin);
        var developmentMetrics = Metrics(developmentRows);
        var testMetrics = Metrics(testRows);

        var result = new
        {
            experiment = "manager-revenue-business-summary-v1",
            role = "manager",
            mappings = new[]
            {
                new { intent = Revenue, tool = "get_revenue", risk = "Read", branchScope = "userContext.BranchId" },
                new { intent = Summary, tool = "get_business_summary", risk = "Read", branchScope = "userContext.BranchId" }
            },
            model = "intfloat/multilingual-e5-small",
            modelArtifact = modelPath,
            tokenizerArtifact = tokenizerPath,
            prefix = "query: ",
            pooling = "attention-mask-aware mean pooling over last_hidden_state, then L2 normalization",
            scoring = new { aggregation = "max", selected.MinimumScore, selected.MinimumMargin },
            timing = new
            {
                processId = Environment.ProcessId,
                adapterInitializationMs = initWatch.Elapsed.TotalMilliseconds,
                referenceDevelopmentTestEmbeddingMs = embeddingWatch.Elapsed.TotalMilliseconds,
                workingSetAfterBytes = Process.GetCurrentProcess().WorkingSet64
            },
            data = new
            {
                reference = FileHash(Path.Combine(experimentRoot, "reference.json")),
                development = FileHash(Path.Combine(experimentRoot, "development.json")),
                test = FileHash(Path.Combine(experimentRoot, "test.json")),
                referenceCount = references.Count,
                developmentCount = development.Count,
                testCount = test.Count
            },
            development = new { metrics = developmentMetrics, rows = developmentRows },
            test = new { metrics = testMetrics, rows = testRows }
        };

        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(output))!);
        await File.WriteAllTextAsync(output, JsonSerializer.Serialize(result, jsonOptions));
        Console.WriteLine($"THRESHOLD|score={selected.MinimumScore.ToString(CultureInfo.InvariantCulture)}|margin={selected.MinimumMargin.ToString(CultureInfo.InvariantCulture)}");
        Console.WriteLine($"DEVELOPMENT|{JsonSerializer.Serialize(developmentMetrics)}");
        Console.WriteLine($"TEST|{JsonSerializer.Serialize(testMetrics)}");
        foreach (var row in testRows)
            Console.WriteLine($"{row.Id}|{row.Text}|expected={row.Intent}|predicted={row.PredictedIntent}|top={row.TopScore:F6}|second={row.SecondScore:F6}|margin={row.Margin:F6}|decision={row.Decision}|{(row.Pass ? "PASS" : "FAIL")}");
        Console.WriteLine($"RESULT_JSON={output}");
        return 0;
    }

    private static ThresholdChoice SelectThreshold(IntentScorer scorer, IReadOnlyList<TwoIntentCase> cases, IReadOnlyList<float[]> vectors)
    {
        ThresholdChoice? best = null;
        for (var score = 0.70; score <= 0.950001; score += 0.01)
        for (var margin = 0.00; margin <= 0.060001; margin += 0.005)
        {
            var rows = Evaluate(scorer, cases, vectors, score, margin);
            var metrics = Metrics(rows);
            var candidate = new ThresholdChoice(score, margin, metrics.InScopeCorrectAccepts, metrics.OutOfScopeRejected,
                metrics.SafeNonAccepts, metrics.InScopeWrongAccepts, metrics.FalseAccepts);
            if (best is null || candidate.CompareTo(best) > 0) best = candidate;
        }
        return best!;
    }

    private static List<TwoIntentRow> Evaluate(IntentScorer scorer, IReadOnlyList<TwoIntentCase> cases,
        IReadOnlyList<float[]> vectors, double minimumScore, double minimumMargin)
    {
        return cases.Select((item, index) =>
        {
            var result = scorer.Score(vectors[index], minimumScore, minimumMargin);
            var decision = result.Status switch
            {
                RecognitionStatus.Accepted => "ACCEPT",
                RecognitionStatus.ClarificationRequired => "CLARIFY",
                _ => "UNSUPPORTED"
            };
            var predicted = result.Top?.IntentId ?? "UNSUPPORTED";
            var pass = item.ExpectedDecision == "ACCEPT"
                ? decision == "ACCEPT" && predicted == item.Intent
                : decision != "ACCEPT";
            return new TwoIntentRow(item.Id, item.Text, item.Intent, item.ExpectedDecision, predicted,
                result.Top?.Score ?? 0, result.Second?.Score ?? 0, result.Margin, decision, pass);
        }).ToList();
    }

    private static TwoIntentMetrics Metrics(IReadOnlyList<TwoIntentRow> rows)
    {
        var inScope = rows.Where(x => x.ExpectedDecision == "ACCEPT").ToArray();
        var outOfScope = rows.Where(x => x.Intent == "OUT_OF_SCOPE" && x.ExpectedDecision == "UNSUPPORTED").ToArray();
        var safe = rows.Where(x => x.ExpectedDecision == "CLARIFY" || x.ExpectedDecision == "UNSUPPORTED").ToArray();
        var accepted = inScope.Where(x => x.Decision == "ACCEPT").ToArray();
        var correct = inScope.Count(x => x.Decision == "ACCEPT" && x.PredictedIntent == x.Intent);
        var wrong = inScope.Count(x => x.Decision == "ACCEPT" && x.PredictedIntent != x.Intent);
        var falseAccepts = rows.Count(x => x.Intent == "OUT_OF_SCOPE" && x.Decision == "ACCEPT");
        var confusion = inScope.Where(x => x.Decision == "ACCEPT")
            .GroupBy(x => $"{x.Intent}->{x.PredictedIntent}")
            .ToDictionary(x => x.Key, x => x.Count());
        return new TwoIntentMetrics(inScope.Length, correct, wrong, outOfScope.Length,
            outOfScope.Count(x => x.Decision != "ACCEPT"), falseAccepts, safe.Length,
            safe.Count(x => x.Decision != "ACCEPT"), confusion,
            inScope.Length == 0 ? 0 : (double)correct / inScope.Length,
            outOfScope.Length == 0 ? 0 : (double)(outOfScope.Length - falseAccepts) / outOfScope.Length);
    }

    private static void ValidateData(IReadOnlyList<TwoIntentReference> references, IReadOnlyList<TwoIntentCase> development, IReadOnlyList<TwoIntentCase> test)
    {
        if (references.Count == 0 || references.Any(x => x.Intent is not Revenue and not Summary)) throw new InvalidDataException("Reference must contain only the two selected intents.");
        if (development.Count == 0 || test.Count == 0) throw new InvalidDataException("Development and test sets must not be empty.");
        var allIds = references.Select(x => x.Id).Concat(development.Select(x => x.Id)).Concat(test.Select(x => x.Id)).ToArray();
        if (allIds.Distinct(StringComparer.Ordinal).Count() != allIds.Length) throw new InvalidDataException("Dataset IDs must be unique across all splits.");
        if (development.Concat(test).Any(x => x.Intent is not Revenue and not Summary and not "OUT_OF_SCOPE")) throw new InvalidDataException("Unexpected intent label.");
        if (development.Concat(test).Any(x => string.IsNullOrWhiteSpace(x.Text))) throw new InvalidDataException("Text must not be empty.");
    }

    private static async Task<List<T>> ReadAsync<T>(string path, JsonSerializerOptions options) =>
        JsonSerializer.Deserialize<List<T>>(await File.ReadAllTextAsync(path), options) ?? throw new InvalidDataException($"Empty dataset: {path}");

    private static string FileHash(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
    private static string Option(string[] args, string name, string fallback) { var i = Array.IndexOf(args, name); return i >= 0 && i + 1 < args.Length ? args[i + 1] : fallback; }

    private sealed record ThresholdChoice(double MinimumScore, double MinimumMargin, int Correct, int OutRejected, int Safe, int Wrong, int FalseAccept)
    {
        public int CompareTo(ThresholdChoice other) => (Correct + OutRejected + Safe, Correct, OutRejected, Safe, -Wrong, -FalseAccept, -MinimumScore, -MinimumMargin)
            .CompareTo((other.Correct + other.OutRejected + other.Safe, other.Correct, other.OutRejected, other.Safe, -other.Wrong, -other.FalseAccept, -other.MinimumScore, -other.MinimumMargin));
    }

    public sealed record TwoIntentRow(string Id, string Text, string Intent, string ExpectedDecision, string PredictedIntent,
        double TopScore, double SecondScore, double Margin, string Decision, bool Pass);
    public sealed record TwoIntentMetrics(int InScopeCount, int InScopeCorrectAccepts, int InScopeWrongAccepts,
        int OutOfScopeCount, int OutOfScopeRejected, int FalseAccepts, int SafeCaseCount, int SafeNonAccepts,
        IReadOnlyDictionary<string, int> Confusion, double InScopeAccuracy, double OutOfScopeRejectionRate);
}
