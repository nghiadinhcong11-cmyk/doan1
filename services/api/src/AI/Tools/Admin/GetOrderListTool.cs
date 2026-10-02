using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using RestaurantPOS.AI.Models;
using RestaurantPOS.AI.Security;
using RestaurantPOS.AI.Utils;
using RestaurantPOS.Application.DTOs.Orders;
using RestaurantPOS.Application.Services;

namespace RestaurantPOS.AI.Tools.Admin
{
    public class GetOrderListTool : IAiTool
    {
        private readonly IOrderService _orderService;
        public string Name => "get_order_list";
        public string Description => "Truy vấn danh sách đơn hàng chi tiết theo thời gian và trạng thái.";
        public string[] AllowedRoles => new[] { "admin", "manager", "employee", "cashier", "kitchen" };
        public ToolRiskLevel RiskLevel => ToolRiskLevel.Read;

        public GetOrderListTool(IOrderService orderService)
        {
            _orderService = orderService;
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
                    fromDate = new { type = "string", description = "Ngày bắt đầu (yyyy-MM-dd)." },
                    toDate = new { type = "string", description = "Ngày kết thúc (yyyy-MM-dd)." },
                    status = new { type = "string", description = "Trạng thái: Hoàn thành, Đang xử lý, Đã hủy." },
                    limit = new { type = "integer", description = "Số lượng tối đa (1-50)." }
                }
            }
        };

        public async Task<AiToolResult> ExecuteAsync(JsonElement arguments, AiUserContext userContext)
        {
            try
            {
                if (userContext.Role == "customer")
                    return AiToolResult.CreateError("Khách hàng không có quyền truy cập danh sách đơn hàng tổng hợp.");

                int limit = 10;
                if (arguments.TryGetProperty("limit", out var limitProp) && limitProp.ValueKind == JsonValueKind.Number)
                {
                    limit = limitProp.GetInt32();
                    if (limit < 1) limit = 1;
                    if (limit > 50) limit = 50;
                }

                string? fromDate = AiToolValidator.GetString(arguments, "fromDate", required: false);
                string? toDate = AiToolValidator.GetString(arguments, "toDate", required: false);
                string? status = AiToolValidator.GetString(arguments, "status", required: false);

                var filter = new OrderQueryFilter
                {
                    FromDate = fromDate,
                    ToDate = toDate,
                    Status = status,
                    BranchId = userContext.BranchId
                };

                // Admin có thể xem toàn hệ thống nếu branchId trong context là null
                if (userContext.Role == "admin")
                {
                     // Giữ nguyên branchId từ context (có thể null)
                }

                var orders = await _orderService.GetOrdersAsync(filter);
                var results = orders.Take(limit).Select(o => new
                {
                    o.InvoiceCode,
                    o.CreatedAt,
                    o.TotalAmount,
                    o.PaidAmount,
                    o.Status,
                    o.TableName,
                    o.BranchName
                });

                if (!results.Any()) return AiToolResult.CreateSuccess("Không tìm thấy đơn hàng nào phù hợp.");

                return AiToolResult.CreateSuccess(JsonSerializer.Serialize(results));
            }
            catch (Exception ex)
            {
                return AiToolResult.CreateError($"Lỗi khi lấy danh sách đơn hàng: {ex.Message}");
            }
        }
    }
}
