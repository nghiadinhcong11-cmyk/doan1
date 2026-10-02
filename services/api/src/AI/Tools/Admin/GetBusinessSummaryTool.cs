using System;
using System.Text.Json;
using System.Threading.Tasks;
using RestaurantPOS.AI.Models;
using RestaurantPOS.AI.Security;
using RestaurantPOS.AI.Utils;
using RestaurantPOS.Application.Services;

namespace RestaurantPOS.AI.Tools.Admin
{
    public class GetBusinessSummaryTool : IAiTool
    {
        private readonly IDashboardService _dashboardService;
        public string Name => "get_business_summary";
        public string Description => "Lấy bản tóm tắt KPI kinh doanh (Doanh thu, Số đơn, AOV, Lợi nhuận ước tính) cho một khoảng thời gian.";
        public string[] AllowedRoles => new[] { "admin", "manager" };
        public ToolRiskLevel RiskLevel => ToolRiskLevel.Read;

        public GetBusinessSummaryTool(IDashboardService dashboardService)
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
                DateTime startVn, endVn;

                switch (period)
                {
                    case "yesterday":
                        startVn = nowVn.Date.AddDays(-1);
                        endVn = nowVn.Date;
                        break;
                    case "this_week":
                        startVn = nowVn.Date.AddDays(-(int)nowVn.DayOfWeek);
                        endVn = startVn.AddDays(7);
                        break;
                    case "last_week":
                        startVn = nowVn.Date.AddDays(-(int)nowVn.DayOfWeek - 7);
                        endVn = startVn.AddDays(7);
                        break;
                    case "this_month":
                        startVn = new DateTime(nowVn.Year, nowVn.Month, 1);
                        endVn = startVn.AddMonths(1);
                        break;
                    case "last_month":
                        startVn = new DateTime(nowVn.Year, nowVn.Month, 1).AddMonths(-1);
                        endVn = startVn.AddMonths(1);
                        break;
                    case "today":
                    default:
                        startVn = nowVn.Date;
                        endVn = startVn.AddDays(1);
                        break;
                }

                var startUtc = TimeZoneInfo.ConvertTimeToUtc(startVn, vnZone);
                var endUtc = TimeZoneInfo.ConvertTimeToUtc(endVn, vnZone);

                var summary = await _dashboardService.GetBusinessSummaryAsync(startUtc, endUtc, userContext.BranchId);

                return AiToolResult.CreateSuccess(JsonSerializer.Serialize(new
                {
                    Period = period,
                    From = startVn.ToString("dd/MM/yyyy HH:mm"),
                    To = endVn.ToString("dd/MM/yyyy HH:mm"),
                    KPIs = new
                    {
                        summary.Revenue,
                        summary.OrderCount,
                        summary.AverageOrderValue,
                        summary.TotalExpenses,
                        EstimatedCOGS = summary.CostOfGoodsSold,
                        EstimatedNetProfit = summary.NetProfit
                    },
                    TopProducts = summary.TopProducts,
                    Note = "Lợi nhuận và giá vốn là số liệu ước tính dựa trên giá vốn hiện tại của sản phẩm, không tính đến sự thay đổi giá vốn trong lịch sử."
                }));
            }
            catch (Exception ex)
            {
                return AiToolResult.CreateError($"Lỗi khi lấy tóm tắt kinh doanh: {ex.Message}");
            }
        }
    }
}
