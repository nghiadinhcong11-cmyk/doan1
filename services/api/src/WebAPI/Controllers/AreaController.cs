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
    /// <summary>Provides endpoints for managing restaurant service areas.</summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AreaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AreaController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>Gets all service areas.</summary>
        [HttpGet]
        [Authorize(Roles = "admin,manager,employee,cashier,kitchen")]
        public async Task<IActionResult> GetAreas([FromQuery] Guid? branchId)
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            Guid? effectiveBranchId = branchId;
            if (!string.Equals(role, "admin", StringComparison.OrdinalIgnoreCase))
            {
                if (!Guid.TryParse(User.FindFirst("branchId")?.Value, out var userBranchId)) return Forbid();
                if (branchId.HasValue && branchId.Value != userBranchId) return Forbid();
                effectiveBranchId = userBranchId;
            }

            var query = _context.Areas.AsNoTracking().OrderBy(a => a.DisplayOrder).AsQueryable();
            if (effectiveBranchId.HasValue)
                query = query.Where(a => a.BranchId == effectiveBranchId.Value).OrderBy(a => a.DisplayOrder);

            return Ok(await query.ToListAsync());
        }

        /// <summary>Creates a service area.</summary>
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> CreateArea(Area area)
        {
            area.Id = Guid.NewGuid();
            area.CreatedAt = DateTime.UtcNow;
            if (area.BranchId == Guid.Empty)
                return BadRequest(new { message = "Khu vực mới phải thuộc một chi nhánh." });
            if (!await _context.Branches.AnyAsync(b => b.Id == area.BranchId))
                return BadRequest(new { message = "Chi nhánh không tồn tại." });
            _context.Areas.Add(area);
            await _context.SaveChangesAsync();
            return Ok(area);
        }

        /// <summary>Updates a service area.</summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateArea(Guid id, Area areaUpdate)
        {
            var area = await _context.Areas.FindAsync(id);
            if (area == null) return NotFound();

            area.Name = areaUpdate.Name;
            area.DisplayOrder = areaUpdate.DisplayOrder;

            await _context.SaveChangesAsync();
            return Ok(area);
        }

        /// <summary>Deletes a service area.</summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteArea(Guid id)
        {
            var area = await _context.Areas.FindAsync(id);
            if (area == null) return NotFound();

            // Kiểm tra xem có bàn nào đang thuộc khu vực này không
            var hasTables = await _context.Tables.AnyAsync(t => t.AreaId == area.Id);
            if (hasTables)
            {
                return BadRequest(new { message = "Không thể xóa khu vực đang có bàn. Vui lòng xóa bàn hoặc chuyển bàn sang khu vực khác trước." });
            }

            _context.Areas.Remove(area);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
