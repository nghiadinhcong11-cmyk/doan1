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
    public class AreaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AreaController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAreas()
        {
            return Ok(await _context.Areas.OrderBy(a => a.DisplayOrder).ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> CreateArea(Area area)
        {
            area.Id = Guid.NewGuid();
            area.CreatedAt = DateTime.UtcNow;
            _context.Areas.Add(area);
            await _context.SaveChangesAsync();
            return Ok(area);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteArea(Guid id)
        {
            var area = await _context.Areas.FindAsync(id);
            if (area == null) return NotFound();

            _context.Areas.Remove(area);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
