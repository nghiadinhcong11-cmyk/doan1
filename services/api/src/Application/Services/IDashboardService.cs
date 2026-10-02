using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantPOS.Application.Services
{
    public interface IDashboardService
    {
        Task<object> GetSummaryAsync(string? branchId);
        Task<decimal> GetRevenueByPeriodAsync(string period, Guid? branchId = null);
        Task<List<TopProductDto>> GetBestSellersAsync(DateTime startUtc, DateTime endUtc, Guid? branchId, int limit);
        Task<(decimal Revenue, int OrderCount)> GetRevenueReportAsync(DateTime startUtc, DateTime endUtc, Guid? branchId);
        Task<BusinessSummaryDto> GetBusinessSummaryAsync(DateTime startUtc, DateTime endUtc, Guid? branchId);
        void InvalidateSummaryCache();
    }

    public class BusinessSummaryDto
    {
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
        public decimal AverageOrderValue { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal CostOfGoodsSold { get; set; }
        public decimal NetProfit { get; set; }
        public decimal ProfitMargin { get; set; }
        public List<ExpenseCategoryDto> ExpenseBreakdown { get; set; } = new();
        public List<TopProductDto> TopProducts { get; set; } = new();
    }

    public class ExpenseCategoryDto
    {
        public string Category { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal Percentage { get; set; }
    }

    public class TopProductDto
    {
        public string ProductName { get; set; } = string.Empty;
        public int TotalQuantity { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
