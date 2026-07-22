using System;

namespace RestaurantPOS.Domain.Entities
{
    public class RestaurantTable
    {
        public Guid Id { get; set; }
        public string Name { get; set; } // Tên bàn (VD: Bàn 1, Bàn 2)
        public string AreaName { get; set; } // Khu vực (VD: Tầng 1, Sân vườn)
        public int SeatCount { get; set; } // Số ghế
        public string? Description { get; set; } // Ghi chú
        public bool IsActive { get; set; } = true;
        public string Status { get; set; } = "Trống"; // Trống, Có khách, Đã đặt
        public string? QrCodeUrl { get; set; } // URL ảnh mã QR để khách scan
        public Guid? BranchId { get; set; } // Liên kết với chi nhánh
        public string? BranchName { get; set; } // Tên chi nhánh (để hiển thị nhanh)
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
