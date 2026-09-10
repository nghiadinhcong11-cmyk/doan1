using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Application.DTOs.Orders;

namespace RestaurantPOS.Application.Services
{
    public interface IKitchenService
    {
        Task<OrderRequest> SendToKitchenAsync(Guid orderId);
        Task<List<object>> GetActiveKitchenRequestsAsync(Guid? branchId);
        Task<OrderRequest?> UpdateRequestStatusAsync(Guid requestId, string status);
        Task<OrderRequest?> UpdateRequestStatusAsync(Guid requestId, string status, string? processedBy);
        Task<(IReadOnlyList<object> Items, int Total)> GetHistoryAsync(string? search, string? status, string? tableName, DateTime? fromDate, DateTime? toDate, Guid? branchId, int page, int pageSize);
        Task<object?> GetRequestDetailAsync(Guid requestId, Guid? branchId);
        Task<Order?> GetOrderRequestOrderAsync(Guid requestId);
        Task<List<OrderKitchenRequestStatusDto>> GetOrderKitchenStatusesAsync(Guid orderId);
    }
}
