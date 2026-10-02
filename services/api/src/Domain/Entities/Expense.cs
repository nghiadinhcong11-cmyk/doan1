using System;

namespace RestaurantPOS.Domain.Entities
{
    /// <summary>Khoản chi của nhà hàng.</summary>
    public class Expense
    {
        public Guid Id { get; set; }
        public Guid BranchId { get; set; }
        public string Description { get; set; } // Nội dung chi
        public decimal Amount { get; set; } // Số tiền
        public DateTime ExpenseDate { get; set; } = DateTime.Now;
        public string Category { get; set; } // Loại chi phí do người dùng ghi nhận
        // Expenses.PaymentMethod is NOT NULL in the existing PostgreSQL schema.
        public string PaymentMethod { get; set; } = RestaurantPOS.Domain.Finance.ExpensePaymentMethods.Cash;
        // Expenses.Note is also NOT NULL in the existing PostgreSQL schema.
        public string Note { get; set; } = string.Empty; // Ghi chú thêm
        public Guid? CreatedBy { get; set; }
        public Guid? StockReceiptId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public StockReceipt? StockReceipt { get; set; }
    }
}
