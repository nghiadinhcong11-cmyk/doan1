using System;

namespace RestaurantPOS.Domain.Entities
{
    public class Product
    {
        public Guid Id { get; set; }
        public string? Code { get; set; } // Mã món (VD: SP00001)
        public string? Name { get; set; } // Tên món (VD: Cà phê muối)
        public string? Category { get; set; } // Loại thực đơn (Đồ uống, Đồ ăn)
        public string? Group { get; set; } // Nhóm món (Cà phê, Trà trái cây...)
        public decimal Price { get; set; } // Giá bán
        public decimal CostPrice { get; set; } // Giá vốn (tùy chọn để tính lãi nhanh)
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; } = true; // Trạng thái kinh doanh
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
