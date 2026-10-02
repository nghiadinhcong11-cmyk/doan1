using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Infrastructure.Persistence;
using RestaurantPOS.Application.Services;
using RestaurantPOS.Application.Common.Security;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

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

        public EmployeeController(ApplicationDbContext context, IPasswordHasher<Employee> passwordHasher, IEmployeeService employeeService)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _employeeService = employeeService;
        }

        /// <summary>Lấy danh sách nhân viên theo các bộ lọc.</summary>
        [HttpGet]
        [Authorize(Roles = "admin,manager")]
        public async Task<IActionResult> GetEmployees([FromQuery] string? search, [FromQuery] bool? isActive, [FromQuery] string? department, [FromQuery] string? position, [FromQuery] Guid? branchId)
        {
            if (!IsAdmin() && !IsManager()) return Forbid();

            var query = _context.Employees.AsQueryable();

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

            var employees = await query.OrderByDescending(e => e.CreatedAt).ToListAsync();
            foreach (var e in employees) e.Password = null;
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

            // Manager chỉ được tạo nhân viên cho chi nhánh mình
            if (IsManager())
            {
                var ownBranch = CurrentUserBranchId();
                if (!ownBranch.HasValue) return Forbid();
                employee.BranchId = ownBranch.Value;
                // Managers may staff their own branch, but cannot create another management principal.
                if (IsManagementRole(employee.Role)) employee.Role = "employee";
            }

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
        public async Task<IActionResult> UpdateEmployee(Guid id, [FromBody] Employee employee)
        {
            if (!IsAdmin() && !IsManager()) return Forbid();
            if (id != employee.Id) return BadRequest(new { message = "ID không khớp" });

            var existing = await _context.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
            if (existing == null) return NotFound();

            if (!IsAdmin())
            {
                if (IsManager())
                {
                    var ownBranch = CurrentUserBranchId();
                    if (!ownBranch.HasValue || existing.BranchId != ownBranch.Value) return Forbid();
                    if (IsManagementRole(existing.Role)) return Forbid();
                    // Manager cannot change branch or elevate a staff member to management.
                    employee.BranchId = existing.BranchId;
                    if (IsManagementRole(employee.Role)) employee.Role = existing.Role;
                }
                else return Forbid();
            }

            // Ép kiểu sang UTC cho PostgreSQL
            employee.StartDate = DateTime.SpecifyKind(employee.StartDate, DateTimeKind.Utc);
            if (employee.BirthDate.HasValue)
            {
                employee.BirthDate = DateTime.SpecifyKind(employee.BirthDate.Value, DateTimeKind.Utc);
            }
            employee.CreatedAt = DateTime.SpecifyKind(employee.CreatedAt, DateTimeKind.Utc);

            if (!string.IsNullOrEmpty(employee.Password))
            {
                var passwordError = PasswordPolicy.Validate(employee.Password);
                if (passwordError != null) return BadRequest(new { message = passwordError });
                employee.Password = _passwordHasher.HashPassword(employee, employee.Password);
            }
            else
            {
                employee.Password = existing.Password;
            }

            _context.Entry(employee).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();

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

            if (IsManager())
            {
                var ownBranch = CurrentUserBranchId();
                if (!ownBranch.HasValue || emp.BranchId != ownBranch.Value || IsManagementRole(emp.Role)) return Forbid();
            }

            emp.IsActive = !emp.IsActive;
            await _context.SaveChangesAsync();
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

            if (IsManager())
            {
                var ownBranch = CurrentUserBranchId();
                if (!ownBranch.HasValue || emp.BranchId != ownBranch.Value || IsManagementRole(emp.Role)) return Forbid();
            }

            _context.Employees.Remove(emp);
            await _context.SaveChangesAsync();
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
    }
}
