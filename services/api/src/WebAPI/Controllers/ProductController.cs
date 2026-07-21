using Microsoft.AspNetCore.Mvc;
using RestaurantPOS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RestaurantPOS.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        // Mock data để demo giao diện
        private static List<Product> _products = new List<Product>
        {
            new Product { Id = Guid.NewGuid(), Code = "SP00001", Name = "Cà phê muối", Category = "Đồ uống", Price = 25000, CostPrice = 12000 },
            new Product { Id = Guid.NewGuid(), Code = "SP00002", Name = "Bạc xỉu", Category = "Đồ uống", Price = 22000, CostPrice = 10000 },
            new Product { Id = Guid.NewGuid(), Code = "SP00003", Name = "Bánh mì kẹp", Category = "Đồ ăn", Price = 35000, CostPrice = 20000 }
        };

        [HttpGet]
        public IActionResult GetProducts([FromQuery] string search, [FromQuery] string category)
        {
            var query = _products.AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(p => p.Name.Contains(search, StringComparison.OrdinalIgnoreCase) || p.Code.Contains(search));

            if (!string.IsNullOrEmpty(category))
                query = query.Where(p => p.Category == category);

            return Ok(query.ToList());
        }

        [HttpPost]
        public IActionResult CreateProduct(Product product)
        {
            product.Id = Guid.NewGuid();
            _products.Add(product);
            return Ok(product);
        }
    }
}
