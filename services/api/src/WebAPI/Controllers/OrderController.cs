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
    public class OrderController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders([FromQuery] string? search, [FromQuery] string? status)
        {
            var query = _context.Orders.Include(o => o.Details).AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(o => o.InvoiceCode.Contains(search) || o.CustomerName.Contains(search));

            if (!string.IsNullOrEmpty(status))
                query = query.Where(o => o.Status == status);

            return Ok(await query.OrderByDescending(o => o.CreatedAt).ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder(Order order)
        {
            try
            {
                order.Id = Guid.NewGuid();
                // Ép kiểu sang UTC cho PostgreSQL
                order.CreatedAt = DateTime.UtcNow;

                if (string.IsNullOrEmpty(order.InvoiceCode))
                {
                    order.InvoiceCode = "HD" + DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                }

                if (order.Details != null)
                {
                    foreach (var detail in order.Details)
                    {
                        detail.Id = Guid.NewGuid();
                    }
                }

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
                return Ok(order);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, inner = ex.InnerException?.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(Guid id)
        {
            var order = await _context.Orders.Include(o => o.Details).FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound();
            return Ok(order);
        }
    }
}
