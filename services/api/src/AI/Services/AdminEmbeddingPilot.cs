using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Tokenizers.HuggingFace.Tokenizer;
using RestaurantPOS.AI.Models;

namespace RestaurantPOS.AI.Services;

public sealed record AdminEmbeddingPilotResult(
    string Decision,
    string Reason,
    string? PredictedIntent,
    string? SecondIntent,
    string? AcceptedIntent,
    string? ProposedTool,
    JsonElement Arguments,
    string Period,
    double TopScore,
    double Margin,
    double SecondScore,
    bool SlotsValid,
    IReadOnlyDictionary<string, string> ParsedSlots,
    IReadOnlyList<string> MissingSlots,
    string? Boundary);

/// <summary>
/// Development/Test Admin E5 decision pilot. Authorization remains in the tool boundary.
/// </summary>
public sealed class AdminEmbeddingPilot : IDisposable
{
    private static readonly IReadOnlySet<string> SupportedIntents = new HashSet<string>(StringComparer.Ordinal)
    {
        "REVENUE_QUERY", "BUSINESS_SUMMARY", "REVENUE_COMPARISON",
        "PRODUCT_RANKING", "ORDER_COUNT", "ORDER_STATUS", "MENU_QUERY", "UPDATE_PRODUCT_PRICE", "UPDATE_PRODUCT_NAME", "CREATE_PRODUCT", "CREATE_TOPPING"
    };

    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<AdminEmbeddingPilot> _logger;
    private readonly SemaphoreSlim _capacity = new(3, 3);
    private readonly SemaphoreSlim _inference = new(1, 1);
    private readonly object _gate = new();
    private readonly List<Reference> _references = new();
    private Tokenizer? _tokenizer;
    private InferenceSession? _session;
    private bool _initialized;
    private Exception? _initializationError;
    private int _activeOperations;
    private bool _disposed;
    private bool _resourcesDisposed;

    public AdminEmbeddingPilot(IConfiguration configuration, IHostEnvironment environment, ILogger<AdminEmbeddingPilot> logger)
    {
        _configuration = configuration;
        _environment = environment;
        _logger = logger;
    }

    public bool IsEnabled =>
        (_environment.IsDevelopment() ||
         string.Equals(_environment.EnvironmentName, "Test", StringComparison.OrdinalIgnoreCase) ||
         string.Equals(_environment.EnvironmentName, "Testing", StringComparison.OrdinalIgnoreCase)) &&
        _configuration.GetValue<bool>("AI:AdminEmbeddingPilot:Enabled");

