using System;

namespace RestaurantPOS.Application.DTOs.Customers
{
    public class CustomerResponseDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? Gender { get; set; }
        public DateTime? Birthday { get; set; }
        public int LoyaltyPoints { get; set; }
        public decimal TotalSpending { get; set; }
        public int TotalOrders { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CustomerGroup { get; set; }
    }

    public class CustomerMinimalDto
    {
        public bool Exists { get; set; }
        public string? FullName { get; set; }
    }
}
