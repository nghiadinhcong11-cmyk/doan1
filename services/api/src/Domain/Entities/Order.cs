using System;
using System.Collections.Generic;

namespace RestaurantPOS.Domain.Entities
{
    /// <summary>Đơn hàng và thông tin thanh toán của khách.</summary>
    public class Order
    {
        public Guid Id { get; set; }
        public string? InvoiceCode { get; set; } // Mã hóa đơn (VD: HD00001)
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PaymentAt { get; set; }
        public Guid? CustomerId { get; set; } // ID khách hàng liên kết
        public string? CustomerName { get; set; } = "Khách lẻ";
        public string? CustomerPhone { get; set; } // Số điện thoại khách hàng
        public string? CustomerEmail { get; set; } // Email khách hàng
        // Nullable snapshots preserve the fact that historical orders were created
        // before P0.2. New/updated orders always receive values from OrderService.
        public decimal? SubTotal { get; set; }    // Tiền hàng trước thuế/phí
        public decimal? VatAmount { get; set; }   // Tiền thuế VAT
        public decimal? VatPercent { get; set; }  // % Thuế VAT tại thời điểm tạo
        public decimal? ServiceFeeAmount { get; set; } // Tiền phí dịch vụ
        public decimal? ServiceFeePercent { get; set; } // % Phí dịch vụ tại thời điểm tạo
        public decimal TotalAmount { get; set; } // Tổng tiền thanh toán cuối cùng
        public decimal Discount { get; set; }    // Giảm giá (nếu có)
        public decimal PaidAmount { get; set; }  // Khách đã trả
        public string? PaymentMethod { get; set; } // Tiền mặt, Thẻ, Chuyển khoản
        public string Status { get; set; } = "Đang xử lý"; // Đang xử lý, Hoàn thành, Đã hủy
        public string? Note { get; set; }
        public string? TableName { get; set; } // Tên bàn
        public string? CreatedBy { get; set; } // Người tạo hóa đơn
        public Guid? BranchId { get; set; }    // ID chi nhánh
        public string? BranchName { get; set; } // Tên chi nhánh

        // Danh sách chi tiết món trong hóa đơn
        public List<OrderDetail> Details { get; set; } = new List<OrderDetail>();
    }

    /// <summary>Một món trong đơn hàng.</summary>
    public class OrderDetail
    {
        public Guid Id { get; set; }
        public Guid? ProductId { get; set; }
        public Guid? ToppingId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public int SentQuantity { get; set; } // Số lượng đã gửi bếp
        public decimal UnitPrice { get; set; }
        public string? Options { get; set; } // Size, Topping chosen
        public decimal SubTotal => Quantity * UnitPrice;
    }
}