    public async Task<AdminEmbeddingPilotResult> DecideAsync(string text, AiUserContext context, CancellationToken cancellationToken)
    {
        EnterOperation();
        try
        {
            if (!IsEnabled || !string.Equals(context.Role, "admin", StringComparison.OrdinalIgnoreCase))
                return Unsupported("PilotDisabledOrRoleNotAllowed");
            if (string.IsNullOrWhiteSpace(text))
                return Clarify("MissingInput");

            await EnsureInitializedAsync(cancellationToken);
            if (_initializationError is not null)
                throw new InvalidOperationException("Admin embedding pilot is unavailable.", _initializationError);

            if (!await _capacity.WaitAsync(TimeSpan.FromMilliseconds(GetTimeout()), cancellationToken))
                throw new TimeoutException("Admin embedding pilot queue is busy.");
            try
            {
                using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeout.CancelAfter(GetTimeout());
                await _inference.WaitAsync(timeout.Token);
                try
                {
                    var vector = Encode(text);
                    var normalized = Fold(text);
                    var slots = ParseSlots(normalized);
                    ApplyOriginalCatalogNames(slots, text);
                    var candidates = _references
                        .GroupBy(x => x.Intent)
                        .Select(group => new
                        {
                            Intent = group.Key,
                            Score = group.Max(x => Cosine(vector, x.Vector))
                        })
                        .OrderByDescending(x => x.Score)
                        .ToArray();

                    if (candidates.Length == 0) return Unsupported("EmptyCatalog", slots: slots);
                    var top = candidates[0];
                    var second = candidates.Length > 1 ? candidates[1].Score : 0;
                    var margin = top.Score - second;
                    var lexicalIntent = ResolveLexicalIntent(normalized);
                    var resolvedIntent = lexicalIntent ?? top.Intent;
                    var predictedTool = SupportedIntents.Contains(resolvedIntent) ? ToolFor(resolvedIntent) : null;
                    var secondIntent = candidates.Length > 1 ? candidates[1].Intent : null;
                    if (IsNegated(normalized)) return Clarify("NegatedRequest", top.Intent, secondIntent, predictedTool, top.Score, margin, slots, MissingSlots(top.Intent, slots), "NEGATION");
                    if (HasUnresolvedBranchReference(normalized)) return Clarify("BranchSelectionRequiresResolution", top.Intent, secondIntent, predictedTool, top.Score, margin, slots, MissingSlots(top.Intent, slots), "BRANCH_SELECTION");
                    if (resolvedIntent != "UPDATE_PRODUCT_PRICE" && IsOutOfScope(normalized)) return Unsupported("UnsupportedCapability", top.Intent, secondIntent, predictedTool, top.Score, margin, slots, MissingSlots(top.Intent, slots), "OUT_OF_SCOPE");
                    if (IsMultiIntent(normalized)) return Clarify("MultipleIntents", top.Intent, secondIntent, predictedTool, top.Score, margin, slots, MissingSlots(top.Intent, slots), "MULTI_INTENT");
                    if (HasAmbiguousCurrentPeriod(normalized)) return Clarify("AmbiguousCurrentPeriod", top.Intent, secondIntent, predictedTool, top.Score, margin, slots, new[] { "period" }, "TEMPORAL_SCOPE");

                    if (top.Score < GetScoreThreshold())
                        return Unsupported("LowSimilarity", top.Intent, secondIntent, predictedTool, top.Score, margin, slots, MissingSlots(top.Intent, slots), "LOW_SCORE");
                    if (margin < GetMarginThreshold())
                        return Clarify("CloseIntentCandidates", top.Intent, secondIntent, predictedTool, top.Score, margin, slots, MissingSlots(top.Intent, slots), "LOW_MARGIN");
                    if (!SupportedIntents.Contains(top.Intent))
                        return Unsupported("UnsupportedCapability", top.Intent, secondIntent, predictedTool, top.Score, margin, slots, MissingSlots(top.Intent, slots), "UNSUPPORTED_INTENT");

                    // A comparison candidate must have explicit comparison evidence. A
                    // single-period revenue question must not inherit the comparison
                    // contract merely because its embedding is close to that intent.
                    if (resolvedIntent == "REVENUE_COMPARISON" && !HasExplicitComparisonIntent(normalized))
                    {
                        if (secondIntent == "REVENUE_QUERY" && HasSinglePeriodRevenueQuery(normalized, slots) && ValidateSlots("REVENUE_QUERY", slots) is null)
                        {
                            var revenueArguments = BuildArguments("REVENUE_QUERY", slots);
                            return new AdminEmbeddingPilotResult(
                                "ACCEPT", "ExplicitRevenueQueryBoundary", top.Intent, secondIntent,
                                "REVENUE_QUERY", "get_revenue", revenueArguments,
                                slots["period"], top.Score, margin, second, true, slots,
                                Array.Empty<string>(), "REVENUE_COMPARISON_WITHOUT_COMPARISON_CUE");
                        }

                        return Clarify("AmbiguousRevenueComparison", top.Intent, secondIntent, predictedTool, top.Score, margin, slots, MissingSlots(top.Intent, slots), "AMBIGUOUS_REVENUE_BOUNDARY");
                    }

                    var slotDecision = ValidateSlots(resolvedIntent, slots);
                    if (slotDecision is not null)
                    {
                        var boundary = slotDecision is "MissingOrUnsupportedPeriod" or "UnsupportedPeriod"
                            ? "TEMPORAL_SCOPE"
                            : slotDecision is "MissingOrUnsupportedComparisonPeriods" or "UnsupportedComparisonPeriods"
                                ? "COMPARISON_SCOPE"
                            : slotDecision == "MissingOrderStatus" ? "ORDER_STATUS_SCOPE"
                                : slotDecision == "MissingProductPriceArguments" ? "PRICE_UPDATE_SCOPE"
                                : slotDecision == "MissingProductNameArguments" ? "NAME_UPDATE_SCOPE"
                                : slotDecision is "MissingProductCreationArguments" or "MissingToppingCreationArguments" ? "CATALOG_CREATION_SCOPE" : null;
                        return Clarify(slotDecision, top.Intent, secondIntent, predictedTool, top.Score, margin, slots, MissingSlots(top.Intent, slots), boundary);
                    }

                    var tool = ToolFor(resolvedIntent);
                    var arguments = BuildArguments(resolvedIntent, slots);
                    return new AdminEmbeddingPilotResult(
                        "ACCEPT", lexicalIntent is null ? "CapabilityValidated" : "LexicalIntentEvidence", resolvedIntent, secondIntent, resolvedIntent, tool, arguments,
                        slots.TryGetValue("period", out var period) ? period : "unknown",
                        top.Score, margin, second, true, slots, Array.Empty<string>(), null);
                }
                finally { _inference.Release(); }
            }
            finally { _capacity.Release(); }
        }
        finally
        {
            ExitOperation();
        }
    }

