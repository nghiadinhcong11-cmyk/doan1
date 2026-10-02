using System;
using System.Text.Json;
using System.Threading.Tasks;
using RestaurantPOS.AI.Models;
using RestaurantPOS.AI.Security;
using RestaurantPOS.AI.Utils;
using RestaurantPOS.Application.Services;

namespace RestaurantPOS.AI.Tools.Admin
{
    public class GetRevenueComparisonTool : IAiTool
    {
        private readonly IDashboardService _dashboardService;
        public string Name => "get_revenue_comparison";
        public string Description => "So sánh doanh thu giữa hai khoảng thời gian để phân tích tăng trưởng.";
        public string[] AllowedRoles => new[] { "admin", "manager" };
        public ToolRiskLevel RiskLevel => ToolRiskLevel.Read;

        public GetRevenueComparisonTool(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public object GetSchema() => new
        {
            name = Name,
            description = Description,
            parameters = new
            {
                type = "object",
                properties = new
                {
                    currentPeriod = new { type = "string", description = "Kỳ hiện tại: today, this_week, this_month." },
                    comparisonPeriod = new { type = "string", description = "Kỳ so sánh: yesterday, last_week, last_month." }
                },
                required = new[] { "currentPeriod", "comparisonPeriod" }
            }
        };

        public async Task<AiToolResult> ExecuteAsync(JsonElement arguments, AiUserContext userContext)
        {
            try
            {
                string current = AiToolValidator.GetString(arguments, "currentPeriod").ToLower();
                string comparison = AiToolValidator.GetString(arguments, "comparisonPeriod").ToLower();

                var vnZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                var nowVn = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnZone);

                var currentRange = GetRange(current, nowVn);
                var comparisonRange = GetRange(comparison, nowVn);

                var currentData = await _dashboardService.GetRevenueReportAsync(
                    TimeZoneInfo.ConvertTimeToUtc(currentRange.start, vnZone),
                    TimeZoneInfo.ConvertTimeToUtc(currentRange.end, vnZone),
                    userContext.BranchId);

                var comparisonData = await _dashboardService.GetRevenueReportAsync(
                    TimeZoneInfo.ConvertTimeToUtc(comparisonRange.start, vnZone),
                    TimeZoneInfo.ConvertTimeToUtc(comparisonRange.end, vnZone),
                    userContext.BranchId);

                decimal diff = currentData.Revenue - comparisonData.Revenue;
                decimal? percent = null;
                if (comparisonData.Revenue > 0)
                {
                    percent = (diff / comparisonData.Revenue) * 100;
                }

                string direction = diff > 0 ? "up" : (diff < 0 ? "down" : "unchanged");

                var result = new
                {
                    Current = new { Period = current, Revenue = currentData.Revenue, Orders = currentData.OrderCount, From = currentRange.start, To = currentRange.end },
                    Comparison = new { Period = comparison, Revenue = comparisonData.Revenue, Orders = comparisonData.OrderCount, From = comparisonRange.start, To = comparisonRange.end },
                    Analysis = new { Difference = diff, Percentage = percent.HasValue ? Math.Round(percent.Value, 2) : (decimal?)null, Direction = direction }
                };

                return AiToolResult.CreateSuccess(JsonSerializer.Serialize(result));
            }
            catch (Exception ex)
            {
                return AiToolResult.CreateError($"Lỗi khi so sánh doanh thu: {ex.Message}");
            }
        }

        private (DateTime start, DateTime end) GetRange(string period, DateTime nowVn)
        {
            return period switch
            {
                "today" => (nowVn.Date, nowVn.Date.AddDays(1)),
                "yesterday" => (nowVn.Date.AddDays(-1), nowVn.Date),
                "this_week" => (nowVn.Date.AddDays(-(int)nowVn.DayOfWeek), nowVn.Date.AddDays(1)),
                "last_week" => (nowVn.Date.AddDays(-(int)nowVn.DayOfWeek - 7), nowVn.Date.AddDays(-(int)nowVn.DayOfWeek)),
                "this_month" => (new DateTime(nowVn.Year, nowVn.Month, 1), nowVn.Date.AddDays(1)),
                "last_month" => (new DateTime(nowVn.Year, nowVn.Month, 1).AddMonths(-1), new DateTime(nowVn.Year, nowVn.Month, 1)),
                _ => (nowVn.Date, nowVn.Date.AddDays(1))
            };
        }
    }
}
