using System;

namespace RestaurantPOS.Domain.Entities
{
    public class PayrollAdjustment
    {
        public Guid Id { get; set; }
        public Guid PayrollId { get; set; }
        public string Type { get; set; } = "Bonus"; // Bonus, Deduction, Allowance
        public decimal Amount { get; set; }
        public string Reason { get; set; } = "";
        public Guid? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
