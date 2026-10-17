using System.Text.RegularExpressions;

namespace MockAiEmbeddingSpike;

public enum CapabilityState { Supported, Unsupported, Undetermined }
public enum ParameterState { NotRequired, Sufficient, NeedsClarification, NotEvaluated }

public sealed record IntentDecision(
    IntentRecognition Semantic,
    IntentRecognition Candidate,
    string EffectiveText,
    RecognitionStatus Status,
    string Reason,
    bool IntentRecognized,
    CapabilityState Capability,
    ParameterState Parameters,
    IReadOnlyDictionary<string, string> Arguments,
    bool BackendAuthorizationRequired,
    bool RequestedOtherSubject,
    string? ToolForBackendCheck);

/// <summary>Offline decision policy. No role filtering, identity inference, tool execution or history memory.</summary>
public sealed class IntentDecisionPipeline
{
    private readonly SentenceEmbeddingAdapter _adapter;
    private readonly IntentScorer _scorer;
    private readonly double _minimumScore;
    private readonly double _minimumMargin;

    public IntentDecisionPipeline(SentenceEmbeddingAdapter adapter, IntentScorer scorer, double minimumScore, double minimumMargin)
    {
        _adapter = adapter;
        _scorer = scorer;
        _minimumScore = minimumScore;
        _minimumMargin = minimumMargin;
    }

