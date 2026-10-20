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
        public string? CustomerPhone { get; set; }
        public string? CustomerName { get; set; }
        public bool CreateMember { get; set; }
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
        /// Internal request correlation value for sanitized diagnostics only.
        /// It is never populated from client input or persisted.
        /// </summary>
        public string? DiagnosticCorrelationId { get; set; }

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
        /// Dùng cho ownership customer: không cho phép nhánh tìm kiếm theo tên
        /// làm rộng phạm vi của số điện thoại trong JWT.
        /// </summary>
        public bool ExactCustomerPhone { get; set; }

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

        // Internal UTC half-open range used by trusted server-side callers such
        // as the Admin read-only AI tool. The public date filters above remain
        // date-only for backward compatibility with existing clients.
        public DateTime? FromDateUtc { get; set; }
        public DateTime? ToDateUtc { get; set; }
    }

    /// <summary>
    /// Thông tin tối thiểu mà Customer Web được phép xem cho lịch sử đơn hàng.
    /// Không đưa các trường vận hành nội bộ (ví dụ CreatedBy) ra ngoài scope này.
    /// </summary>
    public sealed class CustomerOrderDto
    {
        public Guid Id { get; set; }
        public string? InvoiceCode { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PaymentAt { get; set; }
        public string? Status { get; set; }
        public string? PaymentMethod { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal? SubTotal { get; set; }
        public decimal Discount { get; set; }
        public decimal? VatAmount { get; set; }
        public decimal? VatPercent { get; set; }
        public decimal? ServiceFeeAmount { get; set; }
        public decimal? ServiceFeePercent { get; set; }
        public string? Note { get; set; }
        public string? TableName { get; set; }
        public Guid BranchId { get; set; }
        public string? BranchName { get; set; }
        public List<CustomerOrderDetailDto> Details { get; set; } = new();
    }

    public sealed class CustomerOrderDetailDto
    {
        public string? ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string? Options { get; set; }
    }

    /// <summary>
    /// Minimal read model used by the staff notification center for pending web orders.
    /// Keeping this separate from Order avoids sending payment and financial snapshots
    /// when the notification only needs a preview.
    /// </summary>
    public sealed class PendingOrderSummaryDto
    {
        public Guid Id { get; set; }
        public string? Status { get; set; }
        public string? CreatedBy { get; set; }
        public string? TableName { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<PendingOrderSummaryDetailDto> Details { get; set; } = new();
    }

    public sealed class PendingOrderSummaryDetailDto
    {
        public string? ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string? Options { get; set; }
    }

    public sealed class StaffOverviewOrderCountsDto
    {
        public int PendingCount { get; set; }
        public int ActiveCount { get; set; }
        public int CompletedTodayCount { get; set; }
    }

    public sealed class CustomerOrderSummaryDto
    {
        public Guid Id { get; set; }
        public string? InvoiceCode { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PaymentAt { get; set; }
        public string? Status { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
