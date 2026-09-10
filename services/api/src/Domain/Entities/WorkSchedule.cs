using System;

namespace RestaurantPOS.Domain.Entities
{
    /// <summary>Lịch làm việc của nhân viên.</summary>
    public class WorkSchedule
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public DateTime Date { get; set; }
        public string? ShiftName { get; set; } // Sáng, Chiều, Tối
        public string? StartTime { get; set; } // "08:00"
        public string? EndTime { get; set; }   // "12:00"
        public Guid? BranchId { get; set; }    // Cho phép null để tránh lỗi 500
        public string? BranchName { get; set; } // Tên chi nhánh làm việc
    }
}