    public async Task<IntentDecision> DecideAsync(string text, float[] vector, CancellationToken cancellationToken = default)
    {
        var semantic = _scorer.Score(vector, _minimumScore, _minimumMargin);
        var candidate = semantic;
        var effectiveText = text;
        var normalized = Normalize(text);
        var other = Match(normalized, @"\b(người khác|khách khác|khách hàng khác|tài khoản khác)\b");
        var arguments = new Dictionary<string, string>();
        string? toolOverride = null;

        IntentDecision Finish(RecognitionStatus status, string reason, ParameterState parameters = ParameterState.NotEvaluated,
            CapabilityState? capabilityOverride = null)
        {
            var supported = candidate.Top?.ToolName is not null;
            var recognized = candidate.Top is not null && candidate.Top.Score >= _minimumScore && candidate.Margin >= _minimumMargin;
            return new IntentDecision(semantic, candidate, effectiveText, status, reason, recognized,
                capabilityOverride ?? (!recognized ? CapabilityState.Undetermined : supported ? CapabilityState.Supported : CapabilityState.Unsupported),
                parameters, arguments, supported && capabilityOverride != CapabilityState.Unsupported, other,
                status == RecognitionStatus.Accepted ? toolOverride ?? candidate.Top?.ToolName : null);
        }

        if (Match(normalized, @"^(còn|vậy|thế)\b.*\b(thì sao|thế nào)\s*[?.!]*$"))
            return Finish(RecognitionStatus.ClarificationRequired, "MissingHistory");

        var clauses = Regex.Split(normalized, @"\s*(?:[,;]|\bvà\b|\brồi\b|\bnhưng\b)\s*", RegexOptions.CultureInvariant)
            .Where(clause => !string.IsNullOrWhiteSpace(clause)).ToArray();
        bool Negated(string clause) => Match(clause,
            @"^(?:(?:tôi|mình|em)\s+)?(?:không(?:\s+(?:muốn|cần))?|đừng|chớ)\s+(?:xem|hỏi|tra cứu|lấy|báo|cập nhật|đổi|xóa)\b");
        var positives = clauses.Where(clause => !Negated(clause)).ToArray();
        if (positives.Length == 0)
            return Finish(RecognitionStatus.Unsupported, "NegatedRequest");

        // Re-embed only the explicit affirmative clause after removing a negated request.
        // A terminal question particle 'không?' never matches the anchored negation rule.
        if (positives.Length != clauses.Length)
        {
            effectiveText = string.Join("; ", positives);
            candidate = _scorer.Score(await _adapter.EmbedAsync(effectiveText, cancellationToken), _minimumScore, _minimumMargin);
        }

        if (positives.Length > 1)
        {
            var clauseVectors = await _adapter.EmbedBatchAsync(positives, cancellationToken);
            var independentIntents = clauseVectors.Select(v => _scorer.Score(v, _minimumScore, _minimumMargin))
                .Where(result => result.Top?.Score >= _minimumScore && result.Top?.ToolName is not null)
                .Select(result => result.Top!.IntentId).Distinct().ToArray();
            if (independentIntents.Length > 1)
                return Finish(RecognitionStatus.ClarificationRequired, "MultipleIntents");
        }

        normalized = Normalize(effectiveText);
        // A capability veto is not a replacement intent: retain the embedding
        // candidate so a loyalty-to-orders error remains visible in evaluation.
        if (Match(normalized, @"\b(?:loyalty|điểm\s+(?:thưởng|tích lũy|thành viên|hội viên)|(?:tích điểm|tích được)\b.*\bđiểm|chương trình\s+(?:khách hàng thân thiết|tích điểm))\b"))
            return Finish(RecognitionStatus.Unsupported, "LoyaltyCapabilityNotSupported", capabilityOverride: CapabilityState.Unsupported);

        // Similarity to a mutation example is insufficient evidence of a write
        // request. Ask rather than relabel a read request or pick the runner-up.
        if (candidate.Top?.IntentId == "WRITE_INTENT_UNSUPPORTED" &&
            !Match(normalized, @"\b(?:(?:đổi|sửa|cập nhật|tăng|giảm|điều chỉnh)\s+(?:lại\s+)?(?:giá|trạng thái)|(?:xóa|xoá|hủy)\s+(?:(?:giúp|cho)\s+(?:tôi|mình)\s+)?(?:đơn|món|sản phẩm|nhân viên))\b"))
            return Finish(RecognitionStatus.ClarificationRequired, "UnconfirmedMutationIntent", capabilityOverride: CapabilityState.Undetermined);

        if (candidate.Status == RecognitionStatus.Unsupported)
            return Finish(RecognitionStatus.Unsupported, candidate.Top?.ToolName is null ? "CapabilityNotSupported" : "LowSimilarity");
        if (candidate.Top?.ToolName is null)
            return Finish(RecognitionStatus.ClarificationRequired, "CloseCapabilityCandidates");
        var intent = candidate.Top!.IntentId;
        normalized = Normalize(effectiveText);
        if (other && intent is "MY_ORDERS" or "MY_SHIFT")
            return Finish(RecognitionStatus.ClarificationRequired, "SubjectScopeMismatch");

        if (intent is "REVENUE_QUERY" or "REVENUE_COMPARISON" or "PRODUCT_RANKING" or "BUSINESS_SUMMARY")
        {
            var periods = Periods(normalized);
            if (intent == "REVENUE_COMPARISON")
            {
                var pairs = new[] { ("today", "yesterday"), ("this_week", "last_week"), ("this_month", "last_month") };
                var pair = pairs.FirstOrDefault(pair => periods.SetEquals(new[] { pair.Item1, pair.Item2 }));
                if (pair == default) return Finish(RecognitionStatus.ClarificationRequired, "MissingOrUnsupportedComparisonPeriods", ParameterState.NeedsClarification);
                arguments["currentPeriod"] = pair.Item1;
                arguments["comparisonPeriod"] = pair.Item2;
            }
            else
            {
                if (periods.Count != 1) return Finish(RecognitionStatus.ClarificationRequired, "MissingOrAmbiguousPeriod", ParameterState.NeedsClarification);
                var period = periods.Single();
                var allowed = intent == "REVENUE_QUERY" ? new[] { "today", "yesterday", "this_month", "this_year" }
                    : intent == "PRODUCT_RANKING" ? new[] { "today", "yesterday", "this_week", "this_month" }
                    : new[] { "today", "yesterday", "this_week", "last_week", "this_month", "last_month" };
                if (!allowed.Contains(period)) return Finish(RecognitionStatus.ClarificationRequired, "UnsupportedPeriod", ParameterState.NeedsClarification);
                arguments["period"] = intent == "PRODUCT_RANKING" ? period switch { "this_week" => "week", "this_month" => "month", _ => period }
                    : intent == "REVENUE_QUERY" ? period switch { "this_month" => "month", "this_year" => "year", _ => period }
                    : period;
                if (intent == "REVENUE_QUERY" && period == "yesterday") toolOverride = "get_business_summary";
            }
            if (candidate.Status == RecognitionStatus.ClarificationRequired)
                return Finish(RecognitionStatus.ClarificationRequired, "CloseCandidates", ParameterState.Sufficient);
            return Finish(RecognitionStatus.Accepted, "ReadyForBackendChecks", ParameterState.Sufficient);
        }

        if (intent == "MY_SHIFT" && Periods(normalized).Any(period => period != "today"))
            return Finish(RecognitionStatus.ClarificationRequired, "ShiftToolOnlyToday", ParameterState.NeedsClarification);
        if (candidate.Status == RecognitionStatus.ClarificationRequired)
            return Finish(RecognitionStatus.ClarificationRequired, "CloseCandidates", ParameterState.NotRequired);
        return Finish(RecognitionStatus.Accepted, "ReadyForBackendChecks", ParameterState.NotRequired);
    }

    // Strict allowlist; no arbitrary relative date resolution, default period or identity parsing.
    private static HashSet<string> Periods(string text)
    {
        var definitions = new[]
        {
            ("today", @"\b(hôm nay|trong ngày|ngày hiện tại)\b"),
            ("yesterday", @"\bhôm qua\b"), ("this_week", @"\b(?:tuần (?:này|hiện tại)|trong tuần(?!\s+(?:trước|sau|tới|vừa|qua|\d)))\b"),
            ("last_week", @"\btuần trước\b"), ("this_month", @"\btháng (?:này|hiện tại)\b"),
            ("last_month", @"\btháng trước\b"), ("this_year", @"\b(năm nay|cả năm)\b"),
            ("unsupported", @"\b(hôm kia|ngày mai|tuần sau|tháng sau)\b")
        };
        return definitions.Where(item => Match(text, item.Item2)).Select(item => item.Item1).ToHashSet();
    }

    private static string Normalize(string text) => Regex.Replace(text.Normalize().ToLowerInvariant(), @"\s+", " ").Trim();
    private static bool Match(string text, string pattern) => Regex.IsMatch(text, pattern, RegexOptions.CultureInvariant);
}
