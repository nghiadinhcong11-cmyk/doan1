using System;
using System.ComponentModel.DataAnnotations;

namespace RestaurantPOS.Domain.Entities
{
    /// <summary>Thông tin đặt bàn của khách hàng.</summary>
    public class Reservation
    {
        [Key]
        public Guid Id { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public DateTime ReservationTime { get; set; }
        public int NumberOfGuests { get; set; }
        public Guid? BranchId { get; set; }
        public string? BranchName { get; set; }
        public string? Note { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Confirmed, Cancelled, Completed
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Guid? TableId { get; set; }
        public string? TableName { get; set; }
    }
}
