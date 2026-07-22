using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantPOS.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TableController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TableController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetTables([FromQuery] string? search, [FromQuery] string? area, [FromQuery] bool? isActive, [FromQuery] Guid? branchId)
        {
            var query = _context.Tables.AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(t => t.Name.Contains(search) || (t.Description != null && t.Description.Contains(search)));

            if (!string.IsNullOrEmpty(area))
                query = query.Where(t => t.AreaName == area);

            if (isActive.HasValue)
                query = query.Where(t => t.IsActive == isActive.Value);

            if (branchId.HasValue)
                query = query.Where(t => t.BranchId == branchId.Value);

            return Ok(await query.OrderBy(t => t.Name).ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> CreateTable(RestaurantTable table)
        {
            try
            {
                table.Id = Guid.NewGuid();
                table.CreatedAt = DateTime.UtcNow;

                // Đảm bảo các giá trị mặc định nếu bị null từ frontend
                if (string.IsNullOrEmpty(table.Status)) table.Status = "Trống";

                _context.Tables.Add(table);
                await _context.SaveChangesAsync();
                return Ok(table);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new {
                    message = "Lỗi khi lưu bàn vào database",
                    error = ex.Message,
                    inner = ex.InnerException?.Message
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTable(Guid id, RestaurantTable tableUpdate)
        {
            var table = await _context.Tables.FindAsync(id);
            if (table == null) return NotFound();

            table.Name = tableUpdate.Name;
            table.AreaName = tableUpdate.AreaName;
            table.SeatCount = tableUpdate.SeatCount;
            table.IsActive = tableUpdate.IsActive;
            table.Description = tableUpdate.Description;
            table.Status = tableUpdate.Status;
            table.BranchId = tableUpdate.BranchId;
            table.BranchName = tableUpdate.BranchName;

            await _context.SaveChangesAsync();
            return Ok(table);
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] string status)
        {
            var table = await _context.Tables.FindAsync(id);
            if (table == null) return NotFound();

            table.Status = status;
            await _context.SaveChangesAsync();
            return Ok(new { id = table.Id, status = table.Status });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTable(Guid id)
        {
            var table = await _context.Tables.FindAsync(id);
            if (table == null) return NotFound();

            _context.Tables.Remove(table);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
