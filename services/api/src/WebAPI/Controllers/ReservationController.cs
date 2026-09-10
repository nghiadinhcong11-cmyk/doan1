using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RestaurantPOS.Application.Services;
using RestaurantPOS.Domain.Entities;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RestaurantPOS.WebAPI.Controllers
{
    /// <summary>Cung cấp các endpoint quản lý đặt bàn.</summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationController : ControllerBase
    {
        private readonly IReservationService _reservationService;

        public ReservationController(IReservationService reservationService)
        {
            _reservationService = reservationService;
        }

        /// <summary>Lấy danh sách đặt bàn theo bộ lọc.</summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetReservations(
            [FromQuery] Guid? branchId,
            [FromQuery] string? status,
            [FromQuery] string? customerPhone,
            [FromQuery] string? fromDate,
            [FromQuery] string? toDate)
        {
            var effectiveCustomerPhone = customerPhone;

            if (IsCustomer())
            {
                var jwtPhone = User.Identity?.Name;
                if (string.IsNullOrWhiteSpace(jwtPhone)) return Forbid();
                effectiveCustomerPhone = jwtPhone;
            }
            else
            {
                if (!TryResolveBranch(branchId, out var resolvedBranchId)) return Forbid();
                branchId = resolvedBranchId;
            }

            var filter = new ReservationQueryFilter
            {
                BranchId = branchId,
                Status = status,
                CustomerPhone = effectiveCustomerPhone,
                FromDate = fromDate,
                ToDate = toDate
            };

            var reservations = await _reservationService.GetReservationsAsync(filter);
            return Ok(reservations);
        }

        /// <summary>Lấy thông tin chi tiết một lịch hẹn.</summary>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetReservation(Guid id)
        {
            Guid? authorizedBranchId = null;
            string? authorizedCustomerPhone = null;

            if (IsCustomer())
            {
                authorizedCustomerPhone = User.Identity?.Name;
                if (string.IsNullOrWhiteSpace(authorizedCustomerPhone)) return Forbid();
            }
            else if (!IsAdmin())
            {
                authorizedBranchId = GetUserBranchId();
                if (authorizedBranchId == null) return Forbid();
            }

            var res = await _reservationService.GetByIdAsync(id, authorizedBranchId, authorizedCustomerPhone);
            if (res == null) return NotFound();

            return Ok(res);
        }

        /// <summary>Tạo yêu cầu đặt bàn mới (Công khai cho khách vãng lai).</summary>
        [HttpPost]
        public async Task<IActionResult> CreateReservation([FromBody] Reservation reservation)
        {
            try {
                // Nếu khách hàng đã đăng nhập, tự động điền thông tin và kiểm tra phone
                if (IsCustomer())
                {
                    var jwtPhone = User.Identity?.Name;
                    if (!string.IsNullOrWhiteSpace(jwtPhone))
                        reservation.CustomerPhone = jwtPhone;
                }

                // Không cho phép khách vãng lai tự gán trạng thái khác "Pending"
                if (!IsStaff())
                {
                    reservation.Status = "Pending";
                }
                else if (!IsAdmin())
                {
                    // Staff (Manager/Employee) can only create reservations for their own branch
                    var userBranchId = GetUserBranchId();
                    if (userBranchId == null) return Forbid();

                    if (reservation.BranchId.HasValue && reservation.BranchId != userBranchId)
                        return Forbid();

                    reservation.BranchId = userBranchId;
                }

                var result = await _reservationService.CreateReservationAsync(reservation);
                return Ok(result);
            } catch (InvalidOperationException ex) {
                return BadRequest(new { message = ex.Message });
            } catch (Exception ex) {
                return BadRequest(new { message = "Lỗi khi tạo lịch hẹn: " + ex.Message });
            }
        }

        /// <summary>Cập nhật toàn bộ thông tin lịch hẹn.</summary>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateReservation(Guid id, [FromBody] Reservation update)
        {
            try {
                Guid? authorizedBranchId = null;
                string? authorizedCustomerPhone = null;

                if (IsCustomer())
                {
                    authorizedCustomerPhone = User.Identity?.Name;
                    if (string.IsNullOrWhiteSpace(authorizedCustomerPhone)) return Forbid();
                }
                else if (!IsAdmin())
                {
                    authorizedBranchId = GetUserBranchId();
                    if (authorizedBranchId == null) return Forbid();
                }

                var result = await _reservationService.UpdateReservationAsync(id, update, authorizedBranchId, authorizedCustomerPhone);
                if (result == null) return NotFound();

                return Ok(result);
            } catch (InvalidOperationException ex) {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>Cập nhật trạng thái đặt bàn.</summary>
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "admin,manager,employee,cashier")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] string status)
        {
            var branchId = GetUserBranchId();
            if (!IsAdmin() && branchId == null) return Forbid();

            var success = await _reservationService.UpdateStatusAsync(id, status, IsAdmin() ? null : branchId);

            if (!success) return NotFound();
            return Ok(new { id, status });
        }

        /// <summary>Hủy hoặc xóa đặt bàn theo mã định danh.</summary>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteReservation(Guid id)
        {
            Guid? authorizedBranchId = null;
            string? authorizedCustomerPhone = null;

            if (IsCustomer())
            {
                authorizedCustomerPhone = User.Identity?.Name;
                if (string.IsNullOrWhiteSpace(authorizedCustomerPhone)) return Forbid();

                // Khách hàng có thể "hủy" lịch của mình bằng cách xóa nếu nghiệp vụ cho phép,
                // hoặc ta có thể giới hạn chỉ staff mới được xóa vĩnh viễn.
                // Ở đây ta cho phép customer xóa lịch của chính mình.
            }
            else if (!IsAdmin())
            {
                authorizedBranchId = GetUserBranchId();
                if (authorizedBranchId == null) return Forbid();
            }

            var success = await _reservationService.DeleteReservationAsync(id, authorizedBranchId, authorizedCustomerPhone);

            if (!success) return NotFound();
            return NoContent();
        }

        private bool TryResolveBranch(Guid? requestedBranchId, out Guid? effectiveBranchId)
        {
            effectiveBranchId = requestedBranchId;
            if (IsAdmin()) return true;

            var userBranchId = GetUserBranchId();
            if (userBranchId == null) return false;

            if (requestedBranchId.HasValue && requestedBranchId.Value != userBranchId.Value)
                return false;

            effectiveBranchId = userBranchId;
            return true;
        }

        private Guid? GetUserBranchId()
        {
            var claimValue = User.FindFirst("branchId")?.Value;
            return Guid.TryParse(claimValue, out var userBranchId) ? userBranchId : null;
        }

        private bool IsAdmin() => User.IsInRole("admin");
        private bool IsCustomer() => User.IsInRole("customer");
        private bool IsStaff() => User.Identity != null && User.Identity.IsAuthenticated && !IsCustomer();
    }
}
