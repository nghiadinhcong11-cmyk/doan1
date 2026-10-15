using System;

namespace RestaurantPOS.AI.Models;

public sealed class AiConfirmation
{
    public string Type { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal CurrentPrice { get; set; }
    public decimal ProposedPrice { get; set; }
    public DateTimeOffset ExpiresAtUtc { get; set; }
}
