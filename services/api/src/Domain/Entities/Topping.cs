using System;

namespace RestaurantPOS.Domain.Entities
{
    /// <summary>Topping có thể thêm vào món.</summary>
    public class Topping
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal CostPrice { get; set; } // Giá vốn
        public string? ToppingType { get; set; } // Topping Đồ uống, Topping Đồ ăn...
        public string? Category { get; set; } // Nhóm (VD: Trân châu, Thạch...)
        public string? Description { get; set; } // Mô tả topping
        public string? ImageUrl { get; set; } // Ảnh minh họa (nếu có)
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
