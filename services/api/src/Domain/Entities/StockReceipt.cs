using RestaurantPOS.Domain.Inventory;

namespace RestaurantPOS.Domain.Entities;

public class StockReceipt
{
    public Guid Id { get; set; }
    public Guid BranchId { get; set; }
    public string? SupplierName { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public decimal TotalAmount { get; set; }
    public StockDocumentStatus Status { get; set; } = StockDocumentStatus.Draft;
    public string? Note { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public Guid? ConfirmedBy { get; set; }
    public DateTime? ConfirmedAtUtc { get; set; }
    public string? IdempotencyKey { get; set; }

    public Branch? Branch { get; set; }
    public ICollection<StockReceiptItem> Items { get; set; } = new List<StockReceiptItem>();
}
