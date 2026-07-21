using Microsoft.AspNetCore.Mvc;
using RestaurantPOS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RestaurantPOS.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private static List<Order> _orders = new List<Order>
        {
            new Order {
                Id = Guid.NewGuid(),
                InvoiceCode = "HD00001",
                CreatedAt = DateTime.Now.AddHours(-2),
                CustomerName = "Khách lẻ",
                TotalAmount = 150000,
                Discount = 0,
                PaidAmount = 150000,
                PaymentMethod = "Tiền mặt",
                TableName = "Bàn 01"
            },
            new Order {
                Id = Guid.NewGuid(),
                InvoiceCode = "HD00002",
                CreatedAt = DateTime.Now.AddHours(-1),
                CustomerName = "Anh Tuấn",
                TotalAmount = 450000,
                Discount = 50000,
                PaidAmount = 400000,
                PaymentMethod = "Chuyển khoản",
                TableName = "VIP 02"
            }
        };

        [HttpGet]
        public IActionResult GetOrders([FromQuery] string search, [FromQuery] string status, [FromQuery] DateTime? date)
        {
            var query = _orders.AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(o => o.InvoiceCode.Contains(search) || o.CustomerName.Contains(search));

            if (!string.IsNullOrEmpty(status))
                query = query.Where(o => o.Status == status);

            return Ok(query.OrderByDescending(o => o.CreatedAt).ToList());
        }

        [HttpPost]
        public IActionResult CreateOrder(Order order)
        {
            order.Id = Guid.NewGuid();
            order.InvoiceCode = "HD" + DateTime.Now.Ticks.ToString().Substring(10);
            _orders.Add(order);
            return Ok(order);
        }
    }
}
