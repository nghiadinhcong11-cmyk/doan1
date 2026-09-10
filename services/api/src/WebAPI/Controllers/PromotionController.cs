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
    /// <summary>Cung cấp các endpoint quản lý chương trình ưu đãi.</summary>
    [ApiController]
    [Route("api/[controller]")]
    public class PromotionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PromotionController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>Lấy danh sách chương trình ưu đãi.</summary>
        [HttpGet]
        public async Task<IActionResult> GetPromotions()
        {
            return Ok(await _context.Promotions.OrderByDescending(p => p.CreatedAt).ToListAsync());
        }

        /// <summary>Tạo chương trình ưu đãi mới.</summary>
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> CreatePromotion(Promotion promotion)
        {
            try
            {
                promotion.Id = Guid.NewGuid();
                promotion.CreatedAt = DateTime.UtcNow;

                _context.Promotions.Add(promotion);
                await _context.SaveChangesAsync();
                return Ok(promotion);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>Cập nhật chương trình ưu đãi.</summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdatePromotion(Guid id, Promotion promotion)
        {
            if (id != promotion.Id) return BadRequest();

            _context.Entry(promotion).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Promotions.Any(e => e.Id == id)) return NotFound();
                else throw;
            }

            return Ok(promotion);
        }

        /// <summary>Xóa chương trình ưu đãi.</summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeletePromotion(Guid id)
        {
            var promotion = await _context.Promotions.FindAsync(id);
            if (promotion == null) return NotFound();

            _context.Promotions.Remove(promotion);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>Bật hoặc tắt chương trình ưu đãi.</summary>
        [HttpPatch("{id}/toggle")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> ToggleStatus(Guid id)
        {
            var promotion = await _context.Promotions.FindAsync(id);
            if (promotion == null) return NotFound();

            promotion.IsActive = !promotion.IsActive;
            await _context.SaveChangesAsync();
            return Ok(new { isActive = promotion.IsActive });
        }
    }
}
