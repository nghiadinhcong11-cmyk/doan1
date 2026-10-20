using System.Collections.Concurrent;
using System.Security.Cryptography;
using RestaurantPOS.AI.Models;
using RestaurantPOS.Domain.Entities;

namespace RestaurantPOS.AI.Security;

public interface IProductPriceConfirmationStore
{
    AiConfirmation Create(Guid userId, Product product, decimal proposedPrice, TimeSpan? lifetime = null);
    bool TryConsume(string token, Guid userId, out PendingProductPriceConfirmation pending, out string error);
}

public sealed class PendingProductPriceConfirmation
{
    public Guid UserId { get; init; }
    public Guid ProductId { get; init; }
    public decimal ExpectedCurrentPrice { get; init; }
    public decimal ProposedNewPrice { get; init; }
    public DateTimeOffset ExpiresAtUtc { get; init; }
}

public sealed class ProductPriceConfirmationStore : IProductPriceConfirmationStore
{
    private static readonly TimeSpan DefaultLifetime = TimeSpan.FromMinutes(5);
    private readonly ConcurrentDictionary<string, PendingProductPriceConfirmation> _pending = new();

    public AiConfirmation Create(Guid userId, Product product, decimal proposedPrice, TimeSpan? lifetime = null)
    {
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var expiresAtUtc = DateTimeOffset.UtcNow.Add(lifetime ?? DefaultLifetime);
        _pending[token] = new PendingProductPriceConfirmation
        {
            UserId = userId,
            ProductId = product.Id,
            ExpectedCurrentPrice = product.Price,
            ProposedNewPrice = proposedPrice,
            ExpiresAtUtc = expiresAtUtc
        };

        return new AiConfirmation
        {
            Type = "product-price",
            Token = token,
            ProductId = product.Id,
            ProductName = product.Name ?? string.Empty,
            CurrentPrice = product.Price,
            ProposedPrice = proposedPrice,
            ExpiresAtUtc = expiresAtUtc
        };
    }

    public bool TryConsume(string token, Guid userId, out PendingProductPriceConfirmation pending, out string error)
    {
        pending = null!;
        error = string.Empty;
        if (string.IsNullOrWhiteSpace(token) || !_pending.TryGetValue(token, out var stored))
        {
            error = "Xác nhận không hợp lệ hoặc đã được sử dụng.";
            return false;
        }
        if (stored.UserId != userId)
        {
            error = "Xác nhận không thuộc về tài khoản đang đăng nhập.";
            return false;
        }
        if (stored.ExpiresAtUtc <= DateTimeOffset.UtcNow)
        {
            _pending.TryRemove(token, out _);
            error = "Xác nhận đã hết hạn; vui lòng yêu cầu xác nhận lại.";
            return false;
        }
        if (!_pending.TryRemove(token, out var consumed) || consumed is null)
        {
            error = "Xác nhận không hợp lệ hoặc đã được sử dụng.";
            return false;
        }
        pending = consumed;
        return true;
    }
}
