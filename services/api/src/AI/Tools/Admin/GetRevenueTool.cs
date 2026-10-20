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
        public string[] AllowedRoles => new[] { "admin" };
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

                var scope = arguments.TryGetProperty("scope", out var scopeProperty)
                    ? scopeProperty.GetString()?.ToLowerInvariant()
                    : "all";
                var requestedBranchId = string.Equals(scope, "current", StringComparison.OrdinalIgnoreCase)
                    ? userContext.BranchId
                    : null;
                if (string.Equals(scope, "current", StringComparison.OrdinalIgnoreCase) && !requestedBranchId.HasValue)
                    return AiToolResult.CreateError("Không xác định được chi nhánh hiện tại của tài khoản Admin.");

                // Admin dashboard scope is the whole chain. For today, reuse the
                // dashboard read model so the response also explains the total by
                // branch instead of silently reporting only the central branch.
                if (period == "today" && !requestedBranchId.HasValue && string.Equals(userContext.Role, "admin", StringComparison.OrdinalIgnoreCase))
                {
                    var dashboard = await _dashboardService.GetSummaryAsync(null);
                    using var dashboardDocument = JsonDocument.Parse(JsonSerializer.Serialize(dashboard));
                    var root = dashboardDocument.RootElement;
                    var branches = root.TryGetProperty("branchRevenue", out var branchRevenue)
                        ? branchRevenue
                        : JsonSerializer.SerializeToElement(Array.Empty<object>());
                    var todayTotal = root.TryGetProperty("todayRevenue", out var todayRevenue) ? todayRevenue.GetDecimal() : 0m;
                    return AiToolResult.CreateSuccess(JsonSerializer.Serialize(new
                    {
                        Period = period,
                        Revenue = todayTotal,
                        Branches = branches
                    }));
                }

                var revenue = await _dashboardService.GetRevenueByPeriodAsync(period, requestedBranchId);

                return AiToolResult.CreateSuccess($"Doanh thu ({period}): {revenue:N0} VNĐ");
            }
            catch (ArgumentException ex)
            {
                return AiToolResult.CreateError(AiToolValidator.GetSafeArgumentError(ex));
            }
            catch (Exception ex)
            {
                return AiToolResult.CreateInternalError(ex);
            }
        }
    }
}
