using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Tokenizers.HuggingFace.Tokenizer;
using RestaurantPOS.AI.Models;

namespace RestaurantPOS.AI.Services;

public sealed record CustomerGuestEmbeddingPilotResult(
    string Decision,
    string Reason,
    string? Intent,
    string? ProposedTool,
    JsonElement Arguments,
    string? Category,
    double TopScore,
    double Margin,
    string? Boundary);

/// <summary>
/// Development/Test Customer/Guest E5 decision pilot; rejected decisions cannot execute tools.
/// </summary>
public sealed class CustomerGuestEmbeddingPilot : IDisposable
{
    private static readonly IReadOnlySet<string> CustomerIntents =
        new HashSet<string>(StringComparer.Ordinal) { "MENU_QUERY", "MY_ORDERS" };
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<CustomerGuestEmbeddingPilot> _logger;
    private readonly SemaphoreSlim _capacity = new(2, 2);
    private readonly SemaphoreSlim _inference = new(1, 1);
    private readonly object _gate = new();
    private readonly List<Reference> _references = new();
    private Tokenizer? _tokenizer;
    private InferenceSession? _session;
    private bool _initialized;
    private Exception? _initializationError;
    private bool _disposed;

    public CustomerGuestEmbeddingPilot(IConfiguration configuration, IHostEnvironment environment, ILogger<CustomerGuestEmbeddingPilot> logger)
    {
        _configuration = configuration;
        _environment = environment;
        _logger = logger;
    }

    public bool IsEnabled =>
        (_environment.IsDevelopment() || string.Equals(_environment.EnvironmentName, "Test", StringComparison.OrdinalIgnoreCase) || string.Equals(_environment.EnvironmentName, "Testing", StringComparison.OrdinalIgnoreCase)) &&
        _configuration.GetValue<bool>("AI:CustomerGuestEmbeddingPilot:Enabled");

    public bool IsRequested => _configuration.GetValue<bool>("AI:CustomerGuestEmbeddingPilot:Enabled");

