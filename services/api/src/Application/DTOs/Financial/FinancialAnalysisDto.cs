using System;
using System.Collections.Generic;
using RestaurantPOS.Application.Services;

namespace RestaurantPOS.Application.DTOs.Financial
{
    public class FinancialAnalysisDto
    {
        public string Period { get; set; } = string.Empty;
        public DateTime From { get; set; }
        public DateTime To { get; set; }

        public BusinessSummaryDto Current { get; set; } = new();
        public BusinessSummaryDto Previous { get; set; } = new();

        public FinancialGrowthDto Growth { get; set; } = new();
        public List<FinancialAnomalyDto> Anomalies { get; set; } = new();
    }

    public class FinancialGrowthDto
    {
        public decimal? RevenueGrowthPercent { get; set; }
        public decimal? ProfitGrowthPercent { get; set; }
        public decimal? ExpenseGrowthPercent { get; set; }
        public decimal? OrderGrowthPercent { get; set; }
        public decimal? AovGrowthPercent { get; set; }
    }

    public class FinancialAnomalyDto
    {
        public string Type { get; set; } = string.Empty;
        public string Severity { get; set; } = "Warning"; // Warning, Critical, Info
        public string Description { get; set; } = string.Empty;
    }
}
