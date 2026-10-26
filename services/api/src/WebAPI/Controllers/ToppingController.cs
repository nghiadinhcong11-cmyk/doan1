using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Infrastructure.Persistence;
using RestaurantPOS.Application.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Security.Claims;
using RestaurantPOS.Application.Common.Security;
using Microsoft.AspNetCore.SignalR;
using RestaurantPOS.WebAPI.Hubs;

namespace RestaurantPOS.WebAPI.Controllers
{
    /// <summary>Cung cấp các endpoint quản lý topping.</summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ToppingController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IDashboardService? _dashboardService;
        private readonly IHubContext<KitchenHub> _hub;
        public ToppingController(ApplicationDbContext context, IHubContext<KitchenHub> hub, IDashboardService? dashboardService = null)
        {
            _context = context;
            _hub = hub;
            _dashboardService = dashboardService;
        }

        /// <summary>Lấy danh sách topping.</summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Topping>>> GetToppings([FromQuery] Guid? branchId, [FromQuery] bool sellableOnly = false, [FromQuery] bool? isActive = null)
        {
            try
            {
                if (!TryResolveReadBranch(branchId, out var effectiveBranchId)) return Forbid();
                if (sellableOnly && !effectiveBranchId.HasValue) return BadRequest(new { message = "Sellable topping menu requires a verified branch." });

                var query = _context.Toppings.AsNoTracking().AsQueryable();
                if (User.Identity?.IsAuthenticated != true || User.IsInRole("customer") || sellableOnly)
                    query = query.Where(topping => topping.IsActive);
                if (isActive.HasValue) query = query.Where(topping => topping.IsActive == isActive.Value);
                if (sellableOnly)
                {
                    var scopedBranchId = effectiveBranchId!.Value;
                    query = query.Where(topping =>
                        _context.Branches.Any(branch => branch.Id == scopedBranchId && branch.IsActive) &&
                        _context.ToppingBranchAvailabilities.Any(state =>
                            state.ToppingId == topping.Id && state.BranchId == scopedBranchId && state.Status == "Available"));
                }

                var toppings = await query.OrderBy(topping => topping.Name).ToListAsync();
                if (effectiveBranchId.HasValue)
                {
                    var statuses = await _context.ToppingBranchAvailabilities.AsNoTracking()
                        .Where(state => state.BranchId == effectiveBranchId.Value)
                        .ToDictionaryAsync(state => state.ToppingId, state => state.Status);
                    foreach (var topping in toppings)
                        topping.AvailabilityStatus = statuses.TryGetValue(topping.Id, out var status) ? status : "Available";
                }
                return toppings;
            }
            catch (Exception ex)
            {
                var msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return StatusCode(500, new { message = "Lỗi truy vấn Topping: " + msg });
            }
        }

        /// <summary>Tạo topping mới.</summary>
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<Topping>> CreateTopping(Topping topping)
        {
            try
            {
                topping.Id = Guid.NewGuid();
                topping.CreatedAt = DateTime.UtcNow;
                _context.Toppings.Add(topping);
                var branchIds = await _context.Branches.AsNoTracking().Select(branch => branch.Id).ToListAsync();
                _context.ToppingBranchAvailabilities.AddRange(branchIds.Select(branchId => new ToppingBranchAvailability
                {
                    ToppingId = topping.Id,
                    BranchId = branchId,
                    Status = "Available"
                }));
                await _context.SaveChangesAsync();
                return Ok(topping);
            }
            catch (Exception ex)
            {
                var msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return StatusCode(500, new { message = "Lỗi khi tạo Topping: " + msg });
            }
        }

        /// <summary>Cập nhật thông tin topping.</summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateTopping(Guid id, Topping topping)
        {
            if (id != topping.Id) return BadRequest();

            try
            {
                topping.CreatedAt = DateTime.SpecifyKind(topping.CreatedAt, DateTimeKind.Utc);
                _context.Entry(topping).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                _dashboardService?.InvalidateSummaryCache();
                return NoContent();
            }
            catch (Exception ex)
            {
                var msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return StatusCode(500, new { message = "Lỗi khi cập nhật Topping: " + msg });
            }
        }

