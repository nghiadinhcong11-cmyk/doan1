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

        public ProductController(ApplicationDbContext context, IHubContext<KitchenHub> hub, INotificationService notifications)
        {
            _context = context;
            _hub = hub;
            _notifications = notifications;
        }

        /// <summary>Lấy danh sách món theo các bộ lọc.</summary>
        [HttpGet]
        public async Task<IActionResult> GetProducts([FromQuery] string? search, [FromQuery] string? category, [FromQuery] string? group, [FromQuery] bool? isActive)
        {
            var query = _context.Products.AsQueryable();

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

            return Ok(await query.OrderByDescending(p => p.CreatedAt).ToListAsync());
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
        public async Task<IActionResult> UpdateAvailability(Guid id, [FromBody] AvailabilityRequest request)
        {
            var allowed = new[] { "Available", "TemporarilyUnavailable", "OutOfStock" };
            if (request == null || !allowed.Contains(request.Status, StringComparer.OrdinalIgnoreCase))
                return BadRequest(new { message = "Trạng thái món không hợp lệ." });
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            product.AvailabilityStatus = allowed.First(s => s.Equals(request.Status, StringComparison.OrdinalIgnoreCase));
            await _context.SaveChangesAsync();
            await _hub.Clients.All.SendAsync("ProductAvailabilityChanged", new { product.Id, product.Name, product.AvailabilityStatus });
            if (product.AvailabilityStatus != "Available")
                await _notifications.CreateAsync(new RestaurantPOS.Domain.Entities.Notification
                {
                    Type = "OUT_OF_STOCK", Title = "Món tạm hết",
                    Message = $"Món {product.Name} đã được đánh dấu {product.AvailabilityStatus}.",
                    EntityType = "Product", EntityId = product.Id, Route = "/products",
                    TargetRole = "admin", BranchId = null
                });
            return Ok(new { product.Id, product.AvailabilityStatus });
        }

        public sealed class AvailabilityRequest { public string Status { get; set; } = "Available"; }
    }
}
