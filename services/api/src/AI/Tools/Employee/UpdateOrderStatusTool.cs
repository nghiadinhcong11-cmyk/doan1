using System;
using System.Text.Json;
using System.Threading.Tasks;
using RestaurantPOS.AI.Models;
using RestaurantPOS.AI.Security;
using RestaurantPOS.Application.Services;

using RestaurantPOS.AI.Utils;

namespace RestaurantPOS.AI.Tools.Employee
{
    public class UpdateOrderStatusTool : IAiTool
    {
        private readonly IOrderService _orderService;
        public string Name => "update_order_status";
        public string Description => "Cập nhật trạng thái của một đơn hàng (Ví dụ: Đang chế biến, Đã phục vụ, Hoàn thành).";
        public string[] AllowedRoles => new[] { "admin", "manager", "employee", "cashier", "kitchen" };
        public ToolRiskLevel RiskLevel => ToolRiskLevel.Write;

        public UpdateOrderStatusTool(IOrderService orderService)
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
                    orderCode = new { type = "string", description = "Mã đơn hàng cần cập nhật." },
                    newStatus = new { type = "string", description = "Trạng thái mới: Processing, Served, Completed, Cancelled." }
                },
                required = new[] { "orderCode", "newStatus" }
            }
        };

        public async Task<AiToolResult> ExecuteAsync(JsonElement arguments, AiUserContext userContext)
        {
            try
            {
                AiToolValidator.ValidateRequired(arguments, "orderCode", "newStatus");
                string orderCode = AiToolValidator.GetString(arguments, "orderCode");
                string newStatus = AiToolValidator.GetString(arguments, "newStatus");

                var (success, message) = await _orderService.UpdateOrderStatusByCodeAsync(orderCode, newStatus, userContext.BranchId, userContext.Role);
                if (!success)
                {
                    return AiToolResult.CreateError(message);
                }

                return AiToolResult.CreateSuccess(message);
            }
            catch (ArgumentException ex)
            {
                return AiToolResult.CreateError(ex.Message);
            }
            catch (Exception ex)
            {
                return AiToolResult.CreateError($"Lỗi khi cập nhật trạng thái đơn: {ex.Message}");
            }
        }
    }
}
