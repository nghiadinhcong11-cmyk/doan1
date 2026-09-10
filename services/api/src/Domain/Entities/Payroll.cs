using System;
using System.Collections.Generic;

namespace RestaurantPOS.Domain.Entities
{
    public class Payroll
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public Guid BranchId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public string EmployeeType { get; set; } = "FullTime";
        public decimal StandardWorkDays { get; set; }
        public decimal ActualWorkDays { get; set; }
        public decimal TotalHours { get; set; }
        public decimal RegularHours { get; set; }
        public decimal OvertimeHours { get; set; }
        public decimal BaseSalary { get; set; }
        public decimal HourlyRate { get; set; }
        public decimal RegularPay { get; set; }
        public decimal OvertimePay { get; set; }
        public decimal Allowance { get; set; }
        public decimal Bonus { get; set; }
        public decimal Deduction { get; set; }
        public decimal GrossSalary { get; set; }
        public decimal NetSalary { get; set; }
        public string Status { get; set; } = "Draft";
        public DateTime? CalculatedAt { get; set; }
        public DateTime? LockedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public List<PayrollAdjustment> Adjustments { get; set; } = new();
    }
}
