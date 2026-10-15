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

        public async Task<List<Product>> GetAllProductsAsync(string? category, Guid? branchId = null)
        {
            var query = _context.Products.AsNoTracking().Where(p => p.IsActive);
            if (!string.IsNullOrEmpty(category) && category != "Tất cả")
            {
                query = query.Where(p => p.Category == category || p.Group == category);
            }
            var products = await query.OrderBy(p => p.Name).ToListAsync();
            if (branchId.HasValue)
            {
                var statuses = await _context.ProductBranchAvailabilities
                    .AsNoTracking()
                    .Where(state => state.BranchId == branchId.Value)
                    .ToDictionaryAsync(state => state.ProductId, state => state.Status);
                foreach (var product in products)
                {
                    // Product.AvailabilityStatus is legacy transition data only.
                    // A branch-scoped read uses its row, or the existing product
                    // creation default when the row has not been created yet.
                    product.AvailabilityStatus = statuses.TryGetValue(product.Id, out var status)
                        ? status
                        : "Available";
                }
            }

            return products;
        }

        public async Task<List<Product>> FindProductsByExactNameAsync(string productName)
        {
            var normalizedName = productName?.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(normalizedName))
                return new List<Product>();

            return await _context.Products
                .AsNoTracking()
                .Where(p => p.Name != null && p.Name.Trim().ToLower() == normalizedName)
                .OrderBy(p => p.Id)
                .ToListAsync();
        }

        public async Task<(bool Success, string Message, decimal OldPrice, decimal NewPrice)> UpdatePriceByNameAsync(string productName, decimal newPrice)
        {
            if (newPrice <= 0)
                return (false, "Giá mới phải lớn hơn 0.", 0, 0);

            var matches = await FindProductsByExactNameAsync(productName);
            if (matches.Count == 0)
                return (false, $"Không tìm thấy sản phẩm '{productName}'.", 0, 0);
            if (matches.Count > 1)
                return (false, $"Tên sản phẩm '{productName}' không duy nhất; vui lòng chỉ rõ sản phẩm.", 0, 0);

            var product = matches[0];
            return await UpdatePriceByIdAsync(product.Id, product.Price, newPrice);
        }

        public async Task<(bool Success, string Message, decimal OldPrice, decimal NewPrice)> UpdatePriceByIdAsync(
            Guid productId,
            decimal expectedCurrentPrice,
            decimal newPrice)
        {
            if (newPrice <= 0)
                return (false, "Giá mới phải lớn hơn 0.", 0, 0);

            var product = await _context.Products
                .AsNoTracking()
                .Where(p => p.Id == productId)
                .Select(p => new { p.Name, p.Price })
                .SingleOrDefaultAsync();

            if (product == null)
                return (false, "Không tìm thấy sản phẩm cần cập nhật.", 0, 0);

            if (product.Price != expectedCurrentPrice)
                return (false, "Giá hiện tại đã thay đổi; cần xác nhận lại thao tác.", product.Price, newPrice);

            var affectedRows = await _context.Products
                .Where(p => p.Id == productId && p.Price == expectedCurrentPrice)
                .ExecuteUpdateAsync(setters => setters.SetProperty(p => p.Price, newPrice));

            if (affectedRows != 1)
                return (false, "Giá hiện tại đã thay đổi; cần xác nhận lại thao tác.", product.Price, newPrice);

            return (true, $"Đã cập nhật giá món '{product.Name}' từ {expectedCurrentPrice:N0}đ thành {newPrice:N0}đ.", expectedCurrentPrice, newPrice);
        }
    }
}
