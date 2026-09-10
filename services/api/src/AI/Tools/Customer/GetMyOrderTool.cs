using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using RestaurantPOS.AI.Models;
using RestaurantPOS.AI.Security;
using RestaurantPOS.Application.Services;

namespace RestaurantPOS.AI.Tools.Customer
{
    public class GetMyOrderTool : IAiTool
    {
        private readonly IOrderService _orderService;
        public string Name => "get_my_orders";
        public string Description => "Xem danh sách các đơn hàng gần nhất của bản thân khách hàng.";
        public string[] AllowedRoles => new[] { "customer" };
        public ToolRiskLevel RiskLevel => ToolRiskLevel.Read;

        public GetMyOrderTool(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public object GetSchema() => new { name = Name, description = Description, parameters = new { type = "object", properties = new { } } };

        public async Task<AiToolResult> ExecuteAsync(JsonElement arguments, AiUserContext userContext)
        {
            try
            {
                if (userContext.CustomerId == null && string.IsNullOrEmpty(userContext.Email) && string.IsNullOrEmpty(userContext.PhoneNumber))
                    return AiToolResult.CreateError("Không xác định được danh tính khách hàng để truy vấn đơn hàng.");

                var orders = await _orderService.GetOrdersByCustomerAsync(userContext.Email, userContext.PhoneNumber);

                if (!orders.Any()) return AiToolResult.CreateSuccess("Bạn chưa có đơn hàng nào trong hệ thống.");

                var orderStrings = orders.Select(o => $"- Đơn {o.InvoiceCode}: {o.PaidAmount:N0} VNĐ ({o.Status}) - Ngày: {o.CreatedAt:dd/MM/yyyy}");
                return AiToolResult.CreateSuccess("Các đơn hàng gần đây của bạn:\n" + string.Join("\n", orderStrings));
            }
            catch (Exception ex)
            {
                return AiToolResult.CreateError($"Lỗi khi lấy đơn hàng của bạn: {ex.Message}");
            }
        }
    }
}
