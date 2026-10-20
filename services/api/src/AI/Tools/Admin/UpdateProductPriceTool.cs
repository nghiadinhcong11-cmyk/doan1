using System.Text.Json;
using RestaurantPOS.AI.Models;
using RestaurantPOS.AI.Security;
using RestaurantPOS.AI.Utils;
using RestaurantPOS.Application.Common.Security;
using RestaurantPOS.Application.Services;

namespace RestaurantPOS.AI.Tools.Admin;

public sealed class UpdateProductPriceTool : IAiTool, IProductPriceConfirmationTool
{
    private readonly IProductService _productService;
    private readonly IProductPriceConfirmationStore _confirmationStore;

    public UpdateProductPriceTool(IProductService productService, IProductPriceConfirmationStore confirmationStore)
    {
        _productService = productService;
        _confirmationStore = confirmationStore;
    }

    public string Name => "update_product_price";
    public string Description => "Cập nhật giá bán của một sản phẩm duy nhất sau khi quản trị viên xác nhận.";
    public string[] AllowedRoles => new[] { RoleNames.Admin };
    public ToolRiskLevel RiskLevel => ToolRiskLevel.Write;

    public object GetSchema() => new
    {
        name = Name,
        description = Description,
        parameters = new
        {
            type = "object",
            properties = new
            {
                productName = new { type = "string", description = "Exacte naam van het product." },
                newPrice = new { type = "number", description = "Nieuwe verkoopprijs in VND." }
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
            if (userContext.UserId == Guid.Empty)
                return AiToolResult.CreateError("Không xác định được tài khoản quản trị viên.");

            if (arguments.ValueKind == JsonValueKind.Object &&
                (!arguments.TryGetProperty("productName", out _) || !arguments.TryGetProperty("newPrice", out _)))
            {
                var products = await _productService.GetAllProductsAsync(null);
                if (products.Count == 0)
                    return AiToolResult.CreateSuccess("Thực đơn hiện không có sản phẩm đang bán.");

                var menuLines = products.Select((product, index) =>
                    $"{index + 1}. {product.Name} — {product.Price:N0} VNĐ");
                return AiToolResult.CreateSuccess(
                    "Đây là danh sách món và giá hiện tại:\n" +
                    string.Join("\n", menuLines) +
                    "\n\nHãy nhập món và giá mới, ví dụ: Đổi giá món Phở thành 50000.");
            }

            AiToolValidator.ValidateRequired(arguments, "productName", "newPrice");
            var productName = AiToolValidator.GetString(arguments, "productName");
            var newPrice = AiToolValidator.GetDecimal(arguments, "newPrice", min: 1);
            var matches = await _productService.FindProductsByExactNameAsync(productName);
            if (matches.Count == 0)
                return AiToolResult.CreateError($"Không tìm thấy sản phẩm '{productName}'.");
            if (matches.Count > 1)
                return AiToolResult.CreateError($"Tên sản phẩm '{productName}' không duy nhất; vui lòng chỉ rõ sản phẩm.");

            var product = matches[0];
            var confirmation = _confirmationStore.Create(userContext.UserId, product, newPrice);
            return AiToolResult.CreateConfirmation(
                $"Đã xác định món '{product.Name}' (giá hiện tại {product.Price:N0}đ). Vui lòng xác nhận đổi thành {newPrice:N0}đ.",
                confirmation);
        }
        catch (ArgumentException ex)
        {
            return AiToolResult.CreateError(AiToolValidator.GetSafeArgumentError(ex));
        }
        catch (Exception ex)
        {
            return AiToolResult.CreateInternalError(ex);
        }
    }

    public async Task<AiToolResult> ConfirmAsync(string token, AiUserContext userContext)
    {
        if (!string.Equals(userContext.Role, RoleNames.Admin, StringComparison.OrdinalIgnoreCase) || userContext.UserId == Guid.Empty)
            return AiToolResult.CreateError("Bạn không có quyền cập nhật giá sản phẩm.");
        if (!_confirmationStore.TryConsume(token, userContext.UserId, out var pending, out var error))
            return AiToolResult.CreateError(error);

        var result = await _productService.UpdatePriceByIdAsync(
            pending.ProductId, pending.ExpectedCurrentPrice, pending.ProposedNewPrice);
        return result.Success
            ? AiToolResult.CreateSuccess(result.Message)
            : AiToolResult.CreateError(result.Message);
    }
}
