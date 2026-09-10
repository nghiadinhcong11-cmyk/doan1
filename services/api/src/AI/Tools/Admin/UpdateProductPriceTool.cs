using System;
using System.Text.Json;
using System.Threading.Tasks;
using RestaurantPOS.AI.Models;
using RestaurantPOS.AI.Security;
using RestaurantPOS.Application.Services;

using RestaurantPOS.AI.Utils;

namespace RestaurantPOS.AI.Tools.Admin
{
    public class UpdateProductPriceTool : IAiTool
    {
        private readonly IProductService _productService;
        public string Name => "update_product_price";
        public string Description => "Cập nhật giá bán của một sản phẩm.";
        public string[] AllowedRoles => new[] { "admin" };
        public ToolRiskLevel RiskLevel => ToolRiskLevel.Write;

        public UpdateProductPriceTool(IProductService productService)
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
                    productName = new { type = "string", description = "Tên sản phẩm cần đổi giá." },
                    newPrice = new { type = "number", description = "Giá bán mới." }
                },
                required = new[] { "productName", "newPrice" }
            }
        };

        public async Task<AiToolResult> ExecuteAsync(JsonElement arguments, AiUserContext userContext)
        {
            try
            {
                AiToolValidator.ValidateRequired(arguments, "productName", "newPrice");
                string productName = AiToolValidator.GetString(arguments, "productName");
                decimal newPrice = AiToolValidator.GetDecimal(arguments, "newPrice", min: 0);

                var (success, message, oldPrice, updatedPrice) = await _productService.UpdatePriceByNameAsync(productName, newPrice);
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
                return AiToolResult.CreateError($"Lỗi khi cập nhật giá: {ex.Message}");
            }
        }
    }
}
