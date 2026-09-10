using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RestaurantPOS.Application.Common.Interfaces;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Infrastructure.Persistence;
using RestaurantPOS.Application.DTOs.Orders;

namespace RestaurantPOS.Application.Services
{
    public class KitchenService : IKitchenService
    {
        private readonly ApplicationDbContext _context;
        private readonly IKitchenNotifier _kitchenNotifier;
        private readonly INotificationService? _notificationService;

        public KitchenService(ApplicationDbContext context, IKitchenNotifier kitchenNotifier, INotificationService? notificationService = null)
        {
            _context = context;
            _kitchenNotifier = kitchenNotifier;
            _notificationService = notificationService;
        }

        public async Task<OrderRequest> SendToKitchenAsync(Guid orderId)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var order = await _context.Orders
                        .Include(o => o.Details)
                        .FirstOrDefaultAsync(o => o.Id == orderId);

                    if (order == null) throw new InvalidOperationException("Không tìm thấy đơn hàng.");
                    if (order.Status != "Đang xử lý") throw new InvalidOperationException("Đơn hàng đã đóng hoặc đã hủy.");

                    var itemsToSend = order.Details.Where(d => d.Quantity > d.SentQuantity).ToList();
                    if (!itemsToSend.Any()) throw new InvalidOperationException("Không có món mới cần gửi bếp.");

                    var lastRequestNumber = await _context.OrderRequests
                        .Where(r => r.OrderId == orderId)
                        .MaxAsync(r => (int?)r.RequestNumber) ?? 0;

                    var orderRequest = new OrderRequest
                    {
                        Id = Guid.NewGuid(),
                        OrderId = orderId,
                        RequestNumber = lastRequestNumber + 1,
                        Status = "Pending",
                        CreatedAt = DateTime.UtcNow
                    };

                    foreach (var item in itemsToSend)
                    {
                        int unsentQuantity = item.Quantity - item.SentQuantity;
                        orderRequest.Items.Add(new OrderRequestItem
                        {
                            Id = Guid.NewGuid(),
                            OrderRequestId = orderRequest.Id,
                            ProductId = item.ProductId,
                            ProductName = item.ProductName,
                            Quantity = unsentQuantity,
                            Options = item.Options
                        });
                        item.SentQuantity = item.Quantity;
                    }

                    _context.OrderRequests.Add(orderRequest);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    var payload = new
                    {
                        id = orderRequest.Id,
                        orderId = order.Id,
                        requestNumber = orderRequest.RequestNumber,
                        status = orderRequest.Status,
                        tableName = order.TableName,
                        branchId = order.BranchId,
                        createdAt = orderRequest.CreatedAt,
                        items = orderRequest.Items.Select(i => new { productName = i.ProductName, quantity = i.Quantity, options = i.Options })
                    };

                    await _kitchenNotifier.NotifyNewOrderRequestAsync(payload);
                    if (_notificationService != null)
                    {
                        foreach (var role in new[] { "kitchen", "admin" })
                            await _notificationService.CreateAsync(new Notification
                            {
                                Type = "ORDER_CREATED", Title = "Đơn mới gửi bếp",
                                Message = $"{order.TableName ?? "Mang về"} có món mới cần chế biến.",
                                EntityType = "OrderRequest", EntityId = orderRequest.Id,
                                Route = "/kitchen", TargetRole = role, BranchId = order.BranchId
                            });
                    }
                    return orderRequest;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<List<object>> GetActiveKitchenRequestsAsync(Guid? branchId)
        {
            var query = _context.OrderRequests
                .Include(r => r.Items)
                .Where(r => r.Status != "Completed" && r.Status != "Cancelled");

            if (branchId.HasValue)
            {
                var requests = await (from r in query
                                      join o in _context.Orders on r.OrderId equals o.Id
                                      where o.BranchId == branchId.Value
                                      orderby r.CreatedAt
                                      select (object)new
                                      {
                                          r.Id,
                                          r.OrderId,
                                          r.RequestNumber,
                                          r.Status,
                                          r.CreatedAt,
                                          tableName = o.TableName,
                                          items = r.Items
                                      }).ToListAsync();
                return requests;
            }

            var allRequests = await (from r in query
                                     join o in _context.Orders on r.OrderId equals o.Id
                                     orderby r.CreatedAt
                                     select (object)new
                                     {
                                         r.Id,
                                         r.OrderId,
                                         r.RequestNumber,
                                         r.Status,
                                         r.CreatedAt,
                                         tableName = o.TableName,
                                         items = r.Items
                                     }).ToListAsync();

            return allRequests;
        }

