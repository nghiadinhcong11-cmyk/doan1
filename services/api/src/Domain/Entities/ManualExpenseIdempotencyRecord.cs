namespace RestaurantPOS.Domain.Entities;

/// <summary>
/// Durable operation record for a manual expense create request.
/// It is intentionally separate from Expense so idempotency metadata is not
/// exposed by the existing Expense response contract.
/// </summary>
public sealed class ManualExpenseIdempotencyRecord
{
    public Guid Id { get; set; }
    public Guid BranchId { get; set; }
    public Guid UserId { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public string PayloadHash { get; set; } = string.Empty;
    public Guid ExpenseId { get; set; }
    public string ResponseJson { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}
