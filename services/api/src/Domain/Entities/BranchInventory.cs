namespace RestaurantPOS.Domain.Entities;

public class BranchInventory
{
    public Guid Id { get; set; }
    public Guid BranchId { get; set; }
    public Guid InventoryItemId { get; set; }
    public decimal CurrentQuantity { get; set; }
    public decimal MinimumStock { get; set; }
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public Branch? Branch { get; set; }
    public InventoryItem? InventoryItem { get; set; }
}
