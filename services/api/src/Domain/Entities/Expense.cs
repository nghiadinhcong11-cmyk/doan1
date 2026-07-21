using System;

namespace RestaurantPOS.Domain.Entities
{
    public class Expense
    {
        public Guid Id { get; set; }
        public string Description { get; set; } // Nội dung chi
        public decimal Amount { get; set; } // Số tiền
        public DateTime ExpenseDate { get; set; } = DateTime.Now;
        public string Category { get; set; } // Loại chi: Tiền nhà, Điện nước, Lương, Nhập hàng...
        public string PaymentMethod { get; set; } // Tiền mặt, Chuyển khoản
        public string Note { get; set; } // Ghi chú thêm
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
