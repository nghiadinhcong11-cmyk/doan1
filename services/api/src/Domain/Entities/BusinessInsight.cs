using System;

namespace RestaurantPOS.Domain.Entities;

public class BusinessInsight
{
    public Guid Id { get; set; }
    public Guid? BranchId { get; set; }
    public string Type { get; set; } = string.Empty; // e.g., RevenueSignificantDrop
    public string Severity { get; set; } = string.Empty; // Info, Warning, Critical
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string? AiExplanation { get; set; }
    public string? AiRecommendation { get; set; }
    public string EvidenceJson { get; set; } = string.Empty;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public DateTime? ComparisonPeriodStart { get; set; }
    public DateTime? ComparisonPeriodEnd { get; set; }
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Unread"; // Unread, Read, Resolved
    public string DeduplicationKey { get; set; } = string.Empty; // BranchId:Type:Period
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
