using System;

namespace RestaurantPOS.Domain.Entities
{
    /// <summary>Khu vực phục vụ trong nhà hàng.</summary>
    public class Area
    {
        public Guid Id { get; set; }
        // Nullable during phase 1 so existing rows can be backfilled safely.
        public Guid BranchId { get; set; }
        public Branch? Branch { get; set; }
        public string Name { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
