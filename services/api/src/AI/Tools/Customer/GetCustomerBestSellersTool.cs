using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using RestaurantPOS.AI.Models;
using RestaurantPOS.AI.Security;
using RestaurantPOS.Application.Common.Security;
using RestaurantPOS.Application.Services;

namespace RestaurantPOS.AI.Tools.Customer;

public sealed class GetCustomerBestSellersTool : IAiTool
{
    private readonly IDashboardService _dashboardService;

    public GetCustomerBestSellersTool(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    public string Name => "customer_get_best_sellers";
    public string Description => "Gợi ý các món được gọi nhiều nhất tại chi nhánh hiện tại.";
    public string[] AllowedRoles => new[] { RoleNames.Customer };
    public ToolRiskLevel RiskLevel => ToolRiskLevel.Read;

    public object GetSchema() => new
    {
        name = Name,
        description = Description,
        parameters = new
        {
            type = "object",
            properties = new
            {
                limit = new { type = "integer", description = "Số món muốn gợi ý, mặc định 5 và tối đa 5." }
            }
        }
    };

    public async Task<AiToolResult> ExecuteAsync(JsonElement arguments, AiUserContext userContext)
    {
        if (!userContext.BranchId.HasValue || userContext.BranchId.Value == Guid.Empty)
            return AiToolResult.CreateError("Chưa xác định được chi nhánh hiện tại để gợi ý món.");

        try
        {
            var limit = 5;
            if (arguments.ValueKind == JsonValueKind.Object &&
                arguments.TryGetProperty("limit", out var limitProperty) &&
                limitProperty.ValueKind == JsonValueKind.Number)
            {
                limit = Math.Clamp(limitProperty.GetInt32(), 1, 5);
            }

            var vietnamZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var nowVietnam = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamZone);
            var startUtc = TimeZoneInfo.ConvertTimeToUtc(nowVietnam.Date, vietnamZone);
            var endUtc = startUtc.AddDays(1);
            var bestSellers = await _dashboardService.GetBestSellersAsync(startUtc, endUtc, userContext.BranchId, limit);

            if (bestSellers.Count == 0)
                return AiToolResult.CreateSuccess("Hôm nay chi nhánh chưa có đủ dữ liệu bán hàng để gợi ý món.");

            var lines = bestSellers
                .Select((product, index) => $"{index + 1}. {product.ProductName} — đã được gọi {product.TotalQuantity} phần")
                .ToList();
            return AiToolResult.CreateSuccess(
                "Nếu bạn muốn thử món được nhiều khách chọn hôm nay, mình gợi ý:\n" +
                string.Join("\n", lines) +
                "\n\nĐây là gợi ý dựa trên số lượt bán, không phải đánh giá chất lượng tuyệt đối.");
        }
        catch (Exception ex)
        {
            return AiToolResult.CreateInternalError(ex);
        }
    }
}
