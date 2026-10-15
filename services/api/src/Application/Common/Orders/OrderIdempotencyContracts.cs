using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RestaurantPOS.Application.Common.Orders;

public enum OrderIdempotencyConflictCode
{
    KEY_REUSED,
    SCOPE_CONFLICT,
    IN_PROGRESS,
    ORDER_NOT_MUTABLE
}

public sealed record OrderIdempotencyConflict(OrderIdempotencyConflictCode Code);

/// <summary>
/// Server-derived authorization scope used for an operation fingerprint.
/// GuestSessionId must be a server-derived stable identifier, never a raw token.
/// </summary>
public sealed record TrustedOrderOperationScope(
    string ScopeType,
    Guid BranchId,
    Guid? TableId = null,
    string? TableName = null,
    Guid? PrincipalId = null,
    string? Role = null,
    Guid? CustomerId = null,
    string? GuestSessionId = null);

public sealed record OrderOperationDetail(
    Guid? ProductId,
    Guid? ToppingId,
    int Quantity,
    string? Options = null,
    string? Note = null);

public sealed record OrderOperationCanonicalRequest(
    string OperationType,
    Guid? ExplicitOrderId,
    string? OrderNote,
    IReadOnlyList<OrderOperationDetail> Details);

public static class OrderIdempotencyKeyValidator
{
    public const int MinLength = 16;
    public const int MaxLength = 128;

    public static bool TryValidate(string? key, out string error)
    {
        if (string.IsNullOrEmpty(key))
        {
            error = "Idempotency-Key is required when supplied.";
            return false;
        }

        if (key.Length is < MinLength or > MaxLength)
        {
            error = $"Idempotency-Key must contain between {MinLength} and {MaxLength} characters.";
            return false;
        }

        foreach (var character in key)
        {
            if (character > 0x7f || char.IsControl(character) || char.IsWhiteSpace(character))
            {
                error = "Idempotency-Key must contain safe ASCII characters without whitespace.";
                return false;
            }

            if (!(char.IsLetterOrDigit(character) || character is '-' or '_' or '.' or '~'))
            {
                error = "Idempotency-Key contains an unsupported character.";
                return false;
            }
        }

        error = string.Empty;
        return true;
    }
}

public static class OrderOperationScopeFingerprint
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public static string Create(TrustedOrderOperationScope scope)
    {
        ArgumentNullException.ThrowIfNull(scope);
        if (scope.BranchId == Guid.Empty) throw new ArgumentException("BranchId is required.", nameof(scope));
        if (string.IsNullOrWhiteSpace(scope.ScopeType)) throw new ArgumentException("ScopeType is required.", nameof(scope));

        var scopeType = scope.ScopeType.Trim().ToUpperInvariant();
        if (scopeType == "GUEST" && string.IsNullOrWhiteSpace(scope.GuestSessionId))
            throw new ArgumentException("A server-derived guest session identifier is required.", nameof(scope));
        if (scopeType == "CUSTOMER" && !scope.CustomerId.HasValue && !scope.PrincipalId.HasValue)
            throw new ArgumentException("A customer identity is required.", nameof(scope));
        if (scopeType == "STAFF" && (!scope.PrincipalId.HasValue || string.IsNullOrWhiteSpace(scope.Role)))
            throw new ArgumentException("A staff identity and role are required.", nameof(scope));

        var canonical = new
        {
            scopeType,
            branchId = scope.BranchId,
            tableId = scope.TableId,
            tableName = scope.TableName,
            principalId = scope.PrincipalId,
            role = scope.Role?.Trim().ToUpperInvariant(),
            customerId = scope.CustomerId,
            guestSessionId = scope.GuestSessionId
        };

        return ComputeSha256(JsonSerializer.Serialize(canonical, JsonOptions));
    }

    public static string ComputeSha256(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}

