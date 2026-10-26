using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<KitchenService>? _logger;

        public KitchenService(
            ApplicationDbContext context,
            IKitchenNotifier kitchenNotifier,
            INotificationService? notificationService = null,
            ILogger<KitchenService>? logger = null)
        {
            _context = context;
            _kitchenNotifier = kitchenNotifier;
            _notificationService = notificationService;
            _logger = logger;
        }
            //xac dinh mon chua gui, yeu cau che bien
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
                    if (order.BranchId == Guid.Empty || !await _context.Branches.AsNoTracking().AnyAsync(branch => branch.Id == order.BranchId && branch.IsActive))
                        throw new InvalidOperationException("Không thể gửi đơn không có chi nhánh hợp lệ xuống bếp.");
                    if (order.Status != "Đang xử lý") throw new InvalidOperationException("Đơn hàng đã đóng hoặc đã hủy.");
                    if (!await AreOrderProductsAvailableAsync(order))
                        throw new InvalidOperationException("Không thể gửi món không còn khả dụng tại chi nhánh xuống bếp.");
                    
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

        private async Task<bool> AreOrderProductsAvailableAsync(Order order)
        {
            var productIds = order.Details
                .Where(detail => detail.ProductId.HasValue)
                .Select(detail => detail.ProductId!.Value)
                .Distinct()
                .ToArray();

            if (productIds.Length == 0)
                return false;

            var availableProductCount = await _context.Products
                .Where(product => productIds.Contains(product.Id) &&
                    product.IsActive &&
                    _context.ProductBranchAvailabilities.Any(availability =>
                        availability.ProductId == product.Id &&
                        availability.BranchId == order.BranchId &&
                        availability.Branch != null &&
                        availability.Branch.IsActive &&
                        availability.Status == "Available"))
                .CountAsync();

            return availableProductCount == productIds.Length;
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
            if (!IsValidKitchenStatus(status))
                throw new InvalidOperationException("Invalid kitchen request status.");

            var validationRow = await (from r in _context.OrderRequests.AsNoTracking()
                                       join o in _context.Orders.AsNoTracking() on r.OrderId equals o.Id
                                       where r.Id == requestId
                                       select new { Request = r, Order = o }).FirstOrDefaultAsync();
            if (validationRow == null)
                return null;

            if (validationRow.Order.BranchId == Guid.Empty ||
                !await _context.Branches.AsNoTracking().AnyAsync(branch => branch.Id == validationRow.Order.BranchId && branch.IsActive))
            {
                _logger?.LogError("Rejected kitchen status update because order {RequestId} has no valid active branch.", requestId);
                return null;
            }

            if (!CanTransitionKitchenStatus(validationRow.Request.Status, status))
                throw new InvalidOperationException("Kitchen request status transition is not allowed.");

            var isCompleted = string.Equals(status, "Completed", StringComparison.Ordinal);
            var transitionedToCompleted = false;
            OrderRequest? request;

            if (isCompleted)
            {
                // Make the transition itself the idempotency guard. A repeated or concurrent
                // Completed command can therefore not create a second employee notification.
                var now = DateTime.UtcNow;
                var affected = await _context.OrderRequests
                    .Where(r => r.Id == requestId && r.Status != "Completed")
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(r => r.Status, status)
                        .SetProperty(r => r.CompletedAt, now)
                        .SetProperty(r => r.ProcessedBy, r => processedBy ?? r.ProcessedBy));

                request = await _context.OrderRequests.AsNoTracking().FirstOrDefaultAsync(r => r.Id == requestId);
                if (request == null) return null;
                transitionedToCompleted = affected == 1;
            }
            else
            {
                request = await _context.OrderRequests.FindAsync(requestId);
                if (request == null) return null;

                request.Status = status;
                request.ProcessedBy = processedBy ?? request.ProcessedBy;
                var now = DateTime.UtcNow;
                if (status == "Preparing") { request.AcceptedAt ??= now; request.PreparingAt ??= now; }
                await _context.SaveChangesAsync();
            }

            var orderRow = await (from r in _context.OrderRequests.AsNoTracking()
                                  join o in _context.Orders.AsNoTracking() on r.OrderId equals o.Id
                                  where r.Id == requestId
                                  select new { Request = r, Order = o }).FirstOrDefaultAsync();

            var orderBranchId = orderRow?.Order.BranchId;
            try
            {
                if (orderBranchId.HasValue && orderBranchId.Value != Guid.Empty &&
                    await _context.Branches.AsNoTracking().AnyAsync(branch => branch.Id == orderBranchId.Value && branch.IsActive))
                {
                    await _kitchenNotifier.NotifyRequestStatusUpdatedAsync(requestId, status, orderBranchId);
                }
                else
                {
                    _logger?.LogError("Skipped kitchen status broadcast because order {RequestId} has no valid active branch.", requestId);
                    if (_notificationService != null)
                    {
                        await _notificationService.CreateAsync(new Notification
                        {
                            Type = "ORDER_BRANCH_INVALID",
                            Title = "Đơn hàng thiếu chi nhánh hợp lệ",
                            Message = $"Yêu cầu bếp {requestId} đã được cập nhật nhưng không phát realtime do thiếu chi nhánh hợp lệ.",
                            EntityType = "OrderRequest",
                            EntityId = requestId,
                            Route = "/orders",
                            TargetRole = "admin",
                            BranchId = null
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                // The status is already committed. Reconnect/polling can recover the missed event;
                // do not turn a post-commit SignalR failure into a retryable business failure.
                _logger?.LogError(ex, "Failed to broadcast kitchen request status {RequestId}", requestId);
            }

            if (transitionedToCompleted && orderRow != null && _notificationService != null)
            {
                var location = string.IsNullOrWhiteSpace(orderRow.Order.TableName) ? "Mang về" : orderRow.Order.TableName;
                var requestItems = await _context.OrderRequestItems.AsNoTracking()
                    .Where(item => item.OrderRequestId == requestId)
                    .ToListAsync();
                var itemSummary = requestItems.Count > 0
                    ? string.Join(", ", requestItems.Select(item => $"{item.ProductName} x{item.Quantity}"))
                    : "món trong đợt gọi";

                try
                {
                    // ProcessedBy is a display string, not a validated user id. Until an explicit
                    // assignment relation exists, route only to employee users in this branch.
                    await _notificationService.CreateAsync(new Notification
                    {
                        Type = "ORDER_REQUEST_COMPLETED",
                        Title = "Món đã hoàn thành",
                        Message = $"{location} — đợt gọi #{orderRow.Request.RequestNumber} đã hoàn thành: {itemSummary}.",
                        EntityType = "OrderRequest",
                        EntityId = requestId,
                        Route = "/staff",
                        TargetRole = "employee",
                        BranchId = orderRow.Order.BranchId
                    });
                }
                catch (Exception ex)
                {
                    // Notification persistence is attempted after the business commit. A SignalR
                    // failure must not cause the caller to repeat the Completed transition.
                    _logger?.LogError(ex, "Failed to create employee completion notification for {RequestId}", requestId);
                }
            }

            return request;
        }

        private static bool IsValidKitchenStatus(string status) => status is "Pending" or "Preparing" or "Ready" or "Completed" or "Cancelled";

        private static bool CanTransitionKitchenStatus(string current, string target)
        {
            if (string.Equals(current, target, StringComparison.Ordinal))
                return true;

            return current switch
            {
                "Pending" => target is "Preparing" or "Cancelled",
                "Preparing" => target is "Ready" or "Cancelled",
                "Ready" => target is "Completed" or "Cancelled",
                _ => false
            };
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
