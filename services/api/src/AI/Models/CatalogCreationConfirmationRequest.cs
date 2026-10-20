namespace RestaurantPOS.AI.Models;

public sealed class CatalogCreationConfirmationRequest
{
    public string Token { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}
