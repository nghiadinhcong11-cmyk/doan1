using System;
using System.Collections.Generic;

namespace RestaurantPOS.Domain.Entities
{
    /// <summary>Một đợt yêu cầu chế biến được gửi tới bếp.</summary>
    public class OrderRequest
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public int RequestNumber { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Preparing, Ready, Completed, Cancelled
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? AcceptedAt { get; set; }
        public DateTime? PreparingAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? ProcessedBy { get; set; }
        public string? Note { get; set; }

        // Danh sách các món trong đợt gọi này
        public List<OrderRequestItem> Items { get; set; } = new List<OrderRequestItem>();
    }

    /// <summary>Món thuộc một đợt yêu cầu chế biến.</summary>
    public class OrderRequestItem
    {
        public Guid Id { get; set; }
        public Guid OrderRequestId { get; set; }
        public Guid? ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public string? Note { get; set; }
        public string? Options { get; set; }
    }
}
