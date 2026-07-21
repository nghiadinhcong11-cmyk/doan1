using Microsoft.AspNetCore.Mvc;
using RestaurantPOS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RestaurantPOS.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TableController : ControllerBase
    {
        private static List<RestaurantTable> _tables = new List<RestaurantTable>
        {
            new RestaurantTable { Id = Guid.NewGuid(), Name = "Bàn 01", AreaName = "Tầng 1", SeatCount = 4, IsActive = true, Status = "Trống" },
            new RestaurantTable { Id = Guid.NewGuid(), Name = "Bàn 02", AreaName = "Tầng 1", SeatCount = 4, IsActive = true, Status = "Có khách" },
            new RestaurantTable { Id = Guid.NewGuid(), Name = "VIP 01", AreaName = "Tầng 2", SeatCount = 8, IsActive = true, Status = "Trống" },
            new RestaurantTable { Id = Guid.NewGuid(), Name = "Sân vườn 01", AreaName = "Ngoài trời", SeatCount = 2, IsActive = true, Status = "Đã đặt" }
        };

        [HttpGet]
        public IActionResult GetTables([FromQuery] string area, [FromQuery] bool? isActive)
        {
            var query = _tables.AsQueryable();

            if (!string.IsNullOrEmpty(area))
                query = query.Where(t => t.AreaName == area);

            if (isActive.HasValue)
                query = query.Where(t => t.IsActive == isActive.Value);

            return Ok(query.OrderBy(t => t.Name).ToList());
        }

        [HttpPost]
        public IActionResult CreateTable(RestaurantTable table)
        {
            table.Id = Guid.NewGuid();
            table.CreatedAt = DateTime.UtcNow;
            _tables.Add(table);
            return Ok(table);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateTable(Guid id, RestaurantTable tableUpdate)
        {
            var table = _tables.FirstOrDefault(t => t.Id == id);
            if (table == null) return NotFound();

            table.Name = tableUpdate.Name;
            table.AreaName = tableUpdate.AreaName;
            table.SeatCount = tableUpdate.SeatCount;
            table.IsActive = tableUpdate.IsActive;
            table.Description = tableUpdate.Description;

            return Ok(table);
        }
    }
}
