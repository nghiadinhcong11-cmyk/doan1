using System;

namespace RestaurantPOS.Domain.Entities
{
    /// <summary>Chương trình ưu đãi đổi điểm hoặc giảm giá.</summary>
    public class Promotion
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int RequiredPoints { get; set; } // Điểm cần để đổi
        public decimal DiscountValue { get; set; } // Giá trị giảm (tiền hoặc %)
        public string PromotionType { get; set; } = "Gift"; // Gift, Percentage, Fixed
        public bool IsActive { get; set; } = true;
        public DateTime? EndDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
