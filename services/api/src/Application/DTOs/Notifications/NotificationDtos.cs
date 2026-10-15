using System;
using System.Collections.Generic;

namespace RestaurantPOS.Application.DTOs.Notifications;

public sealed class NotificationDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public Guid? EntityId { get; set; }
    public string? Route { get; set; }
    public Guid? BranchId { get; set; }
    public string? BranchName { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
}

public sealed class NotificationPageDto
{
    public IReadOnlyList<NotificationDto> Items { get; init; } = Array.Empty<NotificationDto>();
    public int Total { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
}

public sealed class NotificationCountDto
{
    public int Count { get; init; }
}
