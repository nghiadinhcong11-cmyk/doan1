using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Infrastructure.Persistence;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Text.Json;
using System.Globalization;

namespace RestaurantPOS.WebAPI.Controllers
{
    /// <summary>Cung cấp các endpoint chấm công nhân viên.</summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "admin,manager,cashier,kitchen,employee")]
    public class AttendanceController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AttendanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>Lấy danh sách chấm công theo ngày hoặc nhân viên.</summary>
        [HttpGet]
        public async Task<IActionResult> GetAttendances([FromQuery] DateTime? date, [FromQuery] Guid? employeeId, [FromQuery] Guid? branchId)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var userId = CurrentUserId();
            var claimBranch = CurrentUserBranchId();

            if (!IsAdmin(role) && !IsManager(role))
            {
                if (!userId.HasValue || (employeeId.HasValue && employeeId.Value != userId.Value)) return Forbid();
                employeeId = userId;
            }

            var query = from a in _context.Attendances
                        join workingBranch in _context.Branches on a.BranchId equals workingBranch.Id
                        join emp in _context.Employees on a.EmployeeId equals emp.Id
                        join homeBranch in _context.Branches on emp.BranchId equals homeBranch.Id into hb
                        from homeBranch in hb.DefaultIfEmpty()
                        select new {
                            a.Id,
                            a.EmployeeId,
                            a.EmployeeName,
                            a.CheckInTime,
                            a.CheckOutTime,
                            a.BranchId,
                            BranchName = workingBranch.Name, // Nơi quét QR
                            HomeBranchName = homeBranch != null ? homeBranch.Name : (emp.BranchName ?? "Chưa gán"), // Lấy từ bảng Branch hoặc dự phòng từ Employee
                            a.Status,
                            a.Note
                        };

            if (IsManager(role))
            {
                if (!claimBranch.HasValue) return Forbid();
                query = query.Where(a => a.BranchId == claimBranch.Value);
            }
            else if (IsAdmin(role) && branchId.HasValue)
            {
                query = query.Where(a => a.BranchId == branchId.Value);
            }

            if (date.HasValue)
            {
                var targetDate = date.Value.Date;
                query = query.Where(a => a.CheckInTime.Date == targetDate);
            }

            if (employeeId.HasValue)
                query = query.Where(a => a.EmployeeId == employeeId.Value);

