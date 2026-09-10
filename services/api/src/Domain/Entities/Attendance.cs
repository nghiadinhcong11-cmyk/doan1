using System;

namespace RestaurantPOS.Domain.Entities
{
    /// <summary>Bản ghi chấm công của nhân viên.</summary>
    public class Attendance
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public DateTime CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public Guid BranchId { get; set; }
        public string? Status { get; set; } // Đúng giờ, Muộn, Về sớm...
        public string? Note { get; set; }
        public string? ImagePath { get; set; } // Lưu ảnh check-in nếu cần
    }
}
