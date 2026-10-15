namespace RestaurantPOS.Application.DTOs;

public sealed class TableRequestDto
{
    public string Name { get; init; } = string.Empty;
    public Guid AreaId { get; init; }
    public int SeatCount { get; init; }
    public string? Description { get; init; }
    public bool IsActive { get; init; } = true;
    public string? Status { get; init; }
    public string? QrCodeUrl { get; init; }
    public Guid BranchId { get; init; }
    public string? BranchName { get; init; }
}
