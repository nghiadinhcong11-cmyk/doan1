using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Infrastructure.Persistence;

namespace RestaurantPOS.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;

        public ProductService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Product>> GetAllProductsAsync(string? category)
        {
            var query = _context.Products.AsNoTracking().Where(p => p.IsActive);
            if (!string.IsNullOrEmpty(category) && category != "Tất cả")
            {
                query = query.Where(p => p.Category == category || p.Group == category);
            }
            return await query.OrderBy(p => p.Name).ToListAsync();
        }

        public async Task<(bool Success, string Message, decimal OldPrice, decimal NewPrice)> UpdatePriceByNameAsync(string productName, decimal newPrice)
        {
            if (newPrice <= 0)
                return (false, "Giá mới phải lớn hơn 0.", 0, 0);

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Name != null && p.Name.Contains(productName));
            if (product == null)
                return (false, $"Không tìm thấy sản phẩm '{productName}'.", 0, 0);

            var oldPrice = product.Price;
            product.Price = newPrice;
            await _context.SaveChangesAsync();

            return (true, $"Đã cập nhật giá món '{product.Name}' từ {oldPrice:N0}đ thành {newPrice:N0}đ.", oldPrice, newPrice);
        }
    }
}
