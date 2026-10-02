using RestaurantPOS.Domain.Inventory;

namespace RestaurantPOS.Domain.Entities;

public class StockIssue
{
    public Guid Id { get; set; }
    public Guid BranchId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Note { get; set; }
    public StockDocumentStatus Status { get; set; } = StockDocumentStatus.Draft;
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public Guid? ConfirmedBy { get; set; }
    public DateTime? ConfirmedAtUtc { get; set; }
    public string? IdempotencyKey { get; set; }

    public Branch? Branch { get; set; }
    public ICollection<StockIssueItem> Items { get; set; } = new List<StockIssueItem>();
}
