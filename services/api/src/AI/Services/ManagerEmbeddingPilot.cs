using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Tokenizers.HuggingFace.Tokenizer;
using RestaurantPOS.AI.Models;

namespace RestaurantPOS.AI.Services;

public sealed record ManagerEmbeddingPilotResult(
    string Decision,
    string Reason,
    string? Intent,
    string? ToolName,
    JsonElement Arguments,
    string Period,
    string[] RequiredOutputs,
    double TopScore,
    double Margin);

/// <summary>Development-only semantic gate for the frozen manager v3.3 baseline.</summary>
public sealed class ManagerEmbeddingPilot : IDisposable
{
    private const string ModelSha256 = "4654C156F3E4171ABC9C716CDB771BF9116455D15AC1AAB364AEEEDE0E3205B0";
    private const string TokenizerSha256 = "0B44A9D7B51C3C62626640CDA0E2C2F70FDACDC25BBBD68038369D14EBDF4C39";
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<ManagerEmbeddingPilot> _logger;
    private readonly SemaphoreSlim _capacity;
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

    public ManagerEmbeddingPilot(IConfiguration configuration, IHostEnvironment environment, ILogger<ManagerEmbeddingPilot> logger)
    {
        _configuration = configuration;
        _environment = environment;
        _logger = logger;
        _capacity = new SemaphoreSlim(3, 3);
    }

    public bool IsEnabled => (_environment.IsDevelopment() || string.Equals(_environment.EnvironmentName, "Test", StringComparison.OrdinalIgnoreCase) || string.Equals(_environment.EnvironmentName, "Testing", StringComparison.OrdinalIgnoreCase))
        && _configuration.GetValue<bool>("AI:EmbeddingPilot:Enabled");

