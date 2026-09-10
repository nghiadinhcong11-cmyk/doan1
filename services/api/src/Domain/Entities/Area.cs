using System;

namespace RestaurantPOS.Domain.Entities
{
    /// <summary>Khu vực phục vụ trong nhà hàng.</summary>
    public class Area
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
