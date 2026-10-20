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
    /// <summary>Cung cáº¥p cĂ¡c endpoint tra cá»©u vĂ  cáº­p nháº­t khĂ¡ch hĂ ng.</summary>
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

        /// <summary>Láº¥y danh sĂ¡ch khĂ¡ch hĂ ng, cĂ³ thá»ƒ lá»c theo tá»« khĂ³a.</summary>
        [HttpGet]
        [Authorize(Roles = "admin,manager,employee,cashier")]
        public async Task<IActionResult> GetCustomers(
            [FromQuery] string? search,
            [FromQuery] int? page,
            [FromQuery] int? pageSize)
        {
            var query = _context.Customers.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c => c.FullName.Contains(search) || c.PhoneNumber.Contains(search));
            }

            var pagedRequest = page.HasValue || pageSize.HasValue;
            var requestedPage = Math.Max(page ?? 1, 1);
            var requestedPageSize = Math.Clamp(pageSize ?? 50, 1, 100);

            if (IsManagementStaff())
            {
                // Project only the fields used by the staff response. This avoids
                // loading password hashes and unrelated columns from Supabase.
                IQueryable<Customer> orderedQuery = query.OrderByDescending(c => c.TotalSpending);
                if (pagedRequest)
                {
                    orderedQuery = orderedQuery
                        .Skip((requestedPage - 1) * requestedPageSize)
                        .Take(requestedPageSize + 1);
                }

                var customers = await orderedQuery
                    .Select(c => new CustomerResponseDto
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
                    })
                    .ToListAsync();

                if (pagedRequest)
                {
                    Response.Headers["X-Has-Next-Page"] = (customers.Count > requestedPageSize).ToString().ToLowerInvariant();
                    customers = customers.Take(requestedPageSize).ToList();
                }

                return Ok(customers);
            }

            IQueryable<Customer> minimalOrderedQuery = query.OrderByDescending(c => c.TotalSpending);
            if (pagedRequest)
            {
                minimalOrderedQuery = minimalOrderedQuery
                    .Skip((requestedPage - 1) * requestedPageSize)
                    .Take(requestedPageSize + 1);
            }

            var minimalCustomers = await minimalOrderedQuery
                .Select(c => new
                {
                    c.Id,
                    c.FullName,
                    c.PhoneNumber,
                    c.LoyaltyPoints,
                    c.CustomerGroup
                })
                .ToListAsync();

            if (pagedRequest)
            {
                Response.Headers["X-Has-Next-Page"] = (minimalCustomers.Count > requestedPageSize).ToString().ToLowerInvariant();
                minimalCustomers = minimalCustomers.Take(requestedPageSize).ToList();
            }

            return Ok(minimalCustomers);
        }

        /// <summary>Kiá»ƒm tra sá»± tá»“n táº¡i cá»§a khĂ¡ch hĂ ng (CĂ´ng khai - Phá»¥c vá»¥ chĂ o má»«ng/Ä‘Äƒng nháº­p).</summary>
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

        /// <summary>Tra cá»©u há»“ sÆ¡ Ä‘áº§y Ä‘á»§ khĂ¡ch hĂ ng theo sá»‘ Ä‘iá»‡n thoáº¡i.</summary>
        [HttpGet("{phoneNumber}")]
        [Authorize]
        public async Task<IActionResult> GetByPhone(string phoneNumber)
        {
            var customer = await _context.Customers.AsNoTracking()
                .FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber);

            if (customer == null) return NotFound();

            if (IsCustomer())
            {
                // KhĂ¡ch hĂ ng chá»‰ Ä‘Æ°á»£c xem profile cá»§a chĂ­nh mĂ¬nh
                if (User.Identity?.Name != phoneNumber) return Forbid();
            }
            // Staff Ä‘Æ°á»£c phĂ©p xem Ä‘á»ƒ phá»¥c vá»¥ (Ä‘Ă£ qua [Authorize])

            return Ok(MapForCaller(customer));
        }

        /// <summary>Táº¡o má»›i hoáº·c cáº­p nháº­t khĂ¡ch hĂ ng.</summary>
        [HttpPost]
        [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("moderate-limiter")]
        public async Task<IActionResult> CreateOrUpdate([FromBody] CustomerRegistrationDto request)
        {
            if (GuestSessionContext.IsGuest(User))
                return Forbid();

            if (string.IsNullOrWhiteSpace(request.FullName) || string.IsNullOrWhiteSpace(request.PhoneNumber))
                return BadRequest(new { message = "Họ tên và số điện thoại là bắt buộc." });

            var existing = await _context.Customers.FirstOrDefaultAsync(c => c.PhoneNumber == request.PhoneNumber);

            if (existing != null)
            {
                if (IsCustomer())
                {
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (!Guid.TryParse(userIdClaim, out var userId) || userId != existing.Id)
                        return Forbid();
                }
                else if (User?.Identity == null || !User.Identity.IsAuthenticated)
                {
                    return Unauthorized(new { message = "Vui lòng đăng nhập để cập nhật thông tin." });
                }

                if (!IsCustomer() && !IsManagementStaff())
                    return Forbid();

                existing.FullName = request.FullName;
                existing.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return Ok(MapToDto(existing));
            }

            if (User?.Identity?.IsAuthenticated != true && string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { message = "Mật khẩu là bắt buộc khi đăng ký tài khoản khách hàng." });

            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                FullName = request.FullName.Trim(),
                PhoneNumber = request.PhoneNumber.Trim(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true,
                LoyaltyPoints = 0,
                TotalSpending = 0,
                TotalOrders = 0
            };

            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                var passwordError = PasswordPolicy.Validate(request.Password);
                if (passwordError != null) return BadRequest(new { message = passwordError });
                customer.Password = _passwordHasher.HashPassword(customer, request.Password);
            }

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            return Ok(MapToDto(customer));
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
                return BadRequest(new { message = "Há» tĂªn vĂ  sá»‘ Ä‘iá»‡n thoáº¡i lĂ  báº¯t buá»™c." });

            var existing = await _context.Customers.AsNoTracking()
                .FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber);
            if (existing != null)
                return Conflict(new { message = "Sá»‘ Ä‘iá»‡n thoáº¡i Ä‘Ă£ cĂ³ tĂ i khoáº£n thĂ nh viĂªn." });

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
                return Conflict(new { message = "Sá»‘ Ä‘iá»‡n thoáº¡i Ä‘Ă£ cĂ³ tĂ i khoáº£n thĂ nh viĂªn." });
            }
            return Ok(MapToDto(customer));
        }

        /// <summary>Láº¥y lá»‹ch sá»­ tĂ­ch Ä‘iá»ƒm cá»§a khĂ¡ch hĂ ng.</summary>
        [HttpGet("{id:guid}/loyalty-history")]
        [Authorize(Roles = "admin,manager,customer")]
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

        /// <summary>Cáº­p nháº­t há»“ sÆ¡ khĂ¡ch hĂ ng.</summary>
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

        private object MapForCaller(Customer customer)
        {
            return IsCustomer() || IsManagementStaff()
                ? MapToDto(customer)
                : new
                {
                    customer.Id,
                    customer.FullName,
                    customer.PhoneNumber,
                    customer.LoyaltyPoints,
                    customer.CustomerGroup
                };
        }

        private bool IsCustomer() => User?.FindFirst(ClaimTypes.Role)?.Value?.Equals("customer", StringComparison.OrdinalIgnoreCase) ?? false;

        private bool IsManagementStaff() => User?.IsInRole("admin") == true || User?.IsInRole("manager") == true;

        private static string? NormalizePhone(string? phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber)) return null;
            var normalized = new string(phoneNumber.Where(char.IsDigit).ToArray());
            return normalized.Length >= 10 ? normalized : null;
        }
    }

    /// <summary>Dá»¯ liá»‡u Ä‘Æ°á»£c phĂ©p cáº­p nháº­t trong há»“ sÆ¡ khĂ¡ch hĂ ng.</summary>
    public class CustomerProfileUpdateDto
    {
        /// <summary>Há» tĂªn hiá»ƒn thá»‹ cá»§a khĂ¡ch hĂ ng.</summary>
        public string? FullName { get; set; }
        /// <summary>Äá»‹a chá»‰ email cá»§a khĂ¡ch hĂ ng.</summary>
        public string? Email { get; set; }
        /// <summary>Äá»‹a chá»‰ liĂªn há»‡ cá»§a khĂ¡ch hĂ ng.</summary>
        public string? Address { get; set; }
        /// <summary>Giá»›i tĂ­nh cá»§a khĂ¡ch hĂ ng.</summary>
        public string? Gender { get; set; }
        /// <summary>NgĂ y sinh cá»§a khĂ¡ch hĂ ng.</summary>
        public DateTime? Birthday { get; set; }
        /// <summary>Máº­t kháº©u má»›i, náº¿u khĂ¡ch hĂ ng muá»‘n Ä‘á»•i máº­t kháº©u.</summary>
        public string? NewPassword { get; set; }
    }

    public sealed class WalkInMemberCreateDto
    {
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
    }

    public sealed class CustomerRegistrationDto
    {
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Password { get; set; }
    }
}


