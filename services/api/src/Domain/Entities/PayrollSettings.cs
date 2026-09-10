using System;

namespace RestaurantPOS.Domain.Entities
{
    public class PayrollSettings
    {
        public Guid Id { get; set; }
        public Guid? BranchId { get; set; }
        public decimal StandardWorkDays { get; set; } = 26;
        public decimal StandardWorkHoursPerDay { get; set; } = 8;
        public decimal FullTimeDeductionPerMissingDay { get; set; }
        public decimal DefaultPartTimeHourlyRate { get; set; }
        public bool OvertimeEnabled { get; set; } = true;
        public decimal OvertimeMultiplier { get; set; } = 1.5m;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
