using System.Collections.Concurrent;
using System.Security.Cryptography;
using RestaurantPOS.AI.Models;
using RestaurantPOS.Application.DTOs.Catalog;

namespace RestaurantPOS.AI.Security;

public sealed record PendingCatalogCreation(
    Guid UserId,
    string Type,
    CreateProductRequest? Product,
    CreateToppingRequest? Topping,
    DateTimeOffset ExpiresAtUtc);

public interface ICatalogCreationConfirmationStore
{
    AiConfirmation CreateProduct(Guid userId, CreateProductRequest request, TimeSpan? lifetime = null);
    AiConfirmation CreateTopping(Guid userId, CreateToppingRequest request, TimeSpan? lifetime = null);
    bool TryConsume(string token, Guid userId, string expectedType, out PendingCatalogCreation pending, out string error);
}

public sealed class CatalogCreationConfirmationStore : ICatalogCreationConfirmationStore
{
    private static readonly TimeSpan DefaultLifetime = TimeSpan.FromMinutes(5);
    private readonly ConcurrentDictionary<string, PendingCatalogCreation> _pending = new();

    public AiConfirmation CreateProduct(Guid userId, CreateProductRequest request, TimeSpan? lifetime = null)
    {
        var token = NewToken();
        var expires = DateTimeOffset.UtcNow.Add(lifetime ?? DefaultLifetime);
        _pending[token] = new(userId, "product-create", request, null, expires);
        return new AiConfirmation
        {
            Type = "product-create", Token = token, ProductName = request.Name,
            ProposedPrice = request.Price, CostPrice = request.CostPrice,
            Category = request.Category, Group = request.Group, ExpiresAtUtc = expires
        };
    }

    public AiConfirmation CreateTopping(Guid userId, CreateToppingRequest request, TimeSpan? lifetime = null)
    {
        var token = NewToken();
        var expires = DateTimeOffset.UtcNow.Add(lifetime ?? DefaultLifetime);
        _pending[token] = new(userId, "topping-create", null, request, expires);
        return new AiConfirmation
        {
            Type = "topping-create", Token = token, ProductName = request.Name,
            ProposedPrice = request.Price, CostPrice = request.CostPrice,
            Category = request.Category, ToppingType = request.ToppingType,
            Description = request.Description, ExpiresAtUtc = expires
        };
    }

    public bool TryConsume(string token, Guid userId, string expectedType, out PendingCatalogCreation pending, out string error)
    {
        pending = null!; error = string.Empty;
        if (string.IsNullOrWhiteSpace(token) || !_pending.TryGetValue(token, out var stored))
        { error = "Xác nhận không hợp lệ hoặc đã được sử dụng."; return false; }
        if (stored.UserId != userId)
        { error = "Xác nhận không thuộc về tài khoản đang đăng nhập."; return false; }
        if (!string.Equals(stored.Type, expectedType, StringComparison.OrdinalIgnoreCase))
        { error = "Loại xác nhận không khớp với thao tác."; return false; }
        if (stored.ExpiresAtUtc <= DateTimeOffset.UtcNow)
        { _pending.TryRemove(token, out _); error = "Xác nhận đã hết hạn; vui lòng yêu cầu tạo lại."; return false; }
        if (!_pending.TryRemove(token, out var consumed) || consumed is null)
        { error = "Xác nhận không hợp lệ hoặc đã được sử dụng."; return false; }
        pending = consumed; return true;
    }

    private static string NewToken() => Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
}
