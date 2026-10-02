namespace RestaurantPOS.Domain.Inventory;

public enum StockDocumentStatus
{
    Draft,
    Confirmed,
    Cancelled
}

public enum StockTransactionType
{
    IN,
    OUT,
    ADJUSTMENT
}

public static class StockReferenceTypes
{
    public const string StockReceipt = "StockReceipt";
    public const string StockIssue = "StockIssue";
    public const string StockAdjustment = "StockAdjustment";
}
