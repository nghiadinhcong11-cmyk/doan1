using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using RestaurantPOS.Infrastructure.Persistence;
using RestaurantPOS.Application.Common.Interfaces;
using RestaurantPOS.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RestaurantPOS.WebAPI.Controllers
{
    /// <summary>
    /// API Xác thực, Cấp Token JWT và Quản lý Mật khẩu
    /// </summary>
    /// <summary>Cung cấp các endpoint xác thực và quản lý phiên đăng nhập.</summary>
    [ApiController]
    [Route("api/[controller]")]
    [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("strict-limiter")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly IPasswordHasher<Employee> _employeeHasher;
        private readonly IPasswordHasher<Customer> _customerHasher;

        public AuthController(ApplicationDbContext context, IJwtService jwtService, IPasswordHasher<Employee> employeeHasher, IPasswordHasher<Customer> customerHasher)
        {
            _context = context;
            _jwtService = jwtService;
            _employeeHasher = employeeHasher;
            _customerHasher = customerHasher;
        }

        /// <summary>
        /// Yêu cầu đổi mật khẩu tài khoản
        /// </summary>
        /// <summary>Dữ liệu yêu cầu đổi mật khẩu.</summary>
        public class ChangePasswordRequest
        {
            /// <summary>
            /// ID người dùng
            /// </summary>
            public Guid Id { get; set; }

            /// <summary>
            /// Mật khẩu hiện tại
            /// </summary>
            public string OldPassword { get; set; } = string.Empty;

            /// <summary>
            /// Mật khẩu mới muốn đặt
            /// </summary>
            public string NewPassword { get; set; } = string.Empty;

            /// <summary>
            /// Loại tài khoản: "Employee" hoặc "Customer"
            /// </summary>
            public string Type { get; set; } = "Employee";
        }

        /// <summary>
        /// Yêu cầu đăng nhập hệ thống
        /// </summary>
        /// <summary>Thông tin đăng nhập của nhân viên.</summary>
        public class LoginRequest
        {
            /// <summary>
            /// Tên đăng nhập
            /// </summary>
            public string Username { get; set; } = string.Empty;

            /// <summary>
            /// Mật khẩu
            /// </summary>
            public string Password { get; set; } = string.Empty;

            /// <summary>
            /// Chế độ làm việc: "admin", "cashier", hoặc "kitchen"
            /// </summary>
            public string Mode { get; set; } = "admin";

            /// <summary>
            /// Chi nhánh đăng nhập làm việc (bắt buộc với cashier và kitchen)
            /// </summary>
            public Guid? BranchId { get; set; }
        }

        /// <summary>
        /// Yêu cầu cấp token cho khách hàng
        /// </summary>
        /// <summary>Thông tin dùng để cấp token cho khách hàng.</summary>
        public class CustomerTokenRequest
        {
            /// <summary>
            /// Số điện thoại khách hàng
            /// </summary>
            public string PhoneNumber { get; set; } = string.Empty;

            /// <summary>
            /// Họ tên khách hàng
            /// </summary>
            public string? FullName { get; set; }
        }

        /// <summary>
        /// Đăng nhập và nhận JWT Bearer Token theo quyền tương ứng (admin / cashier / kitchen)
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // Tìm nhân viên theo Username
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Username == request.Username && e.IsActive);

            if (employee == null)
            {
                return Unauthorized(new { message = "Tên đăng nhập hoặc mật khẩu không chính xác" });
            }

            // Kiểm tra mật khẩu
            bool passwordValid = false;
            bool needsUpgrade = false;

            if (string.IsNullOrEmpty(employee.Password))
            {
                return Unauthorized(new { message = "Tài khoản chưa có mật khẩu" });
            }

            try
            {
                var result = _employeeHasher.VerifyHashedPassword(employee, employee.Password, request.Password);
                if (result == PasswordVerificationResult.Success)
                {
                    passwordValid = true;
                }
                else if (result == PasswordVerificationResult.SuccessRehashNeeded)
                {
                    passwordValid = true;
                    needsUpgrade = true;
                }
            }
            catch (FormatException)
            {
                // Mật khẩu có thể đang là plaintext
            }

            // Thử so sánh plaintext (Legacy support) nếu chưa xác thực được bằng hash
            if (!passwordValid && employee.Password == request.Password)
            {
                passwordValid = true;
                needsUpgrade = true;
            }

            if (!passwordValid)
            {
                return Unauthorized(new { message = "Tên đăng nhập hoặc mật khẩu không chính xác" });
            }

            // Tự động nâng cấp lên hash nếu cần
            if (needsUpgrade)
            {
                employee.Password = _employeeHasher.HashPassword(employee, request.Password);
                await _context.SaveChangesAsync();
            }

            string actualRole = employee.Role?.ToLower() ?? "employee";

            // Nếu đăng nhập chế độ thu ngân, kiểm tra chi nhánh
            if (request.Mode == "cashier")
            {
                // Cho phép admin, manager và cashier truy cập POS
                if (actualRole != "admin" && actualRole != "manager" && actualRole != "cashier")
                {
                    return BadRequest(new { message = "Tài khoản của bạn không có quyền truy cập thu ngân" });
                }

                if (request.BranchId.HasValue && employee.BranchId != request.BranchId.Value && actualRole != "admin")
                {
                    return BadRequest(new { message = "Bạn không có quyền đăng nhập vào chi nhánh này" });
                }

                var tokenRole = actualRole == "admin" ? "admin" : (actualRole == "manager" ? "manager" : "cashier");

                var token = _jwtService.GenerateToken(
                    employee.Id,
                    employee.Username ?? employee.EmployeeCode ?? "cashier",
                    employee.FullName,
                    tokenRole,
                    employee.BranchId,
                    employee.BranchName,
                    employee.Position
                );

                return Ok(new
                {
                    token,
                    role = tokenRole,
                    fullName = employee.FullName,
                    branchId = employee.BranchId,
                    employeeId = employee.Id,
                    branchName = employee.BranchName,
                    position = employee.Position
                });
            }

            // Nếu đăng nhập chế độ nhà bếp
            if (request.Mode == "kitchen")
            {
                // Cho phép admin, manager và kitchen truy cập bếp
                if (actualRole != "admin" && actualRole != "manager" && actualRole != "kitchen")
                {
                    return BadRequest(new { message = "Tài khoản của bạn không có quyền truy cập nhà bếp" });
                }

                if (request.BranchId.HasValue && employee.BranchId != request.BranchId.Value && actualRole != "admin")
                {
                    return BadRequest(new { message = "Bạn không có quyền đăng nhập vào nhà bếp của chi nhánh này" });
                }

                var tokenRole = actualRole == "admin" ? "admin" : (actualRole == "manager" ? "manager" : "kitchen");

                var token = _jwtService.GenerateToken(
                    employee.Id,
                    employee.Username ?? employee.EmployeeCode ?? "kitchen",
                    employee.FullName,
                    tokenRole,
                    employee.BranchId,
                    employee.BranchName,
                    employee.Position
                );

                return Ok(new
                {
                    token,
                    role = tokenRole,
                    fullName = employee.FullName,
                    branchId = employee.BranchId,
                    employeeId = employee.Id,
                    branchName = employee.BranchName,
                    position = employee.Position
                });
            }

            // Nếu đăng nhập chế độ admin bằng tài khoản nhân viên
            if (request.Mode == "admin")
            {
                if (actualRole != "admin" && actualRole != "manager")
                {
                    return BadRequest(new { message = "Bạn không có quyền truy cập trang quản trị" });
                }

                var token = _jwtService.GenerateToken(
                    employee.Id,
                    employee.Username ?? employee.EmployeeCode ?? "admin",
                    employee.FullName,
                    actualRole, // Trả về "admin" hoặc "manager"
                    employee.BranchId,
                    employee.BranchName,
                    employee.Position
                );

                return Ok(new
                {
                    token,
                    role = actualRole,
                    fullName = employee.FullName,
                    branchId = employee.BranchId,
                    employeeId = employee.Id,
                    branchName = employee.BranchName,
                    position = employee.Position
                });
            }

            return BadRequest(new { message = "Chế độ đăng nhập không hợp lệ" });
        }

        /// <summary>
        /// Cấp JWT Bearer Token cho khách hàng dựa vào số điện thoại
        /// </summary>
        [HttpPost("customer-token")]
        public async Task<IActionResult> GetCustomerToken([FromBody] CustomerTokenRequest request)
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.PhoneNumber == request.PhoneNumber);
            if (customer == null)
            {
                var guestToken = _jwtService.GenerateToken(
                    Guid.NewGuid(),
                    request.PhoneNumber,
                    request.FullName ?? "Khách hàng",
                    "customer"
                );
                return Ok(new { token = guestToken, role = "customer", fullName = request.FullName ?? "Khách hàng", customerId = (Guid?)null });
            }

            var token = _jwtService.GenerateToken(
                customer.Id,
                customer.PhoneNumber,
                customer.FullName,
                "customer"
            );
            return Ok(new { token, role = "customer", fullName = customer.FullName, customerId = customer.Id });
        }

        /// <summary>
        /// Lấy thông tin cá nhân và quyền hạn của tài khoản đang đăng nhập từ JWT Claims
        /// </summary>
        /// <summary>Lấy thông tin người dùng hiện tại từ JWT.</summary>
        [HttpGet("me")]
        [Authorize]
        public IActionResult GetCurrentUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var fullName = User.FindFirst("fullName")?.Value;
            var branchId = User.FindFirst("branchId")?.Value;
            var branchName = User.FindFirst("branchName")?.Value;
            var position = User.FindFirst("position")?.Value;

            return Ok(new
            {
                userId,
                role,
                fullName,
                branchId,
                branchName,
                position
            });
        }

        /// <summary>
        /// Thay đổi mật khẩu tài khoản nhân viên hoặc khách hàng
        /// </summary>
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out var authenticatedUserId) || authenticatedUserId == Guid.Empty)
                    return Unauthorized(new { message = "JWT không chứa identity hợp lệ" });

                if (request.Id != authenticatedUserId)
                    return Forbid();

                var role = User.FindFirst(ClaimTypes.Role)?.Value;
                var isCustomer = string.Equals(role, "customer", StringComparison.OrdinalIgnoreCase);
                var requestedType = request.Type?.Trim();
                if (isCustomer && !string.Equals(requestedType, "Customer", StringComparison.OrdinalIgnoreCase))
                    return Forbid();
                if (!isCustomer && !string.Equals(requestedType, "Employee", StringComparison.OrdinalIgnoreCase))
                    return Forbid();

                if (!isCustomer)
                {
                    var employee = await _context.Employees.FindAsync(authenticatedUserId);
                    if (employee == null) return NotFound(new { message = "Không tìm thấy nhân viên" });

                    if (string.IsNullOrEmpty(employee.Password)) return BadRequest(new { message = "Lỗi dữ liệu mật khẩu" });

                    bool oldPasswordMatch = false;
                    try
                    {
                        var verification = _employeeHasher.VerifyHashedPassword(employee, employee.Password, request.OldPassword);
                        if (verification != PasswordVerificationResult.Failed) oldPasswordMatch = true;
                    }
                    catch (FormatException) { }

                    if (!oldPasswordMatch && employee.Password == request.OldPassword)
                    {
                        oldPasswordMatch = true;
                    }

                    if (!oldPasswordMatch)
                    {
                        return BadRequest(new { message = "Mật khẩu cũ không chính xác" });
                    }

                    employee.Password = _employeeHasher.HashPassword(employee, request.NewPassword);
                }
                else
                {
                    var customer = await _context.Customers.FindAsync(authenticatedUserId);
                    if (customer == null) return NotFound(new { message = "Không tìm thấy khách hàng" });

                    if (string.IsNullOrEmpty(customer.Password)) return BadRequest(new { message = "Lỗi dữ liệu mật khẩu" });

                    bool oldPasswordMatch = false;
                    try
                    {
                        var verification = _customerHasher.VerifyHashedPassword(customer, customer.Password, request.OldPassword);
                        if (verification != PasswordVerificationResult.Failed) oldPasswordMatch = true;
                    }
                    catch (FormatException) { }

                    if (!oldPasswordMatch && customer.Password == request.OldPassword)
                    {
                        oldPasswordMatch = true;
                    }

                    if (!oldPasswordMatch)
                    {
                        return BadRequest(new { message = "Mật khẩu cũ không chính xác" });
                    }

                    customer.Password = _customerHasher.HashPassword(customer, request.NewPassword);
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = "Đổi mật khẩu thành công!" });
            } catch (Exception ex) {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
