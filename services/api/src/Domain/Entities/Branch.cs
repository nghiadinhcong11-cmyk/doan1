using System;

namespace RestaurantPOS.Domain.Entities
{
    /// <summary>Thông tin chi nhánh nhà hàng.</summary>
    public class Branch
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsMain { get; set; }
        public bool IsActive { get; set; } = true;
        public string? BankName { get; set; }
        public string? AccountNumber { get; set; }
        public string? AccountHolder { get; set; }
        public string? ImageUrl { get; set; }
        public string? TaxCode { get; set; }
        public string? RepresentativeName { get; set; }
        public string? RepresentativeEmail { get; set; }
        public string? Industry { get; set; }
        public string? BusinessType { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
