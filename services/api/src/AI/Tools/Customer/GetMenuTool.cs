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
        public string[] AllowedRoles => new[] { "admin", "customer" };
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
                if (userContext.IsGuest && (!userContext.HasVerifiedGuestSession || !userContext.BranchId.HasValue))
                    return AiToolResult.CreateError("Không xác định được chi nhánh của phiên khách.");

                string? category = AiToolValidator.GetString(arguments, "category", required: false);
                if (string.IsNullOrEmpty(category)) category = null;

                // Both authenticated Customers and verified Guest QR sessions see
                // the shared active catalog. Guest branch context remains
                // authoritative for ordering, not for menu visibility.
                var products = await _productService.GetAllProductsAsync(category);

                if (!products.Any()) return AiToolResult.CreateSuccess("Thực đơn chung hiện không có món phù hợp.");

                var sb = new StringBuilder();
                sb.AppendLine("THỰC ĐƠN CHUNG CỦA NHÀ HÀNG (món có thể khác nhau theo chi nhánh):");
                var categories = products.Select(p => p.Category).Where(c => !string.IsNullOrWhiteSpace(c)).Distinct().ToArray();
                if (categories.Length > 0) sb.AppendLine($"Danh mục: {string.Join(", ", categories)}");

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
                return AiToolResult.CreateInternalError(ex);
            }
        }
    }
}
