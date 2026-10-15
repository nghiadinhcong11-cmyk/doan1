namespace RestaurantPOS.Domain.Entities;

/// <summary>
/// Branch-specific sellability for a shared product catalog.
/// Product identity, price and metadata remain global; this state does not.
/// </summary>
public class ProductBranchAvailability
{
    public Guid ProductId { get; set; }
    public Guid BranchId { get; set; }
    public string Status { get; set; } = "Available";
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public Product? Product { get; set; }
    public Branch? Branch { get; set; }
}
