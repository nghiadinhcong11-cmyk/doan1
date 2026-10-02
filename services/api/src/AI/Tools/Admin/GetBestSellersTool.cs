using System;
using System.Text.Json;
using System.Threading.Tasks;
using RestaurantPOS.AI.Models;
using RestaurantPOS.AI.Security;
using RestaurantPOS.AI.Utils;
using RestaurantPOS.Application.Services;

namespace RestaurantPOS.AI.Tools.Admin
{
    public class GetBestSellersTool : IAiTool
    {
        private readonly IDashboardService _dashboardService;
        public string Name => "get_best_sellers";
        public string Description => "Xác định các món bán chạy nhất trong một khoảng thời gian.";
        public string[] AllowedRoles => new[] { "admin", "manager", "employee", "cashier", "kitchen" };
        public ToolRiskLevel RiskLevel => ToolRiskLevel.Read;

        public GetBestSellersTool(IDashboardService dashboardService)
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
                    period = new { type = "string", description = "Khoảng thời gian: today, yesterday, week, month." },
                    limit = new { type = "integer", description = "Số lượng món (mặc định 5, tối đa 20)." }
                },
                required = new[] { "period" }
            }
        };

        public async Task<AiToolResult> ExecuteAsync(JsonElement arguments, AiUserContext userContext)
        {
            try
            {
                if (userContext.Role == "customer")
                    return AiToolResult.CreateError("Quyền truy cập bị từ chối.");

                string period = AiToolValidator.GetString(arguments, "period").ToLower();

                int limit = 5;
                if (arguments.TryGetProperty("limit", out var limitProp) && limitProp.ValueKind == JsonValueKind.Number)
                {
                    limit = limitProp.GetInt32();
                    if (limit < 1) limit = 1;
                    if (limit > 20) limit = 20;
                }

                var vnZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                var nowVn = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnZone);
                DateTime startVn, endVn;

                switch (period)
                {
                    case "yesterday":
                        startVn = nowVn.Date.AddDays(-1);
                        endVn = nowVn.Date;
                        break;
                    case "week":
                        startVn = nowVn.Date.AddDays(-(int)nowVn.DayOfWeek);
                        endVn = startVn.AddDays(7);
                        break;
                    case "month":
                        startVn = new DateTime(nowVn.Year, nowVn.Month, 1);
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

                var bestSellers = await _dashboardService.GetBestSellersAsync(startUtc, endUtc, userContext.BranchId, limit);

                if (bestSellers.Count == 0)
                    return AiToolResult.CreateSuccess($"Không có dữ liệu bán hàng trong khoảng thời gian {period}.");

                return AiToolResult.CreateSuccess(JsonSerializer.Serialize(bestSellers));
            }
            catch (Exception ex)
            {
                return AiToolResult.CreateError($"Lỗi khi lấy danh sách bán chạy: {ex.Message}");
            }
        }
    }
}
