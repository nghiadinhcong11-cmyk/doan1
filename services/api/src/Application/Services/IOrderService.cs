using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestaurantPOS.Application.DTOs.Orders;
using RestaurantPOS.Domain.Entities;

namespace RestaurantPOS.Application.Services
{
    public enum PaymentAttemptResult
    {
        Paid,
        NotFound,
        AlreadyPaid,
        NotPayable
    }

    public enum WebOrderAcceptResult
    {
        Accepted,
        NotFound,
        Conflict,
        Forbidden,
        InvalidTable
    }

    public interface IOrderService
    {
        Task<List<Order>> GetOrdersAsync(OrderQueryFilter filter);
        Task<Order?> GetOrderByIdAsync(Guid id);
        Task<Order> CreateOrUpdateOrderAsync(Order order);
        Task<(WebOrderAcceptResult Result, Order? Order)> AcceptWebOrderAsync(Guid id, Guid? tableId, Guid? allowedBranchId, string acceptedBy);
        Task<(PaymentAttemptResult Result, Order? Order)> PayOrderAsync(Guid id, decimal amount, string paymentMethod);
        Task<Order?> UpdateOrderStatusAsync(Guid id, OrderUpdateDto update);
        Task<bool> DeleteOrderAsync(Guid id);
        Task<(bool Success, string Message)> UpdateOrderStatusByTableOrInvoiceAsync(string? invoiceCode, string? tableName, string newStatus);
        Task<(bool Success, string Message)> UpdateOrderStatusByCodeAsync(string orderCode, string newStatus, Guid? userBranchId, string role);
        Task<List<Order>> GetCustomerOrdersAsync(string customerPhone);
        Task<List<Order>> GetOrdersByCustomerAsync(string? email, string? phone);
    }
}
