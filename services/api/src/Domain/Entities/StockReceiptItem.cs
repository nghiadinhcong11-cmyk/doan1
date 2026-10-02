namespace RestaurantPOS.Domain.Entities;

public class StockReceiptItem
{
    public Guid Id { get; set; }
    public Guid StockReceiptId { get; set; }
    public Guid InventoryItemId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    public StockReceipt? StockReceipt { get; set; }
    public InventoryItem? InventoryItem { get; set; }
}
