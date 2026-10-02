using RestaurantPOS.Domain.Entities;

namespace RestaurantPOS.Application.DTOs;

public sealed class TableResponseDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string AreaName { get; init; } = string.Empty;
    public int SeatCount { get; init; }
    public string? Description { get; init; }
    public bool IsActive { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? QrCodeUrl { get; init; }
    public Guid? BranchId { get; init; }
    public string? BranchName { get; init; }
    public DateTime CreatedAt { get; init; }

    public static TableResponseDto From(RestaurantTable table) => new()
    {
        Id = table.Id,
        Name = table.Name,
        AreaName = table.AreaName,
        SeatCount = table.SeatCount,
        Description = table.Description,
        IsActive = table.IsActive,
        Status = table.Status,
        QrCodeUrl = table.QrCodeUrl,
        BranchId = table.BranchId,
        BranchName = table.BranchName,
        CreatedAt = table.CreatedAt
    };
}
