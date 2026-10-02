using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RestaurantPOS.Application.DTOs.Financial;

namespace RestaurantPOS.Application.Services
{
    public class FinancialAnalysisService : IFinancialAnalysisService
    {
        private readonly IDashboardService _dashboardService;

        public FinancialAnalysisService(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public async Task<FinancialAnalysisDto> GetFinancialAnalysisAsync(DateTime startUtc, DateTime endUtc, DateTime prevStartUtc, DateTime prevEndUtc, Guid? branchId, string periodName)
        {
            var current = await _dashboardService.GetBusinessSummaryAsync(startUtc, endUtc, branchId);
            var previous = await _dashboardService.GetBusinessSummaryAsync(prevStartUtc, prevEndUtc, branchId);

            var growth = new FinancialGrowthDto
            {
                RevenueGrowthPercent = CalculateGrowth(current.Revenue, previous.Revenue),
                ProfitGrowthPercent = CalculateGrowth(current.NetProfit, previous.NetProfit),
                ExpenseGrowthPercent = CalculateGrowth(current.TotalExpenses, previous.TotalExpenses),
                OrderGrowthPercent = CalculateGrowth((decimal)current.OrderCount, (decimal)previous.OrderCount),
                AovGrowthPercent = CalculateGrowth(current.AverageOrderValue, previous.AverageOrderValue)
            };

            var anomalies = DetectAnomalies(current, previous, growth);

            return new FinancialAnalysisDto
            {
                Period = periodName,
                From = startUtc,
                To = endUtc,
                Current = current,
                Previous = previous,
                Growth = growth,
                Anomalies = anomalies
            };
        }

        private decimal? CalculateGrowth(decimal current, decimal previous)
        {
            if (previous == 0) return null;
            // Using Math.Abs in denominator to handle negative previous values (e.g. profit)
            return Math.Round((current - previous) / Math.Abs(previous) * 100, 2);
        }

        private List<FinancialAnomalyDto> DetectAnomalies(BusinessSummaryDto curr, BusinessSummaryDto prev, FinancialGrowthDto growth)
        {
            var anomalies = new List<FinancialAnomalyDto>();

            // 1. Revenue
            if (growth.RevenueGrowthPercent <= -20)
            {
                anomalies.Add(new FinancialAnomalyDto { Type = "RevenueSignificantDrop", Severity = "Critical", Description = $"Doanh thu giảm mạnh ({growth.RevenueGrowthPercent}%)." });
            }
            else if (growth.RevenueGrowthPercent >= 20)
            {
                anomalies.Add(new FinancialAnomalyDto { Type = "RevenueSignificantIncrease", Severity = "Info", Description = $"Doanh thu tăng trưởng tốt ({growth.RevenueGrowthPercent}%)." });
            }

            // 2. Profit vs Revenue
            if (growth.RevenueGrowthPercent >= -5 && growth.ProfitGrowthPercent <= -15)
            {
                anomalies.Add(new FinancialAnomalyDto { Type = "ProfitMarginPressure", Severity = "Warning", Description = "Doanh thu ổn định hoặc tăng nhưng lợi nhuận ước tính giảm đáng kể (>15%)." });
            }

            // 3. AOV
            if (Math.Abs(growth.AovGrowthPercent ?? 0) >= 20)
            {
                anomalies.Add(new FinancialAnomalyDto { Type = "AovSignificantChange", Severity = "Warning", Description = $"Giá trị đơn hàng trung bình (AOV) thay đổi đáng kể ({growth.AovGrowthPercent}%)." });
            }

            // 4. Expense Spikes
            foreach (var currExp in curr.ExpenseBreakdown)
            {
                var prevExp = prev.ExpenseBreakdown.FirstOrDefault(x => x.Category == currExp.Category);
                if (prevExp != null && prevExp.Amount > 0)
                {
                    var expGrowth = (currExp.Amount - prevExp.Amount) / prevExp.Amount * 100;
                    if (expGrowth >= 50)
                    {
                        anomalies.Add(new FinancialAnomalyDto { Type = "ExpenseCategorySpike", Severity = "Warning", Description = $"Chi phí nhóm '{currExp.Category}' tăng đột biến ({Math.Round(expGrowth, 1)}%)." });
                    }
                }
            }

            return anomalies;
        }
    }
}
