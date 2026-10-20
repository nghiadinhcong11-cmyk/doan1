using System;
using System.Linq;
using System.Text.Json;
using System.Globalization;
using System.Threading.Tasks;
using RestaurantPOS.AI.Models;
using RestaurantPOS.AI.Security;
using RestaurantPOS.AI.Utils;
using RestaurantPOS.Application.DTOs.Orders;
using RestaurantPOS.Application.Services;
using RestaurantPOS.Domain.Entities;

namespace RestaurantPOS.AI.Tools.Admin
{
    public class GetOrderListTool : IAiTool
    {
        private readonly IOrderService _orderService;
        public string Name => "get_order_list";
        public string Description => "Truy vấn danh sách đơn hàng chi tiết theo thời gian và trạng thái.";
        public string[] AllowedRoles => new[] { "admin" };
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
                var fromDateUtc = ParseUtcBoundary(arguments, "fromDateUtc");
                var toDateUtc = ParseUtcBoundary(arguments, "toDateUtc");
                if (fromDateUtc.HasValue && toDateUtc.HasValue && fromDateUtc.Value >= toDateUtc.Value)
                    return AiToolResult.CreateError("Khoảng thời gian UTC không hợp lệ.");
                string? status = AiToolValidator.GetString(arguments, "status", required: false);

                var filter = new OrderQueryFilter
                {
                    FromDate = fromDate,
                    ToDate = toDate,
                    FromDateUtc = fromDateUtc,
                    ToDateUtc = toDateUtc,
                    Status = status,
                    // Admin order queries are chain-wide, including when the JWT
                    // also carries a branch claim for other application flows.
                    BranchId = null
                };

                // Admin có thể xem toàn hệ thống nếu branchId trong context là null
                if (userContext.Role == "admin")
                {
                     // Giữ nguyên branchId từ context (có thể null)
                }

                var orders = await _orderService.GetOrdersAsync(filter);
                var results = orders.Take(limit).Select(o => CreateRoleScopedResult(o, userContext.Role));

                if (!results.Any()) return AiToolResult.CreateSuccess("Không tìm thấy đơn hàng nào phù hợp.");

                return AiToolResult.CreateSuccess(JsonSerializer.Serialize(results));
            }
            catch (Exception ex)
            {
                return AiToolResult.CreateInternalError(ex);
            }
        }

        private static DateTime? ParseUtcBoundary(JsonElement arguments, string propertyName)
        {
            var value = AiToolValidator.GetString(arguments, propertyName, required: false);
            return DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var parsed)
                ? parsed.UtcDateTime
                : null;
        }

        private static object CreateRoleScopedResult(Order order, string role)
        {
            if (CanViewPaymentAmounts(role))
            {
                return new
                {
                    order.InvoiceCode,
                    order.CreatedAt,
                    order.TotalAmount,
                    order.PaidAmount,
                    order.Status,
                    order.TableName,
                    order.BranchName
                };
            }

            return new
            {
                order.InvoiceCode,
                order.CreatedAt,
                order.Status,
                order.TableName,
                order.BranchName
            };
        }

        private static bool CanViewPaymentAmounts(string role) =>
            string.Equals(role, "admin", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(role, "manager", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(role, "cashier", StringComparison.OrdinalIgnoreCase);
    }
}
