using System;
using System.ComponentModel.DataAnnotations;

namespace RestaurantPOS.Domain.Entities
{
    /// <summary>Ca làm việc và số liệu doanh thu của nhân viên.</summary>
    public class Shift
    {
        [Key]
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public Guid BranchId { get; set; }
        public string? BranchName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public decimal StartingCash { get; set; } // Tiền mặt đầu ca
        public decimal? EndingCash { get; set; }   // Tiền mặt thực tế khi chốt
        public decimal TotalRevenue { get; set; } // Tổng doanh thu (Máy tính)
        public decimal CashRevenue { get; set; }  // Doanh thu tiền mặt
        public decimal TransferRevenue { get; set; } // Doanh thu chuyển khoản
        public string Status { get; set; }        // Open, Closed
        public string Note { get; set; }
    }
}