        public Task<OrderRequest?> UpdateRequestStatusAsync(Guid requestId, string status) => UpdateRequestStatusAsync(requestId, status, null);

        public async Task<OrderRequest?> UpdateRequestStatusAsync(Guid requestId, string status, string? processedBy)
        {
            var request = await _context.OrderRequests.FindAsync(requestId);
            if (request == null) return null;

            request.Status = status;
            request.ProcessedBy = processedBy ?? request.ProcessedBy;
            var now = DateTime.UtcNow;
            if (status == "Preparing") { request.AcceptedAt ??= now; request.PreparingAt ??= now; }
            if (status == "Completed") request.CompletedAt = now;
            await _context.SaveChangesAsync();

            var orderBranchId = await (from o in _context.Orders
                                       where o.Id == request.OrderId
                                       select o.BranchId).FirstOrDefaultAsync();
            if (orderBranchId.HasValue)
                await _kitchenNotifier.NotifyRequestStatusUpdatedAsync(requestId, status, orderBranchId);
            else
                await _kitchenNotifier.NotifyRequestStatusUpdatedAsync(requestId, status);
            return request;
        }

        public async Task<(IReadOnlyList<object> Items, int Total)> GetHistoryAsync(string? search, string? status, string? tableName, DateTime? fromDate, DateTime? toDate, Guid? branchId, int page, int pageSize)
        {
            var query = from request in _context.OrderRequests.Include(r => r.Items)
                        join order in _context.Orders on request.OrderId equals order.Id
                        where (!branchId.HasValue || order.BranchId == branchId) &&
                              (string.IsNullOrWhiteSpace(search) || order.InvoiceCode!.Contains(search) || order.Id.ToString().Contains(search)) &&
                              (string.IsNullOrWhiteSpace(status) || request.Status == status) &&
                              (string.IsNullOrWhiteSpace(tableName) || order.TableName == tableName) &&
                              (!fromDate.HasValue || request.CreatedAt >= fromDate.Value) &&
                              (!toDate.HasValue || request.CreatedAt < toDate.Value.AddDays(1))
                        select new { request, order };
            var total = await query.CountAsync();
            var rows = await query.OrderByDescending(x => x.request.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            var items = rows.Select(x => (object)new
            {
                x.request.Id, x.request.OrderId, x.request.RequestNumber, x.request.Status, x.request.CreatedAt,
                x.request.AcceptedAt, x.request.PreparingAt, x.request.CompletedAt, x.request.ProcessedBy,
                x.request.Note, x.order.InvoiceCode, x.order.TableName, x.order.CreatedBy,
                Items = x.request.Items.Select(i => new { i.ProductId, i.ProductName, i.Quantity, i.Note, i.Options })
            }).ToList();
            return (items, total);
        }

        public async Task<object?> GetRequestDetailAsync(Guid requestId, Guid? branchId)
        {
            var row = await (from request in _context.OrderRequests.Include(r => r.Items)
                             join order in _context.Orders.Include(o => o.Details) on request.OrderId equals order.Id
                             where request.Id == requestId && (!branchId.HasValue || order.BranchId == branchId)
                             select new { request, order }).FirstOrDefaultAsync();
            if (row == null) return null;
            return new { row.request, row.order };
        }

        public async Task<Order?> GetOrderRequestOrderAsync(Guid requestId)
        {
            return await (from request in _context.OrderRequests
                          join order in _context.Orders on request.OrderId equals order.Id
                          where request.Id == requestId
                          select order).FirstOrDefaultAsync();
        }

        public async Task<List<OrderKitchenRequestStatusDto>> GetOrderKitchenStatusesAsync(Guid orderId)
        {
            return await _context.OrderRequests
                .AsNoTracking()
                .Where(r => r.OrderId == orderId)
                .OrderBy(r => r.RequestNumber)
                .Select(r => new OrderKitchenRequestStatusDto
                {
                    Id = r.Id,
                    RequestNumber = r.RequestNumber,
                    Status = r.Status,
                    CreatedAt = r.CreatedAt,
                    Items = r.Items.Select(i => new OrderKitchenRequestItemStatusDto
                    {
                        ProductName = i.ProductName,
                        Quantity = i.Quantity,
                        Options = i.Options
                    }).ToList()
                })
                .ToListAsync();
        }
    }
}
