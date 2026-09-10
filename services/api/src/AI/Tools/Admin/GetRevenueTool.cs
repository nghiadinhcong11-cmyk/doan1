using System;
using System.Text.Json;
using System.Threading.Tasks;
using RestaurantPOS.AI.Models;
using RestaurantPOS.AI.Security;
using RestaurantPOS.Application.Services;

using RestaurantPOS.AI.Utils;

namespace RestaurantPOS.AI.Tools.Admin
{
    public class GetRevenueTool : IAiTool
    {
        private readonly IDashboardService _dashboardService;
        public string Name => "get_revenue";
        public string Description => "Lấy doanh thu nhà hàng theo thời gian (today, month, year).";
        public string[] AllowedRoles => new[] { "admin", "manager" };
        public ToolRiskLevel RiskLevel => ToolRiskLevel.Read;

        public GetRevenueTool(IDashboardService dashboardService)
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
                    period = new
                    {
                        type = "string",
                        description = "Khoảng thời gian cần xem: today, month hoặc year."
                    }
                },
                required = new[] { "period" }
            }
        };

        public async Task<AiToolResult> ExecuteAsync(JsonElement arguments, AiUserContext userContext)
        {
            try
            {
                AiToolValidator.ValidateRequired(arguments, "period");
                string period = AiToolValidator.GetString(arguments, "period").ToLower();

                if (period != "today" && period != "month" && period != "year")
                {
                    return AiToolResult.CreateError("Giá trị period không hợp lệ. Phải là today, month hoặc year.");
                }

                var revenue = await _dashboardService.GetRevenueByPeriodAsync(period, userContext.BranchId);

                return AiToolResult.CreateSuccess($"Doanh thu ({period}): {revenue:N0} VNĐ");
            }
            catch (ArgumentException ex)
            {
                return AiToolResult.CreateError(ex.Message);
            }
            catch (Exception ex)
            {
                return AiToolResult.CreateError($"Lỗi khi lấy doanh thu: {ex.Message}");
            }
        }
    }
}
