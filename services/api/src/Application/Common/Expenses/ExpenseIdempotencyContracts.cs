using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RestaurantPOS.Application.Common.Expenses;

public static class ExpenseIdempotencyKeyValidator
{
    public const int MinLength = 16;
    public const int MaxLength = 128;

    public static bool TryValidate(string? key, out string error)
    {
        if (string.IsNullOrEmpty(key))
        {
            error = "Idempotency-Key is required.";
            return false;
        }

        if (key.Length is < MinLength or > MaxLength)
        {
            error = $"Idempotency-Key must contain between {MinLength} and {MaxLength} characters.";
            return false;
        }

        foreach (var character in key)
        {
            if (character > 0x7f || char.IsControl(character) || char.IsWhiteSpace(character) ||
                !(char.IsLetterOrDigit(character) || character is '-' or '_' or '.' or '~'))
            {
                error = "Idempotency-Key must contain safe ASCII characters without whitespace.";
                return false;
            }
        }

        error = string.Empty;
        return true;
    }
}

public sealed record ManualExpenseOperationRequest(
    Guid BranchId,
    Guid UserId,
    string Category,
    string Description,
    decimal Amount,
    DateTime ExpenseDate,
    string PaymentMethod,
    string Note);

public static class ManualExpenseOperationCanonicalizer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never
    };

    public static string ComputeHash(ManualExpenseOperationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var canonical = new
        {
            operationType = "MANUAL_EXPENSE_CREATE",
            branchId = request.BranchId,
            userId = request.UserId,
            category = request.Category,
            description = request.Description,
            amount = request.Amount,
            expenseDateUtc = request.ExpenseDate.ToUniversalTime(),
            paymentMethod = request.PaymentMethod,
            note = request.Note
        };

        return Convert.ToHexString(SHA256.HashData(
            Encoding.UTF8.GetBytes(JsonSerializer.Serialize(canonical, JsonOptions)))).ToLowerInvariant();
    }
}
