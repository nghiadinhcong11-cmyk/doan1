using System.Security.Cryptography;
using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace MockAiEmbeddingSpike;

public sealed record DecisionCase(string Id, string Text, string ExpectedIntent, string Category,
    string? ExpectedStatus = null, string? Actor = null, string? Rationale = null);
public sealed record BehaviorExpectation(string Id, string Status, string Rationale);

/// <summary>Offline component scoring only. Never connects to application services or executes tools.</summary>
public static class DecisionReviewRunner
{
    public static async Task<int> RunAsync(string[] args)
    {
        string Option(string name, string fallback)
        {
            var index = Array.IndexOf(args, name);
            return index >= 0 && index + 1 < args.Length ? args[index + 1] : fallback;
        }
        var path = Option("--calibration", "calibration.json");
        var output = Option("--review-output", "artifacts/decision-review.json");
        var profile = Option("--catalog-profile", "full-baseline");
        var scoring = Option("--scoring", "max");
        var aggregation = scoring switch
        {
            "max" => IntentScoreAggregation.Max,
            "top2-mean" => IntentScoreAggregation.TopTwoMean,
            _ => throw new ArgumentException("Scoring must be max or top2-mean.")
        };
        double Threshold(string name, string fallback)
        {
            if (!double.TryParse(Option(name, fallback), NumberStyles.Float, CultureInfo.InvariantCulture, out var value)
                || !double.IsFinite(value) || value < 0 || value > 1)
                throw new ArgumentException($"{name} must be a finite number between 0 and 1.");
            return value;
        }
        var minimumScore = Threshold("--minimum-score", "0.85");
        var minimumMargin = Threshold("--minimum-margin", "0.02");
        var examples = profile switch
        {
            "full-baseline" => IntentCatalog.Examples,
            "targeted-revision-v2" => IntentCatalog.TargetedRevision,
            "three-intent-examples" => IntentCatalog.WithThreeIntentExamples,
            _ => throw new ArgumentException("Catalog profile must be full-baseline, targeted-revision-v2 or three-intent-examples.")
        };
        var json = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
        var cases = JsonSerializer.Deserialize<DecisionCase[]>(await File.ReadAllTextAsync(path), json)!;
        var expectations = JsonSerializer.Deserialize<BehaviorExpectation[]>(await File.ReadAllTextAsync("calibration-behavior.json"), json)!.ToDictionary(x => x.Id);
        if (cases.Select(x => x.Id).Distinct().Count() != cases.Length || cases.Any(x => string.IsNullOrWhiteSpace(x.Text)))
            throw new InvalidDataException("Case IDs must be unique and text must be present.");
        var root = Option("--artifact-root", "artifacts");
        using var adapter = new SentenceEmbeddingAdapter(new SentenceEmbeddingOptions(
            Path.Combine(root, "onnx", "model_O4.onnx"), Path.Combine(root, "onnx", "tokenizer.json"), QueueCapacity: 2));
        var exampleVectors = await adapter.EmbedBatchAsync(examples.Select(x => x.Text).ToArray());
        var cache = examples.Select((x, i) => (x.Id, Vector: exampleVectors[i])).ToDictionary(x => x.Id, x => x.Vector);
        var scorer = new IntentScorer(examples, cache, aggregation);
        var pipeline = new IntentDecisionPipeline(adapter, scorer, minimumScore, minimumMargin);
        var queryVectors = await adapter.EmbedBatchAsync(cases.Select(x => x.Text).ToArray());
        var results = new List<object>();
        var counts = new Dictionary<string, int>();
        for (var i = 0; i < cases.Length; i++)
        {
            var item = cases[i];
            var expectedStatus = item.ExpectedStatus ?? expectations[item.Id].Status;
            var decision = await pipeline.DecideAsync(item.Text, queryVectors[i]);
            var status = decision.Status.ToString();
            var topMatch = decision.Candidate.Top?.IntentId == item.ExpectedIntent;
            var outcome = status == "Accepted" ? expectedStatus == "Accepted" && topMatch ? "CorrectAccept" : "FalseAccept"
                : expectedStatus == "Accepted" ? "ValidDeferred"
                : status == expectedStatus ? "CorrectNonAccept" : "DecisionMismatch";
            counts[outcome] = counts.GetValueOrDefault(outcome) + 1;
            Console.WriteLine($"{item.Id}|{decision.Candidate.Top?.IntentId}|{decision.Candidate.Top?.Score:F6}|{decision.Candidate.Second?.IntentId}|{decision.Candidate.Second?.Score:F6}|{decision.Candidate.Margin:F6}|{status}|{decision.Reason}|{outcome}");
            results.Add(new { item.Id, item.Text, item.ExpectedIntent, expectedStatus, item.Category,
                rationale = item.Rationale ?? expectations.GetValueOrDefault(item.Id)?.Rationale,
                topMatch, outcome, decision });
        }
        Console.WriteLine(JsonSerializer.Serialize(counts));
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(output))!);
        await File.WriteAllTextAsync(output, JsonSerializer.Serialize(new
        {
            source = Path.GetFileName(path), sha256 = Convert.ToHexString(SHA256.HashData(await File.ReadAllBytesAsync(path))),
            catalogProfile = profile, catalogCount = examples.Count, scoring, minimumScore, minimumMargin,
            singletonPolicy = "Use the one available sample without duplication; empty intents do not compete.",
            samplesPerIntent = examples.Where(x => x.IntentId != "AMBIGUOUS_MULTI_INTENT")
                .GroupBy(x => x.IntentId).ToDictionary(group => group.Key, group => group.Count()),
            cacheVectorBytes = cache.Count * 384 * sizeof(float), counts, results
        }, json));
        return 0;
    }
}
