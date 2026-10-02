using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using RestaurantPOS.AI.Models;
using RestaurantPOS.AI.Security;
using RestaurantPOS.AI.Utils;
using RestaurantPOS.Application.Services;

namespace RestaurantPOS.AI.Tools.Admin
{
    public class GetFinancialAnalysisTool : IAiTool
    {
        private readonly IFinancialAnalysisService _financialService;

        public string Name => "get_financial_analysis";
        public string Description => "Phân tích tình hình tài chính chi tiết (Doanh thu, Chi phí, Lợi nhuận, Tăng trưởng, Bất thường) cho một khoảng thời gian.";
        public string[] AllowedRoles => new[] { "admin", "manager" };
        public ToolRiskLevel RiskLevel => ToolRiskLevel.Read;

        public GetFinancialAnalysisTool(IFinancialAnalysisService financialService)
        {
            _financialService = financialService;
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
                    period = new { type = "string", description = "Khoảng thời gian: today, yesterday, this_week, last_week, this_month, last_month." }
                },
                required = new[] { "period" }
            }
        };

        public async Task<AiToolResult> ExecuteAsync(JsonElement arguments, AiUserContext userContext)
        {
            try
            {
                string period = AiToolValidator.GetString(arguments, "period").ToLower();

                var vnZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                var nowVn = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnZone);

                var ranges = GetCurrentAndPreviousRanges(period, nowVn);

                var startUtc = TimeZoneInfo.ConvertTimeToUtc(ranges.Current.Start, vnZone);
                var endUtc = TimeZoneInfo.ConvertTimeToUtc(ranges.Current.End, vnZone);
                var prevStartUtc = TimeZoneInfo.ConvertTimeToUtc(ranges.Previous.Start, vnZone);
                var prevEndUtc = TimeZoneInfo.ConvertTimeToUtc(ranges.Previous.End, vnZone);

                var analysis = await _financialService.GetFinancialAnalysisAsync(
                    startUtc, endUtc, prevStartUtc, prevEndUtc, userContext.BranchId, period);

                return AiToolResult.CreateSuccess(JsonSerializer.Serialize(new
                {
                    Period = period,
                    From = ranges.Current.Start.ToString("dd/MM/yyyy HH:mm"),
                    To = ranges.Current.End.ToString("dd/MM/yyyy HH:mm"),
                    ComparisonFrom = ranges.Previous.Start.ToString("dd/MM/yyyy HH:mm"),
                    ComparisonTo = ranges.Previous.End.ToString("dd/MM/yyyy HH:mm"),
                    Financials = new
                    {
                        Revenue = analysis.Current.Revenue,
                        Orders = analysis.Current.OrderCount,
                        AOV = analysis.Current.AverageOrderValue,
                        TotalExpenses = analysis.Current.TotalExpenses,
                        EstimatedCOGS = analysis.Current.CostOfGoodsSold,
                        EstimatedNetProfit = analysis.Current.NetProfit,
                        EstimatedProfitMargin = analysis.Current.ProfitMargin
                    },
                    Growth = new
                    {
                        RevenueGrowth = analysis.Growth.RevenueGrowthPercent,
                        ProfitGrowth = analysis.Growth.ProfitGrowthPercent,
                        ExpenseGrowth = analysis.Growth.ExpenseGrowthPercent,
                        OrderGrowth = analysis.Growth.OrderGrowthPercent,
                        AovGrowth = analysis.Growth.AovGrowthPercent
                    },
                    ExpenseBreakdown = analysis.Current.ExpenseBreakdown,
                    Anomalies = analysis.Anomalies,
                    TopProducts = analysis.Current.TopProducts,
                    Caveats = new[]
                    {
                        "Lợi nhuận và giá vốn là số liệu ước tính dựa trên giá vốn hiện tại của sản phẩm.",
                        "Lịch sử giá vốn (historical cost) chưa được áp dụng.",
                        "Báo cáo chi phí chỉ phản ánh các khoản đã được ghi nhận trong module Chi phí."
                    }
                }));
            }
            catch (Exception ex)
            {
                return AiToolResult.CreateError($"Lỗi khi phân tích tài chính: {ex.Message}");
            }
        }

        private ( (DateTime Start, DateTime End) Current, (DateTime Start, DateTime End) Previous ) GetCurrentAndPreviousRanges(string period, DateTime nowVn)
        {
            DateTime start, end, prevStart, prevEnd;

            switch (period)
            {
                case "yesterday":
                    start = nowVn.Date.AddDays(-1);
                    end = nowVn.Date;
                    prevStart = start.AddDays(-1);
                    prevEnd = start;
                    break;
                case "this_week":
                    start = nowVn.Date.AddDays(-(int)nowVn.DayOfWeek);
                    end = start.AddDays(7);
                    prevStart = start.AddDays(-7);
                    prevEnd = start;
                    break;
                case "last_week":
                    start = nowVn.Date.AddDays(-(int)nowVn.DayOfWeek - 7);
                    end = start.AddDays(7);
                    prevStart = start.AddDays(-7);
                    prevEnd = start;
                    break;
                case "this_month":
                    start = new DateTime(nowVn.Year, nowVn.Month, 1);
                    end = start.AddMonths(1);
                    prevStart = start.AddMonths(-1);
                    prevEnd = start;
                    break;
                case "last_month":
                    start = new DateTime(nowVn.Year, nowVn.Month, 1).AddMonths(-1);
                    end = start.AddMonths(1);
                    prevStart = start.AddMonths(-1);
                    prevEnd = start;
                    break;
                case "today":
                default:
                    start = nowVn.Date;
                    end = start.AddDays(1);
                    prevStart = start.AddDays(-1);
                    prevEnd = start;
                    break;
            }

            return ((start, end), (prevStart, prevEnd));
        }
    }
}
