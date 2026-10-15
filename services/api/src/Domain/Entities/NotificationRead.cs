using System;

namespace RestaurantPOS.Domain.Entities;

/// <summary>Per-user read state for a notification.</summary>
public sealed class NotificationRead
{
    public Guid NotificationId { get; set; }
    public Guid UserId { get; set; }
    public DateTime ReadAt { get; set; }
    public Notification? Notification { get; set; }
}
