using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RestaurantPOS.Application.Common.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using RestaurantPOS.Application.DTOs.Orders;
using RestaurantPOS.Application.Services;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Infrastructure.Persistence;
using System.Security.Claims;
using RestaurantPOS.WebAPI.Hubs;
using RestaurantPOS.Application.Common.Security;

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
        private readonly ILogger<OrderController>? _logger;
        private readonly INotificationService? _notificationService;

        public OrderController(IOrderService orderService, IKitchenService kitchenService, ILoyaltyService loyaltyService, ApplicationDbContext? context = null, IHubContext<KitchenHub>? hub = null, ILogger<OrderController>? logger = null, INotificationService? notificationService = null)
        {
            _orderService = orderService;
            _kitchenService = kitchenService;
            _loyaltyService = loyaltyService;
            _context = context;
            _hub = hub;
            _logger = logger;
            _notificationService = notificationService;
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
            var correlationId = HttpContext.Items.TryGetValue(OrderRequestDiagnostics.CorrelationItemKey, out var correlation)
                ? correlation as string
                : null;
            HttpContext.Items[OrderRequestDiagnostics.PhaseItemKey] = "CONTROLLER_ENTRY";
            _logger?.LogInformation(
                "ORDER_GET_DIAGNOSTIC CODE=ORDER_GET_CONTROLLER_ENTRY PHASE=CONTROLLER_ENTRY CORRELATION_ID={CorrelationId}",
                correlationId ?? "UNAVAILABLE");

            var customerPhoneFilter = customerPhone;
            if (GuestSessionContext.IsGuest(User))
            {
                if (!TryGetGuestSession(out var guest) || _context == null)
                    return Forbid();

                // A direct guest session without a table may create a takeaway order,
                // but it must never read a branch-wide order list.
                if (guest!.TableId == Guid.Empty)
                    return Forbid();

                var table = await _context.Tables.AsNoTracking()
                    .FirstOrDefaultAsync(candidate => candidate.Id == guest.TableId && candidate.BranchId == guest.BranchId && candidate.IsActive);
                if (table is null) return Forbid();

                var guestFilter = new OrderQueryFilter
                {
                    Search = search,
                    Status = status,
                    BranchId = guest.BranchId,
                    TableName = table.Name,
                    FromDate = fromDate,
                    ToDate = toDate
                };
                var guestOrders = await _orderService.GetOrdersAsync(guestFilter);
                return Ok(guestOrders.Select(MapCustomerOrder));
            }

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

            HttpContext.Items[OrderRequestDiagnostics.PhaseItemKey] = "BRANCH_SCOPE";
            _logger?.LogInformation(
                "ORDER_GET_DIAGNOSTIC CODE=ORDER_GET_BRANCH_SCOPE_RESOLVED PHASE=BRANCH_SCOPE BRANCH_SCOPE=AUTHENTICATED CORRELATION_ID={CorrelationId}",
                correlationId ?? "UNAVAILABLE");

            var filter = new OrderQueryFilter
            {
                Search = search,
                Status = status,
                CustomerPhone = customerPhoneFilter,
                ExactCustomerPhone = IsCustomer(),
                BranchId = effectiveBranchId,
                TableName = tableName,
                FromDate = fromDate,
                ToDate = toDate,
                DiagnosticCorrelationId = correlationId
            };

            HttpContext.Items[OrderRequestDiagnostics.PhaseItemKey] = "EF_QUERY_EXECUTION";
            var orders = await _orderService.GetOrdersAsync(filter);
            HttpContext.Items[OrderRequestDiagnostics.PhaseItemKey] = "RESPONSE_SERIALIZATION";
            return IsCustomer()
                ? Ok(orders.Select(MapCustomerOrder))
                : Ok(orders);
        }

        /// <summary>Returns only the fields needed by the staff pending-order notification preview.</summary>
        [HttpGet("pending-summary")]
        [Authorize(Roles = "admin,manager,cashier,employee")]
        public async Task<IActionResult> GetPendingOrderSummary([FromQuery] Guid? branchId)
        {
            if (!TryResolveBranch(branchId, out var effectiveBranchId))
                return Forbid();

            return Ok(await _orderService.GetPendingOrderSummariesAsync(effectiveBranchId));
        }

        /// <summary>Returns only order counts used by the staff overview cards.</summary>
        [HttpGet("staff-overview-counts")]
        [Authorize(Roles = "admin,manager,employee,cashier")]
        public async Task<IActionResult> GetStaffOverviewCounts([FromQuery] Guid? branchId)
        {
            if (!TryResolveBranch(branchId, out var effectiveBranchId) || !effectiveBranchId.HasValue)
                return Forbid();

            return Ok(await _orderService.GetStaffOverviewOrderCountsAsync(effectiveBranchId.Value));
        }

        /// <summary>Returns the authenticated customer's order history without line-item details.</summary>
        [HttpGet("customer-summary")]
        [Authorize(Roles = "customer")]
        public async Task<IActionResult> GetCustomerOrderSummary()
        {
            var customerPhone = User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(customerPhone))
                return Forbid();

            return Ok(await _orderService.GetCustomerOrderSummariesAsync(customerPhone.Trim()));
        }

        /// <summary>
        /// Tạo mới hoặc cập nhật thông tin đơn hàng POS
        /// </summary>
        /// <summary>Tạo đơn hàng mới.</summary>
        [HttpPost]
        [Authorize]
        [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("order-limiter")]
        public async Task<IActionResult> CreateOrder([FromBody] Order order)
        {
            try
            {
                var guestMarker = GuestSessionContext.IsGuest(User);
                var guestContextValid = TryGetGuestSession(out var guestContext);
                _logger?.LogInformation(
                    "Order create context: role={Role}, authenticated={Authenticated}, guestMarker={GuestMarker}, guestContextValid={GuestContextValid}, branchPresent={BranchPresent}, tablePresent={TablePresent}",
                    User.FindFirst(ClaimTypes.Role)?.Value ?? "none",
                    User.Identity?.IsAuthenticated == true,
                    guestMarker,
                    guestContextValid,
                    order.BranchId != Guid.Empty,
                    !string.IsNullOrWhiteSpace(order.TableName));

                if (guestMarker)
                {
                    if (!guestContextValid || _context == null || order.Id != Guid.Empty)
                    {
                        _logger?.LogWarning(
                            "Rejected guest order context: guestContextValid={GuestContextValid}, databaseContextPresent={DatabaseContextPresent}, existingOrderId={ExistingOrderId}",
                            guestContextValid,
                            _context != null,
                            order.Id != Guid.Empty);
                        return Forbid();
                    }

                    var table = guestContext!.TableId == Guid.Empty
                        ? null
                        : await _context.Tables.AsNoTracking()
                            .FirstOrDefaultAsync(candidate => candidate.Id == guestContext.TableId && candidate.BranchId == guestContext.BranchId && candidate.IsActive);

                    var branch = await _context.Branches.AsNoTracking()
                        .FirstOrDefaultAsync(candidate => candidate.Id == guestContext.BranchId && candidate.IsActive);
                    if (branch is null || (guestContext.TableId != Guid.Empty && table is null))
                    {
                        _logger?.LogWarning("Rejected guest order context: activeBranch={ActiveBranch}, tablePresent={TablePresent}", branch is not null, table is not null);
                        return Forbid();
                    }

                    // Branch and optional table come exclusively from the signed guest session.
                    // A direct session without a table is explicitly takeaway.
                    order.TableId = table?.Id;
                    order.TableName = table?.Name ?? "Mang về";
                    order.BranchId = guestContext.BranchId;
                    order.BranchName = branch.Name;
                    var isRegisteredQrCustomer = string.Equals(
                        User.FindFirst("customerAuthMethod")?.Value,
                        "registeredQr",
                        StringComparison.OrdinalIgnoreCase);
                    Customer? registeredQrCustomer = null;
                    if (isRegisteredQrCustomer &&
                        Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var registeredQrCustomerId))
                    {
                        registeredQrCustomer = await _context.Customers.AsNoTracking()
                            .FirstOrDefaultAsync(customer => customer.Id == registeredQrCustomerId && customer.IsActive);
                    }

                    order.CustomerName = registeredQrCustomer?.FullName ?? (table is null ? "Khách vãng lai" : "Khách tại bàn");
                    order.CustomerId = registeredQrCustomer?.Id;
                    order.CustomerPhone = registeredQrCustomer?.PhoneNumber;
                    order.CustomerEmail = null;
                    _logger?.LogInformation("Resolved guest order context: branchPresent={BranchPresent}, tablePresent={TablePresent}", true, table is not null);
                }

                if (order.Id != Guid.Empty)
                {
                    var existingOrder = await _orderService.GetOrderByIdAsync(order.Id);
                    if (existingOrder == null) return NotFound();
                    if (!HasBranchAccess(existingOrder.BranchId)) return Forbid();
                    if (order.BranchId != existingOrder.BranchId) return Forbid();
                }

                if (!HasBranchAccess(order.BranchId))
                    return Forbid();

                if (IsCustomer() && !guestMarker)
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
                else if (guestMarker)
                {
                    // Guest claims are not member identities and cannot earn points or
                    // attach client-supplied customer contact data to the order.
                    order.CustomerId = null;
                    order.CustomerPhone = null;
                    order.CustomerEmail = null;
                }
                else if (User.Identity?.IsAuthenticated == true && !string.IsNullOrWhiteSpace(order.CustomerPhone) && _context != null)
                {
                    // Resolve the member from the server-side phone record. A client
                    // supplied CustomerId is never accepted as the identity source.
                    var normalizedPhone = new string(order.CustomerPhone.Where(char.IsDigit).ToArray());
                    var customer = await _context.Customers.AsNoTracking()
                        .FirstOrDefaultAsync(candidate => candidate.PhoneNumber == normalizedPhone && candidate.IsActive);
                    order.CustomerId = customer?.Id;
                    order.CustomerPhone = customer?.PhoneNumber ?? normalizedPhone;
                    if (customer != null) order.CustomerName = customer.FullName;
                }
                else if (User.Identity == null || !User.Identity.IsAuthenticated)
                {
                    // Anonymous users cannot set CustomerId
                    order.CustomerId = null;
                }
                else if (string.IsNullOrWhiteSpace(order.CustomerPhone))
                {
                    // Staff must explicitly link a member by phone; never trust a
                    // standalone CustomerId from the request body.
                    order.CustomerId = null;
                }

                // Customer orders must carry a branch resolved from trusted guest
                // context or an authenticated branch-scoped flow before reaching
                // the financial/order persistence service.
                if (order.BranchId == Guid.Empty)
                {
                    _logger?.LogWarning(
                        "Rejected order without trusted branch context: role={Role}, authenticated={Authenticated}, guestMarker={GuestMarker}, guestContextValid={GuestContextValid}, tablePresent={TablePresent}",
                        User.FindFirst(ClaimTypes.Role)?.Value ?? "none",
                        User.Identity?.IsAuthenticated == true,
                        guestMarker,
                        guestContextValid,
                        !string.IsNullOrWhiteSpace(order.TableName));
                    return BadRequest("Order branch context is required.");
                }

                // A registered customer may choose an active public branch from
                // the normal menu. The client-provided ID is only a request for
                // context; the server resolves the branch and rejects inactive
                // or unknown branches before persistence.
                if (IsCustomer() && !guestMarker)
                {
                    if (_context == null)
                        return BadRequest("A valid branch context is required.");

                    var customerBranch = await _context.Branches.AsNoTracking()
                        .FirstOrDefaultAsync(candidate => candidate.Id == order.BranchId && candidate.IsActive);
                    if (customerBranch == null)
                        return BadRequest("A valid active branch is required.");

                    order.BranchName = customerBranch.Name;
                }

                if (order.Details == null || order.Details.Count == 0 || order.Details.Any(d => d.Quantity <= 0))
                    return BadRequest("Order must contain items with a quantity greater than zero.");

                var productIds = order.Details.Where(d => d.ProductId.HasValue).Select(d => d.ProductId!.Value).Distinct().ToList();
                var toppingIds = order.Details.Where(d => d.ToppingId.HasValue).Select(d => d.ToppingId!.Value).Distinct().ToList();
                if (_context != null && productIds.Count > 0 && await _context.Products.CountAsync(p => productIds.Contains(p.Id)) != productIds.Count)
                    return BadRequest("One or more products do not exist.");
                if (_context != null && toppingIds.Count > 0 && await _context.Toppings.CountAsync(t => toppingIds.Contains(t.Id)) != toppingIds.Count)
                    return BadRequest("One or more toppings do not exist.");

                var deferAvailabilityValidation = guestMarker || IsCustomer();
                var existingAcceptedTableOrderId = Guid.Empty;
                if (deferAvailabilityValidation && order.Id == Guid.Empty && _context != null &&
                    (order.TableId.HasValue || !string.IsNullOrWhiteSpace(order.TableName)))
                {
                    existingAcceptedTableOrderId = await _context.Orders
                        .AsNoTracking()
                        .Where(candidate => candidate.BranchId == order.BranchId &&
                            candidate.Status == "Đang xử lý" &&
                            candidate.CreatedBy != null && candidate.CreatedBy != "" &&
                            (order.TableId.HasValue
                                ? (candidate.TableId == order.TableId.Value ||
                                   (candidate.TableId == null && candidate.TableName == order.TableName))
                                : candidate.TableName == order.TableName))
                        .Select(candidate => candidate.Id)
                        .FirstOrDefaultAsync();
                }

                var result = await _orderService.CreateOrUpdateOrderAsync(order, deferAvailabilityValidation);
                if (string.Equals(result.Status, "Đang xử lý", StringComparison.Ordinal) && string.IsNullOrWhiteSpace(result.CreatedBy))
                    await NotifyPendingOrderChangedAsync(result.BranchId);

                if (existingAcceptedTableOrderId != Guid.Empty && result.Id == existingAcceptedTableOrderId)
                {
                    try
                    {
                        if (_notificationService != null)
                        {
                            await _notificationService.CreateAsync(new Notification
                            {
                                Type = "ORDER_FOLLOW_UP",
                                Title = "Khách gọi thêm món",
                                Message = $"{result.TableName ?? "Bàn khách"} vừa gọi thêm món, cần kiểm tra để gửi bếp.",
                                EntityType = "Order",
                                EntityId = result.Id,
                                Route = "/pos",
                                BranchId = result.BranchId
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(ex, "Could not create follow-up order notification for {OrderId}", result.Id);
                    }
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi Database: " + ex.Message });
            }
        }

        //hàm Mã Endpoint tiếp nhận đơn web và xác định phạm vi chi nhánh của người dùng.
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
            if (result.Result == WebOrderAcceptResult.Accepted)
            {
                await NotifyPendingOrderChangedAsync(result.Order?.BranchId);
            }

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
                WebOrderAcceptResult.UnavailableProducts => Conflict(new { success = false, message = "Một hoặc nhiều món không còn khả dụng tại chi nhánh. Vui lòng điều chỉnh đơn trước khi tiếp nhận." }),
                _ => Conflict(new { success = false, message = "Đơn này đã được nhân viên khác tiếp nhận hoặc không còn chờ xử lý." })
            };
        }

        [HttpPost("{id:guid}/transfer-table")]
        [Authorize(Roles = "admin,manager,employee,cashier")]
        public async Task<IActionResult> TransferOrder(Guid id, [FromBody] TransferOrderDto request)
        {
            if (request is null || request.TargetTableId == Guid.Empty)
                return BadRequest(new { success = false, message = "Bàn đích không hợp lệ." });

            var existingOrder = await _orderService.GetOrderByIdAsync(id);
            if (existingOrder is null)
                return NotFound();
            if (!HasBranchAccess(existingOrder.BranchId))
                return Forbid();

            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
            Guid? allowedBranchId = IsBranchScopedRole(role)
                ? (Guid.TryParse(User.FindFirst("branchId")?.Value, out var branchId) ? branchId : null)
                : null;
            if (IsBranchScopedRole(role) && !allowedBranchId.HasValue)
                return Forbid();

            var result = await _orderService.TransferOrderAsync(id, request.TargetTableId, allowedBranchId);
            if (!result.Success)
                return Conflict(new { success = false, message = result.Message });

            return Ok(new
            {
                success = true,
                message = result.Message,
                order = result.Order
            });
        }

        /// <summary>
        /// Gửi các món mới gọi xuống màn hình Nhà bếp (tự động tính chênh lệch SentQuantity)
        /// </summary>
        /// <summary>Gửi đơn hàng tới bếp để chế biến.</summary>
        [HttpPost("{id}/send-to-kitchen")]
        [Authorize(Roles = "admin,manager,employee,cashier,kitchen")]
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
        [Authorize(Roles = "admin,manager,employee,cashier,kitchen")]
        public async Task<IActionResult> GetKitchenStatus(Guid id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null) return NotFound();
            if (!HasBranchAccess(order.BranchId)) return Forbid();

            return Ok(await _kitchenService.GetOrderKitchenStatusesAsync(id));
        }

        [HttpPost("{id:guid}/payment")]
        [Authorize(Roles = "admin,manager,cashier")]
        //kiem tra thanh toan
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

            var result = await _orderService.PayOrderAsync(
                id,
                request.Amount,
                request.PaymentMethod.Trim(),
                request.CustomerPhone,
                request.CustomerName,
                request.CreateMember);
            if (result.Result == PaymentAttemptResult.Paid && result.Order != null && _hub != null)
            {
                var paymentGroups = PaymentEventRouting.Groups(result.Order.BranchId);
                if (paymentGroups.Count > 0)
                {
                    await _hub.Clients.Groups(paymentGroups).SendAsync("PaymentCompleted", new
                    {
                        orderId = result.Order.Id,
                        invoiceCode = result.Order.InvoiceCode,
                        status = result.Order.Status,
                        paidAmount = result.Order.PaidAmount,
                        paymentMethod = result.Order.PaymentMethod,
                        branchId = result.Order.BranchId
                    });
                }
            }
            return result.Result switch
            {
                PaymentAttemptResult.Paid => Ok(result.Order),
                PaymentAttemptResult.NotFound => NotFound(),
                PaymentAttemptResult.AlreadyPaid => Conflict(new { message = "Order has already been paid." }),
                PaymentAttemptResult.MemberAlreadyRewarded => Conflict(new { message = "Đơn hàng đã được cộng điểm và không thể đổi thành viên nhận điểm." }),
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
        [Authorize(Roles = "admin,manager,employee,cashier")]
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

            if (GuestSessionContext.IsGuest(User) && !await GuestOwnsOrderAsync(order))
                return Forbid();
            if (!GuestSessionContext.IsGuest(User) && IsCustomer() && !OwnsOrder(order))
                return Forbid();
            if (!IsCustomer() && !HasBranchAccess(order.BranchId))
                return Forbid();

            return IsCustomer() || GuestSessionContext.IsGuest(User)
                ? Ok(MapCustomerOrder(order))
                : Ok(order);
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

            OrderRequest? request;
            try
            {
                request = await _kitchenService.UpdateRequestStatusAsync(id, status, User.FindFirst("fullName")?.Value ?? User.Identity?.Name);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
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
            if (!User.IsInRole("admin") && !branchId.HasValue) return Forbid();
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
        private static bool IsTerminal(string? status) => OrderStatusPolicy.IsTerminal(status);

        private bool OwnsOrder(Order order) =>
            !string.IsNullOrWhiteSpace(User.Identity?.Name) &&
            string.Equals(order.CustomerPhone?.Trim(), User.Identity!.Name.Trim(), StringComparison.OrdinalIgnoreCase);

        private bool TryGetGuestSession(out GuestSessionContext? context) =>
            GuestSessionContext.TryCreate(User, out context);

        private static CustomerOrderDto MapCustomerOrder(Order order) => new()
        {
            Id = order.Id,
            InvoiceCode = order.InvoiceCode,
            CreatedAt = order.CreatedAt,
            PaymentAt = order.PaymentAt,
            Status = order.Status,
            PaymentMethod = order.PaymentMethod,
            TotalAmount = order.TotalAmount,
            PaidAmount = order.PaidAmount,
            SubTotal = order.SubTotal,
            Discount = order.Discount,
            VatAmount = order.VatAmount,
            VatPercent = order.VatPercent,
            ServiceFeeAmount = order.ServiceFeeAmount,
            ServiceFeePercent = order.ServiceFeePercent,
            Note = order.Note,
            TableName = order.TableName,
            BranchId = order.BranchId,
            BranchName = order.BranchName,
            Details = order.Details.Select(detail => new CustomerOrderDetailDto
            {
                ProductName = detail.ProductName,
                Quantity = detail.Quantity,
                UnitPrice = detail.UnitPrice,
                Options = detail.Options
            }).ToList()
        };

        private async Task<bool> GuestOwnsOrderAsync(Order order)
        {
            if (!TryGetGuestSession(out var guest) || _context == null || order.BranchId != guest!.BranchId)
                return false;

            var table = await _context.Tables.AsNoTracking()
                .FirstOrDefaultAsync(candidate => candidate.Id == guest.TableId && candidate.BranchId == guest.BranchId && candidate.IsActive);
            return table is not null && string.Equals(order.TableName, table.Name, StringComparison.Ordinal);
        }

        private async Task NotifyPendingOrderChangedAsync(Guid? branchId)
        {
            if (_hub == null) return;

            if (!branchId.HasValue || branchId.Value == Guid.Empty)
            {
                _logger?.LogError("Skipped pending-order realtime notification because the order has no valid branch.");
                return;
            }

            var groups = new List<string> { "kitchen-admin" };
            foreach (var role in new[] { "manager", "employee", "cashier" })
                groups.Add($"branch:{branchId.Value}:role:{role}");

            // This event only invalidates the client-side pending-order query. It does
            // not carry a count, so the server remains the source of truth.
            try
            {
                await _hub.Clients.Groups(groups).SendAsync("PendingOrderChanged", new { branchId });
            }
            catch (Exception ex)
            {
                // Realtime invalidation must not turn a successful order mutation into
                // an API failure. Clients retain the periodic refresh fallback.
                _logger?.LogWarning(ex, "Could not publish pending-order invalidation for branch {BranchId}", branchId);
            }
        }
    }
}
