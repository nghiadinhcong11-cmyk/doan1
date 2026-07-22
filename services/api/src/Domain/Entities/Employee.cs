using System;

namespace RestaurantPOS.Domain.Entities
{
    public class Employee
    {
        public Guid Id { get; set; }
        public string? EmployeeCode { get; set; } // Mã nhân viên (VD: NV00001)
        public string FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Position { get; set; } // Chức danh: Thu ngân, Phục vụ, Quản lý
        public string? Department { get; set; } // Phòng ban
        public string? BranchName { get; set; } // Chi nhánh làm việc
        public string? CitizenId { get; set; } // CMND/CCCD
        public DateTime? BirthDate { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public DateTime StartDate { get; set; } = DateTime.UtcNow; // Ngày bắt đầu làm việc
        public bool IsActive { get; set; } = true; // Đang làm việc / Đã nghỉ
        public decimal BasicSalary { get; set; } // Lương cơ bản
        public string? Note { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
