using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

namespace RestaurantPOS.WebAPI.Controllers
{
    /// <summary>Cung cấp các endpoint quản lý lịch làm việc.</summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "admin,cashier,kitchen,employee")]
    public class WorkScheduleController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public WorkScheduleController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>Lấy lịch làm việc theo khoảng thời gian và nhân viên.</summary>
        [HttpGet]
        public async Task<IActionResult> GetSchedules([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] Guid? employeeId, [FromQuery] Guid? branchId)
        {
            try {
                var role = User.FindFirst(ClaimTypes.Role)?.Value;
                var userId = CurrentUserId();
                var claimBranch = CurrentUserBranchId();

                if (!string.Equals(role, "admin", StringComparison.OrdinalIgnoreCase) && !string.Equals(role, "manager", StringComparison.OrdinalIgnoreCase))
                {
                    if (!userId.HasValue || (employeeId.HasValue && employeeId.Value != userId.Value)) return Forbid();
                    employeeId = userId;
                }
                var query = _context.WorkSchedules.AsNoTracking();

                if (string.Equals(role, "manager", StringComparison.OrdinalIgnoreCase))
                {
                    if (!claimBranch.HasValue) return Forbid();
                    query = query.Where(s => s.BranchId == claimBranch.Value);
                }
                else if (string.Equals(role, "admin", StringComparison.OrdinalIgnoreCase) && branchId.HasValue)
                {
                    query = query.Where(s => s.BranchId == branchId.Value);
                }

                if (startDate.HasValue)
                    query = query.Where(s => s.Date >= startDate.Value.Date);

                if (endDate.HasValue)
                    query = query.Where(s => s.Date <= endDate.Value.Date);

                if (employeeId.HasValue)
                    query = query.Where(s => s.EmployeeId == employeeId.Value);

                return Ok(await query.OrderBy(s => s.Date).ToListAsync());
            }
            catch (Exception ex) {
                return StatusCode(500, new { message = "Không thể lấy lịch làm", error = ex.Message });
            }
        }

        /// <summary>Tạo lịch làm việc mới.</summary>
        [HttpPost]
        public async Task<IActionResult> CreateSchedule(WorkSchedule schedule)
        {
            if (!IsAdmin() && !IsManager()) return Forbid();
            if (IsManager())
            {
                // Manager chỉ được tạo lịch cho chi nhánh mình
                if (schedule.BranchId != CurrentUserBranchId()) return Forbid();
            }
            try
            {
                schedule.Id = Guid.NewGuid();
                schedule.Date = DateTime.SpecifyKind(schedule.Date.Date, DateTimeKind.Utc);

                _context.WorkSchedules.Add(schedule);
                await _context.SaveChangesAsync();
                return Ok(schedule);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new {
                    message = "Lỗi khi lưu lịch làm việc",
                    detail = ex.Message,
                    inner = ex.InnerException?.Message
                });
            }
        }

        /// <summary>Xóa lịch làm việc theo mã định danh.</summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSchedule(Guid id)
        {
            if (!IsAdmin() && !IsManager()) return Forbid();
            var schedule = await _context.WorkSchedules.FindAsync(id);
            if (schedule == null) return NotFound();

            if (IsManager() && schedule.BranchId != CurrentUserBranchId()) return Forbid();

            _context.WorkSchedules.Remove(schedule);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool IsAdmin() => string.Equals(User.FindFirst(ClaimTypes.Role)?.Value, "admin", StringComparison.OrdinalIgnoreCase);
        private bool IsManager() => string.Equals(User.FindFirst(ClaimTypes.Role)?.Value, "manager", StringComparison.OrdinalIgnoreCase);
        private Guid? CurrentUserId() => Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : null;
        private Guid? CurrentUserBranchId() => Guid.TryParse(User.FindFirst("branchId")?.Value, out var id) ? id : null;
    }
}