    private async Task EnsureInitializedAsync(CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(AdminEmbeddingPilot));
            if (_initialized) return;
            try
            {
                var modelPath = ResolvePath("AI:AdminEmbeddingPilot:ModelPath");
                var tokenizerPath = ResolvePath("AI:AdminEmbeddingPilot:TokenizerPath");
                var catalogPath = ResolvePath("AI:AdminEmbeddingPilot:CatalogPath");
                if (!File.Exists(modelPath) || !File.Exists(tokenizerPath) || !File.Exists(catalogPath))
                    throw new FileNotFoundException("Admin v3 embedding artifact is missing.");

                _tokenizer = Tokenizer.FromFile(tokenizerPath);
                _session = new InferenceSession(modelPath);
                var rows = JsonSerializer.Deserialize<ReferenceRow[]>(
                    File.ReadAllText(catalogPath),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? Array.Empty<ReferenceRow>();
                var validRows = rows.Where(x => SupportedIntents.Contains(x.ExpectedIntent)).ToArray();
                if (validRows.Length == 0) throw new InvalidDataException("Admin v3 reference catalog is empty.");
                foreach (var row in validRows)
                    _references.Add(new Reference(row.ExpectedIntent, row.Text, Encode(row.Text)));
            }
            catch (Exception ex)
            {
                _initializationError = ex;
                _logger.LogError(ex, "Admin embedding pilot initialization failed; pilot is fail-closed.");
            }
            _initialized = true;
        }
        await Task.CompletedTask;
        cancellationToken.ThrowIfCancellationRequested();
    }

    private JsonElement BuildArguments(string intent, IReadOnlyDictionary<string, string> slots)
    {
        object args = intent switch
        {
            "MENU_QUERY" => new { category = (string?)null },
            "REVENUE_QUERY" => new
            {
                period = slots["period"] == "this_month" ? "month" : slots["period"],
                scope = slots.GetValueOrDefault("branchScope", "all")
            },
            "BUSINESS_SUMMARY" or "ORDER_COUNT" => new { period = slots["period"] },
            "REVENUE_COMPARISON" => new { currentPeriod = slots["currentPeriod"], comparisonPeriod = slots["comparisonPeriod"] },
            "PRODUCT_RANKING" => new { period = slots["period"] switch { "this_week" => "week", "this_month" => "month", _ => slots["period"] }, limit = slots.TryGetValue("limit", out var limit) && int.TryParse(limit, out var parsed) ? parsed : 5 },
            "ORDER_STATUS" => BuildOrderStatusArguments(slots["period"], slots["status"]),
            "UPDATE_PRODUCT_PRICE" => slots.ContainsKey("productName") && slots.ContainsKey("newPrice")
                ? new { productName = slots["productName"], newPrice = decimal.Parse(slots["newPrice"], CultureInfo.InvariantCulture) }
                : new { },
            "UPDATE_PRODUCT_NAME" => slots.ContainsKey("productName") && slots.ContainsKey("newName")
                ? new { productName = slots["productName"], newName = slots["newName"] }
                : new { },
            "CREATE_PRODUCT" => BuildCreateProductArguments(slots),
            "CREATE_TOPPING" => BuildCreateToppingArguments(slots),
            _ => throw new InvalidOperationException($"Unsupported Admin intent: {intent}")
        };
        return JsonSerializer.SerializeToElement(args);
    }

    private static object BuildOrderStatusArguments(string period, string status)
    {
        var vnZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        var (fromUtc, toUtc) = AdminOrderPeriodResolver.Resolve(period, DateTime.UtcNow, vnZone);
        var start = TimeZoneInfo.ConvertTimeFromUtc(fromUtc, vnZone);
        var end = TimeZoneInfo.ConvertTimeFromUtc(toUtc, vnZone);
        return new
        {
            fromDate = start.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            toDate = end.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            fromDateUtc = fromUtc.ToString("O", CultureInfo.InvariantCulture),
            toDateUtc = toUtc.ToString("O", CultureInfo.InvariantCulture),
            status = StatusValue(status),
            limit = 10
        };
    }

    private static string StatusValue(string status) => status switch
    {
        "processing" => "Đang xử lý",
        "completed" => "Hoàn thành",
        "cancelled" => "Đã hủy",
        _ => throw new InvalidOperationException("Unsupported order status.")
    };

    private static string? ValidateSlots(string intent, IReadOnlyDictionary<string, string> slots)
    {
        if (intent is "REVENUE_QUERY" or "BUSINESS_SUMMARY" or "PRODUCT_RANKING" or "ORDER_COUNT" or "ORDER_STATUS")
            if (!slots.ContainsKey("period")) return "MissingOrUnsupportedPeriod";
        if (intent == "REVENUE_QUERY" && !new[] { "today", "this_month", "year" }.Contains(slots["period"])) return "UnsupportedPeriod";
        if (intent == "BUSINESS_SUMMARY" && !new[] { "today", "yesterday", "this_week", "last_week", "this_month", "last_month" }.Contains(slots["period"])) return "UnsupportedPeriod";
        if (intent == "REVENUE_COMPARISON" && (!slots.ContainsKey("currentPeriod") || !slots.ContainsKey("comparisonPeriod"))) return "MissingOrUnsupportedComparisonPeriods";
        if (intent == "REVENUE_COMPARISON" && (!new[] { "today", "this_week", "this_month" }.Contains(slots["currentPeriod"]) || !new[] { "yesterday", "last_week", "last_month" }.Contains(slots["comparisonPeriod"]))) return "UnsupportedComparisonPeriods";
        if (intent == "PRODUCT_RANKING" && !new[] { "today", "yesterday", "this_week", "this_month" }.Contains(slots["period"])) return "UnsupportedPeriod";
        if (intent == "ORDER_STATUS" && !new[] { "today", "yesterday", "this_week", "last_week", "this_month", "last_month" }.Contains(slots["period"])) return "UnsupportedPeriod";
        if (intent == "ORDER_STATUS" && !slots.ContainsKey("status")) return "MissingOrderStatus";
        if (intent == "UPDATE_PRODUCT_PRICE" &&
            (slots.ContainsKey("productName") ^ slots.ContainsKey("newPrice"))) return "MissingProductPriceArguments";
        if (intent == "UPDATE_PRODUCT_NAME" && (!slots.ContainsKey("productName") || !slots.ContainsKey("newName"))) return "MissingProductNameArguments";
        if (intent == "CREATE_PRODUCT" && (!slots.ContainsKey("productName") || !slots.ContainsKey("newPrice") || !slots.ContainsKey("category") || !slots.ContainsKey("group"))) return "MissingProductCreationArguments";
        if (intent == "CREATE_TOPPING" && (!slots.ContainsKey("toppingName") || !slots.ContainsKey("newPrice") || !slots.ContainsKey("toppingType") || !slots.ContainsKey("toppingCategory"))) return "MissingToppingCreationArguments";
        return null;
    }

    private static string ToolFor(string intent) => intent switch
    {
        "REVENUE_QUERY" => "get_revenue",
        "BUSINESS_SUMMARY" or "ORDER_COUNT" => "get_business_summary",
        "REVENUE_COMPARISON" => "get_revenue_comparison",
        "PRODUCT_RANKING" => "get_best_sellers",
        "ORDER_STATUS" => "get_order_list",
        "MENU_QUERY" => "customer_get_menu",
        "UPDATE_PRODUCT_PRICE" => "update_product_price",
        "UPDATE_PRODUCT_NAME" => "update_product_name",
        "CREATE_PRODUCT" => "create_product",
        "CREATE_TOPPING" => "create_topping",
        _ => throw new InvalidOperationException($"No tool contract for {intent}")
    };

    private static bool IsNegated(string value) => Regex.IsMatch(value,
        @"(^|\s)(khong|dung|chua)(?=\s+(?:xem|can|muon|yeu cau|cho|thuc hien|kiem tra|lay|bao|doi gia|cap nhat|sua))",
        RegexOptions.CultureInvariant);

    private static string? ResolveLexicalIntent(string value)
    {
        if (HasExplicitComparisonIntent(value)) return "REVENUE_COMPARISON";
        if (Regex.IsMatch(value, @"\b(?:doi|cap nhat|sua)\s+(?:gia|gia ban)\b|\b(?:gia|gia ban)\s+(?:mon|san pham).*(?:thanh|sang)\b", RegexOptions.CultureInvariant))
            return "UPDATE_PRODUCT_PRICE";
        if (Regex.IsMatch(value, @"\b(?:doi|cap nhat|sua)\s+(?:ten\s+)?(?:mon|san pham)\b|\bten\s+(?:mon|san pham).*(?:thanh|sang|la)\b", RegexOptions.CultureInvariant))
            return "UPDATE_PRODUCT_NAME";
        if (Regex.IsMatch(value, @"\b(?:tao|them)\s+(?:mon|san pham)\b|\btao mon moi\b", RegexOptions.CultureInvariant))
            return "CREATE_PRODUCT";
        if (Regex.IsMatch(value, @"\b(?:tao|them)\s+topping\b|\btao topping moi\b", RegexOptions.CultureInvariant))
            return "CREATE_TOPPING";
        if (Regex.IsMatch(value, @"\b(?:thuc don|menu|danh muc mon|mon an|do uong|khai vi|mon chinh)\b", RegexOptions.CultureInvariant))
            return "MENU_QUERY";
        if (Regex.IsMatch(value, @"\b(?:xep hang mon|mon nao.*(?:ban chay|dung dau|goi nhieu)|ban chay|goi nhieu|top \d+ mon)\b", RegexOptions.CultureInvariant))
            return "PRODUCT_RANKING";
        if (Regex.IsMatch(value, @"\b(?:don|danh sach don).*(?:dang xu ly|hoan thanh|da huy)|(?:dang xu ly|hoan thanh|da huy).*\b(?:don|danh sach)\b", RegexOptions.CultureInvariant))
            return "ORDER_STATUS";
        if (Regex.IsMatch(value, @"\b(?:so don|tong so don|tong cong.*don|bao nhieu don|dem don|luong don)\b", RegexOptions.CultureInvariant))
            return "ORDER_COUNT";
        if (Regex.IsMatch(value, @"\b(?:chi so|kpi|tong quan|tom tat|bao cao|tinh hinh|chi phi|loi|lai|loi nhuan|gia von|gia tri don trung binh|aov)\b", RegexOptions.CultureInvariant))
            return "BUSINESS_SUMMARY";
        if (Regex.IsMatch(value, @"\b(?:doanh thu|doanh so|tien ban|tien ban hang|thu ve|thu nhap|kiem duoc|ban duoc bao nhieu tien)\b", RegexOptions.CultureInvariant))
            return "REVENUE_QUERY";
        return null;
    }

    private static bool IsOutOfScope(string value) => Regex.IsMatch(value,
        @"\b(?:ton kho|nguyen lieu|ca lam|lich lam|nhan vien|mo cua|gio phuc vu|xoa|tich diem|loyalty|tao don|dat mon|huy don|cap nhat trang thai)\b",
        RegexOptions.CultureInvariant);

    private static bool HasAmbiguousCurrentPeriod(string value) => Regex.IsMatch(value,
        @"\b(?:hien tai|bay gio|luc nay|hien gio|gan day)\b",
        RegexOptions.CultureInvariant) &&
        !Regex.IsMatch(value,
            @"\b(?:hom nay|hom qua|tuan nay|tuan truoc|thang nay|thang truoc|nam nay)\b",
            RegexOptions.CultureInvariant);

    private static bool HasUnresolvedBranchReference(string value)
    {
        var mentionsBranch = Regex.IsMatch(value, @"\b(?:chi nhanh|co so|branch)(?:\s+[a-z0-9]+)?\b", RegexOptions.CultureInvariant);
        var resolvedScope = Regex.IsMatch(value, @"\b(?:chi nhanh|co so|branch)\s+(?:hien tai|nay|cua toi|current|all|tat ca|ca hai|hai co so)\b", RegexOptions.CultureInvariant) ||
            Regex.IsMatch(value, @"\b(?:ca hai|hai co so|tat ca chi nhanh|toan chuoi|toan bo)\b", RegexOptions.CultureInvariant);
        return mentionsBranch && !resolvedScope;
    }

    private static bool IsMultiIntent(string value)
    {
        if (Regex.IsMatch(value, @"\b(?:tom tat|tong quan|tong hop|bao cao)\b", RegexOptions.CultureInvariant)) return false;
        var families = new[]
        {
            Regex.IsMatch(value, @"\b(?:doanh thu|doanh so|tien ban|thu ve)\b"),
            Regex.IsMatch(value, @"\b(?:don hang|so don|danh sach don)\b"),
            Regex.IsMatch(value, @"\b(?:ban chay|xep hang mon|mon nao)\b")
        };
        return families.Count(x => x) >= 2 && Regex.IsMatch(value, @"\b(?:va|roi)\b");
    }

    private static bool HasExplicitComparisonIntent(string value) => Regex.IsMatch(
        value,
        @"\b(?:so sanh|doi chieu|tang hay giam|tang giam|ky nay.*ky truoc|so voi|vs)\b",
        RegexOptions.CultureInvariant);

    private static bool HasSinglePeriodRevenueQuery(string value, IReadOnlyDictionary<string, string> slots) =>
        slots.ContainsKey("period") && !slots.ContainsKey("comparisonPeriod") &&
        Regex.IsMatch(value, @"\b(?:doanh thu|doanh so|tien ban|thu ve)\b", RegexOptions.CultureInvariant);

    private static Dictionary<string, string> ParseSlots(string value)
    {
        var slots = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var periodTokens = new[]
        {
            ("hom nay", "today"), ("hom qua", "yesterday"), ("tuan nay", "this_week"),
            ("tuan truoc", "last_week"), ("thang nay", "this_month"), ("thang truoc", "last_month"),
            ("nam nay", "year"), ("ngay nay", "today"), ("ngay truoc", "yesterday"),
            ("thang hien tai", "this_month"), ("thang lien truoc", "last_month")
        };
        var periods = periodTokens.Where(x => value.Contains(x.Item1, StringComparison.Ordinal)).Select(x => x.Item2).ToArray();
        if (periods.Length > 0) slots["period"] = periods[0];
        if (periods.Length > 0) slots["currentPeriod"] = periods[0];
        if (periods.Length > 1) slots["comparisonPeriod"] = periods[1];
        if (Regex.IsMatch(value, @"\b(?:chi nhanh|co so)\s+(?:hien tai|nay|cua toi)\b", RegexOptions.CultureInvariant))
            slots["branchScope"] = "current";
        else if (Regex.IsMatch(value, @"\b(?:ca hai|hai co so|tat ca chi nhanh|toan chuoi|toan bo)\b", RegexOptions.CultureInvariant))
            slots["branchScope"] = "all";
        if (value.Contains("tuan sau") || value.Contains("thang sau") || value.Contains("nam sau") || value.Contains("ngay mai")) slots["period"] = "unsupported";
        if (value.Contains("hoan thanh")) slots["status"] = "completed";
        else if (value.Contains("dang xu ly")) slots["status"] = "processing";
        else if (value.Contains("da huy")) slots["status"] = "cancelled";
        var limit = Regex.Match(value, @"(?:top|hang dau|dau)\s*(\d+|nam|muoi|ba|hai)", RegexOptions.CultureInvariant);
        if (limit.Success) slots["limit"] = limit.Groups[1].Value switch { "nam" => "5", "muoi" => "10", "ba" => "3", "hai" => "2", var number => number };
        var priceMatch = Regex.Match(value, @"\b(?:mon|san pham)\s+(.+?)\s+(?:thanh|sang|la)\s+([0-9][0-9\.,]*)\b", RegexOptions.CultureInvariant);
        if (priceMatch.Success)
        {
            var numericPrice = priceMatch.Groups[2].Value.Replace(".", string.Empty).Replace(",", string.Empty);
            if (decimal.TryParse(numericPrice, NumberStyles.None, CultureInfo.InvariantCulture, out var parsedPrice))
            {
                slots["productName"] = priceMatch.Groups[1].Value.Trim();
                slots["newPrice"] = parsedPrice.ToString(CultureInfo.InvariantCulture);
            }
        }
        var nameMatch = Regex.Match(value, @"\b(?:mon|san pham)\s+(.+?)\s+(?:thanh|sang|la)\s+(.+)$", RegexOptions.CultureInvariant);
        if (nameMatch.Success && !Regex.IsMatch(nameMatch.Groups[2].Value, @"^[0-9][0-9\.,]*$", RegexOptions.CultureInvariant))
        {
            slots["productName"] = nameMatch.Groups[1].Value.Trim();
            slots["newName"] = nameMatch.Groups[2].Value.Trim();
        }
        var createProduct = Regex.Match(value, @"\b(?:mon|san pham)\s+(.+?)\s+(?:gia|ban gia)\s+([0-9][0-9\.,]*)\b", RegexOptions.CultureInvariant);
        if (createProduct.Success)
        {
            var numericPrice = createProduct.Groups[2].Value.Replace(".", string.Empty).Replace(",", string.Empty);
            if (decimal.TryParse(numericPrice, NumberStyles.None, CultureInfo.InvariantCulture, out var parsedPrice))
            {
                slots["productName"] = createProduct.Groups[1].Value.Trim();
                slots["newPrice"] = parsedPrice.ToString(CultureInfo.InvariantCulture);
            }
        }
        var category = Regex.Match(value, @"\b(?:loai|thuoc loai)\s+(do uong|do an|dich vu)\b", RegexOptions.CultureInvariant);
        if (category.Success) slots["category"] = category.Groups[1].Value switch { "do uong" => "Đồ uống", "do an" => "Đồ ăn", _ => "Dịch vụ" };
        var group = Regex.Match(value, @"\b(?:nhom|nhom mon)\s+(.+?)(?:\s+(?:gia|co|la)\b|$)", RegexOptions.CultureInvariant);
        if (group.Success) slots["group"] = group.Groups[1].Value.Trim();
        var topping = Regex.Match(value, @"\btopping\s+(.+?)\s+(?:gia|ban gia)\s+([0-9][0-9\.,]*)\b", RegexOptions.CultureInvariant);
        if (topping.Success)
        {
            var numericPrice = topping.Groups[2].Value.Replace(".", string.Empty).Replace(",", string.Empty);
            if (decimal.TryParse(numericPrice, NumberStyles.None, CultureInfo.InvariantCulture, out var parsedPrice))
            {
                slots["toppingName"] = topping.Groups[1].Value.Trim();
                slots["newPrice"] = parsedPrice.ToString(CultureInfo.InvariantCulture);
            }
        }
        var toppingType = Regex.Match(value, @"\b(topping do uong|topping do an)\b", RegexOptions.CultureInvariant);
        if (toppingType.Success) slots["toppingType"] = toppingType.Groups[1].Value == "topping do an" ? "Topping Đồ ăn" : "Topping Đồ uống";
        var toppingCategory = Regex.Match(value, @"\b(?:nhom topping|nhom)\s+(.+?)(?:\s+(?:gia|loai|mo ta)\b|$)", RegexOptions.CultureInvariant);
        if (toppingCategory.Success) slots["toppingCategory"] = toppingCategory.Groups[1].Value.Trim();
        return slots;
    }

    private static void ApplyOriginalCatalogNames(Dictionary<string, string> slots, string originalText)
    {
        var rename = Regex.Match(originalText,
            @"\b(?:món|mon|sản phẩm|san pham)\s+(.+?)\s+(?:thành|thanh|sang|là|la)\s+(.+)$",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        if (rename.Success)
        {
            slots["productName"] = CleanCatalogName(rename.Groups[1].Value);
            slots["newName"] = CleanCatalogName(rename.Groups[2].Value);
        }

        var product = Regex.Match(originalText,
            @"\b(?:món|mon|sản phẩm|san pham)\s+(.+?)\s+(?:giá|gia|bán giá|ban gia)\s+[0-9][0-9\.,]*\b",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        if (product.Success)
            slots["productName"] = CleanCatalogName(product.Groups[1].Value);

        var topping = Regex.Match(originalText,
            @"\btopping\s+(.+?)\s+(?:giá|gia|bán giá|ban gia)\s+[0-9][0-9\.,]*\b",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        if (topping.Success)
            slots["toppingName"] = CleanCatalogName(topping.Groups[1].Value);
    }

    private static string CleanCatalogName(string value) => value.Trim().TrimEnd('.', ',', '!', '?', ';', ':').Trim();

    private static object BuildCreateProductArguments(IReadOnlyDictionary<string, string> slots) =>
        slots.ContainsKey("productName") && slots.ContainsKey("newPrice")
            ? new { productName = slots["productName"], price = decimal.Parse(slots["newPrice"], CultureInfo.InvariantCulture), category = slots.GetValueOrDefault("category", "Đồ uống"), group = slots.GetValueOrDefault("group", "Chưa phân loại") }
            : new { };

    private static object BuildCreateToppingArguments(IReadOnlyDictionary<string, string> slots) =>
        slots.ContainsKey("toppingName") && slots.ContainsKey("newPrice")
            ? new { toppingName = slots["toppingName"], price = decimal.Parse(slots["newPrice"], CultureInfo.InvariantCulture), toppingType = slots.GetValueOrDefault("toppingType", "Topping Đồ uống"), category = slots.GetValueOrDefault("toppingCategory", "Chưa phân loại") }
            : new { };

    private float[] Encode(string text)
    {
        if (_tokenizer is null || _session is null) throw new InvalidOperationException("Embedding session unavailable.");
        var encoding = _tokenizer.Encode("query: " + text, true, includeTypeIds: true, includeTokens: false, includeAttentionMask: true).First();
        var ids = encoding.Ids.Select(x => (long)x).Take(512).ToArray();
        var mask = encoding.AttentionMask.Select(x => (long)x).Take(512).ToArray();
        var input = new DenseTensor<long>(new[] { 1, ids.Length });
        var types = new DenseTensor<long>(new[] { 1, ids.Length });
        var attention = new DenseTensor<long>(new[] { 1, ids.Length });
        for (var i = 0; i < ids.Length; i++) { input[0, i] = ids[i]; attention[0, i] = mask[i]; }
        using var results = _session.Run(new[] { NamedOnnxValue.CreateFromTensor("input_ids", input), NamedOnnxValue.CreateFromTensor("token_type_ids", types), NamedOnnxValue.CreateFromTensor("attention_mask", attention) });
        var output = results.First().AsTensor<float>();
        var pooled = new float[384]; var count = 0f;
        for (var i = 0; i < ids.Length; i++) if (mask[i] != 0) { count++; for (var d = 0; d < 384; d++) pooled[d] += output[0, i, d]; }
        for (var d = 0; d < 384; d++) pooled[d] /= count;
        var norm = Math.Sqrt(pooled.Sum(x => x * x));
        for (var d = 0; d < 384; d++) pooled[d] /= (float)norm;
        return pooled;
    }

    private string ResolvePath(string key) => Path.GetFullPath(Path.Combine(_environment.ContentRootPath, _configuration[key] ?? throw new InvalidOperationException($"Missing {key}.")));
    private double GetScoreThreshold() => _configuration.GetValue<double?>("AI:AdminEmbeddingPilot:ScoreThreshold") ?? 0.70;
    private double GetMarginThreshold() => _configuration.GetValue<double?>("AI:AdminEmbeddingPilot:MarginThreshold") ?? 0.00;
    private int GetTimeout() => Math.Clamp(_configuration.GetValue<int?>("AI:AdminEmbeddingPilot:TimeoutMilliseconds") ?? 1500, 100, 10000);
    private static double Cosine(float[] a, float[] b) => a.Zip(b, (x, y) => x * y).Sum();
    private static string Fold(string text)
    {
        var mapped = text.Replace('đ', 'd').Replace('Đ', 'D');
        return new string(mapped.Normalize(System.Text.NormalizationForm.FormD).Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray()).ToLowerInvariant();
    }
    private static IReadOnlyList<string> MissingSlots(string intent, IReadOnlyDictionary<string, string> slots)
    {
        var missing = new List<string>();
        if (intent is "REVENUE_QUERY" or "BUSINESS_SUMMARY" or "PRODUCT_RANKING" or "ORDER_COUNT" or "ORDER_STATUS")
            if (!slots.ContainsKey("period")) missing.Add("period");
        if (intent == "REVENUE_COMPARISON")
        {
            if (!slots.ContainsKey("currentPeriod")) missing.Add("currentPeriod");
            if (!slots.ContainsKey("comparisonPeriod")) missing.Add("comparisonPeriod");
        }
        if (intent == "ORDER_STATUS" && !slots.ContainsKey("status")) missing.Add("status");
        if (intent == "UPDATE_PRODUCT_PRICE")
        {
            if (!slots.ContainsKey("productName")) missing.Add("productName");
            if (!slots.ContainsKey("newPrice")) missing.Add("newPrice");
        }
        if (intent == "UPDATE_PRODUCT_NAME")
        {
            if (!slots.ContainsKey("productName")) missing.Add("productName");
            if (!slots.ContainsKey("newName")) missing.Add("newName");
        }
        if (intent == "CREATE_PRODUCT")
        {
            if (!slots.ContainsKey("productName")) missing.Add("productName");
            if (!slots.ContainsKey("newPrice")) missing.Add("price");
            if (!slots.ContainsKey("category")) missing.Add("category");
            if (!slots.ContainsKey("group")) missing.Add("group");
        }
        if (intent == "CREATE_TOPPING")
        {
            if (!slots.ContainsKey("toppingName")) missing.Add("toppingName");
            if (!slots.ContainsKey("newPrice")) missing.Add("price");
            if (!slots.ContainsKey("toppingType")) missing.Add("toppingType");
            if (!slots.ContainsKey("toppingCategory")) missing.Add("category");
        }
        return missing;
    }
    private static AdminEmbeddingPilotResult Unsupported(string reason, string? predictedIntent = null, string? secondIntent = null, string? proposedTool = null, double score = 0, double margin = 0, IReadOnlyDictionary<string, string>? slots = null, IReadOnlyList<string>? missingSlots = null, string? boundary = null) => new("UNSUPPORTED", reason, predictedIntent, secondIntent, null, proposedTool, default, slots?.GetValueOrDefault("period") ?? "unknown", score, margin, score - margin, missingSlots?.Count == 0, slots ?? new Dictionary<string, string>(), missingSlots ?? Array.Empty<string>(), boundary);
    private static AdminEmbeddingPilotResult Clarify(string reason, string? predictedIntent = null, string? secondIntent = null, string? proposedTool = null, double score = 0, double margin = 0, IReadOnlyDictionary<string, string>? slots = null, IReadOnlyList<string>? missingSlots = null, string? boundary = null) => new("CLARIFY", reason, predictedIntent, secondIntent, null, proposedTool, default, slots?.GetValueOrDefault("period") ?? "unknown", score, margin, score - margin, missingSlots?.Count == 0, slots ?? new Dictionary<string, string>(), missingSlots ?? Array.Empty<string>(), boundary);
    private void EnterOperation() { lock (_gate) { if (_disposed) throw new ObjectDisposedException(nameof(AdminEmbeddingPilot)); _activeOperations++; } }
    private void ExitOperation() { lock (_gate) { _activeOperations--; if (_disposed && _activeOperations == 0) DisposeResourcesLocked(); } }
    public void Dispose() { lock (_gate) { if (_disposed) return; _disposed = true; if (_activeOperations == 0) DisposeResourcesLocked(); } }
    private void DisposeResourcesLocked() { if (_resourcesDisposed) return; _resourcesDisposed = true; _inference.Dispose(); _capacity.Dispose(); _session?.Dispose(); _tokenizer?.Dispose(); }
    private sealed record Reference(string Intent, string Text, float[] Vector);
    private sealed record ReferenceRow(string Id, string ExpectedIntent, string Text);
}
