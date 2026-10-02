using RestaurantPOS.Domain.Inventory;

namespace RestaurantPOS.Domain.Entities;

public class StockTransaction
{
    public Guid Id { get; set; }
    public Guid BranchId { get; set; }
    public Guid InventoryItemId { get; set; }
    public StockTransactionType Type { get; set; }
    public decimal Quantity { get; set; }
    public decimal BeforeQuantity { get; set; }
    public decimal AfterQuantity { get; set; }
    public string ReferenceType { get; set; } = string.Empty;
    public Guid ReferenceId { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public string? Note { get; set; }

    public Branch? Branch { get; set; }
    public InventoryItem? InventoryItem { get; set; }
}
