using RestaurantPOS.Application.DTOs.Inventory;
using RestaurantPOS.Domain.Inventory;

namespace RestaurantPOS.Application.Services;

public sealed record InventoryUserContext(string Role, Guid UserId, Guid? BranchId)
{
    public bool IsAdmin => string.Equals(Role, "admin", StringComparison.OrdinalIgnoreCase);
    public bool IsManager => string.Equals(Role, "manager", StringComparison.OrdinalIgnoreCase);
    public bool IsOperational => IsAdmin || IsManager ||
        string.Equals(Role, "employee", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(Role, "cashier", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(Role, "kitchen", StringComparison.OrdinalIgnoreCase);
}

public interface IInventoryService
{
    Task<IReadOnlyCollection<InventoryItemResponse>> GetItemsAsync(bool? activeOnly);
    Task<InventoryItemResponse> GetItemAsync(Guid id);
    Task<InventoryItemResponse> CreateItemAsync(CreateInventoryItemRequest request, InventoryUserContext user);
    Task<InventoryItemResponse> UpdateItemAsync(Guid id, UpdateInventoryItemRequest request, InventoryUserContext user);
    Task<IReadOnlyCollection<BranchInventoryResponse>> GetBranchInventoryAsync(Guid branchId, InventoryUserContext user);
    Task<InventoryOverviewResponse> GetOverviewAsync(Guid branchId, InventoryUserContext user);
    Task<BranchInventoryResponse> UpdateMinimumStockAsync(Guid branchId, Guid inventoryItemId, UpdateMinimumStockRequest request, InventoryUserContext user);
    Task<StockReceiptResponse> GetReceiptAsync(Guid id, InventoryUserContext user);
    Task<InventoryPage<StockReceiptResponse>> GetReceiptsAsync(Guid? branchId, StockDocumentStatus? status, DateTime? fromDate, DateTime? toDate, int page, int pageSize, InventoryUserContext user);
    Task<StockReceiptResponse> CreateReceiptAsync(CreateStockReceiptRequest request, InventoryUserContext user);
    Task<StockReceiptResponse> UpdateReceiptAsync(Guid id, UpdateStockReceiptRequest request, InventoryUserContext user);
    Task<StockReceiptResponse> ConfirmReceiptAsync(Guid id, InventoryUserContext user);
    Task<StockReceiptResponse> CancelReceiptAsync(Guid id, InventoryUserContext user);
    Task<StockIssueResponse> GetIssueAsync(Guid id, InventoryUserContext user);
    Task<InventoryPage<StockIssueResponse>> GetIssuesAsync(Guid? branchId, StockDocumentStatus? status, DateTime? fromDate, DateTime? toDate, int page, int pageSize, InventoryUserContext user);
    Task<StockIssueResponse> CreateIssueAsync(CreateStockIssueRequest request, InventoryUserContext user);
    Task<StockIssueResponse> UpdateIssueAsync(Guid id, UpdateStockIssueRequest request, InventoryUserContext user);
    Task<StockIssueResponse> ConfirmIssueAsync(Guid id, InventoryUserContext user);
    Task<StockIssueResponse> CancelIssueAsync(Guid id, InventoryUserContext user);
    Task<InventoryPage<StockTransactionResponse>> GetTransactionsAsync(Guid? branchId, Guid? inventoryItemId, StockTransactionType? type, DateTime? fromDate, DateTime? toDate, int page, int pageSize, InventoryUserContext user);
}
