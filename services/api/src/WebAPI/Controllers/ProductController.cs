using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using RestaurantPOS.WebAPI.Hubs;
using RestaurantPOS.Application.Services;
using RestaurantPOS.Application.Common.Security;
using System.Security.Claims;

namespace RestaurantPOS.WebAPI.Controllers
{
    /// <summary>Cung cấp các endpoint quản lý món trong thực đơn.</summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<KitchenHub> _hub;
        private readonly INotificationService _notifications;
        private readonly IDashboardService? _dashboardService;

        public ProductController(ApplicationDbContext context, IHubContext<KitchenHub> hub, INotificationService notifications, IDashboardService? dashboardService = null)
        {
            _context = context;
            _hub = hub;
            _notifications = notifications;
            _dashboardService = dashboardService;
        }

        /// <summary>Lấy danh sách món theo các bộ lọc.</summary>
        [HttpGet]
        public async Task<IActionResult> GetProducts([FromQuery] string? search, [FromQuery] string? category, [FromQuery] string? group, [FromQuery] bool? isActive, [FromQuery] Guid? branchId)
        {
            if (!TryResolveReadBranch(branchId, out var effectiveBranchId))
                return Forbid();

            var query = _context.Products.AsNoTracking().AsQueryable();

            // Public and customer-facing menu reads must not expose disabled products.
            if (User.Identity?.IsAuthenticated != true || User.IsInRole("customer"))
                query = query.Where(p => p.IsActive);

            if (!string.IsNullOrEmpty(search))
                query = query.Where(p => p.Name.Contains(search) || p.Code.Contains(search));

            if (!string.IsNullOrEmpty(category))
                query = query.Where(p => p.Category == category);

            if (!string.IsNullOrEmpty(group))
                query = query.Where(p => p.Group == group);

            if (isActive.HasValue)
                query = query.Where(p => p.IsActive == isActive.Value);

            var products = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
            if (effectiveBranchId.HasValue)
            {
                var statuses = await _context.ProductBranchAvailabilities
                    .AsNoTracking()
                    .Where(state => state.BranchId == effectiveBranchId.Value)
                    .ToDictionaryAsync(state => state.ProductId, state => state.Status);

                foreach (var product in products)
                {
                    // A branch-aware read must never fall back to the legacy global
                    // value. Missing rows retain the create-product default instead.
                    product.AvailabilityStatus = statuses.TryGetValue(product.Id, out var status)
                        ? status
                        : "Available";
                }
            }

            return Ok(products);
        }

        /// <summary>Tạo món mới.</summary>
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> CreateProduct(Product product)
        {
            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                if (product.ImageUrl.Length > 700000)
                    return BadRequest(new { message = "Ảnh quá lớn. Giới hạn là 500KB." });

                if (!product.ImageUrl.StartsWith("data:image/jpeg;base64,") &&
                    !product.ImageUrl.StartsWith("data:image/png;base64,") &&
                    !product.ImageUrl.StartsWith("data:image/webp;base64,"))
                {
                    return BadRequest(new { message = "Định dạng ảnh không hỗ trợ. Vui lòng dùng JPG, PNG hoặc WebP." });
                }
            }

