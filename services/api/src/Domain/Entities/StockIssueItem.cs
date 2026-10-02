namespace RestaurantPOS.Domain.Entities;

public class StockIssueItem
{
    public Guid Id { get; set; }
    public Guid StockIssueId { get; set; }
    public Guid InventoryItemId { get; set; }
    public decimal Quantity { get; set; }

    public StockIssue? StockIssue { get; set; }
    public InventoryItem? InventoryItem { get; set; }
}
