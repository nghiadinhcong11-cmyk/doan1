using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using RestaurantPOS.Application.DTOs.Catalog;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Infrastructure.Persistence;

namespace RestaurantPOS.Application.Services;

public sealed class ToppingService : IToppingService
{
    private readonly ApplicationDbContext _context;

    public ToppingService(ApplicationDbContext context) => _context = context;

    public async Task<(bool Success, string Message, Topping? Topping)> CreateToppingAsync(CreateToppingRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name)) return (false, "Tên topping không được để trống.", null);
        if (request.Price <= 0) return (false, "Giá topping phải lớn hơn 0.", null);
        if (request.CostPrice < 0) return (false, "Giá vốn topping không được âm.", null);

        var name = Normalize(request.Name);
        var existingNames = await _context.Toppings.AsNoTracking()
            .Where(topping => topping.IsActive)
            .Select(topping => topping.Name)
            .ToListAsync();
        if (existingNames.Any(existingName => Normalize(existingName) == name))
            return (false, $"Đã có topping đang hoạt động tên '{request.Name}'.", null);

        var topping = new Topping
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Price = request.Price,
            CostPrice = request.CostPrice,
            ToppingType = string.IsNullOrWhiteSpace(request.ToppingType) ? "Topping Đồ uống" : request.ToppingType.Trim(),
            Category = string.IsNullOrWhiteSpace(request.Category) ? "Chưa phân loại" : request.Category.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };
        _context.Toppings.Add(topping);
        await _context.SaveChangesAsync();
        return (true, $"Đã tạo topping '{topping.Name}' với giá {topping.Price:N0}đ.", topping);
    }

    private static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        var decomposed = value.Trim().Replace('đ', 'd').Replace('Đ', 'D').Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);
        foreach (var character in decomposed)
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
                builder.Append(char.ToLowerInvariant(character));
        return string.Join(' ', builder.ToString().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }
}