            try
            {
                product.Id = Guid.NewGuid();
                product.CreatedAt = DateTime.UtcNow;

                if (string.IsNullOrEmpty(product.Code))
                {
                    var count = await _context.Products.CountAsync();
                    product.Code = $"SP{count + 1:D5}";
                }

                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                return Ok(product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, inner = ex.InnerException?.Message });
            }
        }

        /// <summary>Xóa món theo mã định danh.</summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            _dashboardService?.InvalidateSummaryCache();
            return NoContent();
        }

        /// <summary>Cập nhật thông tin món.</summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateProduct(Guid id, Product product)
        {
            if (id != product.Id) return BadRequest();

            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                if (product.ImageUrl.Length > 700000)
                    return BadRequest(new { message = "Ảnh quá lớn. Giới hạn là 500KB." });

                if (!product.ImageUrl.StartsWith("data:image/jpeg;base64,") &&
                    !product.ImageUrl.StartsWith("data:image/png;base64,") &&
                    !product.ImageUrl.StartsWith("data:image/webp;base64,"))
                {
                    return BadRequest(new { message = "Định dạng ảnh không hỗ trợ. Vui lòng dùng JPG, PNG hoặc WebP." });
                }
            }

            // Nếu mã bị trống, giữ lại mã cũ
            if (string.IsNullOrEmpty(product.Code))
            {
                var existingCode = await _context.Products
                    .Where(p => p.Id == id)
                    .Select(p => p.Code)
                    .FirstOrDefaultAsync();
                product.Code = existingCode;
            }

            product.CreatedAt = DateTime.SpecifyKind(product.CreatedAt, DateTimeKind.Utc);
            _context.Entry(product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                _dashboardService?.InvalidateSummaryCache();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Products.Any(e => e.Id == id)) return NotFound();
                else throw;
            }

            return Ok(product);
        }

        /// <summary>Bật hoặc tắt trạng thái kinh doanh của món.</summary>
        [HttpPatch("{id}/toggle-status")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> ToggleStatus(Guid id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            product.IsActive = !product.IsActive;
            await _context.SaveChangesAsync();
            return Ok(new { isActive = product.IsActive });
        }

        [HttpPatch("{id}/availability")]
        [Authorize(Roles = "admin,manager,kitchen")]
        public async Task<IActionResult> UpdateAvailability(Guid id, [FromQuery] Guid? branchId, [FromBody] AvailabilityRequest request)
        {
            var allowed = new[] { "Available", "TemporarilyUnavailable", "OutOfStock" };
            if (!TryResolveStaffBranch(branchId, out var effectiveBranchId))
                return Forbid();
            if (!await _context.Branches.AsNoTracking().AnyAsync(branch => branch.Id == effectiveBranchId && branch.IsActive))
                return BadRequest(new { message = "Branch is invalid or inactive." });
            if (request == null || !allowed.Contains(request.Status, StringComparer.OrdinalIgnoreCase))
                return BadRequest(new { message = "Trạng thái món không hợp lệ." });
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            var normalizedStatus = allowed.First(s => s.Equals(request.Status, StringComparison.OrdinalIgnoreCase));
            var branchState = await _context.ProductBranchAvailabilities
                .SingleOrDefaultAsync(state => state.ProductId == product.Id && state.BranchId == effectiveBranchId);
            if (branchState == null)
            {
                branchState = new ProductBranchAvailability
                {
                    ProductId = product.Id,
                    BranchId = effectiveBranchId,
                    Status = normalizedStatus
                };
                _context.ProductBranchAvailabilities.Add(branchState);
            }
            else
            {
                branchState.Status = normalizedStatus;
                branchState.UpdatedAtUtc = DateTime.UtcNow;
            }
            await _context.SaveChangesAsync();
            await _hub.Clients.Groups("kitchen-admin", $"branch:{effectiveBranchId}:role:kitchen", $"branch:{effectiveBranchId}:role:manager")
                .SendAsync("ProductAvailabilityChanged", new { product.Id, product.Name, availabilityStatus = normalizedStatus, branchId = effectiveBranchId });
            if (normalizedStatus != "Available")
                await _notifications.CreateAsync(new RestaurantPOS.Domain.Entities.Notification
                {
                    Type = "OUT_OF_STOCK", Title = "Món tạm hết",
                    Message = $"Món {product.Name} đã được đánh dấu {normalizedStatus}.",
                    EntityType = "Product", EntityId = product.Id, Route = "/products",
                    TargetRole = "admin", BranchId = effectiveBranchId
                });
            return Ok(new { product.Id, availabilityStatus = normalizedStatus, branchId = effectiveBranchId });
        }

        private bool TryResolveReadBranch(Guid? requestedBranchId, out Guid? effectiveBranchId)
        {
            effectiveBranchId = requestedBranchId;
            if (User.IsInRole("admin")) return true;

            if (GuestSessionContext.TryCreate(User, out var guest))
            {
                if (requestedBranchId.HasValue && requestedBranchId.Value != guest!.BranchId)
                    return false;
                effectiveBranchId = guest.BranchId;
                return true;
            }

            var role = User.FindFirstValue(ClaimTypes.Role);
            if (!string.IsNullOrWhiteSpace(role) && !string.Equals(role, "customer", StringComparison.OrdinalIgnoreCase))
            {
                if (!TryResolveStaffBranch(requestedBranchId, out var staffBranch))
                    return false;
                effectiveBranchId = staffBranch;
                return true;
            }

            return true;
        }

        private bool TryResolveStaffBranch(Guid? requestedBranchId, out Guid effectiveBranchId)
        {
            effectiveBranchId = Guid.Empty;
            if (User.IsInRole("admin"))
                return requestedBranchId.HasValue && requestedBranchId.Value != Guid.Empty && (effectiveBranchId = requestedBranchId.Value) != Guid.Empty;

            if (!Guid.TryParse(User.FindFirst("branchId")?.Value, out var userBranchId) || userBranchId == Guid.Empty)
                return false;
            if (requestedBranchId.HasValue && requestedBranchId.Value != userBranchId)
                return false;

            effectiveBranchId = userBranchId;
            return true;
        }

        public sealed class AvailabilityRequest { public string Status { get; set; } = "Available"; }
    }
}
