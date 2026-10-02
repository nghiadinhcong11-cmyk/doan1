using System;
using System.Collections.Generic;

namespace RestaurantPOS.Application.DTOs.Financial;

public class BusinessInsightListDto
{
    public Guid Id { get; set; }
    public Guid? BranchId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime DetectedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class BusinessInsightDetailDto : BusinessInsightListDto
{
    public string? AiExplanation { get; set; }
    public string? AiRecommendation { get; set; }
    public object? Evidence { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public DateTime? ComparisonPeriodStart { get; set; }
    public DateTime? ComparisonPeriodEnd { get; set; }
}
