using System;

namespace RestaurantPOS.Domain.Entities;

public class Notification
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public Guid? EntityId { get; set; }
    public string? Route { get; set; }
    public string? TargetRole { get; set; }
    public Guid? TargetUserId { get; set; }
    public Guid? BranchId { get; set; }
    public bool IsRead { get; set; }
    /// <summary>
    /// Existing notifications retain their historical shared IsRead semantics.
    /// New notifications use NotificationRead rows per authenticated user.
    /// </summary>
    public bool UseLegacyReadState { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
