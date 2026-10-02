using RestaurantPOS.Domain.Inventory;

namespace RestaurantPOS.Application.DTOs.Inventory;

public sealed record InventoryItemResponse(
    Guid Id,
    string Name,
    string UnitCode,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

public sealed record CreateInventoryItemRequest(string? Name, string? UnitCode);

public sealed record UpdateInventoryItemRequest(string? Name, string? UnitCode, bool IsActive);

public sealed record BranchInventoryResponse(
    Guid BranchId,
    Guid InventoryItemId,
    string Name,
    string UnitCode,
    decimal CurrentQuantity,
    decimal MinimumStock,
    bool IsLowStock,
    bool IsOutOfStock,
    DateTime UpdatedAtUtc);

public sealed record InventoryQuantityByUnitResponse(string UnitCode, decimal Quantity);

public sealed record InventoryLowStockItemResponse(
    Guid InventoryItemId,
    string Name,
    string UnitCode,
    decimal CurrentQuantity,
    decimal MinimumStock,
    bool IsOutOfStock);

public sealed record InventoryOverviewResponse(
    Guid BranchId,
    DateTime PeriodStartUtc,
    DateTime PeriodEndUtc,
    int ActiveItemCount,
    int LowStockItemCount,
    int OutOfStockItemCount,
    IReadOnlyCollection<InventoryQuantityByUnitResponse> ReceivedThisMonthByUnit,
    IReadOnlyCollection<InventoryQuantityByUnitResponse> IssuedThisMonthByUnit,
    decimal ImportCostThisMonth,
    IReadOnlyCollection<InventoryLowStockItemResponse> LowStockItems);

public sealed record UpdateMinimumStockRequest(decimal MinimumStock);

public sealed record StockDocumentItemRequest(Guid InventoryItemId, decimal Quantity, decimal? UnitPrice = null);

public sealed record CreateStockReceiptRequest(
    Guid BranchId,
    string? SupplierName,
    DateTime? PurchaseDate,
    string? Note,
    string? IdempotencyKey,
    IReadOnlyCollection<StockDocumentItemRequest>? Items);

public sealed record UpdateStockReceiptRequest(
    string? SupplierName,
    DateTime? PurchaseDate,
    string? Note,
    IReadOnlyCollection<StockDocumentItemRequest>? Items);

public sealed record CreateStockIssueRequest(
    Guid BranchId,
    string? Reason,
    string? Note,
    string? IdempotencyKey,
    IReadOnlyCollection<StockDocumentItemRequest>? Items);

public sealed record UpdateStockIssueRequest(
    string? Reason,
    string? Note,
    IReadOnlyCollection<StockDocumentItemRequest>? Items);

public sealed record StockDocumentItemResponse(
    Guid Id,
    Guid InventoryItemId,
    string InventoryItemName,
    string UnitCode,
    decimal Quantity,
    decimal? UnitPrice);

public sealed record StockReceiptResponse(
    Guid Id,
    Guid BranchId,
    string? SupplierName,
    DateTime? PurchaseDate,
    decimal TotalAmount,
    StockDocumentStatus Status,
    string? Note,
    Guid CreatedBy,
    DateTime CreatedAtUtc,
    Guid? ConfirmedBy,
    DateTime? ConfirmedAtUtc,
    IReadOnlyCollection<StockDocumentItemResponse> Items);

public sealed record StockIssueResponse(
    Guid Id,
    Guid BranchId,
    string Reason,
    string? Note,
    StockDocumentStatus Status,
    Guid CreatedBy,
    DateTime CreatedAtUtc,
    Guid? ConfirmedBy,
    DateTime? ConfirmedAtUtc,
    IReadOnlyCollection<StockDocumentItemResponse> Items);

public sealed record StockTransactionResponse(
    Guid Id,
    Guid BranchId,
    Guid InventoryItemId,
    string InventoryItemName,
    string UnitCode,
    StockTransactionType Type,
    decimal Quantity,
    decimal BeforeQuantity,
    decimal AfterQuantity,
    string ReferenceType,
    Guid ReferenceId,
    Guid CreatedBy,
    DateTime CreatedAtUtc,
    string? Note);

public sealed record InventoryPage<T>(IReadOnlyCollection<T> Items, int Total, int Page, int PageSize);