            return Ok(await query.OrderByDescending(a => a.CheckInTime).ToListAsync());
        }

        private static TimeZoneInfo GetVietnamTimeZone()
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); }
            catch { return TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh"); }
        }

        /// <summary>Ghi nhận nhân viên check-in.</summary>
        [HttpPost("check-in")]
        public async Task<IActionResult> CheckIn([FromBody] CheckInRequest request)
        {
            try
            {
                var employeeId = CurrentUserId();
                if (!employeeId.HasValue) return Unauthorized();
                var employee = await _context.Employees.FindAsync(employeeId.Value);
                if (employee == null || !employee.IsActive) return Forbid();

                if (!TryValidateQr(request.QrPayload, out var qrBranchId, out var qrError))
                    return BadRequest(new { message = qrError });
                if (!employee.BranchId.HasValue || employee.BranchId.Value != qrBranchId)
                    return StatusCode(403, new { message = "Mã QR không thuộc chi nhánh của nhân viên." });

                var openAttendance = await _context.Attendances
                    .AnyAsync(a => a.EmployeeId == employeeId.Value && a.CheckOutTime == null);
                if (openAttendance) return Conflict(new { message = "Bạn đã check-in và chưa check-out." });

                var attendance = new Attendance {
                    EmployeeId = employeeId.Value,
                    EmployeeName = employee.FullName,
                    BranchId = qrBranchId
                };
                attendance.Id = Guid.NewGuid();
                attendance.CheckInTime = DateTime.UtcNow;

                var vnZone = GetVietnamTimeZone();
                var nowVn = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnZone);
                var today = nowVn.Date;
                var currentMinutes = nowVn.Hour * 60 + nowVn.Minute;

                Console.WriteLine($"[Attendance] Chấm công cho: {attendance.EmployeeName} lúc {nowVn:HH:mm:ss}");

                // Tìm lịch làm việc
                var schedules = await _context.WorkSchedules
                    .Where(s => s.EmployeeId == attendance.EmployeeId && s.Date.Year == today.Year && s.Date.Month == today.Month && s.Date.Day == today.Day)
                    .ToListAsync();

                if (schedules.Any())
                {
                    var targetSchedule = schedules
                        .Select(s => {
                            int startMin = 0;
                            if (!string.IsNullOrEmpty(s.StartTime))
                            {
                                var parts = s.StartTime.Trim().Split(':');
                                if (parts.Length >= 2 && int.TryParse(parts[0], out int h) && int.TryParse(parts[1], out int m))
                                    startMin = h * 60 + m;
                            }
                            return new { Schedule = s, StartMin = startMin, Diff = Math.Abs(currentMinutes - startMin) };
                        })
                        .OrderBy(x => x.Diff)
                        .FirstOrDefault();

                    if (targetSchedule != null && targetSchedule.Diff < 240)
                    {
                        // Cho phép đi muộn tối đa 15 phút
                        if (currentMinutes <= targetSchedule.StartMin + 15)
                        {
                            attendance.Status = "Đúng giờ";
                            attendance.Note = $"Ca {targetSchedule.Schedule.StartTime} (Đúng giờ)";
                        }
                        else
                        {
                            attendance.Status = "Muộn";
                            attendance.Note = $"Ca {targetSchedule.Schedule.StartTime} (Trễ {currentMinutes - targetSchedule.StartMin}p)";
                        }
                    }
                    else
                    {
                        attendance.Status = "Đúng giờ";
                        attendance.Note = "Làm việc ngoài ca";
                    }
                }
                else
                {
                    attendance.Status = "Đúng giờ";
                    attendance.Note = "Không có lịch (Tăng ca)";
                }

                Console.WriteLine($"[Attendance] Kết quả: {attendance.Status} - {attendance.Note}");

                _context.Attendances.Add(attendance);
                await _context.SaveChangesAsync();
                return Ok(attendance);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Attendance] Lỗi: {ex.Message}");
                return StatusCode(500, new { message = "Lỗi check-in", error = ex.Message });
            }
        }

        /// <summary>Ghi nhận nhân viên check-out.</summary>
        [HttpPatch("{id}/check-out")]
        public async Task<IActionResult> CheckOut(Guid id, [FromBody] CheckInRequest request)
        {
            var attendance = await _context.Attendances.FindAsync(id);
            if (attendance == null) return NotFound();
            var userId = CurrentUserId();
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (!IsAdmin(role) && !IsManager(role) && (!userId.HasValue || attendance.EmployeeId != userId.Value)) return Forbid();

            if (IsManager(role))
            {
                var ownBranch = CurrentUserBranchId();
                if (!ownBranch.HasValue || attendance.BranchId != ownBranch.Value) return Forbid();
            }

            if (attendance.CheckOutTime.HasValue) return Conflict(new { message = "Bản ghi này đã check-out." });
            if (!TryValidateQr(request.QrPayload, out var qrBranchId, out var qrError))
                return BadRequest(new { message = qrError });
            if (attendance.BranchId != qrBranchId) return StatusCode(403, new { message = "Mã QR không thuộc chi nhánh của bản ghi." });

            attendance.CheckOutTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(attendance);
        }

        public sealed class CheckInRequest
        {
            public string? QrPayload { get; set; }
        }

        private Guid? CurrentUserId() => Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : null;
        private Guid? CurrentUserBranchId() => Guid.TryParse(User.FindFirst("branchId")?.Value, out var id) ? id : null;
        private static bool IsAdmin(string? role) => string.Equals(role, "admin", StringComparison.OrdinalIgnoreCase);
        private static bool IsManager(string? role) => string.Equals(role, "manager", StringComparison.OrdinalIgnoreCase);

        private static bool TryValidateQr(string? payload, out Guid branchId, out string error)
        {
            branchId = Guid.Empty;
            error = "Mã QR không hợp lệ.";
            if (string.IsNullOrWhiteSpace(payload)) return false;
            try
            {
                using var json = JsonDocument.Parse(payload);
                var root = json.RootElement;
                if (!root.TryGetProperty("type", out var type) || type.GetString() != "ATTENDANCE_POINT")
                { error = "Sai loại mã QR."; return false; }
                if (!root.TryGetProperty("branchId", out var branch) || !Guid.TryParse(branch.GetString(), out branchId))
                { error = "Mã QR thiếu chi nhánh."; return false; }
                if (root.TryGetProperty("date", out var date) &&
                    DateTime.TryParseExact(date.GetString(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var qrDate) &&
                    qrDate.Date != DateTime.UtcNow.AddHours(7).Date)
                { error = "Mã QR đã hết hạn."; return false; }
                return true;
            }
            catch (JsonException) { return false; }
        }
    }
}
