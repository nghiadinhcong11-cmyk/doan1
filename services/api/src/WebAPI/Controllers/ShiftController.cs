using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using RestaurantPOS.Infrastructure.Persistence;
using RestaurantPOS.Domain.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Claims;

namespace RestaurantPOS.WebAPI.Controllers
{
    /// <summary>Cung cấp các endpoint mở, chốt và tra cứu ca làm việc.</summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "admin,manager,cashier")]
    public class ShiftController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ShiftController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>Lấy ca đang mở của nhân viên.</summary>
        [HttpGet("current/{employeeId}")]
        public async Task<IActionResult> GetCurrentShift(Guid employeeId)
        {
            if (!IsAdmin() && !IsManager() && CurrentUserId() != employeeId) return Forbid();
            var shift = await _context.Shifts
                .Where(s => s.EmployeeId == employeeId && s.Status == "Open")
                .OrderByDescending(s => s.StartTime)
                .FirstOrDefaultAsync();
            
            if (shift == null) return NotFound(new { message = "Chưa mở ca làm việc" });
            if (!HasBranchAccess(shift.BranchId)) return Forbid();
            return Ok(shift);
        }

        /// <summary>Dữ liệu mở ca làm việc.</summary>
        public class OpenShiftRequest
        {
            public Guid EmployeeId { get; set; }
            public string EmployeeName { get; set; }
            public Guid BranchId { get; set; }
            public string? BranchName { get; set; }
            public decimal StartingCash { get; set; }
        }

        /// <summary>Mở ca làm việc cho nhân viên.</summary>
        [HttpPost("open")]
        public async Task<IActionResult> OpenShift([FromBody] OpenShiftRequest request)
        {
            try {
                if (!IsAdmin() && !IsManager())
                {
                    var userId = CurrentUserId();
                    if (!userId.HasValue || userId.Value != request.EmployeeId) return Forbid();
                    if (!Guid.TryParse(User.FindFirst("branchId")?.Value, out var userBranchId) || userBranchId != request.BranchId) return Forbid();
                }
                if (!HasBranchAccess(request.BranchId)) return Forbid();

                var employee = await _context.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.Id == request.EmployeeId);
                if (employee == null) return BadRequest(new { message = "Employee does not exist." });
                if (employee.BranchId != request.BranchId) return Forbid();

                if (await _context.Shifts.AnyAsync(s => s.EmployeeId == request.EmployeeId && s.Status == "Open"))
                    return Conflict(new { message = "Nhân viên đang có một ca mở." });

                var shift = new Shift {
                    Id = Guid.NewGuid(),
                    EmployeeId = request.EmployeeId,
                    EmployeeName = employee.FullName,
                    BranchId = request.BranchId,
                    BranchName = request.BranchName,
                    StartTime = DateTime.UtcNow,
                    StartingCash = request.StartingCash,
                    Status = "Open",
                    Note = ""
                };

                _context.Shifts.Add(shift);
                await _context.SaveChangesAsync();
                return Ok(shift);
            } catch (Exception ex) {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>Dữ liệu chốt ca làm việc.</summary>
        public class CloseShiftRequest
        {
            public decimal EndingCash { get; set; }
            public string? Note { get; set; }
        }

        /// <summary>Chốt một ca đang mở.</summary>
        [HttpPost("close/{id}")]
        public async Task<IActionResult> CloseShift(Guid id, [FromBody] CloseShiftRequest request)
        {
            var shift = await _context.Shifts.FindAsync(id);
            if (shift == null) return NotFound();
            if (!HasBranchAccess(shift.BranchId)) return Forbid();
            if (!IsAdmin() && !IsManager() && CurrentUserId() != shift.EmployeeId) return Forbid();
            if (shift.Status != "Open") return Conflict(new { message = "Ca này đã được chốt." });

            // Lấy danh sách các đơn hàng hoàn thành trong ca này
            var shiftOrders = await _context.Orders
                .Where(o => o.BranchId == shift.BranchId && o.CreatedAt >= shift.StartTime && o.Status == "Hoàn thành" && o.CreatedBy == shift.EmployeeName)
                .ToListAsync();

            var cashRev = shiftOrders.Where(o => o.PaymentMethod == "Tiền mặt").Sum(o => (double)o.PaidAmount);
            var transferRev = shiftOrders.Where(o => o.PaymentMethod == "Chuyển khoản").Sum(o => (double)o.PaidAmount);
            var totalRev = cashRev + transferRev;

            shift.EndTime = DateTime.UtcNow;
            shift.EndingCash = request.EndingCash;
            shift.CashRevenue = (decimal)cashRev;
            shift.TransferRevenue = (decimal)transferRev;
            shift.TotalRevenue = (decimal)totalRev;
            shift.Status = "Closed";
            shift.Note = request.Note ?? "";

            await _context.SaveChangesAsync();
            return Ok(shift);
        }

        /// <summary>Lấy lịch sử ca theo bộ lọc.</summary>
        [HttpGet]
        public async Task<IActionResult> GetShifts([FromQuery] Guid? employeeId, [FromQuery] Guid? branchId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            if (!IsAdmin() && !IsManager()) employeeId = CurrentUserId();
            if (!TryResolveBranch(branchId, out var effectiveBranchId)) return Forbid();
            var query = _context.Shifts.AsQueryable();

            if (employeeId.HasValue)
                query = query.Where(s => s.EmployeeId == employeeId.Value);

            if (effectiveBranchId.HasValue)
                query = query.Where(s => s.BranchId == effectiveBranchId.Value);

            if (fromDate.HasValue)
            {
                var from = DateTime.SpecifyKind(fromDate.Value, DateTimeKind.Utc);
                query = query.Where(s => s.StartTime >= from);
            }

            if (toDate.HasValue)
            {
                var to = DateTime.SpecifyKind(toDate.Value.AddDays(1), DateTimeKind.Utc);
                query = query.Where(s => s.StartTime < to);
            }

            return Ok(await query.OrderByDescending(s => s.StartTime).ToListAsync());
        }

        private bool TryResolveBranch(Guid? requestedBranchId, out Guid? effectiveBranchId)
        {
            effectiveBranchId = requestedBranchId;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (string.Equals(role, "admin", StringComparison.OrdinalIgnoreCase)) return true;

            var claimValue = User.FindFirst("branchId")?.Value;
            if (!Guid.TryParse(claimValue, out var userBranchId)) return false;
            if (requestedBranchId.HasValue && requestedBranchId.Value != userBranchId) return false;
            effectiveBranchId = userBranchId;
            return true;
        }

        private bool HasBranchAccess(Guid resourceBranchId)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (string.Equals(role, "admin", StringComparison.OrdinalIgnoreCase)) return true;
            return Guid.TryParse(User.FindFirst("branchId")?.Value, out var userBranchId) && userBranchId == resourceBranchId;
        }

        private bool IsAdmin() => string.Equals(User.FindFirst(ClaimTypes.Role)?.Value, "admin", StringComparison.OrdinalIgnoreCase);
        private bool IsManager() => string.Equals(User.FindFirst(ClaimTypes.Role)?.Value, "manager", StringComparison.OrdinalIgnoreCase);
        private Guid? CurrentUserId() => Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : null;
    }
}
