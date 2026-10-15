using System;
using System.Text.Json;
using System.Threading.Tasks;
using RestaurantPOS.AI.Models;
using RestaurantPOS.AI.Security;
using RestaurantPOS.Application.Services;
using RestaurantPOS.Application.Common.Security;
using System.Linq;

using RestaurantPOS.AI.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace RestaurantPOS.AI.Tools.Admin
{
    public class UpdateProductPriceTool : IAiTool
    {
        private readonly IProductService _productService;
        private readonly IProductPriceConfirmationStore _confirmationStore;
        public string Name => "update_product_price";
        public string Description => "Cập nhật giá bán của một sản phẩm.";
        public string[] AllowedRoles => new[] { "admin" };
        public ToolRiskLevel RiskLevel => ToolRiskLevel.Write;

        public UpdateProductPriceTool(IProductService productService)
            : this(productService, new ProductPriceConfirmationStore())
        {
        }

        [ActivatorUtilitiesConstructor]
        public UpdateProductPriceTool(
            IProductService productService,
            IProductPriceConfirmationStore confirmationStore)
        {
            _productService = productService;
            _confirmationStore = confirmationStore;
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
                if (!string.Equals(userContext.Role, RoleNames.Admin, StringComparison.OrdinalIgnoreCase))
                    return AiToolResult.CreateError("Bạn không có quyền cập nhật giá sản phẩm.");

                AiToolValidator.ValidateRequired(arguments, "productName", "newPrice");
                string productName = AiToolValidator.GetString(arguments, "productName");
                if (userContext.UserId == Guid.Empty)
                    return AiToolResult.CreateError("Không xác định được tài khoản quản trị viên.");
                decimal newPrice = AiToolValidator.GetDecimal(arguments, "newPrice", min: 0);

                var matches = await _productService.FindProductsByExactNameAsync(productName);
                if (matches.Count == 0)
                    return AiToolResult.CreateError($"Không tìm thấy sản phẩm '{productName}'.");
                if (matches.Count > 1)
                    return AiToolResult.CreateError($"Tên sản phẩm '{productName}' không duy nhất; vui lòng chỉ rõ sản phẩm.");
                if (newPrice <= 0)
                    return AiToolResult.CreateError("Giá mới phải lớn hơn 0.");

                var product = matches.Single();
                var confirmation = _confirmationStore.Create(userContext.UserId, product, newPrice);
                return AiToolResult.CreateConfirmation(
                    $"Đã xác định món '{product.Name}' (giá hiện tại {product.Price:N0}đ). Vui lòng xác nhận đổi thành {newPrice:N0}đ.",
                    confirmation);
            }
            catch (ArgumentException ex)
            {
                return AiToolResult.CreateError(ex.Message);
            }
            catch (Exception ex)
            {
                return AiToolResult.CreateInternalError(ex);
            }
        }
    }
}
