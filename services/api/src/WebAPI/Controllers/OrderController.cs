using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using RestaurantPOS.Application.DTOs.Orders;
using RestaurantPOS.Application.Services;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Infrastructure.Persistence;
using System.Security.Claims;
using RestaurantPOS.WebAPI.Hubs;

namespace RestaurantPOS.WebAPI.Controllers
{
    /// <summary>
    /// API Quản lý Hóa đơn, Đơn hàng và Điều phối Chế biến Nhà bếp
    /// </summary>
    /// <summary>Cung cấp các endpoint tạo, tra cứu và xử lý đơn hàng.</summary>
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IKitchenService _kitchenService;
        private readonly ILoyaltyService _loyaltyService;
        private readonly ApplicationDbContext? _context;
        private readonly IHubContext<KitchenHub>? _hub;

        public OrderController(IOrderService orderService, IKitchenService kitchenService, ILoyaltyService loyaltyService, ApplicationDbContext? context = null, IHubContext<KitchenHub>? hub = null)
        {
            _orderService = orderService;
            _kitchenService = kitchenService;
            _loyaltyService = loyaltyService;
            _context = context;
            _hub = hub;
        }

        /// <summary>
        /// Lấy danh sách hóa đơn theo bộ lọc tìm kiếm
        /// </summary>
        /// <summary>Lấy danh sách đơn hàng theo bộ lọc.</summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetOrders(
            [FromQuery] string? search,
            [FromQuery] string? status,
            [FromQuery] string? customerPhone,
            [FromQuery] Guid? branchId,
            [FromQuery] string? tableName,
            [FromQuery] string? fromDate,
            [FromQuery] string? toDate)
        {
            var customerPhoneFilter = customerPhone;
            if (IsCustomer())
            {
                // Customer identity comes from the signed JWT. Client-supplied phone/branch
                // filters must never widen this scope (guest tokens still carry their phone
                // as the signed Name claim).
                var jwtPhone = User.Identity?.Name;
                if (string.IsNullOrWhiteSpace(jwtPhone))
                    return Forbid();

                customerPhoneFilter = jwtPhone.Trim();
            }

            if (!TryResolveBranch(branchId, out var effectiveBranchId))
                return Forbid();

            var filter = new OrderQueryFilter
            {
                Search = search,
                Status = status,
                CustomerPhone = customerPhoneFilter,
                BranchId = effectiveBranchId,
                TableName = tableName,
                FromDate = fromDate,
                ToDate = toDate
            };

            var orders = await _orderService.GetOrdersAsync(filter);
            return Ok(orders);
        }

        /// <summary>
        /// Tạo mới hoặc cập nhật thông tin đơn hàng POS
        /// </summary>
        /// <summary>Tạo đơn hàng mới.</summary>
        [HttpPost]
        [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("order-limiter")]
        public async Task<IActionResult> CreateOrder([FromBody] Order order)
        {
            try
            {
                if (order.Id != Guid.Empty)
                {
                    var existingOrder = await _orderService.GetOrderByIdAsync(order.Id);
                    if (existingOrder == null) return NotFound();
                    if (!HasBranchAccess(existingOrder.BranchId)) return Forbid();
                    if (order.BranchId != existingOrder.BranchId) return Forbid();
                }

                if (!HasBranchAccess(order.BranchId))
                    return Forbid();

                if (IsCustomer())
                {
                    var jwtPhone = User.Identity?.Name;
                    if (!string.IsNullOrWhiteSpace(jwtPhone))
                    {
                        order.CustomerPhone = jwtPhone;
                    }

                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (Guid.TryParse(userIdClaim, out var customerId))
                    {
                        // Gán CustomerId nếu đó là khách hàng thực sự trong DB (không phải guest ngẫu nhiên)
                        if (_context != null && await _context.Customers.AnyAsync(c => c.Id == customerId))
                        {
                            order.CustomerId = customerId;
                        }
                        else
                        {
                            // Nếu claim Id không tồn tại trong DB, xóa customerId khỏi body để tránh impersonation
                            order.CustomerId = null;
                        }
                    }
                    else
                    {
                        order.CustomerId = null;
                    }
                }
                else if (User.Identity == null || !User.Identity.IsAuthenticated)
                {
                    // Anonymous users cannot set CustomerId
                    order.CustomerId = null;
                }

                if (order.Details == null || order.Details.Count == 0 || order.Details.Any(d => d.Quantity <= 0))
                    return BadRequest("Order must contain items with a quantity greater than zero.");

                var productIds = order.Details.Where(d => d.ProductId.HasValue).Select(d => d.ProductId!.Value).Distinct().ToList();
                var toppingIds = order.Details.Where(d => d.ToppingId.HasValue).Select(d => d.ToppingId!.Value).Distinct().ToList();
                if (_context != null && productIds.Count > 0 && await _context.Products.CountAsync(p => productIds.Contains(p.Id)) != productIds.Count)
                    return BadRequest("One or more products do not exist.");
                if (_context != null && toppingIds.Count > 0 && await _context.Toppings.CountAsync(t => toppingIds.Contains(t.Id)) != toppingIds.Count)
                    return BadRequest("One or more toppings do not exist.");

                var result = await _orderService.CreateOrUpdateOrderAsync(order);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi Database: " + ex.Message });
            }
        }

        [HttpPost("{id:guid}/accept")]
        [Authorize(Roles = "admin,manager,employee,cashier")]
        public async Task<IActionResult> AcceptWebOrder(Guid id, [FromBody] AcceptWebOrderDto request)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            Guid? allowedBranchId = IsBranchScopedRole(role ?? string.Empty)
                ? (Guid.TryParse(User.FindFirst("branchId")?.Value, out var branchId) ? branchId : null)
                : null;
            if (IsBranchScopedRole(role ?? string.Empty) && !allowedBranchId.HasValue)
                return Forbid();

            var acceptedBy = User.FindFirst("fullName")?.Value
                ?? User.FindFirst(ClaimTypes.Name)?.Value
                ?? "Nhân viên";

            var result = await _orderService.AcceptWebOrderAsync(id, request?.TableId, allowedBranchId, acceptedBy);
            return result.Result switch
            {
                WebOrderAcceptResult.Accepted => Ok(new
                {
                    success = true,
                    orderId = result.Order!.Id,
                    message = "Đơn hàng đã được tiếp nhận",
                    order = result.Order
                }),
                WebOrderAcceptResult.NotFound => NotFound(),
                WebOrderAcceptResult.Forbidden => Forbid(),
                WebOrderAcceptResult.InvalidTable => BadRequest(new { success = false, message = "Bàn không tồn tại hoặc không hoạt động." }),
                _ => Conflict(new { success = false, message = "Đơn này đã được nhân viên khác tiếp nhận hoặc không còn chờ xử lý." })
            };
        }

        /// <summary>
        /// Gửi các món mới gọi xuống màn hình Nhà bếp (tự động tính chênh lệch SentQuantity)
        /// </summary>
        /// <summary>Gửi đơn hàng tới bếp để chế biến.</summary>
        [HttpPost("{id}/send-to-kitchen")]
        [Authorize(Roles = "admin,employee,cashier,kitchen")]
        public async Task<IActionResult> SendToKitchen(Guid id)
        {
            try
            {
                var existingOrder = await _orderService.GetOrderByIdAsync(id);
                if (existingOrder != null && !HasBranchAccess(existingOrder.BranchId))
                    return Forbid();

                var orderRequest = await _kitchenService.SendToKitchenAsync(id);
                return Ok(orderRequest);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi gửi bếp: " + ex.Message });
            }
        }

        [HttpGet("{id:guid}/kitchen-status")]
        [Authorize(Roles = "admin,employee,cashier,kitchen")]
        public async Task<IActionResult> GetKitchenStatus(Guid id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null) return NotFound();
            if (!HasBranchAccess(order.BranchId)) return Forbid();

            return Ok(await _kitchenService.GetOrderKitchenStatusesAsync(id));
        }

        [HttpPost("{id:guid}/payment")]
        [Authorize(Roles = "admin,employee,cashier")]
        public async Task<IActionResult> PayOrder(Guid id, [FromBody] OrderPaymentDto request)
        {
            if (request.Amount <= 0)
                return BadRequest("Payment amount must be greater than zero.");
            if (string.IsNullOrWhiteSpace(request.PaymentMethod) ||
                !new[] { "Tiền mặt", "Chuyển khoản" }.Contains(request.PaymentMethod.Trim(), StringComparer.OrdinalIgnoreCase))
                return BadRequest("Unsupported payment method.");

            var existingOrder = await _orderService.GetOrderByIdAsync(id);
            if (existingOrder == null) return NotFound();
            if (!HasBranchAccess(existingOrder.BranchId)) return Forbid();
            if (request.Amount != existingOrder.TotalAmount)
                return BadRequest("Payment amount must equal the order total.");

            var result = await _orderService.PayOrderAsync(id, request.Amount, request.PaymentMethod.Trim());
            if (result.Result == PaymentAttemptResult.Paid && result.Order != null && _hub != null)
            {
                await _hub.Clients.All.SendAsync("PaymentCompleted", new
                {
                    orderId = result.Order.Id,
                    invoiceCode = result.Order.InvoiceCode,
                    status = result.Order.Status,
                    paidAmount = result.Order.PaidAmount,
                    paymentMethod = result.Order.PaymentMethod,
                    branchId = result.Order.BranchId
                });
            }
            return result.Result switch
            {
                PaymentAttemptResult.Paid => Ok(result.Order),
                PaymentAttemptResult.NotFound => NotFound(),
                PaymentAttemptResult.AlreadyPaid => Conflict(new { message = "Order has already been paid." }),
                _ => BadRequest(new { message = "Order is not payable in its current state." })
            };
        }

        [HttpGet("{id:guid}/loyalty")]
        [Authorize]
        public async Task<IActionResult> GetOrderLoyalty(Guid id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null) return NotFound();

            if (IsCustomer() && !OwnsOrder(order)) return Forbid();
            if (!IsCustomer() && !HasBranchAccess(order.BranchId)) return Forbid();

            var loyalty = await _loyaltyService.GetOrderLoyaltyAsync(id);
            return Ok(loyalty);
        }

        [HttpPost("{id:guid}/redeem")]
        [Authorize(Roles = "admin,employee,cashier")]
        public async Task<IActionResult> RedeemPoints(Guid id, [FromBody] OrderRedeemDto request)
        {
            var existingOrder = await _orderService.GetOrderByIdAsync(id);
            if (existingOrder == null) return NotFound();
            if (!HasBranchAccess(existingOrder.BranchId)) return Forbid();

            var result = await _loyaltyService.RedeemPointsAsync(request.CustomerId, id, request.Points);
            if (result == null)
                return BadRequest(new { message = "Không thể đổi điểm. Vui lòng kiểm tra số điểm hoặc trạng thái đơn hàng." });

            return Ok(new {
                message = "Đã áp dụng giảm giá từ điểm tích lũy",
                transaction = result,
                order = await _orderService.GetOrderByIdAsync(id)
            });
        }

        /// <summary>
        /// Lấy chi tiết đơn hàng theo mã định danh (Id)
        /// </summary>
        /// <summary>Lấy chi tiết đơn hàng.</summary>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetOrder(Guid id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null) return NotFound();

            if (IsCustomer() && !OwnsOrder(order))
                return Forbid();
            if (!IsCustomer() && !HasBranchAccess(order.BranchId))
                return Forbid();

            return Ok(order);
        }

        /// <summary>
        /// Cập nhật trạng thái đơn hàng (Hoàn thành, Đã hủy, ...)
        /// </summary>
        /// <summary>Cập nhật trạng thái đơn hàng.</summary>
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "admin,manager,employee,cashier,kitchen")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] OrderUpdateDto update)
        {
            var existingOrder = await _orderService.GetOrderByIdAsync(id);
            if (existingOrder != null && !HasBranchAccess(existingOrder.BranchId))
                return Forbid();
            if (existingOrder != null && IsTerminal(existingOrder.Status))
                return BadRequest("Completed or cancelled orders cannot be changed.");
            if (existingOrder?.BranchId != update.BranchId && update.BranchId.HasValue && !IsAdmin())
                return Forbid();

            Order? order;
            try
            {
                order = await _orderService.UpdateOrderStatusAsync(id, update);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            if (order == null) return NotFound();
            return Ok(order);
        }

        /// <summary>
        /// Xóa vĩnh viễn đơn hàng
        /// </summary>
        /// <summary>Xóa đơn hàng theo mã định danh.</summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin,manager,employee,cashier,kitchen")]
        public async Task<IActionResult> DeleteOrder(Guid id)
        {
            var existingOrder = await _orderService.GetOrderByIdAsync(id);
            if (existingOrder != null && !HasBranchAccess(existingOrder.BranchId))
                return Forbid();
            if (existingOrder != null && IsTerminal(existingOrder.Status))
                return BadRequest("Completed or cancelled orders cannot be deleted.");

            var success = await _orderService.DeleteOrderAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }

        /// <summary>
        /// Lấy danh sách các đợt yêu cầu chế biến đang hoạt động của Nhà bếp
        /// </summary>
        /// <summary>Lấy các yêu cầu bếp đang hoạt động.</summary>
        [HttpGet("kitchen/active-requests")]
        [Authorize(Roles = "admin,manager,kitchen")]
        public async Task<IActionResult> GetActiveKitchenRequests([FromQuery] Guid? branchId)
        {
            if (!TryResolveBranch(branchId, out var effectiveBranchId))
                return Forbid();

            var requests = await _kitchenService.GetActiveKitchenRequestsAsync(effectiveBranchId);
            return Ok(requests);
        }

        /// <summary>
        /// Cập nhật trạng thái đợt yêu cầu chế biến (Chờ xử lý -> Đang nấu -> Hoàn tất)
        /// </summary>
        /// <summary>Cập nhật trạng thái yêu cầu bếp.</summary>
        [HttpPatch("kitchen/requests/{id}/status")]
        [Authorize(Roles = "admin,manager,kitchen")]
        public async Task<IActionResult> UpdateRequestStatus(Guid id, [FromBody] string status)
        {
            var requestOrder = await _kitchenService.GetOrderRequestOrderAsync(id);
            if (requestOrder == null) return NotFound();
            if (!HasBranchAccess(requestOrder.BranchId)) return Forbid();

            var request = await _kitchenService.UpdateRequestStatusAsync(id, status, User.FindFirst("fullName")?.Value ?? User.Identity?.Name);
            if (request == null) return NotFound();
            return Ok(request);
        }

        [HttpGet("kitchen/history")]
        [Authorize(Roles = "admin,manager,kitchen")]
        public async Task<IActionResult> GetKitchenHistory([FromQuery] string? search, [FromQuery] string? status, [FromQuery] string? tableName, [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] Guid? branchId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (!TryResolveBranch(branchId, out var effectiveBranchId)) return Forbid();
            page = Math.Max(1, page); pageSize = Math.Clamp(pageSize, 1, 100);
            var result = await _kitchenService.GetHistoryAsync(search, status, tableName, from, to, effectiveBranchId, page, pageSize);
            return Ok(new { items = result.Items, total = result.Total, page, pageSize });
        }

        [HttpGet("kitchen/requests/{id:guid}")]
        [Authorize(Roles = "admin,manager,kitchen")]
        public async Task<IActionResult> GetKitchenRequestDetail(Guid id)
        {
            var branchId = Guid.TryParse(User.FindFirst("branchId")?.Value, out var parsed) ? parsed : (Guid?)null;
            var detail = await _kitchenService.GetRequestDetailAsync(id, User.IsInRole("admin") ? null : branchId);
            return detail == null ? NotFound() : Ok(detail);
        }

        private bool TryResolveBranch(Guid? requestedBranchId, out Guid? effectiveBranchId)
        {
            effectiveBranchId = requestedBranchId;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (string.Equals(role, "admin", StringComparison.OrdinalIgnoreCase) ||
                (role != null && !IsBranchScopedRole(role)))
                return true;

            var claimValue = User.FindFirst("branchId")?.Value;
            if (!Guid.TryParse(claimValue, out var userBranchId))
                return false;

            if (requestedBranchId.HasValue && requestedBranchId.Value != userBranchId)
                return false;

            effectiveBranchId = userBranchId;
            return true;
        }

        private bool HasBranchAccess(Guid? resourceBranchId)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (string.Equals(role, "admin", StringComparison.OrdinalIgnoreCase) ||
                (role != null && !IsBranchScopedRole(role)))
                return true;

            var claimValue = User.FindFirst("branchId")?.Value;
            return Guid.TryParse(claimValue, out var userBranchId) && resourceBranchId == userBranchId;
        }

        private static bool IsBranchScopedRole(string role) =>
            !string.IsNullOrEmpty(role) && (
            role.Equals("employee", StringComparison.OrdinalIgnoreCase) ||
            role.Equals("cashier", StringComparison.OrdinalIgnoreCase) ||
            role.Equals("kitchen", StringComparison.OrdinalIgnoreCase) ||
            role.Equals("manager", StringComparison.OrdinalIgnoreCase));

        private bool IsCustomer() => string.Equals(User.FindFirst(ClaimTypes.Role)?.Value, "customer", StringComparison.OrdinalIgnoreCase);
        private bool IsAdmin() => string.Equals(User.FindFirst(ClaimTypes.Role)?.Value, "admin", StringComparison.OrdinalIgnoreCase);
        private static bool IsTerminal(string? status) =>
            string.Equals(status, "Completed", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(status, "Hoàn thành", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(status, "Cancelled", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(status, "Đã hủy", StringComparison.OrdinalIgnoreCase);

        private bool OwnsOrder(Order order) =>
            !string.IsNullOrWhiteSpace(User.Identity?.Name) &&
            string.Equals(order.CustomerPhone?.Trim(), User.Identity!.Name.Trim(), StringComparison.OrdinalIgnoreCase);
    }
}
