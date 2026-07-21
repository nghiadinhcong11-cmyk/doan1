using System;
using System.Collections.Generic;

namespace RestaurantPOS.Domain.Entities
{
    public class Order
    {
        public Guid Id { get; set; }
        public string InvoiceCode { get; set; } // Mã hóa đơn (VD: HD00001)
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string CustomerName { get; set; } = "Khách lẻ";
        public decimal TotalAmount { get; set; } // Tổng tiền hàng
        public decimal Discount { get; set; }    // Giảm giá
        public decimal PaidAmount { get; set; }  // Khách đã trả
        public string PaymentMethod { get; set; } // Tiền mặt, Thẻ, Chuyển khoản
        public string Status { get; set; } = "Hoàn thành"; // Đang xử lý, Hoàn thành, Đã hủy
        public string Note { get; set; }
        public string TableName { get; set; } // Tên bàn

        // Danh sách chi tiết món trong hóa đơn
        public List<OrderDetail> Details { get; set; } = new List<OrderDetail>();
    }

    public class OrderDetail
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal SubTotal => Quantity * UnitPrice;
    }
}
