using System.Collections.Concurrent;
using System.Security.Cryptography;
using RestaurantPOS.AI.Models;
using RestaurantPOS.Domain.Entities;

namespace RestaurantPOS.AI.Security;

public sealed record PendingProductNameConfirmation(
    Guid UserId,
    Guid ProductId,
    string ExpectedCurrentName,
    string ProposedNewName,
    DateTimeOffset ExpiresAtUtc);

public interface IProductNameConfirmationStore
{
    AiConfirmation Create(Guid userId, Product product, string proposedName, TimeSpan? lifetime = null);
    bool TryConsume(string token, Guid userId, out PendingProductNameConfirmation pending, out string error);
}

public sealed class ProductNameConfirmationStore : IProductNameConfirmationStore
{
    private static readonly TimeSpan DefaultLifetime = TimeSpan.FromMinutes(5);
    private readonly ConcurrentDictionary<string, PendingProductNameConfirmation> _pending = new();

    public AiConfirmation Create(Guid userId, Product product, string proposedName, TimeSpan? lifetime = null)
    {
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var expires = DateTimeOffset.UtcNow.Add(lifetime ?? DefaultLifetime);
        _pending[token] = new(userId, product.Id, product.Name ?? string.Empty, proposedName.Trim(), expires);
        return new AiConfirmation
        {
            Type = "product-name", Token = token, ProductId = product.Id,
            ProductName = product.Name ?? string.Empty, ProposedName = proposedName.Trim(),
            ExpiresAtUtc = expires
        };
    }

    public bool TryConsume(string token, Guid userId, out PendingProductNameConfirmation pending, out string error)
    {
        pending = null!; error = string.Empty;
        if (string.IsNullOrWhiteSpace(token) || !_pending.TryGetValue(token, out var stored))
        { error = "Xác nhận không hợp lệ hoặc đã được sử dụng."; return false; }
        if (stored.UserId != userId)
        { error = "Xác nhận không thuộc về tài khoản đang đăng nhập."; return false; }
        if (stored.ExpiresAtUtc <= DateTimeOffset.UtcNow)
        { _pending.TryRemove(token, out _); error = "Xác nhận đã hết hạn; vui lòng yêu cầu đổi tên lại."; return false; }
        if (!_pending.TryRemove(token, out var consumed) || consumed is null)
        { error = "Xác nhận không hợp lệ hoặc đã được sử dụng."; return false; }
        pending = consumed; return true;
    }
}
