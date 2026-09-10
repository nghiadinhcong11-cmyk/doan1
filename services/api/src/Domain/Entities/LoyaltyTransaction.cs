using System;

namespace RestaurantPOS.Domain.Entities
{
    public class LoyaltyTransaction
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid? OrderId { get; set; }
        public string Type { get; set; } // Earn, Redeem, Adjustment, Refund, Reversal
        public int Points { get; set; }
        public int BalanceAfter { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