public static class OrderOperationCanonicalizer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never
    };

    public static string Canonicalize(
        OrderOperationCanonicalRequest request,
        TrustedOrderOperationScope scope,
        out string payloadHash)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(scope);
        if (string.IsNullOrWhiteSpace(request.OperationType))
            throw new ArgumentException("OperationType is required.", nameof(request));
        if (request.Details is null)
            throw new ArgumentNullException(nameof(request.Details));

        // Keep every detail and its position. The current merge behavior is not
        // reimplemented here; silently aggregating lines could change business meaning.
        var canonical = new
        {
            operationType = request.OperationType.Trim().ToUpperInvariant(),
            explicitOrderId = request.ExplicitOrderId,
            scopeFingerprint = OrderOperationScopeFingerprint.Create(scope),
            branchId = scope.BranchId,
            tableId = scope.TableId,
            tableName = scope.TableName,
            orderNote = NormalizeLineEndings(request.OrderNote),
            details = request.Details.Select(detail => new
            {
                productId = detail.ProductId,
                toppingId = detail.ToppingId,
                quantity = detail.Quantity,
                // OrderService compares Options after trimming; retain that behavior.
                options = detail.Options?.Trim(),
                note = NormalizeLineEndings(detail.Note)
            }).ToArray()
        };

        var json = JsonSerializer.Serialize(canonical, JsonOptions);
        payloadHash = OrderOperationScopeFingerprint.ComputeSha256(json);
        return json;
    }

    private static string? NormalizeLineEndings(string? value) =>
        value?.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
}

public sealed record OrderOperationResponseSnapshot(
    int Version,
    int StatusCode,
    string ContentType,
    string PayloadJson,
    DateTimeOffset CreatedAtUtc)
{
    public const int CurrentVersion = 1;
    public const int MaxPayloadBytes = 256 * 1024;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public static OrderOperationResponseSnapshot Create<T>(int statusCode, T payload, DateTimeOffset? createdAtUtc = null)
    {
        ArgumentNullException.ThrowIfNull(payload);
        var json = JsonSerializer.Serialize(payload, JsonOptions);
        return CreateFromJson(statusCode, json, createdAtUtc);
    }

    public static OrderOperationResponseSnapshot CreateFromJson(
        int statusCode,
        string payloadJson,
        DateTimeOffset? createdAtUtc = null,
        string contentType = "application/json")
    {
        if (statusCode is < 100 or > 599) throw new ArgumentOutOfRangeException(nameof(statusCode));
        if (string.IsNullOrWhiteSpace(payloadJson)) throw new ArgumentException("Payload is required.", nameof(payloadJson));
        if (string.IsNullOrWhiteSpace(contentType)) throw new ArgumentException("Content type is required.", nameof(contentType));

        using var document = JsonDocument.Parse(payloadJson);
        EnsureSafeJson(document.RootElement);
        var bytes = Encoding.UTF8.GetByteCount(payloadJson);
        if (bytes > MaxPayloadBytes)
            throw new ArgumentException($"Snapshot payload exceeds {MaxPayloadBytes} bytes.", nameof(payloadJson));

        return new(CurrentVersion, statusCode, contentType, payloadJson, createdAtUtc ?? DateTimeOffset.UtcNow);
    }

    public T DeserializePayload<T>()
    {
        if (Version != CurrentVersion) throw new InvalidOperationException("Unsupported order operation snapshot version.");
        return JsonSerializer.Deserialize<T>(PayloadJson, JsonOptions)
            ?? throw new InvalidOperationException("Snapshot payload is empty.");
    }

    private static void EnsureSafeJson(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                var name = property.Name.Replace("_", string.Empty, StringComparison.Ordinal).ToLowerInvariant();
                if (name.Contains("authorization", StringComparison.Ordinal) ||
                    name.Contains("password", StringComparison.Ordinal) ||
                    name.Contains("secret", StringComparison.Ordinal) ||
                    name.Contains("token", StringComparison.Ordinal) ||
                    name.Contains("cookie", StringComparison.Ordinal) ||
                    name.Contains("apikey", StringComparison.Ordinal) ||
                    name.Contains("connectionstring", StringComparison.Ordinal))
                    throw new ArgumentException("Snapshot contains a sensitive field.", nameof(element));
                EnsureSafeJson(property.Value);
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray()) EnsureSafeJson(item);
        }
    }
}
