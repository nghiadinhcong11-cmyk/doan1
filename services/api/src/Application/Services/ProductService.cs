using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Application.DTOs.Catalog;
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
            if (branchId.HasValue)
                return await GetSellableProductsAsync(category, branchId.Value);

            var products = await _context.Products
                .AsNoTracking()
                .Where(p => p.IsActive)
                .OrderBy(p => p.Name)
                .ToListAsync();

            return FilterCategory(products, category);
        }

        public async Task<List<Product>> GetSellableProductsAsync(string? category, Guid branchId)
        {
            // A sellable menu is intentionally an inner join to an explicit
            // Available row. Missing availability is fail-closed and therefore
            // cannot expose a catalog item at an unconfigured branch.
            var products = await _context.Products
                .AsNoTracking()
                .Where(product => product.IsActive &&
                    _context.ProductBranchAvailabilities.Any(state =>
                        state.ProductId == product.Id &&
                        state.BranchId == branchId &&
                        state.Branch != null &&
                        state.Branch.IsActive &&
                        state.Status == "Available"))
                .OrderBy(product => product.Name)
                .ToListAsync();

            return FilterCategory(products, category);
        }

        private static List<Product> FilterCategory(List<Product> products, string? category)
        {
            var normalizedCategory = NormalizeCategory(category);
            if (string.IsNullOrWhiteSpace(normalizedCategory) || normalizedCategory == "tat ca")
                return products;

            return products
                .Where(product => NormalizeCategory(product.Category) == normalizedCategory ||
                    NormalizeCategory(product.Group) == normalizedCategory)
                .ToList();
        }

        private static string NormalizeCategory(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var decomposed = value.Trim()
                .Replace('đ', 'd')
                .Replace('Đ', 'D')
                .Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(decomposed.Length);
            foreach (var character in decomposed)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
                    builder.Append(char.ToLowerInvariant(character));
            }

            return string.Join(' ', builder.ToString().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        }

        public async Task<List<Product>> FindProductsByExactNameAsync(string productName)
        {
            var normalizedName = NormalizeProductName(productName);
            if (string.IsNullOrWhiteSpace(normalizedName))
                return new List<Product>();

            var products = await _context.Products
                .AsNoTracking()
                .Where(p => p.Name != null && p.IsActive)
                .OrderBy(p => p.Id)
                .ToListAsync();

            return products
                .Where(product => NormalizeProductName(product.Name) == normalizedName)
                .ToList();
        }

        private static string NormalizeProductName(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var decomposed = value.Trim()
                .Replace('đ', 'd')
                .Replace('Đ', 'D')
                .Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(decomposed.Length);
            foreach (var character in decomposed)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
                    builder.Append(char.ToLowerInvariant(character));
            }

            return string.Join(' ', builder.ToString().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
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

        public async Task<(bool Success, string Message, Product? Product)> CreateProductAsync(CreateProductRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name)) return (false, "Tên món không được để trống.", null);
            if (request.Price <= 0) return (false, "Giá bán phải lớn hơn 0.", null);
            if (request.CostPrice < 0) return (false, "Giá vốn không được âm.", null);

            var name = NormalizeProductName(request.Name);
            var existingNames = await _context.Products.AsNoTracking()
                .Where(product => product.IsActive && product.Name != null)
                .Select(product => product.Name)
                .ToListAsync();
            if (existingNames.Any(existingName => NormalizeProductName(existingName) == name))
                return (false, $"Đã có món đang hoạt động tên '{request.Name}'.", null);

            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
            var branchIds = await _context.Branches.AsNoTracking().Select(branch => branch.Id).ToListAsync();
            await using var transaction = await _context.Database.BeginTransactionAsync();
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Code = $"SP{(await _context.Products.CountAsync()) + 1:D5}",
                Name = request.Name.Trim(),
                Category = string.IsNullOrWhiteSpace(request.Category) ? "Đồ uống" : request.Category.Trim(),
                Group = string.IsNullOrWhiteSpace(request.Group) ? "Chưa phân loại" : request.Group.Trim(),
                Price = request.Price,
                CostPrice = request.CostPrice,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow,
                AvailabilityStatus = "TemporarilyUnavailable"
            };
            _context.Products.Add(product);
            _context.ProductBranchAvailabilities.AddRange(branchIds.Select(branchId => new ProductBranchAvailability
            {
                ProductId = product.Id,
                BranchId = branchId,
                Status = "TemporarilyUnavailable"
            }));
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return (true, $"Đã tạo món '{product.Name}' với giá {product.Price:N0}đ. Món đang tạm thời chưa bán tại các chi nhánh.", product);
            });
        }

        public async Task<(bool Success, string Message, string OldName, string NewName)> UpdateNameByIdAsync(
            Guid productId, string expectedCurrentName, string newName)
        {
            if (string.IsNullOrWhiteSpace(newName)) return (false, "Tên mới không được để trống.", expectedCurrentName, newName);
            var normalizedNewName = NormalizeProductName(newName);
            var existingNames = await _context.Products.AsNoTracking()
                .Where(product => product.IsActive && product.Id != productId && product.Name != null)
                .Select(product => product.Name)
                .ToListAsync();
            if (existingNames.Any(name => NormalizeProductName(name) == normalizedNewName))
                return (false, $"Đã có món đang hoạt động tên '{newName}'.", expectedCurrentName, newName);

            var affectedRows = await _context.Products
                .Where(product => product.Id == productId && product.Name == expectedCurrentName)
                .ExecuteUpdateAsync(setters => setters.SetProperty(product => product.Name, newName.Trim()));
            if (affectedRows != 1)
                return (false, "Tên món hiện tại đã thay đổi; cần yêu cầu đổi tên lại.", expectedCurrentName, newName);
            return (true, $"Đã đổi tên món '{expectedCurrentName}' thành '{newName.Trim()}'.", expectedCurrentName, newName.Trim());
        }
    }
}
