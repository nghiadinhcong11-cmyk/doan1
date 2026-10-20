using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MockAiEmbeddingSpike;

public static class CustomerGuestEmbeddingResearchRunner
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true, WriteIndented = true };

    public static async Task<int> RunGuestBoundaryRegressionAsync(string[] args)
    {
        var root = GetOption(args, "--customer-guest-root") ?? Path.Combine("experiments", "customer-guest-embedding-v2");
        var manifest = Load<Manifest>(Path.Combine(root, "manifest.json"));
        var model = Path.GetFullPath(Path.Combine(root, "../../artifacts/onnx/model_O4.onnx"));
        var tokenizer = Path.GetFullPath(Path.Combine(root, "../../artifacts/onnx/tokenizer.json"));
        var reference = LoadCases(Path.Combine(root, manifest.Splits.Reference));
        using var adapter = new SentenceEmbeddingAdapter(new SentenceEmbeddingOptions(model, tokenizer));
        var referenceVectors = (await adapter.EmbedBatchAsync(reference.Select(x => x.Text).ToArray())).Select((vector, index) => (x: reference[index], vector)).ToArray();
        var guestAccepted = new[]
        {
            "Cho tôi xem thực đơn.", "Quán có những đồ uống nào?", "cho toi xem mon khai vi",
            "Cho xem món chính.", "Có những món nào đang bán?", "Danh sách món ăn của quán.",
            "Thực đơn hôm nay có gì?"
        };
        var guestRejected = new[]
        {
            "Cho tôi xem những đơn đã đặt.", "Cho xem đơn của khách hàng khác.", "Các đơn tôi đã gọi đâu?",
            "Đơn gần đây của tôi.", "Tôi đã đặt những gì?", "Lịch sử đơn hàng của tôi.",
            "cho toi xem don da dat", "Đơn này đang xử lý chưa?"
        };
        var customerAccepted = new[] { "Đơn của tôi đâu?", "Cho tôi xem thực đơn." };
        var customerRejected = new[] { "Cho xem đơn của khách hàng khác.", "Doanh thu hôm nay bao nhiêu?", "Cập nhật giá món phở thành 50000." };
        var failures = new List<string>();
        foreach (var text in guestAccepted.Concat(guestRejected))
        {
            var item = new ResearchCase("BOUNDARY", "guest", null, null, text, null, guestRejected.Contains(text) ? "unauthorized" : "accepted");
            var decision = Decide(item, await adapter.EmbedAsync(text), referenceVectors, manifest.Policy);
            var expected = guestRejected.Contains(text) ? "REJECT" : "ACCEPT";
            if (!string.Equals(decision.ResponseDecision, expected, StringComparison.Ordinal)) failures.Add($"{text} => {decision.ResponseDecision}, expected {expected}");
        }
        foreach (var text in customerAccepted.Concat(customerRejected))
        {
            var item = new ResearchCase("BOUNDARY", "customer", null, null, text, null, customerRejected.Contains(text) ? "unauthorized" : "accepted");
            var decision = Decide(item, await adapter.EmbedAsync(text), referenceVectors, manifest.Policy);
            var expected = customerRejected.Contains(text) ? "REJECT" : "ACCEPT";
            if (!string.Equals(decision.ResponseDecision, expected, StringComparison.Ordinal)) failures.Add($"customer: {text} => {decision.ResponseDecision}, expected {expected}");
        }
        var total = guestAccepted.Length + guestRejected.Length + customerAccepted.Length + customerRejected.Length;
        Console.WriteLine($"GUEST_CUSTOMER_BOUNDARY_REGRESSION|pass={total - failures.Count}/{total}");
        foreach (var failure in failures) Console.WriteLine($"FAIL|{failure}");
        return failures.Count == 0 ? 0 : 1;
    }

    public static async Task<int> RunAsync(string[] args)
    {
        var root = GetOption(args, "--customer-guest-root") ?? Path.Combine("experiments", "customer-guest-embedding-v1");
        var manifest = Load<Manifest>(Path.Combine(root, "manifest.json"));
        var model = Path.GetFullPath(Path.Combine(root, "../../artifacts/onnx/model_O4.onnx"));
        var tokenizer = Path.GetFullPath(Path.Combine(root, "../../artifacts/onnx/tokenizer.json"));
        if (!File.Exists(model) || !File.Exists(tokenizer))
        {
            Console.Error.WriteLine($"Missing canonical E5 artifacts. model={model}, tokenizer={tokenizer}");
            return 2;
        }

        var reference = LoadCases(Path.Combine(root, manifest.Splits.Reference));
        var sets = new Dictionary<string, IReadOnlyList<ResearchCase>>(StringComparer.OrdinalIgnoreCase)
        {
            ["development"] = LoadCases(Path.Combine(root, manifest.Splits.Development)),
            ["calibration"] = LoadCases(Path.Combine(root, manifest.Splits.Calibration)),
            ["holdout"] = LoadCases(Path.Combine(root, manifest.Splits.Holdout))
        };

        using var adapter = new SentenceEmbeddingAdapter(new SentenceEmbeddingOptions(model, tokenizer));
        var referenceVectors = (await adapter.EmbedBatchAsync(reference.Select(x => x.Text).ToArray()))
            .Select((vector, index) => (x: reference[index], vector)).ToArray();
        var output = new Dictionary<string, EvaluationReport>(StringComparer.OrdinalIgnoreCase);
        foreach (var set in sets)
            output[set.Key] = await Evaluate(set.Key, set.Value, referenceVectors, adapter, manifest.Policy);

        var result = new
        {
            manifest = "manifest.json",
            model = Path.GetFileName(model),
            tokenizer = Path.GetFileName(tokenizer),
            modelSha256 = Sha256(model),
            tokenizerSha256 = Sha256(tokenizer),
            output,
            runtimeIntegrated = false,
            toolsExecuted = false,
            holdoutUsedForTuning = false
        };
        var outputPath = GetOption(args, "--output") ?? Path.Combine(root, "results.json");
        await File.WriteAllTextAsync(outputPath, JsonSerializer.Serialize(result, JsonOptions));
        foreach (var item in output)
            Console.WriteLine($"{item.Key}|accuracy={item.Value.Accuracy:F3}|macroF1={item.Value.MacroF1:F3}|category={item.Value.CategoryAccuracy:F3}|unsupported={item.Value.UnsupportedCapabilityRejectionRate:F3}|unauthorized={item.Value.UnauthorizedRejectionRate:F3}|ood={item.Value.OodRejectionRate:F3}|negation={item.Value.NegationRejectionRate:F3}|multi={item.Value.MultiIntentHandlingRate:F3}|falseAcceptance={item.Value.FalseAcceptanceCount}|falseRejection={item.Value.FalseRejectionCount}|response={item.Value.ResponseDecisionAccuracy:F3}");
        Console.WriteLine($"RESULT_JSON={Path.GetFullPath(outputPath)}");
        return 0;
    }

    private static async Task<EvaluationReport> Evaluate(string setName, IReadOnlyList<ResearchCase> cases,
        IReadOnlyList<(ResearchCase x, float[] vector)> reference, SentenceEmbeddingAdapter adapter, ManifestPolicy policy)
    {
        var accepted = cases.Where(x => x.CaseType == "accepted").ToArray();
        var classifications = new List<Classification>();
        var rows = new List<CaseEvaluation>();
        var categoryTotal = 0; var categoryCorrect = 0;
        var oodTotal = 0; var oodRejected = 0; var unauthorizedTotal = 0; var unauthorizedRejected = 0;
        var unsupportedTotal = 0; var unsupportedRejected = 0; var negationTotal = 0; var negationRejected = 0;
        var multiTotal = 0; var multiHandled = 0; var falseAcceptance = 0; var falseRejection = 0; var responseCorrect = 0;

        foreach (var item in cases)
        {
            var decision = Decide(item, await adapter.EmbedAsync(item.Text), reference, policy);
            classifications.Add(decision);
            var expectedDecision = ExpectedResponse(item);
            if (decision.ResponseDecision == expectedDecision) responseCorrect++;
            if (item.CaseType == "accepted")
            {
                if (decision.ResponseDecision != "ACCEPT" || decision.Intent != item.Intent) falseRejection++;
            }
            else if (decision.ResponseDecision == "ACCEPT") falseAcceptance++;
            if (item.Category is not null) { categoryTotal++; if (decision.Category == item.Category) categoryCorrect++; }
            if (item.CaseType == "ood") { oodTotal++; if (decision.ResponseDecision != "ACCEPT") oodRejected++; }
            if (item.CaseType is "unauthorized" or "write") { unauthorizedTotal++; if (decision.ResponseDecision != "ACCEPT") unauthorizedRejected++; }
            if (item.CaseType is "unsupported" or "ood") { unsupportedTotal++; if (decision.ResponseDecision != "ACCEPT") unsupportedRejected++; }
            if (item.CaseType == "negation") { negationTotal++; if (decision.ResponseDecision != "ACCEPT") negationRejected++; }
            if (item.CaseType == "multi_intent") { multiTotal++; if (decision.ResponseDecision == "CLARIFY") multiHandled++; }
            rows.Add(new CaseEvaluation(item.Id, item.Role, item.CaseType, item.Text, item.Intent, item.Tool, decision.Intent, decision.Tool,
                item.Category, decision.Category, item.Category is null || decision.Category == item.Category, decision.Score, decision.Margin,
                expectedDecision, decision.ResponseDecision, decision.Reason));
        }

        var intents = accepted.Select(x => x.Intent!).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        var confusion = intents.ToDictionary(intent => intent, intent => intents.ToDictionary(other => other, _ => 0), StringComparer.OrdinalIgnoreCase);
        var acceptedWithIndexes = cases.Select((x, index) => (x, index)).Where(item => item.x.CaseType == "accepted").ToArray();
        foreach (var item in acceptedWithIndexes)
        {
            var predicted = classifications[item.index].Intent;
            if (predicted is not null && confusion.ContainsKey(item.x.Intent!) && confusion[item.x.Intent!].ContainsKey(predicted)) confusion[item.x.Intent!][predicted]++;
        }
        var correctIntent = acceptedWithIndexes.Count(item => item.x.Intent == classifications[item.index].Intent);
        var f1 = intents.Select(intent => F1(intent, cases, classifications)).ToArray();
        return new EvaluationReport(setName, cases.Count, accepted.Length,
            accepted.Length == 0 ? 0 : (double)correctIntent / accepted.Length,
            f1.Length == 0 ? 0 : f1.Average(), categoryTotal == 0 ? 1 : (double)categoryCorrect / categoryTotal,
            oodTotal == 0 ? 1 : (double)oodRejected / oodTotal,
            unauthorizedTotal == 0 ? 1 : (double)unauthorizedRejected / unauthorizedTotal,
            unsupportedTotal == 0 ? 1 : (double)unsupportedRejected / unsupportedTotal,
            negationTotal == 0 ? 1 : (double)negationRejected / negationTotal,
            multiTotal == 0 ? 1 : (double)multiHandled / multiTotal,
            falseAcceptance, accepted.Length - correctIntent, cases.Count == 0 ? 0 : (double)responseCorrect / cases.Count, confusion, rows);
    }

    private static Classification Decide(ResearchCase item, float[] query, IReadOnlyList<(ResearchCase x, float[] vector)> reference, ManifestPolicy policy)
    {
        var normalized = Normalize(item.Text);
        if (ContainsWrite(normalized)) return Reject("WRITE_OR_DESTRUCTIVE");
        if (ContainsNegation(normalized)) return Reject("NEGATION");
        if (ContainsMultiIntent(normalized)) return new Classification(null, null, "CLARIFY", "MULTI_INTENT", 0, 0, ExtractCategory(normalized));
        if (ContainsUnsupportedMenuQuery(normalized)) return Reject("UNSUPPORTED_MENU_FILTER");
        if (item.Role.Equals("guest", StringComparison.OrdinalIgnoreCase) && ContainsGuestUnauthorizedRequest(normalized)) return Reject("GUEST_SCOPE");
        if (item.Role.Equals("customer", StringComparison.OrdinalIgnoreCase) && ContainsCustomerUnauthorizedRequest(normalized)) return Reject("CUSTOMER_SCOPE");

        var allowed = item.Role.Equals("guest", StringComparison.OrdinalIgnoreCase) ? policy.GuestIntents : policy.CustomerIntents;
        var candidates = reference.Where(x => x.x.Intent is not null && allowed.Contains(x.x.Intent, StringComparer.OrdinalIgnoreCase))
            .GroupBy(x => x.x.Intent!, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.Select(x => new { Intent = group.Key, x.x.Tool, Score = Cosine(query, x.vector) }).OrderByDescending(x => x.Score).First())
            .OrderByDescending(x => x.Score).ToArray();
        if (candidates.Length == 0) return Reject("NO_ALLOWED_INTENT");
        var top = candidates[0];
        var margin = candidates.Length == 1 ? top.Score : top.Score - candidates[1].Score;
        var category = top.Intent.Equals("MENU_QUERY", StringComparison.OrdinalIgnoreCase) ? ExtractCategory(normalized) : null;
        if (ContainsUnresolvedCategory(normalized)) return new Classification(top.Intent, top.Tool, "CLARIFY", "UNRESOLVED_CATEGORY", top.Score, margin, category);
        if (top.Score < policy.MinimumScore) return new Classification(top.Intent, top.Tool, "REJECT", "LOW_SCORE", top.Score, margin, category);
        if (margin < policy.MinimumMargin) return new Classification(top.Intent, top.Tool, "CLARIFY", "LOW_MARGIN", top.Score, margin, category);
        return new Classification(top.Intent, top.Tool, "ACCEPT", "SEMANTIC_MATCH", top.Score, margin, category);
    }

    private static Classification Reject(string reason) => new(null, null, "REJECT", reason, 0, 0, null);
    private static string ExpectedResponse(ResearchCase item) => item.CaseType switch { "accepted" => "ACCEPT", "multi_intent" or "clarify" => "CLARIFY", _ => "REJECT" };
    private static double F1(string intent, IReadOnlyList<ResearchCase> cases, IReadOnlyList<Classification> decisions)
    {
        var tp = 0; var fp = 0; var fn = 0;
        for (var i = 0; i < cases.Count; i++)
        {
            var acceptedPrediction = decisions[i].ResponseDecision == "ACCEPT";
            if (acceptedPrediction && decisions[i].Intent == intent && cases[i].Intent == intent) tp++;
            else if (acceptedPrediction && decisions[i].Intent == intent) fp++;
            else if (cases[i].Intent == intent) fn++;
        }
        var precision = tp + fp == 0 ? 0 : (double)tp / (tp + fp); var recall = tp + fn == 0 ? 0 : (double)tp / (tp + fn);
        return precision + recall == 0 ? 0 : 2 * precision * recall / (precision + recall);
    }
    private static string? ExtractCategory(string value)
    {
        if (ContainsAny(value, "do uong", "nuoc uong", "loai nuoc", "thuc uong")) return "do_uong";
        if (ContainsAny(value, "khai vi")) return "khai_vi";
        if (ContainsAny(value, "mon chinh")) return "mon_chinh";
        if (ContainsAny(value, "mon an", "do an", "gia mon")) return "mon_an";
        return null;
    }
    private static bool ContainsWrite(string value) => ContainsAny(value, "doi gia", "doi ten", "cap nhat", "xoa", "sua gia", "sua ten", "dat mon", "tao don", "sua san pham");
    private static bool ContainsNegation(string value) => ContainsAny(value, "dung xem", "khong xem", "khong cho", "khong muon");
    private static bool ContainsGuestUnauthorizedRequest(string value) =>
        ContainsAny(value, "doanh thu", "dashboard", "chi nhanh khac", "khach khac", "nguoi khac", "don cua khach", "don ca nhan", "don hang ca nhan", "don cua minh", "du lieu cua") ||
        Regex.IsMatch(value, @"(?<!thuc )\bdon\b|\b(?:da dat|dat nhung gi|da goi|goi nhung gi)\b", RegexOptions.CultureInvariant);
    private static bool ContainsCustomerUnauthorizedRequest(string value) => ContainsAny(value, "doanh thu", "dashboard", "chi nhanh khac", "khach khac", "khach hang khac", "nguoi khac", "tai khoan khac", "don cua khach", "don ca nhan", "don hang ca nhan", "du lieu cua");
    private static bool ContainsUnsupportedMenuQuery(string value) => ContainsAny(value, "duoi 50000", "tren 50000", "nam muoi nghin", "thap hon", "cao hon", "ton kho", "trong kho", "con bao nhieu", "tim mon", "doi mat khau", "bao cao nhan su", "luong nhan vien") || LooksLikeItemLookup(value);
    private static bool LooksLikeItemLookup(string value) => Regex.IsMatch(value, @"\bco\s+\S+(?:\s+\S+){0,3}\s+khong\b", RegexOptions.CultureInvariant) && !ContainsAny(value, "mon", "do uong", "khai vi", "mon chinh", "menu", "thuc don", "don", "dat");
    private static bool ContainsUnresolvedCategory(string value) => value.Contains("danh muc nay", StringComparison.Ordinal);
    private static bool ContainsMultiIntent(string value) =>
        (value.Contains(" va ", StringComparison.Ordinal) || value.Contains(" roi ", StringComparison.Ordinal) || value.Contains(" sau do ", StringComparison.Ordinal) || value.Contains(" dong thoi ", StringComparison.Ordinal)) &&
        ContainsAny(value, "menu", "mon", "thuc don") && ContainsAny(value, "don hang", "don cua", "doanh thu");
    private static bool ContainsAny(string value, params string[] candidates) => candidates.Any(value.Contains);
    private static string Normalize(string value)
    {
        var folded = value.Replace('đ', 'd').Replace('Đ', 'D').Normalize(NormalizationForm.FormD);
        return new string(folded.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray()).ToLowerInvariant();
    }
    private static double Cosine(IReadOnlyList<float> a, IReadOnlyList<float> b) => a.Zip(b, (x, y) => x * y).Sum();
    private static string Sha256(string path) => Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(path)));
    private static IReadOnlyList<ResearchCase> LoadCases(string path) => Load<ResearchCase[]>(path);
    private static T Load<T>(string path) => JsonSerializer.Deserialize<T>(File.ReadAllText(path), JsonOptions) ?? throw new InvalidDataException($"Empty JSON: {path}");
    private static string? GetOption(string[] args, string name) { var i = Array.FindIndex(args, x => string.Equals(x, name, StringComparison.OrdinalIgnoreCase)); return i >= 0 && i + 1 < args.Length ? args[i + 1] : null; }

    private sealed record Manifest(ManifestSplits Splits, ManifestPolicy Policy);
    private sealed record ManifestSplits(string Reference, string Development, string Calibration, string Holdout);
    private sealed record ManifestPolicy(double MinimumScore, double MinimumMargin, string[] CustomerIntents, string[] GuestIntents, string[] CustomerTools, string[] GuestTools);
    private sealed record ResearchCase(string Id, string Role, string? Intent, string? Tool, string Text, string? Category, string CaseType);
    private sealed record Classification(string? Intent, string? Tool, string ResponseDecision, string Reason, double Score, double Margin, string? Category);
    private sealed record CaseEvaluation(string Id, string Role, string CaseType, string Text, string? ExpectedIntent, string? ExpectedTool, string? PredictedIntent, string? PredictedTool, string? ExpectedCategory, string? ParsedCategory, bool CategoryPass, double Score, double Margin, string ExpectedDecision, string ActualDecision, string Reason);
    private sealed record EvaluationReport(string Set, int CaseCount, int AcceptedCaseCount, double Accuracy, double MacroF1, double CategoryAccuracy, double OodRejectionRate, double UnauthorizedRejectionRate, double UnsupportedCapabilityRejectionRate, double NegationRejectionRate, double MultiIntentHandlingRate, int FalseAcceptanceCount, int FalseRejectionCount, double ResponseDecisionAccuracy, Dictionary<string, Dictionary<string, int>> ConfusionMatrix, IReadOnlyList<CaseEvaluation> Cases);
}
