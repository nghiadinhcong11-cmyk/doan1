using System;

namespace RestaurantPOS.Domain.Entities;

public class SystemSetting
{
    public Guid Id { get; set; }
    public Guid? BranchId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Type { get; set; } = "string";
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