    public async Task<CustomerGuestEmbeddingPilotResult> DecideAsync(string text, AiUserContext context, CancellationToken cancellationToken)
    {
        if (!IsEnabled) return Unsupported("PilotDisabledOrRoleNotAllowed");
        if (string.IsNullOrWhiteSpace(text)) return Clarify("MissingInput");
        if (context.IsGuest && (!context.HasVerifiedGuestSession || !context.BranchId.HasValue))
            return Reject("GuestSessionOrBranchUnavailable", "GUEST_AUTHORITY");
        if (!context.IsGuest && (!string.Equals(context.Role, "customer", StringComparison.OrdinalIgnoreCase) || context.CustomerId is null || context.CustomerId == Guid.Empty))
            return Reject("CustomerIdentityUnavailable", "CUSTOMER_AUTHORITY");
        if (!context.IsGuest && !string.Equals(context.Role, "customer", StringComparison.OrdinalIgnoreCase))
            return Unsupported("PilotDisabledOrRoleNotAllowed");

        var normalized = Normalize(text);
        // Punctuation-only or symbol-only input must not be semantically
        // promoted to MENU_QUERY by an embedding similarity match.
        if (!normalized.Any(char.IsLetterOrDigit))
            return Clarify("InputNotMeaningful", boundary: "INVALID_INPUT");
        var boundary = EvaluateScopeBoundary(text, context.IsGuest);
        if (boundary is not null) return boundary;

        // Popular-dish requests are a safe, read-only recommendation capability.
        // Resolve explicit Vietnamese cues before the embedding catalog so the
        // request cannot fall through to the generic full-menu tool.
        if (LooksLikePopularQuery(normalized))
        {
            if (!context.BranchId.HasValue)
                return Clarify("CustomerBranchUnavailable", boundary: "CUSTOMER_BRANCH_REQUIRED");

            return new("ACCEPT", "LexicalIntentEvidence", "PRODUCT_RANKING", "customer_get_best_sellers",
                JsonSerializer.SerializeToElement(new { limit = 5 }), null, 1, 1, null);
        }

        await EnsureInitializedAsync(cancellationToken);
        if (_initializationError is not null) throw new InvalidOperationException("Customer/Guest embedding pilot is unavailable.", _initializationError);
        if (!await _capacity.WaitAsync(TimeSpan.FromMilliseconds(GetTimeout()), cancellationToken)) throw new TimeoutException("Customer/Guest embedding queue is busy.");
        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(GetTimeout());
            await _inference.WaitAsync(timeout.Token);
            try
            {
                var vector = Encode(text);
                var candidates = _references
                    .Where(x => x.Role is "customer|guest" || (context.IsGuest ? x.Role.Contains("guest", StringComparison.OrdinalIgnoreCase) : x.Role.Contains("customer", StringComparison.OrdinalIgnoreCase)))
                    .GroupBy(x => x.Intent)
                    .Select(g => new { Intent = g.Key, Score = g.Max(x => Cosine(vector, x.Vector)) })
                    .OrderByDescending(x => x.Score).ToArray();
                if (candidates.Length == 0) return Unsupported("EmptyCatalog");
                var top = candidates[0];
                var second = candidates.Length > 1 ? candidates[1].Score : 0d;
                var margin = top.Score - second;
                if (top.Score < GetScoreThreshold()) return Unsupported("LowSimilarity", top.Score, margin, "LOW_SCORE");
                if (margin < GetMarginThreshold()) return Clarify("CloseIntentCandidates", top.Score, margin, "LOW_MARGIN");
                if (context.IsGuest && top.Intent != "MENU_QUERY") return Reject("GuestCapabilityDenied", "GUEST_SCOPE");
                if (LooksLikeOrderQuery(normalized) && top.Intent != "MY_ORDERS") return Clarify("OrderIntentMismatch", top.Score, margin, "INTENT_BOUNDARY");
                if (LooksLikeMenuQuery(normalized) && top.Intent != "MENU_QUERY") return Clarify("MenuIntentMismatch", top.Score, margin, "INTENT_BOUNDARY");
                var category = ExtractCategory(normalized, out var categoryRequested, out var categoryAmbiguous);
                if (categoryAmbiguous) return Clarify("CategoryNeedsClarification", top.Score, margin, "CATEGORY_AMBIGUOUS");
                if (top.Intent == "MENU_QUERY")
                {
                    if (context.IsGuest && !context.BranchId.HasValue)
                        return Reject("CustomerBranchUnavailable", "CUSTOMER_AUTHORITY");
                    var args = JsonSerializer.SerializeToElement(new { category });
                    return new("ACCEPT", "CapabilityValidated", top.Intent, "customer_get_menu", args, category, top.Score, margin, null);
                }
                if (categoryRequested) return Unsupported("MenuCategoryDoesNotMatchOrderQuery", top.Score, margin, "INTENT_BOUNDARY");
                return new("ACCEPT", "CapabilityValidated", "MY_ORDERS", "get_my_orders", JsonSerializer.SerializeToElement(new { }), null, top.Score, margin, null);
            }
            finally { _inference.Release(); }
        }
        finally { _capacity.Release(); }
    }

    public static CustomerGuestEmbeddingPilotResult? EvaluateScopeBoundary(string text, bool guest)
    {
        var value = Normalize(text);
        if (Regex.IsMatch(value, @"\b(?:doanh thu|doanh so|doanh tien|loi nhuan|chi phi|bao cao|dashboard|kpi|nhan vien|chi nhanh khac|chi nhanh nao khac)\b", RegexOptions.CultureInvariant)) return guest ? Reject("GuestScopeDenied", "GUEST_SCOPE") : Reject("CustomerScopeDenied", "CUSTOMER_SCOPE");
        if (Regex.IsMatch(value, @"\b(?:cap nhat|sua|doi gia|xoa|tao don|huy don|dat mon|them mon|bot mon|gui bep|thanh toan|hoan tien|dat ban|giam gia|quan ly kho|quan ly nhan vien|chap nhan don|tu choi don)\b", RegexOptions.CultureInvariant)) return Reject("WriteOrDestructiveRequest", "WRITE_OR_DESTRUCTIVE");
        if (Regex.IsMatch(value, @"(?:^|\s)(?:dung|khong|chua)(?=\s+(?:xem|can|muon|yeu cau|lay|cho|thuc hien|kiem tra))", RegexOptions.CultureInvariant)) return Clarify("NegatedRequest", boundary: "NEGATION");
        // A guest order request must be rejected before embedding candidates are
        // scored. The standalone token is intentional: it covers natural forms
        // such as "don da dat", "don nay", and "don cua khach hang khac" after
        // Vietnamese diacritic folding, without matching the menu token "mon".
        if (guest && IsGuestOrderRequest(value)) return Reject("GuestOrdersDenied", "GUEST_SCOPE");
        if (!guest && Regex.IsMatch(value, @"\b(?:don cua nguoi khac|don cua khach khac|don cua khach hang khac|don hang cua nguoi khac|don hang cua khach khac|don hang cua khach hang khac|khach hang khac|so dien thoai|email|customerid)\b", RegexOptions.CultureInvariant)) return Reject("CustomerIdentityScopeDenied", "CUSTOMER_SCOPE");
        if (Regex.IsMatch(value, @"\b(?:gia duoi|duoi \d+|ton kho|con hang|het hang|tim mon|mon ten|ten mon|mon nao co san)\b", RegexOptions.CultureInvariant)) return Unsupported("UnsupportedMenuCapability", boundary: "UNSUPPORTED_CAPABILITY");
        if (Regex.IsMatch(value, @"\b(?:mon nay|mon do|gia mon)\b.*\b(?:gia|bao nhieu|tien)\b|\b(?:gia|bao nhieu)\b.*\b(?:mon nay|mon do)\b", RegexOptions.CultureInvariant)) return Clarify("MenuItemNeedsClarification", boundary: "MENU_ITEM_UNSPECIFIED");
        if (Regex.IsMatch(value, @"\b(?:va|cung|dong thoi)\b", RegexOptions.CultureInvariant) && Regex.IsMatch(value, @"\b(?:menu|thuc don|mon)\b", RegexOptions.CultureInvariant) && Regex.IsMatch(value, @"\b(?:don hang|don cua toi)\b", RegexOptions.CultureInvariant)) return Clarify("MultipleIntents", boundary: "MULTI_INTENT");
        return null;
    }

    private static bool LooksLikeOrderQuery(string value) =>
        Regex.IsMatch(value, @"\b(?:don cua toi|don hang cua toi|don da dat|don gan day|trang thai don|lich su don|ma don cua toi)\b", RegexOptions.CultureInvariant);

    private static bool LooksLikeMenuQuery(string value) =>
        Regex.IsMatch(value, @"\b(?:menu|thuc don|danh muc mon|mon an|do uong|khai vi|mon chinh)\b", RegexOptions.CultureInvariant);

    private static bool LooksLikePopularQuery(string value) =>
        Regex.IsMatch(value, @"\b(?:ban chay|goi nhieu|duoc ua chuong|mon ngon nhat|mon nao.*(?:ngon|ban chay)|(?:nen thu|nen goi|goi y).*(?:mon|mon nao))\b", RegexOptions.CultureInvariant);

    private static bool IsGuestOrderRequest(string value) =>
        Regex.IsMatch(value, @"(?<!thuc )\bdon\b|\b(?:da dat|dat nhung gi|da goi|goi nhung gi)\b", RegexOptions.CultureInvariant);

    private static string? ExtractCategory(string value, out bool requested, out bool ambiguous)
    {
        requested = Regex.IsMatch(value, @"\b(?:danh muc|loai mon)\b", RegexOptions.CultureInvariant);
        ambiguous = value.Contains("danh muc nay", StringComparison.Ordinal);
        if (Regex.IsMatch(value, @"\b(?:do uong|nuoc|tra|cafe|ca phe)\b", RegexOptions.CultureInvariant)) return "\u0110\u1ed3 u\u1ed1ng";
        if (Regex.IsMatch(value, @"\bkhai vi\b", RegexOptions.CultureInvariant)) return "Khai v\u1ecb";
        if (Regex.IsMatch(value, @"\bmon chinh\b", RegexOptions.CultureInvariant)) return "M\u00f3n ch\u00ednh";
        var explicitCategory = Regex.Match(value, @"\b(?:danh muc|loai mon)\s+(?:mon\s+)?(?<name>.+?)(?:\s+(?:co gi|co khong|nao|gi))?$", RegexOptions.CultureInvariant);
        if (explicitCategory.Success)
        {
            var name = explicitCategory.Groups["name"].Value.Trim();
            if (name is not ("mon" or "mon an" or "nao" or "gi")) return name;
        }
        return null;
    }

    private async Task EnsureInitializedAsync(CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(CustomerGuestEmbeddingPilot));
            if (_initialized) return;
            try
            {
                var modelPath = ResolvePath("AI:CustomerGuestEmbeddingPilot:ModelPath");
                var tokenizerPath = ResolvePath("AI:CustomerGuestEmbeddingPilot:TokenizerPath");
                var catalogPath = ResolvePath("AI:CustomerGuestEmbeddingPilot:CatalogPath");
                if (!File.Exists(modelPath) || !File.Exists(tokenizerPath) || !File.Exists(catalogPath)) throw new FileNotFoundException("Customer/Guest embedding artifact is missing.");
                _tokenizer = Tokenizer.FromFile(tokenizerPath);
                _session = new InferenceSession(modelPath);
                var rows = JsonSerializer.Deserialize<ReferenceRow[]>(File.ReadAllText(catalogPath), new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? Array.Empty<ReferenceRow>();
                foreach (var row in rows.Where(x => CustomerIntents.Contains(x.Intent) && (x.Tool is "customer_get_menu" or "get_my_orders"))) _references.Add(new Reference(row.Role, row.Intent, Encode(row.Text)));
                if (_references.Count == 0) throw new InvalidDataException("Customer/Guest reference catalog is empty.");
            }
            catch (Exception ex) { _initializationError = ex; _logger.LogError(ex, "Customer/Guest embedding pilot initialization failed; pilot is fail-closed."); }
            _initialized = true;
        }
        await Task.CompletedTask;
        cancellationToken.ThrowIfCancellationRequested();
    }

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
    private int GetTimeout() => Math.Clamp(_configuration.GetValue<int?>("AI:CustomerGuestEmbeddingPilot:TimeoutMilliseconds") ?? 1500, 100, 10000);
    private double GetScoreThreshold() => _configuration.GetValue<double?>("AI:CustomerGuestEmbeddingPilot:ScoreThreshold") ?? 0.70;
    private double GetMarginThreshold() => _configuration.GetValue<double?>("AI:CustomerGuestEmbeddingPilot:MarginThreshold") ?? 0.015;
    private static double Cosine(float[] a, float[] b) => a.Zip(b, (x, y) => x * y).Sum();
    private static string Normalize(string text) => new string(text.Normalize(System.Text.NormalizationForm.FormD).Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray()).Replace('đ', 'd').Replace('Đ', 'd').ToLowerInvariant();
    private static CustomerGuestEmbeddingPilotResult Reject(string reason, string boundary) => new("REJECT", reason, null, null, default, null, 0, 0, boundary);
    private static CustomerGuestEmbeddingPilotResult Unsupported(string reason, double score = 0, double margin = 0, string? boundary = null) => new("UNSUPPORTED", reason, null, null, default, null, score, margin, boundary);
    private static CustomerGuestEmbeddingPilotResult Clarify(string reason, double score = 0, double margin = 0, string? boundary = null) => new("CLARIFY", reason, null, null, default, null, score, margin, boundary);
    public void Dispose() { lock (_gate) { if (_disposed) return; _disposed = true; _session?.Dispose(); _tokenizer?.Dispose(); _inference.Dispose(); _capacity.Dispose(); } }
    private sealed record Reference(string Role, string Intent, float[] Vector);
    private sealed record ReferenceRow(string Id, string Role, string Intent, string Tool, string Text);
}