        /// <summary>Xóa topping theo mã định danh.</summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteTopping(Guid id)
        {
            try
            {
                var topping = await _context.Toppings.FindAsync(id);
                if (topping == null) return NotFound();
                _context.Toppings.Remove(topping);
                await _context.SaveChangesAsync();
                _dashboardService?.InvalidateSummaryCache();
                return NoContent();
            }
            catch (Exception ex)
            {
                var msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return StatusCode(500, new { message = "Lỗi khi xóa Topping: " + msg });
            }
        }

        [HttpPatch("{id}/availability")]
        [Authorize(Roles = "admin,manager,kitchen")]
        public async Task<IActionResult> UpdateAvailability(Guid id, [FromQuery] Guid? branchId, [FromBody] AvailabilityRequest request)
        {
            var allowed = new[] { "Available", "TemporarilyUnavailable", "OutOfStock" };
            if (!TryResolveStaffBranch(branchId, out var effectiveBranchId)) return Forbid();
            if (!await _context.Branches.AsNoTracking().AnyAsync(branch => branch.Id == effectiveBranchId && branch.IsActive))
                return BadRequest(new { message = "Branch is invalid or inactive." });
            if (request == null || !allowed.Contains(request.Status, StringComparer.OrdinalIgnoreCase))
                return BadRequest(new { message = "Trạng thái topping không hợp lệ." });
            var topping = await _context.Toppings.FindAsync(id);
            if (topping == null) return NotFound();

            var normalizedStatus = allowed.First(status => status.Equals(request.Status, StringComparison.OrdinalIgnoreCase));
            var state = await _context.ToppingBranchAvailabilities
                .SingleOrDefaultAsync(item => item.ToppingId == id && item.BranchId == effectiveBranchId);
            if (state == null)
            {
                state = new ToppingBranchAvailability { ToppingId = id, BranchId = effectiveBranchId, Status = normalizedStatus };
                _context.ToppingBranchAvailabilities.Add(state);
            }
            else
            {
                state.Status = normalizedStatus;
                state.UpdatedAtUtc = DateTime.UtcNow;
            }
            await _context.SaveChangesAsync();
            await _hub.Clients.Groups("kitchen-admin", $"branch:{effectiveBranchId}:role:kitchen", $"branch:{effectiveBranchId}:role:manager")
                .SendAsync("ToppingAvailabilityChanged", new { topping.Id, topping.Name, availabilityStatus = normalizedStatus, branchId = effectiveBranchId });
            return Ok(new { topping.Id, availabilityStatus = normalizedStatus, branchId = effectiveBranchId });
        }

        private bool TryResolveReadBranch(Guid? requestedBranchId, out Guid? effectiveBranchId)
        {
            effectiveBranchId = requestedBranchId;
            if (User.IsInRole("admin")) return true;
            if (GuestSessionContext.TryCreate(User, out var guest))
            {
                if (requestedBranchId.HasValue && requestedBranchId.Value != guest!.BranchId) return false;
                effectiveBranchId = guest!.BranchId;
                return true;
            }
            var role = User.FindFirstValue(ClaimTypes.Role);
            if (!string.IsNullOrWhiteSpace(role) && !string.Equals(role, "customer", StringComparison.OrdinalIgnoreCase))
            {
                if (!TryResolveStaffBranch(requestedBranchId, out var staffBranch)) return false;
                effectiveBranchId = staffBranch;
            }
            return true;
        }

        private bool TryResolveStaffBranch(Guid? requestedBranchId, out Guid effectiveBranchId)
        {
            effectiveBranchId = Guid.Empty;
            if (User.IsInRole("admin"))
                return requestedBranchId.HasValue && requestedBranchId.Value != Guid.Empty && (effectiveBranchId = requestedBranchId.Value) != Guid.Empty;
            if (!Guid.TryParse(User.FindFirst("branchId")?.Value, out var userBranchId) || userBranchId == Guid.Empty) return false;
            if (requestedBranchId.HasValue && requestedBranchId.Value != userBranchId) return false;
            effectiveBranchId = userBranchId;
            return true;
        }

        public sealed class AvailabilityRequest { public string Status { get; set; } = "Available"; }
    }
}
