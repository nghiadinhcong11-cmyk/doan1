using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using RestaurantPOS.AI.Models;
using RestaurantPOS.AI.Security;
using RestaurantPOS.Application.Services;

namespace RestaurantPOS.AI.Tools.Employee
{
    public class GetTableSummaryTool : IAiTool
    {
        private readonly ITableService _tableService;
        public string Name => "employee_get_table_summary";
        public string Description => "Xem thống kê tình trạng các bàn trong nhà hàng (Trống, Có khách, Đã đặt).";
        public string[] AllowedRoles => new[] { "admin", "manager", "employee" };
        public ToolRiskLevel RiskLevel => ToolRiskLevel.Read;

        public GetTableSummaryTool(ITableService tableService)
        {
            _tableService = tableService;
        }

        public object GetSchema() => new { name = Name, description = Description, parameters = new { type = "object", properties = new { } } };

        public async Task<AiToolResult> ExecuteAsync(JsonElement arguments, AiUserContext userContext)
        {
            try
            {
                var branchId = (userContext.Role == "employee" || userContext.Role == "manager" || userContext.Role == "cashier" || userContext.Role == "kitchen")
                    ? userContext.BranchId
                    : null;

                var stats = await _tableService.GetTableStatusCountsAsync(branchId);

                if (!stats.Any()) return AiToolResult.CreateSuccess("Hiện chưa có dữ liệu bàn nào.");

                var result = string.Join(", ", stats.Select(s => $"{s.Key}: {s.Value}"));
                return AiToolResult.CreateSuccess($"Tình trạng bàn hiện tại: {result}");
            }
            catch (Exception ex)
            {
                return AiToolResult.CreateError($"Lỗi khi lấy tình trạng bàn: {ex.Message}");
            }
        }
    }
}
