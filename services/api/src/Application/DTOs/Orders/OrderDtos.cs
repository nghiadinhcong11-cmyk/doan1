using System;
using System.Collections.Generic;

namespace RestaurantPOS.Application.DTOs.Orders
{
    public class AcceptWebOrderDto
    {
        public Guid? TableId { get; set; }
    }

    public class OrderKitchenRequestStatusDto
    {
        public Guid Id { get; set; }
        public int RequestNumber { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<OrderKitchenRequestItemStatusDto> Items { get; set; } = new();
    }

    public class OrderKitchenRequestItemStatusDto
    {
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string? Options { get; set; }
    }

    public class OrderPaymentDto
    {
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
    }

    public class OrderRedeemDto
    {
        public Guid CustomerId { get; set; }
        public int Points { get; set; }
    }

    /// <summary>
    /// DTO cập nhật trạng thái hoặc thông tin cơ bản của đơn hàng
    /// </summary>
    public class OrderUpdateDto
    {
        /// <summary>
        /// Trạng thái đơn hàng (Đang xử lý, Hoàn thành, Đã hủy)
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Người tạo / Nhân viên cập nhật
        /// </summary>
        public string? CreatedBy { get; set; }

        /// <summary>
        /// Tên chi nhánh phục vụ
        /// </summary>
        public string? BranchName { get; set; }

        /// <summary>
        /// Mã định danh chi nhánh
        /// </summary>
        public Guid? BranchId { get; set; }
    }

    /// <summary>
    /// Bộ lọc tìm kiếm và tra cứu danh sách đơn hàng
    /// </summary>
    public class OrderQueryFilter
    {
        /// <summary>
        /// Từ khóa tìm kiếm (theo mã đơn, tên món, tên khách)
        /// </summary>
        public string? Search { get; set; }

        /// <summary>
        /// Lọc theo trạng thái đơn hàng
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Số điện thoại khách hàng
        /// </summary>
        public string? CustomerPhone { get; set; }

        /// <summary>
        /// Lọc theo ID chi nhánh
        /// </summary>
        public Guid? BranchId { get; set; }

        /// <summary>
        /// Tên bàn (VD: Bàn 01)
        /// </summary>
        public string? TableName { get; set; }

        /// <summary>
        /// Ngày bắt đầu (định dạng yyyy-MM-dd)
        /// </summary>
        public string? FromDate { get; set; }

        /// <summary>
        /// Ngày kết thúc (định dạng yyyy-MM-dd)
        /// </summary>
        public string? ToDate { get; set; }
    }
}
