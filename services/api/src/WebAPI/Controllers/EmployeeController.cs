using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Infrastructure.Persistence;
using RestaurantPOS.Application.Services;
using RestaurantPOS.Application.Common.Security;
using RestaurantPOS.Application.DTOs;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Text.Json;

namespace RestaurantPOS.WebAPI.Controllers
{
    /// <summary>Cung cấp các endpoint quản lý nhân viên.</summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EmployeeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<Employee> _passwordHasher;
        private readonly IEmployeeService _employeeService;
        private readonly IDashboardService? _dashboardService;

        public EmployeeController(ApplicationDbContext context, IPasswordHasher<Employee> passwordHasher, IEmployeeService employeeService, IDashboardService? dashboardService = null)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _employeeService = employeeService;
            _dashboardService = dashboardService;
        }

        /// <summary>Lấy danh sách nhân viên theo các bộ lọc.</summary>
        [HttpGet]
        [Authorize(Roles = "admin,manager")]
        public async Task<IActionResult> GetEmployees([FromQuery] string? search, [FromQuery] bool? isActive, [FromQuery] string? department, [FromQuery] string? position, [FromQuery] Guid? branchId)
        {
            if (!IsAdmin() && !IsManager()) return Forbid();

            var query = _context.Employees.AsNoTracking().AsQueryable();

            if (IsManager())
            {
                var ownBranch = CurrentUserBranchId();
                if (ownBranch == null) return Forbid();
                query = query.Where(e => e.BranchId == ownBranch.Value);
            }
            else if (branchId.HasValue)
            {
                query = query.Where(e => e.BranchId == branchId.Value);
            }

            if (!string.IsNullOrEmpty(search))
                query = query.Where(e => e.FullName.Contains(search) || e.EmployeeCode.Contains(search) || (e.PhoneNumber != null && e.PhoneNumber.Contains(search)));

            if (isActive.HasValue)
                query = query.Where(e => e.IsActive == isActive.Value);

            if (!string.IsNullOrEmpty(department))
                query = query.Where(e => e.Department == department);

            if (!string.IsNullOrEmpty(position))
                query = query.Where(e => e.Position == position);

            // Keep the established entity-shaped response for existing Admin Web consumers,
            // but never materialize the password hash for a read-only list response.
            var employees = await query
                .OrderByDescending(e => e.CreatedAt)
                .Select(e => new Employee
                {
                    Id = e.Id,
                    EmployeeCode = e.EmployeeCode,
                    EmployeeType = e.EmployeeType,
                    FullName = e.FullName,
                    PhoneNumber = e.PhoneNumber,
                    Position = e.Position,
                    Department = e.Department,
                    BasicSalary = e.BasicSalary,
                    BranchId = e.BranchId,
                    BranchName = e.BranchName,
                    CitizenId = e.CitizenId,
                    BirthDate = e.BirthDate,
                    Gender = e.Gender,
                    Address = e.Address,
                    StartDate = e.StartDate,
                    IsActive = e.IsActive,
                    Role = e.Role,
                    Note = e.Note,
                    Username = e.Username,
                    CreatedAt = e.CreatedAt
                })
                .ToListAsync();
            return Ok(employees);
        }

        /// <summary>Lấy thông tin một nhân viên.</summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "admin,manager,employee,cashier,kitchen")]
        public async Task<IActionResult> GetEmployee(Guid id)
        {
            if (!IsEmployeeRole()) return Forbid();
            var emp = await _context.Employees.FindAsync(id);
            if (emp == null) return NotFound();

            if (!IsAdmin() && CurrentUserId() != id)
            {
                var ownBranch = CurrentUserBranchId();
                if (!IsManager() || !ownBranch.HasValue || emp.BranchId != ownBranch.Value)
                    return Forbid();
            }

            emp.Password = null;
            return Ok(emp);
        }

        /// <summary>Tạo nhân viên mới.</summary>
        [HttpPost]
        [Authorize(Roles = "admin,manager")]
        public async Task<IActionResult> CreateEmployee(Employee employee)
        {
            if (!IsAdmin() && !IsManager()) return Forbid();

            if (!RoleNames.TryNormalizeStaffRole(employee.Role, out var normalizedRole))
                return BadRequest(new { message = "Role nhân viên không hợp lệ." });
            employee.Role = normalizedRole;

            // Manager chỉ được tạo nhân viên cho chi nhánh mình
            if (IsManager())
            {
                var ownBranch = CurrentUserBranchId();
                if (!ownBranch.HasValue) return Forbid();
                employee.BranchId = ownBranch.Value;
                // Managers may staff their own branch, but cannot create another management principal.
                if (IsManagementRole(employee.Role)) employee.Role = "employee";
            }

            if (employee.BranchId.HasValue && !await IsActiveBranchAsync(employee.BranchId.Value))
                return BadRequest(new { message = "Chi nhánh không tồn tại hoặc đã ngừng hoạt động." });

            try
            {
                employee.Id = Guid.NewGuid();
                employee.CreatedAt = DateTime.UtcNow;

                // Ép kiểu sang UTC cho PostgreSQL
                employee.StartDate = DateTime.SpecifyKind(employee.StartDate, DateTimeKind.Utc);
                if (employee.BirthDate.HasValue)
                {
                    employee.BirthDate = DateTime.SpecifyKind(employee.BirthDate.Value, DateTimeKind.Utc);
                }

                if (string.IsNullOrEmpty(employee.EmployeeCode))
                {
                    var count = await _context.Employees.CountAsync();
                    employee.EmployeeCode = $"NV{count + 1:D5}";
                }

                if (!string.IsNullOrEmpty(employee.Password))
                {
                    var passwordError = PasswordPolicy.Validate(employee.Password);
                    if (passwordError != null) return BadRequest(new { message = passwordError });
                    employee.Password = _passwordHasher.HashPassword(employee, employee.Password);
                }

                _context.Employees.Add(employee);
                await _context.SaveChangesAsync();
                _dashboardService?.InvalidateSummaryCache();

                employee.Password = null;
                return Ok(employee);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, inner = ex.InnerException?.Message });
            }
        }

        /// <summary>Cập nhật thông tin nhân viên.</summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "admin,manager")]
        public async Task<IActionResult> UpdateEmployee(Guid id, [FromBody] EmployeeUpdateRequest request)
        {
            if (!IsAdmin() && !IsManager()) return Forbid();
            if (request.Id.HasValue && id != request.Id.Value) return BadRequest(new { message = "ID không khớp" });

            var existing = await _context.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
            if (existing == null) return NotFound();

            if (!TryReadBranchId(request.BranchId, out var branchSpecified, out var requestedBranchId))
                return BadRequest(new { message = "BranchId không hợp lệ." });

            var targetRole = request.Role ?? existing.Role;
            if (!RoleNames.TryNormalizeStaffRole(targetRole, out var normalizedRole))
                return BadRequest(new { message = "Role nhân viên không hợp lệ." });
            targetRole = normalizedRole;

            var targetIsActive = request.IsActive ?? existing.IsActive;
            if (CurrentUserId() == id && !targetIsActive)
                return Conflict(new { message = "Bạn không thể tự vô hiệu hóa tài khoản đang đăng nhập." });

            var targetBranchId = branchSpecified ? requestedBranchId : existing.BranchId;

            if (!IsAdmin())
            {
                if (IsManager())
                {
                    var ownBranch = CurrentUserBranchId();
                    if (!ownBranch.HasValue || existing.BranchId != ownBranch.Value) return Forbid();
                    if (IsManagementRole(existing.Role)) return Forbid();
                    // Manager cannot change branch or elevate a staff member to management.
                    targetBranchId = existing.BranchId;
                    if (IsManagementRole(targetRole)) targetRole = existing.Role;
                }
                else return Forbid();
            }

            // Chỉ kiểm tra chi nhánh khi có thao tác gán/chuyển; sửa hồ sơ ở chi nhánh cũ vẫn được phép.
            Branch? targetBranch = null;
            if (targetBranchId.HasValue && targetBranchId != existing.BranchId)
            {
                targetBranch = await GetActiveBranchAsync(targetBranchId.Value);
                if (targetBranch == null)
                    return BadRequest(new { message = "Chi nhánh không tồn tại hoặc đã ngừng hoạt động." });
            }

            var employee = new Employee
            {
                Id = id,
                EmployeeCode = request.EmployeeCode ?? existing.EmployeeCode,
                EmployeeType = request.EmployeeType ?? existing.EmployeeType,
                FullName = request.FullName ?? existing.FullName,
                PhoneNumber = request.PhoneNumber,
                Position = request.Position,
                Department = request.Department,
                BasicSalary = request.BasicSalary ?? existing.BasicSalary,
                BranchId = targetBranchId,
                BranchName = targetBranch?.Name ?? (targetBranchId == existing.BranchId ? existing.BranchName : null),
                CitizenId = request.CitizenId,
                BirthDate = request.BirthDate,
                Gender = request.Gender,
                Address = request.Address,
                StartDate = request.StartDate ?? existing.StartDate,
                IsActive = targetIsActive,
                Role = targetRole,
                Note = request.Note,
                Username = request.Username,
                CreatedAt = existing.CreatedAt
            };

            // Ép kiểu sang UTC cho PostgreSQL
            employee.StartDate = DateTime.SpecifyKind(employee.StartDate, DateTimeKind.Utc);
            if (employee.BirthDate.HasValue)
            {
                employee.BirthDate = DateTime.SpecifyKind(employee.BirthDate.Value, DateTimeKind.Utc);
            }
            employee.CreatedAt = DateTime.SpecifyKind(employee.CreatedAt, DateTimeKind.Utc);

            if (!string.IsNullOrEmpty(request.Password))
            {
                var passwordError = PasswordPolicy.Validate(request.Password);
                if (passwordError != null) return BadRequest(new { message = passwordError });
                employee.Password = _passwordHasher.HashPassword(employee, request.Password);
            }
            else
            {
                employee.Password = existing.Password;
            }

            _context.Entry(employee).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                _dashboardService?.InvalidateSummaryCache();

                employee.Password = null;
                return Ok(employee);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, inner = ex.InnerException?.Message });
            }
        }

        /// <summary>Bật hoặc tắt trạng thái làm việc của nhân viên.</summary>
        [HttpPatch("{id}/toggle-status")]
        [Authorize(Roles = "admin,manager")]
        public async Task<IActionResult> ToggleStatus(Guid id)
        {
            if (!IsAdmin() && !IsManager()) return Forbid();

            var emp = await _context.Employees.FindAsync(id);
            if (emp == null) return NotFound();

            if (CurrentUserId() == id)
                return Conflict(new { message = "Bạn không thể tự vô hiệu hóa tài khoản đang đăng nhập." });

            if (IsManager())
            {
                var ownBranch = CurrentUserBranchId();
                if (!ownBranch.HasValue || emp.BranchId != ownBranch.Value || IsManagementRole(emp.Role)) return Forbid();
            }

            emp.IsActive = !emp.IsActive;
            await _context.SaveChangesAsync();
            _dashboardService?.InvalidateSummaryCache();
            return Ok(new { isActive = emp.IsActive });
        }

        /// <summary>Xóa nhân viên theo mã định danh.</summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin,manager")]
        public async Task<IActionResult> DeleteEmployee(Guid id)
        {
            if (!IsAdmin() && !IsManager()) return Forbid();

            var emp = await _context.Employees.FindAsync(id);
            if (emp == null) return NotFound();

            if (CurrentUserId() == id)
                return Conflict(new { message = "Bạn không thể tự xóa tài khoản đang đăng nhập." });

            if (IsManager())
            {
                var ownBranch = CurrentUserBranchId();
                if (!ownBranch.HasValue || emp.BranchId != ownBranch.Value || IsManagementRole(emp.Role)) return Forbid();
            }

            _context.Employees.Remove(emp);
            await _context.SaveChangesAsync();
            _dashboardService?.InvalidateSummaryCache();
            return NoContent();
        }

        private bool IsAdmin() => string.Equals(User.FindFirst(ClaimTypes.Role)?.Value, "admin", StringComparison.OrdinalIgnoreCase);
        private bool IsManager() => string.Equals(User.FindFirst(ClaimTypes.Role)?.Value, "manager", StringComparison.OrdinalIgnoreCase);
        private bool IsEmployeeRole()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            return string.Equals(role, "admin", StringComparison.OrdinalIgnoreCase)
                || string.Equals(role, "manager", StringComparison.OrdinalIgnoreCase)
                || string.Equals(role, "employee", StringComparison.OrdinalIgnoreCase)
                || string.Equals(role, "cashier", StringComparison.OrdinalIgnoreCase)
                || string.Equals(role, "kitchen", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsManagementRole(string? role) =>
            string.Equals(role, "admin", StringComparison.OrdinalIgnoreCase)
            || string.Equals(role, "manager", StringComparison.OrdinalIgnoreCase);

        private Guid? CurrentUserId() => Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : null;
        private Guid? CurrentUserBranchId() => Guid.TryParse(User.FindFirst("branchId")?.Value, out var id) ? id : null;
        private Task<bool> IsActiveBranchAsync(Guid branchId) => _context.Branches.AsNoTracking().AnyAsync(b => b.Id == branchId && b.IsActive);
        private Task<Branch?> GetActiveBranchAsync(Guid branchId) => _context.Branches.AsNoTracking().FirstOrDefaultAsync(b => b.Id == branchId && b.IsActive);

        private static bool TryReadBranchId(JsonElement branchValue, out bool specified, out Guid? branchId)
        {
            specified = branchValue.ValueKind != JsonValueKind.Undefined;
            branchId = null;
            if (!specified || branchValue.ValueKind == JsonValueKind.Null) return true;
            if (branchValue.ValueKind != JsonValueKind.String || !Guid.TryParse(branchValue.GetString(), out var parsed))
                return false;

            branchId = parsed;
            return true;
        }
    }
}