    public async Task<ManagerEmbeddingPilotResult> DecideAsync(string text, AiUserContext context, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(ManagerEmbeddingPilot));
            _activeOperations++;
        }
        try
        {
        if (!IsEnabled || !string.Equals(context.Role, "manager", StringComparison.OrdinalIgnoreCase))
            return Unsupported("PilotDisabledOrRoleNotAllowed");
        if (!context.BranchId.HasValue)
            return Clarify("MissingBranchContext");
        if (string.IsNullOrWhiteSpace(text))
            return Clarify("MissingInput");

        var policy = ParsePolicy(text);
        if (policy.OutOfScope) return Unsupported("OutOfScopeCapability");
        if (policy.Negated) return Clarify("NegatedRequest");
        if (policy.Period is "unsupported") return Clarify("UnsupportedPeriod");
        if (policy.Period is "missing") return Clarify("MissingPeriod");
        if (policy.IntentOutputs.Count == 1 && policy.IntentOutputs[0] == "revenue" && (policy.Period is "yesterday" or "this_week" or "last_week") && !Regex.IsMatch(Fold(text), "tong quan|tom tat|kpi|bao cao"))
            return Clarify("UnsupportedPeriod");

        await EnsureInitializedAsync(cancellationToken);
        if (_initializationError is not null) throw new InvalidOperationException("Embedding pilot is unavailable.", _initializationError);
        if (!await _capacity.WaitAsync(TimeSpan.FromMilliseconds(GetTimeout()), cancellationToken))
            throw new TimeoutException("Embedding pilot queue is busy.");
        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(GetTimeout());
            await _inference.WaitAsync(timeout.Token);
            try
            {
                var vector = Encode(text);
                var candidates = _references.GroupBy(x => x.Intent).Select(g => new
                {
                    Intent = g.Key,
                    Score = g.Max(x => Cosine(vector, x.Vector))
                }).OrderByDescending(x => x.Score).ToArray();
                var top = candidates[0];
                var second = candidates.Length > 1 ? candidates[1].Score : 0;
                var margin = top.Score - second;
                var resolved = top.Intent;
                var tool = resolved == "REVENUE_QUERY" ? "get_revenue" : "get_business_summary";

                if (resolved == "BUSINESS_SUMMARY" && policy.IntentOutputs.Count == 1 && policy.IntentOutputs[0] == "revenue" && new[] { "today", "this_month", "year" }.Contains(policy.Period))
                {
                    resolved = "REVENUE_QUERY";
                    tool = "get_revenue";
                }
                else if (resolved == "REVENUE_QUERY" && policy.IntentOutputs.Any(x => x != "revenue"))
                {
                    resolved = "BUSINESS_SUMMARY";
                    tool = "get_business_summary";
                }

                if (policy.IntentOutputs.Count > 0 && resolved == "REVENUE_QUERY" && policy.IntentOutputs.Any(x => x != "revenue"))
                    return Clarify("CapabilityContractMismatch", top.Score, margin);
                if (resolved == "REVENUE_QUERY" && !new[] { "today", "month", "year" }.Contains(ToToolPeriod(resolved, policy.Period)))
                    return Clarify("UnsupportedPeriod", top.Score, margin);
                if (resolved == "BUSINESS_SUMMARY" && !new[] { "today", "yesterday", "this_week", "last_week", "this_month", "last_month" }.Contains(policy.Period))
                    return Clarify("UnsupportedPeriod", top.Score, margin);
                if (top.Score < 0.75) return Unsupported("LowSimilarity", top.Score, margin);
                if (margin < 0.0) return Clarify("CloseIntentCandidates", top.Score, margin);

                var args = JsonSerializer.SerializeToElement(new Dictionary<string, string> { ["period"] = ToToolPeriod(resolved, policy.Period) });
                return new ManagerEmbeddingPilotResult("ACCEPT", "CapabilityValidated", resolved, tool, args, policy.Period, policy.IntentOutputs.ToArray(), top.Score, margin);
            }
            finally { _inference.Release(); }
        }
        finally { _capacity.Release(); }
        }
        finally
        {
            lock (_gate)
            {
                _activeOperations--;
                if (_disposed && _activeOperations == 0) DisposeResourcesLocked();
            }
        }
    }

    private async Task EnsureInitializedAsync(CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(ManagerEmbeddingPilot));
        }
        if (_initialized) return;
        lock (_gate)
        {
            if (_initialized) return;
            try
            {
                var model = ResolvePath("AI:EmbeddingPilot:ModelPath");
                var tokenizer = ResolvePath("AI:EmbeddingPilot:TokenizerPath");
                var catalog = ResolvePath("AI:EmbeddingPilot:CatalogPath");
                if (!string.Equals(SHA256.HashData(File.ReadAllBytes(model)).ToHex(), ModelSha256, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("Embedding model checksum does not match frozen v3.3 baseline.");
                if (!string.Equals(SHA256.HashData(File.ReadAllBytes(tokenizer)).ToHex(), TokenizerSha256, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("Tokenizer checksum does not match frozen v3.3 baseline.");
                _tokenizer = Tokenizer.FromFile(tokenizer);
                _session = new InferenceSession(model);
                var rows = JsonSerializer.Deserialize<ReferenceRow[]>(File.ReadAllText(catalog), new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? Array.Empty<ReferenceRow>();
                if (rows.Count(x => x.Intent is "REVENUE_QUERY" or "BUSINESS_SUMMARY") == 0) throw new InvalidDataException("Pilot catalog is empty.");
                foreach (var row in rows.Where(x => x.Intent is "REVENUE_QUERY" or "BUSINESS_SUMMARY")) _references.Add(new Reference(row.Intent, row.Text, Encode(row.Text)));
            }
            catch (Exception ex)
            {
                _initializationError = ex;
                _logger.LogError(ex, "Embedding pilot initialization failed; pilot is fail-closed.");
            }
            _initialized = true;
        }
        await Task.CompletedTask;
        cancellationToken.ThrowIfCancellationRequested();
    }

    private string ResolvePath(string key)
    {
        var value = _configuration[key] ?? throw new InvalidOperationException($"Missing {key}.");
        return Path.GetFullPath(Path.Combine(_environment.ContentRootPath, value));
    }
    private int GetTimeout() => Math.Clamp(_configuration.GetValue<int?>("AI:EmbeddingPilot:TimeoutMilliseconds") ?? 1500, 100, 10000);
    private static string ToToolPeriod(string intent, string period) => intent == "REVENUE_QUERY" && period == "this_month" ? "month" : period;
    private static ManagerEmbeddingPilotResult Unsupported(string reason, double score = 0, double margin = 0) => new("UNSUPPORTED", reason, null, null, default, "unknown", Array.Empty<string>(), score, margin);
    private static ManagerEmbeddingPilotResult Clarify(string reason, double score = 0, double margin = 0) => new("CLARIFY", reason, null, null, default, "unknown", Array.Empty<string>(), score, margin);
    private static double Cosine(float[] a, float[] b) => a.Zip(b, (x, y) => x * y).Sum();

    private float[] Encode(string text)
    {
        if (_tokenizer is null || _session is null) throw new InvalidOperationException("Embedding session is unavailable.");
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

    private static ParsedPolicy ParsePolicy(string text)
    {
        var t = Fold(text);
        var negated = Regex.IsMatch(t, @"(^|\s)(khong|dung)(?=\s|$|[,;.!?])");
        var outOfScope = Regex.IsMatch(t, @"thuc don|menu|danh sach mon|mon an|gia ban|gia mon|mo cua|gio phuc vu|phuc vu den|diem thanh vien|loyalty|ca lam|lich ca|ton kho|nguyen lieu|cap nhat trang thai|trang thai don|xep hang mon|mon ban chay|don dang cho|don cho xu ly");
        var period = Regex.IsMatch(t, "tuan sau|thang sau|nam sau|ngay mai") ? "unsupported" : Regex.IsMatch(t, "hom nay") ? "today" : Regex.IsMatch(t, "hom qua") ? "yesterday" : Regex.IsMatch(t, "tuan nay") ? "this_week" : Regex.IsMatch(t, "tuan truoc") ? "last_week" : Regex.IsMatch(t, "thang nay") ? "this_month" : Regex.IsMatch(t, "thang truoc") ? "last_month" : Regex.IsMatch(t, "nam nay") ? "year" : "missing";
        var outputs = new HashSet<string>();
        if (Regex.IsMatch(t, "doanh thu|doanh so|ban hang|thu duoc|thu ve|kiem duoc")) outputs.Add("revenue");
        if (Regex.IsMatch(t, "so don|don hang|luong don")) outputs.Add("order_count");
        if (Regex.IsMatch(t, "kpi|tong quan|tom tat|tinh hinh") || (Regex.IsMatch(t, "bao cao") && !Regex.IsMatch(t, "doanh thu|doanh so|ban hang"))) outputs.UnionWith(new[] { "revenue", "order_count", "aov", "expenses", "estimated_cogs", "estimated_profit", "top_products" });
        var intentOutputs = outputs.ToList();
        return new ParsedPolicy(negated, outOfScope, period, intentOutputs);
    }
    private static string Fold(string text)
    {
        var mapped = text.Replace('\u0111', 'd').Replace('\u0110', 'D');
        return new string(mapped.Normalize(System.Text.NormalizationForm.FormD).Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark).ToArray()).ToLowerInvariant();
    }
    public void Dispose()
    {
        lock (_gate)
        {
            if (_disposed) return;
            _disposed = true;
            if (_activeOperations == 0) DisposeResourcesLocked();
        }
    }

    private void DisposeResourcesLocked()
    {
        if (_resourcesDisposed) return;
        _resourcesDisposed = true;
        _inference.Dispose();
        _capacity.Dispose();
        _session?.Dispose();
        _tokenizer?.Dispose();
    }
    private sealed record Reference(string Intent, string Text, float[] Vector);
    private sealed record ReferenceRow(string Id, string Intent, string Text);
    private sealed record ParsedPolicy(bool Negated, bool OutOfScope, string Period, List<string> IntentOutputs);
}

internal static class HashExtensions
{
    public static string ToHex(this byte[] bytes) => Convert.ToHexString(bytes);
}
