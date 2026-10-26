namespace RestaurantPOS.Domain.Entities;

/// <summary>
/// Branch-specific availability for a shared topping catalog entry.
/// </summary>
public class ToppingBranchAvailability
{
    public Guid ToppingId { get; set; }
    public Guid BranchId { get; set; }
    public string Status { get; set; } = "Available";
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public Topping? Topping { get; set; }
    public Branch? Branch { get; set; }
}
