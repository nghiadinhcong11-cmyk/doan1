using System;

namespace RestaurantPOS.Domain.Entities
{
    public class EmployeeSalaryProfile
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public Guid BranchId { get; set; }
        public string EmployeeType { get; set; } = "FullTime";
        public decimal MonthlySalary { get; set; }
        public decimal HourlyRate { get; set; }
        public decimal StandardWorkDays { get; set; } = 26;
        public decimal StandardWorkHours { get; set; } = 8;
        public decimal OvertimeRate { get; set; } = 1.5m;
        public decimal Allowance { get; set; }
        public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;
        public DateTime? EffectiveTo { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
