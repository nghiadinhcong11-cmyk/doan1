using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantPOS.WebAPI.Controllers
{
    /// <summary>Cung cấp các endpoint quản lý topping.</summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ToppingController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public ToppingController(ApplicationDbContext context) => _context = context;

        /// <summary>Lấy danh sách topping.</summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Topping>>> GetToppings()
        {
            try
            {
                return await _context.Toppings.ToListAsync();
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
                return NoContent();
            }
            catch (Exception ex)
            {
                var msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return StatusCode(500, new { message = "Lỗi khi xóa Topping: " + msg });
            }
        }
    }
}
