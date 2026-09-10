using System;

namespace RestaurantPOS.AI.Models
{
    public class AiUserContext
    {
        public Guid UserId { get; set; }
        public string Role { get; set; } = string.Empty; // admin, employee, customer
        public Guid? BranchId { get; set; }
        public Guid? CustomerId { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
