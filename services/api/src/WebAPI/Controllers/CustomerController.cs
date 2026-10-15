using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Infrastructure.Persistence;
using RestaurantPOS.Application.Services;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using RestaurantPOS.Application.DTOs.Customers;
using RestaurantPOS.Application.Common.Security;

namespace RestaurantPOS.WebAPI.Controllers
{
    /// <summary>Cung cấp các endpoint tra cứu và cập nhật khách hàng.</summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoyaltyService _loyaltyService;
        private readonly IPasswordHasher<Customer> _passwordHasher;

        public CustomerController(ApplicationDbContext context, ILoyaltyService loyaltyService, IPasswordHasher<Customer> passwordHasher)
        {
            _context = context;
            _loyaltyService = loyaltyService;
            _passwordHasher = passwordHasher;
        }

        /// <summary>Lấy danh sách khách hàng, có thể lọc theo từ khóa.</summary>
        [HttpGet]
        [Authorize(Roles = "admin,manager,employee,cashier")]
        public async Task<IActionResult> GetCustomers([FromQuery] string? search)
        {
            var query = _context.Customers.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c => c.FullName.Contains(search) || c.PhoneNumber.Contains(search));
            }

            var customers = await query.OrderByDescending(c => c.TotalSpending).ToListAsync();

            return Ok(customers.Select(MapToDto));
        }

        /// <summary>Kiểm tra sự tồn tại của khách hàng (Công khai - Phục vụ chào mừng/đăng nhập).</summary>
        [HttpGet("exists/{phoneNumber}")]
        [AllowAnonymous]
        [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("moderate-limiter")]
        public async Task<IActionResult> CheckExists(string phoneNumber)
        {
            var customer = await _context.Customers.AsNoTracking()
                .FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber);

            return Ok(new CustomerMinimalDto
            {
                Exists = customer != null,
                FullName = customer?.FullName
            });
        }

        /// <summary>Tra cứu hồ sơ đầy đủ khách hàng theo số điện thoại.</summary>
        [HttpGet("{phoneNumber}")]
        [Authorize]
        public async Task<IActionResult> GetByPhone(string phoneNumber)
        {
            var customer = await _context.Customers.AsNoTracking()
                .FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber);

            if (customer == null) return NotFound();

            if (IsCustomer())
            {
                // Khách hàng chỉ được xem profile của chính mình
                if (User.Identity?.Name != phoneNumber) return Forbid();
            }
            // Staff được phép xem để phục vụ (đã qua [Authorize])

            return Ok(MapToDto(customer));
        }

        /// <summary>Tạo mới hoặc cập nhật khách hàng.</summary>
        [HttpPost]
        [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("moderate-limiter")]
        public async Task<IActionResult> CreateOrUpdate(Customer customer)
        {
            if (GuestSessionContext.IsGuest(User))
                return Forbid();

            var existing = await _context.Customers.FirstOrDefaultAsync(c => c.PhoneNumber == customer.PhoneNumber);

            if (existing != null)
            {
                // TRƯỜNG HỢP CẬP NHẬT: Phải có quyền
                if (IsCustomer())
                {
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (!Guid.TryParse(userIdClaim, out var userId) || userId != existing.Id)
                        return Forbid();
                }
                else if (User?.Identity == null || !User.Identity.IsAuthenticated)
                {
                    // Người dùng chưa đăng nhập không được cập nhật khách hàng đã tồn tại
                    return Unauthorized(new { message = "Vui lòng đăng nhập để cập nhật thông tin." });
                }

                // Cập nhật thông tin cơ bản
                existing.FullName = customer.FullName;
                if (!string.IsNullOrEmpty(customer.Email)) existing.Email = customer.Email;
                if (!string.IsNullOrEmpty(customer.Address)) existing.Address = customer.Address;
                if (!string.IsNullOrEmpty(customer.Gender)) existing.Gender = customer.Gender;
                if (customer.Birthday.HasValue) existing.Birthday = DateTime.SpecifyKind(customer.Birthday.Value, DateTimeKind.Utc);

                existing.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return Ok(MapToDto(existing));
            }
            else
            {
                // TRƯỜNG HỢP ĐĂNG KÝ MỚI: Cho phép công khai
                // Public registered-customer accounts must establish a password.
                // Staff-created walk-in records may remain passwordless and cannot
                // use the registered customer token endpoint until a password exists.
                if (User?.Identity?.IsAuthenticated != true && string.IsNullOrWhiteSpace(customer.Password))
                    return BadRequest(new { message = "Mật khẩu là bắt buộc khi đăng ký tài khoản khách hàng." });

                customer.Id = Guid.NewGuid();
                customer.CreatedAt = DateTime.UtcNow;
                customer.UpdatedAt = DateTime.UtcNow;
                customer.IsActive = true;
                customer.LoyaltyPoints = 0;
                customer.TotalSpending = 0;
                customer.TotalOrders = 0;
                if (customer.Birthday.HasValue) customer.Birthday = DateTime.SpecifyKind(customer.Birthday.Value, DateTimeKind.Utc);

                if (!string.IsNullOrWhiteSpace(customer.Password))
                {
                    var passwordError = PasswordPolicy.Validate(customer.Password);
                    if (passwordError != null) return BadRequest(new { message = passwordError });
                    customer.Password = _passwordHasher.HashPassword(customer, customer.Password);
                }

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();
                return Ok(MapToDto(customer));
            }
        }

        /// <summary>
        /// Creates a membership record at the cashier after explicit confirmation.
        /// No fixed/default password is assigned.
        /// </summary>
        [HttpPost("walk-in")]
        [Authorize(Roles = "admin,manager,employee,cashier")]
        public async Task<IActionResult> CreateWalkInMember([FromBody] WalkInMemberCreateDto request)
        {
            var phoneNumber = NormalizePhone(request?.PhoneNumber);
            if (phoneNumber == null || string.IsNullOrWhiteSpace(request?.FullName))
                return BadRequest(new { message = "Họ tên và số điện thoại là bắt buộc." });

            var existing = await _context.Customers.AsNoTracking()
                .FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber);
            if (existing != null)
                return Conflict(new { message = "Số điện thoại đã có tài khoản thành viên." });

            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                FullName = request.FullName.Trim(),
                PhoneNumber = phoneNumber,
                Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim(),
                Password = null,
                IsActive = true,
                LoyaltyPoints = 0,
                TotalSpending = 0,
                TotalOrders = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Customers.Add(customer);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // The unique phone index is the final race-safe boundary.
                return Conflict(new { message = "Số điện thoại đã có tài khoản thành viên." });
            }
            return Ok(MapToDto(customer));
        }

        /// <summary>Lấy lịch sử tích điểm của khách hàng.</summary>
        [HttpGet("{id:guid}/loyalty-history")]
        [Authorize(Roles = "admin,manager,cashier,employee,customer")]
        public async Task<IActionResult> GetLoyaltyHistory(Guid id)
        {
            if (IsCustomer())
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out var userId) || userId != id)
                    return Forbid();
            }

            var history = await _loyaltyService.GetHistoryAsync(id);
            return Ok(history);
        }

        /// <summary>Cập nhật hồ sơ khách hàng.</summary>
        [HttpPatch("{phoneNumber}/profile")]
        [Authorize(Roles = "admin,manager,customer")]
        public async Task<IActionResult> UpdateProfile(string phoneNumber, [FromBody] CustomerProfileUpdateDto update)
        {
            var existing = await _context.Customers.FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber);
            if (existing == null) return NotFound();

            if (IsCustomer())
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out var userId) || userId != existing.Id)
                    return Forbid();
            }

            if (!string.IsNullOrEmpty(update.FullName)) existing.FullName = update.FullName;
            if (update.Email != null) existing.Email = update.Email;
            if (update.Address != null) existing.Address = update.Address;
            if (update.Gender != null) existing.Gender = update.Gender;
            if (update.Birthday.HasValue)
            {
                existing.Birthday = DateTime.SpecifyKind(update.Birthday.Value, DateTimeKind.Utc);
            }
            if (!string.IsNullOrEmpty(update.NewPassword))
            {
                var passwordError = PasswordPolicy.Validate(update.NewPassword);
                if (passwordError != null) return BadRequest(new { message = passwordError });
                existing.Password = _passwordHasher.HashPassword(existing, update.NewPassword);
            }

            await _context.SaveChangesAsync();
            return Ok(MapToDto(existing));
        }

        private static CustomerResponseDto MapToDto(Customer c) => new()
        {
            Id = c.Id,
            FullName = c.FullName,
            PhoneNumber = c.PhoneNumber,
            Email = c.Email,
            Address = c.Address,
            Gender = c.Gender,
            Birthday = c.Birthday,
            LoyaltyPoints = c.LoyaltyPoints,
            TotalSpending = c.TotalSpending,
            TotalOrders = c.TotalOrders,
            CreatedAt = c.CreatedAt,
            CustomerGroup = c.CustomerGroup
        };

        private bool IsCustomer() => User?.FindFirst(ClaimTypes.Role)?.Value?.Equals("customer", StringComparison.OrdinalIgnoreCase) ?? false;

        private static string? NormalizePhone(string? phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber)) return null;
            var normalized = new string(phoneNumber.Where(char.IsDigit).ToArray());
            return normalized.Length >= 10 ? normalized : null;
        }
    }

    /// <summary>Dữ liệu được phép cập nhật trong hồ sơ khách hàng.</summary>
    public class CustomerProfileUpdateDto
    {
        /// <summary>Họ tên hiển thị của khách hàng.</summary>
        public string? FullName { get; set; }
        /// <summary>Địa chỉ email của khách hàng.</summary>
        public string? Email { get; set; }
        /// <summary>Địa chỉ liên hệ của khách hàng.</summary>
        public string? Address { get; set; }
        /// <summary>Giới tính của khách hàng.</summary>
        public string? Gender { get; set; }
        /// <summary>Ngày sinh của khách hàng.</summary>
        public DateTime? Birthday { get; set; }
        /// <summary>Mật khẩu mới, nếu khách hàng muốn đổi mật khẩu.</summary>
        public string? NewPassword { get; set; }
    }

    public sealed class WalkInMemberCreateDto
    {
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
    }
}
