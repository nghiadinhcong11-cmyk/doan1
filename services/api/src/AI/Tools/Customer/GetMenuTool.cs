using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using RestaurantPOS.AI.Models;
using RestaurantPOS.AI.Security;
using RestaurantPOS.Application.Services;

using RestaurantPOS.AI.Utils;

namespace RestaurantPOS.AI.Tools.Customer
{
    public class GetMenuTool : IAiTool
    {
        private readonly IProductService _productService;
        public string Name => "customer_get_menu";
        public string Description => "Xem thực đơn của nhà hàng (tên món, giá bán, kích cỡ và topping đi kèm).";
        public string[] AllowedRoles => new[] { "admin", "manager", "employee", "cashier", "kitchen", "customer" };
        public ToolRiskLevel RiskLevel => ToolRiskLevel.Read;

        public GetMenuTool(IProductService productService)
        {
            _productService = productService;
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
                    category = new { type = "string", description = "Lọc theo danh mục món ăn (Ví dụ: Đồ uống, Khai vị)." }
                }
            }
        };

        public async Task<AiToolResult> ExecuteAsync(JsonElement arguments, AiUserContext userContext)
        {
            try
            {
                string? category = AiToolValidator.GetString(arguments, "category", required: false);
                if (string.IsNullOrEmpty(category)) category = null;

                var products = await _productService.GetAllProductsAsync(category);

                if (!products.Any()) return AiToolResult.CreateSuccess("Hiện tại thực đơn chưa có món nào khả dụng.");

                var sb = new StringBuilder();
                sb.AppendLine("THỰC ĐƠN NHÀ HÀNG:");

                foreach (var p in products)
                {
                    sb.Append($"- {p.Name}: {p.Price:N0} VNĐ");

                    if (!string.IsNullOrEmpty(p.SizesJson))
                    {
                        try {
                            using var doc = JsonDocument.Parse(p.SizesJson);
                            var sizes = doc.RootElement.EnumerateArray()
                                .Select(s => $"{s.GetProperty("name").GetString()}(+{s.GetProperty("price").GetDecimal():N0}đ)")
                                .ToList();
                            if (sizes.Any()) sb.Append($" [Size: {string.Join(", ", sizes)}]");
                        } catch {}
                    }

                    if (!string.IsNullOrEmpty(p.ToppingsJson))
                    {
                        try {
                            using var doc = JsonDocument.Parse(p.ToppingsJson);
                            var toppings = doc.RootElement.EnumerateArray()
                                .Select(t => $"{t.GetProperty("name").GetString()}(+{t.GetProperty("price").GetDecimal():N0}đ)")
                                .ToList();
                            if (toppings.Any()) sb.Append($" [Topping: {string.Join(", ", toppings)}]");
                        } catch {}
                    }
                    sb.AppendLine();
                }

                return AiToolResult.CreateSuccess(sb.ToString());
            }
            catch (Exception ex)
            {
                return AiToolResult.CreateError($"Lỗi khi lấy menu: {ex.Message}");
            }
        }
    }
}
