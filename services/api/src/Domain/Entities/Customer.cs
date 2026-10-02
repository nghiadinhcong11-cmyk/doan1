using System;

namespace RestaurantPOS.Domain.Entities
{
    /// <summary>Thông tin hồ sơ và điểm tích lũy của khách hàng.</summary>
    public class Customer
    {
        public Guid Id { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public DateTime? Birthday { get; set; }
        public string? Gender { get; set; } // Nam, Nữ, Khác
        public string? CustomerGroup { get; set; } = "Khách lẻ"; // Khách VIP, Khách quen, Khách lẻ
        public decimal TotalSpending { get; set; } = 0;
        public int TotalOrders { get; set; } = 0;
        public int LoyaltyPoints { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public string? Password { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastOrderDate { get; set; }
    }
}
