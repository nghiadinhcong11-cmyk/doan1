using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        public async Task<IActionResult> GetAreas()
        {
            return Ok(await _context.Areas.OrderBy(a => a.DisplayOrder).ToListAsync());
        }

        /// <summary>Creates a service area.</summary>
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> CreateArea(Area area)
        {
            area.Id = Guid.NewGuid();
            area.CreatedAt = DateTime.UtcNow;
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

            // Lưu tên cũ để cập nhật các bàn liên quan
            var oldName = area.Name;

            area.Name = areaUpdate.Name;
            area.DisplayOrder = areaUpdate.DisplayOrder;

            // Cập nhật tên khu vực cho tất cả các bàn thuộc khu vực này
            var tablesInArea = await _context.Tables.Where(t => t.AreaName == oldName).ToListAsync();
            foreach (var table in tablesInArea)
            {
                table.AreaName = areaUpdate.Name;
            }

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
            var hasTables = await _context.Tables.AnyAsync(t => t.AreaName == area.Name);
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
