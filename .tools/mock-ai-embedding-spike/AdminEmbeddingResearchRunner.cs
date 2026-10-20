using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MockAiEmbeddingSpike;

/// <summary>
/// Offline Admin/owner research evaluator. It deliberately stops at semantic
/// recognition, slot parsing and policy decisions; it never resolves or calls a
/// production AI tool and never opens a database connection.
/// </summary>
public static class AdminEmbeddingResearchRunner
{
    private static readonly IReadOnlySet<string> SupportedAdminIntents = new HashSet<string>(StringComparer.Ordinal)
    {
        "REVENUE_QUERY", "BUSINESS_SUMMARY", "REVENUE_COMPARISON", "PRODUCT_RANKING", "ORDER_COUNT", "ORDER_STATUS"
    };
    public static async Task<int> RunAsync(string[] args)
    {
        var artifactRoot = Option(args, "--artifact-root") ?? "artifacts";
        var experimentRoot = Option(args, "--admin-root") ?? Path.Combine("experiments", "admin-owner-embedding-v1");
        var experimentId = Path.GetFileName(experimentRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        var output = Option(args, "--output") ?? Path.Combine(artifactRoot, $"{experimentId}-result.json");
        var model = Path.Combine(artifactRoot, "onnx", "model_O4.onnx");
        var tokenizer = Path.Combine(artifactRoot, "onnx", "tokenizer.json");
        var reference = Load<AdminCase>(Path.Combine(experimentRoot, "reference.json"));
        var development = Load<AdminCase>(Path.Combine(experimentRoot, "development.json"));
        var calibration = Load<AdminCase>(Path.Combine(experimentRoot, "calibration.json"));
        var holdout = Load<AdminCase>(Path.Combine(experimentRoot, "holdout.json"));
        EnsureDisjoint(reference, development, calibration, holdout);
        ValidateToolContracts(reference, development, calibration, holdout);

        using var adapter = new SentenceEmbeddingAdapter(new SentenceEmbeddingOptions(model, tokenizer));
        var all = reference.Concat(development).Concat(calibration).Concat(holdout).ToArray();
        var vectors = await adapter.EmbedBatchAsync(all.Select(x => x.Text).ToArray());
        var referenceVectors = vectors[..reference.Length];
        var developmentVectors = vectors[reference.Length..(reference.Length + development.Length)];
        var calibrationStart = reference.Length + development.Length;
        var calibrationVectors = vectors[calibrationStart..(calibrationStart + calibration.Length)];
        var holdoutVectors = vectors[(calibrationStart + calibration.Length)..];
        var examples = reference.Select((x, i) => new IntentExample(x.Id, x.ExpectedIntent, x.Text, x.ToolName, "admin")).ToArray();
        var scorer = new IntentScorer(examples, reference.Select((x, i) => (x.Id, vectors: referenceVectors[i])).ToDictionary(x => x.Id, x => x.vectors));
        var selected = Calibrate(scorer, calibration, calibrationVectors);
        var developmentRows = development.Select((x, i) => Decide(x, scorer.Score(developmentVectors[i], selected.Score, selected.Margin), selected)).ToArray();
        var calibrationRows = calibration.Select((x, i) => Decide(x, scorer.Score(calibrationVectors[i], selected.Score, selected.Margin), selected)).ToArray();
        var holdoutRows = holdout.Select((x, i) => Decide(x, scorer.Score(holdoutVectors[i], selected.Score, selected.Margin), selected)).ToArray();
        var result = new
        {
            experiment = experimentId,
            role = "admin",
            model = "intfloat/multilingual-e5-small",
            artifact = Path.GetFileName(model),
            tokenizer = Path.GetFileName(tokenizer),
            prefix = "query: ",
            pooling = "attention-mask-aware mean pooling over last_hidden_state, then L2 normalization",
            scoring = new { cosine = "dot product of normalized vectors", selected.Score, selected.Margin },
            referenceCount = reference.Length,
            development = Metrics(developmentRows),
            calibration = Metrics(calibrationRows),
            holdout = Metrics(holdoutRows),
            slots = new
            {
                period = SlotMetrics(holdoutRows, "period"),
                status = SlotMetrics(holdoutRows, "status"),
                limit = SlotMetrics(holdoutRows, "limit")
            },
            confusionMatrix = Confusion(holdoutRows),
            rows = new { development = developmentRows, calibration = calibrationRows, holdout = holdoutRows }
        };
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(output))!);
        await File.WriteAllTextAsync(output, JsonSerializer.Serialize(result, new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        }));
        Console.WriteLine($"ADMIN_CONFIG|score={selected.Score.ToString(CultureInfo.InvariantCulture)}|margin={selected.Margin.ToString(CultureInfo.InvariantCulture)}");
        Print("DEVELOPMENT", developmentRows);
        Print("CALIBRATION", calibrationRows);
        Print("HOLDOUT", holdoutRows);
        Console.WriteLine($"RESULT_JSON={output}");
        return 0;
    }

    private static Threshold Calibrate(IntentScorer scorer, IReadOnlyList<AdminCase> rows, float[][] vectors)
    {
        Threshold? best = null;
        for (var score = 0.70; score <= 0.90; score += 0.01)
        for (var margin = 0.00; margin <= 0.05; margin += 0.005)
        {
            var candidate = new Threshold(Math.Round(score, 3), Math.Round(margin, 3));
            var evaluated = rows.Select((x, i) => Decide(x, scorer.Score(vectors[i], candidate.Score, candidate.Margin), candidate)).ToArray();
            var falseAccepts = evaluated.Count(x => x.ExpectedDecision != x.Decision && x.Decision == "ACCEPT");
            var correct = evaluated.Count(x => x.Pass);
            var value = correct * 10 - falseAccepts * 30;
            if (best is null || value > best.Value)
                best = candidate with { Value = value };
        }
        return best!;
    }

    private static AdminResult Decide(AdminCase expected, IntentRecognition recognition, Threshold threshold)
    {
        var top = recognition.Top;
        var predicted = top?.IntentId ?? "NONE";
        var predictedSlots = ParseSlots(expected.Text);
        var normalized = Fold(expected.Text);
        var lexicalIntent = ResolveLexicalIntent(normalized);
        predicted = lexicalIntent ?? predicted;
        var decision = DecidePolicy(normalized, predicted, recognition, predictedSlots);
        var slotPass = expected.ExpectedSlots.All(pair => predictedSlots.TryGetValue(pair.Key, out var actual) && actual == pair.Value);
        var pass = decision == expected.ExpectedDecision &&
            (expected.ExpectedDecision != "ACCEPT" || predicted == expected.ExpectedIntent) && slotPass;
        return new AdminResult(expected.Id, expected.Text, expected.ExpectedIntent, expected.ExpectedDecision,
            predicted, decision, top?.Score ?? 0, recognition.Second?.Score ?? 0, recognition.Margin,
            expected.ExpectedSlots, predictedSlots, pass);
    }

    private static string DecidePolicy(string normalized, string predicted, IntentRecognition recognition, IReadOnlyDictionary<string, string> slots)
    {
        if (IsNegated(normalized)) return "CLARIFY";
        if (IsOutOfScope(normalized)) return "UNSUPPORTED";
        if (IsMultiIntent(normalized)) return "CLARIFY";
        if (recognition.Status == RecognitionStatus.Unsupported || predicted == "OUT_OF_SCOPE") return "UNSUPPORTED";
        if (recognition.Status == RecognitionStatus.ClarificationRequired || predicted is "NEGATED_REQUEST" or "MULTI_INTENT") return "CLARIFY";
        if (RequiresPeriod(predicted) && !slots.ContainsKey("period")) return "CLARIFY";
        if (predicted == "REVENUE_COMPARISON" && (!slots.ContainsKey("currentPeriod") || !slots.ContainsKey("comparisonPeriod"))) return "CLARIFY";
        if (predicted == "ORDER_STATUS" && !slots.ContainsKey("status")) return "CLARIFY";
        return "ACCEPT";
    }

    private static bool RequiresPeriod(string intent) => intent is "REVENUE_QUERY" or "BUSINESS_SUMMARY" or "PRODUCT_RANKING" or "ORDER_COUNT" or "ORDER_STATUS";

    private static bool IsNegated(string value) => Regex.IsMatch(value,
        @"(^|\s)(khong|dung|chua)(?=\s+(?:xem|can|muon|yeu cau|cho|thuc hien|kiem tra|lay|bao))",
        RegexOptions.CultureInvariant);

    private static string? ResolveLexicalIntent(string value)
    {
        if (Regex.IsMatch(value, @"\b(?:so sanh|doi chieu|tang hay giam|tang giam|ky nay.*ky truoc|so voi|vs)\b", RegexOptions.CultureInvariant))
            return "REVENUE_COMPARISON";
        if (Regex.IsMatch(value, @"\b(?:xep hang mon|mon nao.*(?:ban chay|dung dau)|ban chay|top \d+ mon)\b", RegexOptions.CultureInvariant))
            return "PRODUCT_RANKING";
        if (Regex.IsMatch(value, @"\b(?:don|danh sach don).*(?:dang xu ly|hoan thanh|da huy)|(?:dang xu ly|hoan thanh|da huy).*\b(?:don|danh sach)\b", RegexOptions.CultureInvariant))
            return "ORDER_STATUS";
        if (Regex.IsMatch(value, @"\b(?:so don|bao nhieu don|dem don|luong don)\b", RegexOptions.CultureInvariant))
            return "ORDER_COUNT";
        if (Regex.IsMatch(value, @"\b(?:chi so|kpi|tong quan|tom tat|bao cao|tinh hinh)\b", RegexOptions.CultureInvariant))
            return "BUSINESS_SUMMARY";
        if (Regex.IsMatch(value, @"\b(?:doanh thu|doanh so|tien ban|thu ve|ban duoc bao nhieu tien)\b", RegexOptions.CultureInvariant))
            return "REVENUE_QUERY";
        return null;
    }

    private static bool IsOutOfScope(string value) =>
        Regex.IsMatch(value, @"\b(?:ton kho|nguyen lieu|ca lam|lich lam|nhan vien|mo cua|gio phuc vu|doi gia|cap nhat|xoa|sua gia|tich diem|loyalty)\b", RegexOptions.CultureInvariant);

    private static bool IsMultiIntent(string value)
    {
        var summaryLanguage = Regex.IsMatch(value, @"\b(?:tom tat|tong quan|tong hop|bao cao)\b", RegexOptions.CultureInvariant);
        if (summaryLanguage) return false;

        var capabilityFamilies = new[]
        {
            Regex.IsMatch(value, @"\b(?:doanh thu|doanh so|tien ban|thu ve)\b", RegexOptions.CultureInvariant),
            Regex.IsMatch(value, @"\b(?:menu|thuc don|mon ban|mon an)\b", RegexOptions.CultureInvariant),
            Regex.IsMatch(value, @"\b(?:ban trong|ban nao|tinh trang ban)\b", RegexOptions.CultureInvariant),
            Regex.IsMatch(value, @"\b(?:don hang|so don|danh sach don)\b", RegexOptions.CultureInvariant)
        };
        return capabilityFamilies.Count(x => x) >= 2 && Regex.IsMatch(value, @"\b(?:va|roi|rồi)\b", RegexOptions.CultureInvariant);
    }

    private static Dictionary<string, string> ParseSlots(string text)
    {
        var value = Fold(text);
        var slots = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var period = value switch
        {
            var x when x.Contains("hom nay") => "today",
            var x when x.Contains("hom qua") => "yesterday",
            var x when x.Contains("tuan nay") => "this_week",
            var x when x.Contains("tuan truoc") => "last_week",
            var x when x.Contains("thang nay") => "this_month",
            var x when x.Contains("thang truoc") => "last_month",
            var x when x.Contains("nam nay") => "year",
            _ => null
        };
        if (period is not null) slots["period"] = period;
        var periodTokens = new[] { ("hom nay", "today"), ("hom qua", "yesterday"), ("tuan nay", "this_week"), ("tuan truoc", "last_week"), ("thang nay", "this_month"), ("thang truoc", "last_month"), ("nam nay", "year") };
        var periods = periodTokens.Where(x => value.Contains(x.Item1, StringComparison.Ordinal)).Select(x => x.Item2).ToArray();
        if (periods.Length > 0) slots["currentPeriod"] = periods[0];
        if (periods.Length > 1) slots["comparisonPeriod"] = periods[1];
        var status = value.Contains("hoan thanh") ? "completed" : value.Contains("dang xu ly") ? "processing" : value.Contains("da huy") ? "cancelled" : null;
        if (status is not null) slots["status"] = status;
        var limit = Regex.Match(value, @"(?:top|hang dau|dau)\s*(\d+|nam|muoi|ba)", RegexOptions.CultureInvariant);
        if (limit.Success) slots["limit"] = limit.Groups[1].Value switch { "nam" => "5", "muoi" => "10", "ba" => "3", var number => number };
        if (slots.ContainsKey("limit") == false)
        {
            if (Regex.IsMatch(value, @"\bnam mon\b")) slots["limit"] = "5";
            else if (Regex.IsMatch(value, @"\bmuoi don\b")) slots["limit"] = "10";
            else if (Regex.IsMatch(value, @"\bba mon\b")) slots["limit"] = "3";
            else if (Regex.IsMatch(value, @"\bhai don\b")) slots["limit"] = "2";
        }
        return slots;
    }

    private static Dictionary<string, int> SlotMetrics(IReadOnlyList<AdminResult> rows, string slot)
    {
        var applicable = rows.Where(x => x.ExpectedSlots.ContainsKey(slot)).ToArray();
        return new Dictionary<string, int>
        {
            ["cases"] = applicable.Length,
            ["correct"] = applicable.Count(x => x.PredictedSlots.TryGetValue(slot, out var value) && value == x.ExpectedSlots[slot])
        };
    }

    private static object Metrics(IReadOnlyList<AdminResult> rows)
    {
        var labels = rows.Select(x => x.ExpectedIntent).Concat(rows.Select(x => x.PredictedIntent)).Distinct().OrderBy(x => x).ToArray();
        var perIntent = labels.ToDictionary(label => label, label =>
        {
            var tp = rows.Count(x => x.ExpectedIntent == label && x.PredictedIntent == label);
            var fp = rows.Count(x => x.ExpectedIntent != label && x.PredictedIntent == label);
            var fn = rows.Count(x => x.ExpectedIntent == label && x.PredictedIntent != label);
            return new { precision = Ratio(tp, tp + fp), recall = Ratio(tp, tp + fn), f1 = F1(tp, fp, fn), support = rows.Count(x => x.ExpectedIntent == label) };
        });
        var supported = labels.Where(SupportedAdminIntents.Contains).Select(label => perIntent[label].f1).ToArray();
        return new
        {
            cases = rows.Count,
            pass = rows.Count(x => x.Pass),
            accuracy = Ratio(rows.Count(x => x.Pass), rows.Count),
            falseAcceptance = rows.Count(x => x.ExpectedDecision != "ACCEPT" && x.Decision == "ACCEPT"),
            falseRejection = rows.Count(x => x.ExpectedDecision == "ACCEPT" && x.Decision != "ACCEPT"),
            oodRejection = rows.Count(x => x.ExpectedIntent == "OUT_OF_SCOPE" && x.Decision == "UNSUPPORTED"),
            negationRejection = rows.Count(x => x.ExpectedIntent == "NEGATED_REQUEST" && x.Decision == "CLARIFY"),
            multiIntentRejection = rows.Count(x => x.ExpectedIntent == "MULTI_INTENT" && x.Decision == "CLARIFY"),
            supportedMacroF1 = supported.Length == 0 ? 0 : Math.Round(supported.Average(), 4),
            perIntent
        };
    }

    private static Dictionary<string, Dictionary<string, int>> Confusion(IReadOnlyList<AdminResult> rows) =>
        rows.GroupBy(x => x.ExpectedIntent).ToDictionary(g => g.Key, g => g.GroupBy(x => x.PredictedIntent).ToDictionary(x => x.Key, x => x.Count()));

    private static double Ratio(int a, int b) => b == 0 ? 0 : Math.Round((double)a / b, 4);
    private static double F1(int tp, int fp, int fn) => tp == 0 ? 0 : Math.Round(2d * tp / (2 * tp + fp + fn), 4);
    private static string Fold(string text)
    {
        var mapped = text.Replace('đ', 'd').Replace('Đ', 'D');
        return new string(mapped.Normalize(System.Text.NormalizationForm.FormD)
            .Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark)
            .ToArray()).ToLowerInvariant();
    }

    private static AdminCase[] Load<AdminCase>(string path) => JsonSerializer.Deserialize<AdminCase[]>(File.ReadAllText(path), new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? Array.Empty<AdminCase>();

    private static void EnsureDisjoint(params AdminCase[][] sets)
    {
        var rows = sets.SelectMany(x => x).ToArray();
        var ids = rows.Select(x => x.Id).ToArray();
        if (ids.Length != ids.Distinct(StringComparer.Ordinal).Count()) throw new InvalidDataException("Admin dataset IDs overlap.");
        var texts = rows.Select(x => Fold(x.Text)).ToArray();
        if (texts.Length != texts.Distinct(StringComparer.Ordinal).Count()) throw new InvalidDataException("Admin dataset texts overlap.");
    }

    private static void ValidateToolContracts(params AdminCase[][] sets)
    {
        var supported = new HashSet<string>(StringComparer.Ordinal)
        {
            "get_revenue", "get_business_summary", "get_revenue_comparison", "get_best_sellers",
            "get_order_list", "customer_get_menu", "employee_get_table_summary"
        };
        foreach (var row in sets.SelectMany(x => x).Where(x => x.ExpectedDecision == "ACCEPT"))
        {
            if (row.ToolName is null || !supported.Contains(row.ToolName))
                throw new InvalidDataException($"Unsupported or missing Admin tool contract for {row.Id}: {row.ToolName ?? "NONE"}.");
        }
    }

    private static void Print(string label, IReadOnlyList<AdminResult> rows) => Console.WriteLine($"{label}|pass={rows.Count(x => x.Pass)}/{rows.Count}");
    private static string? Option(string[] args, string name) { var i = Array.IndexOf(args, name); return i >= 0 && i + 1 < args.Length ? args[i + 1] : null; }

    private sealed record Threshold(double Score, double Margin, int Value = 0);
    private sealed record AdminCase(string Id, string Text, string ExpectedIntent, string ExpectedDecision, string? ToolName, Dictionary<string, string> ExpectedSlots, string Source, string DataType);
    private sealed record AdminResult(string Id, string Text, string ExpectedIntent, string ExpectedDecision, string PredictedIntent, string Decision, double TopScore, double SecondScore, double Margin, Dictionary<string, string> ExpectedSlots, Dictionary<string, string> PredictedSlots, bool Pass);
}
