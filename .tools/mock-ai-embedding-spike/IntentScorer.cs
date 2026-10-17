namespace MockAiEmbeddingSpike;

public enum RecognitionStatus { Accepted, ClarificationRequired, Unsupported }
public enum IntentScoreAggregation { Max, TopTwoMean }

public sealed record ExampleSimilarity(string ExampleId, double Cosine);
public sealed record IntentCandidate(string IntentId, string? ToolName, double Score, string ExampleId,
    IReadOnlyList<ExampleSimilarity>? Evidence = null);
public sealed record IntentRecognition(IntentCandidate? Top, IntentCandidate? Second, double Margin, RecognitionStatus Status,
    IReadOnlyList<IntentCandidate>? RankedCandidates = null);

public sealed class IntentScorer
{
    private readonly IReadOnlyList<IntentExample> _examples;
    private readonly IReadOnlyDictionary<string, float[]> _vectors;
    private readonly IntentScoreAggregation _aggregation;

    public IntentScorer(IReadOnlyList<IntentExample> examples, IReadOnlyDictionary<string, float[]> vectors,
        IntentScoreAggregation aggregation = IntentScoreAggregation.Max)
    {
        _examples = examples;
        _vectors = vectors;
        _aggregation = aggregation;
    }

    public IntentRecognition Score(IReadOnlyList<float> query, double minimumScore, double minimumMargin)
    {
        var candidates = _examples
            // Multi-intent is a decision about clauses, not a semantic business capability.
            // Keep historical sample IDs in the catalog, but do not let compound sentences
            // compete as a single tool intent with their own constituent queries.
            .Where(example => example.IntentId != "AMBIGUOUS_MULTI_INTENT")
            .GroupBy(example => example.IntentId)
            .Select(group =>
            {
                var best = group.Select(example => new ExampleSimilarity(example.Id, Cosine(query, _vectors[example.Id])))
                    .OrderByDescending(example => example.Cosine).Take(2).ToArray();
                // Average only the available distinct samples. A singleton keeps its own
                // score; do not duplicate it or invent a second prototype.
                var score = _aggregation == IntentScoreAggregation.TopTwoMean
                    ? best.Average(example => example.Cosine) : best[0].Cosine;
                return new IntentCandidate(group.Key, group.First().ToolName, score, best[0].ExampleId, best);
            })
            .OrderByDescending(candidate => candidate.Score)
            .ToArray();
        var top = candidates[0];
        var second = candidates.ElementAtOrDefault(1);
        var margin = second is null ? top.Score : top.Score - second.Score;
        var status = top.Score < minimumScore
            ? RecognitionStatus.Unsupported
            : margin < minimumMargin
                ? RecognitionStatus.ClarificationRequired
                : top.IntentId is "UNSUPPORTED_INTENT" or "WRITE_INTENT_UNSUPPORTED"
                    ? RecognitionStatus.Unsupported
                    : RecognitionStatus.Accepted;
        return new IntentRecognition(top, second, margin, status, candidates);
    }

    private static double Cosine(IReadOnlyList<float> a, IReadOnlyList<float> b) => a.Zip(b, (x, y) => x * y).Sum();
}
