using System.Text.Json;
using RestaurantPOS.AI.Models;
using RestaurantPOS.AI.Security;
using RestaurantPOS.AI.Utils;
using RestaurantPOS.Application.Common.Security;
using RestaurantPOS.Application.Services;

namespace RestaurantPOS.AI.Tools.Admin;

public sealed class UpdateProductNameTool : IAiTool, IProductNameConfirmationTool
{
    private readonly IProductService _products;
    private readonly IProductNameConfirmationStore _confirmations;
    public UpdateProductNameTool(IProductService products, IProductNameConfirmationStore confirmations) { _products = products; _confirmations = confirmations; }
    public string Name => "update_product_name";
    public string Description => "Đổi tên một sản phẩm duy nhất sau khi quản trị viên xác nhận.";
    public string[] AllowedRoles => new[] { RoleNames.Admin };
    public ToolRiskLevel RiskLevel => ToolRiskLevel.Write;
    public object GetSchema() => new { name = Name, description = Description, parameters = new { type = "object", properties = new { productName = new { type = "string" }, newName = new { type = "string" } }, required = new[] { "productName", "newName" } } };

    public async Task<AiToolResult> ExecuteAsync(JsonElement arguments, AiUserContext userContext)
    {
        try
        {
            if (userContext.Role != RoleNames.Admin || userContext.UserId == Guid.Empty) return AiToolResult.CreateError("Bạn không có quyền đổi tên món.");
            AiToolValidator.ValidateRequired(arguments, "productName", "newName");
            var productName = AiToolValidator.GetString(arguments, "productName");
            var newName = AiToolValidator.GetString(arguments, "newName");
            if (string.Equals(productName.Trim(), newName.Trim(), StringComparison.OrdinalIgnoreCase)) return AiToolResult.CreateError("Tên mới phải khác tên hiện tại.");
            var matches = await _products.FindProductsByExactNameAsync(productName);
            if (matches.Count == 0) return AiToolResult.CreateError($"Không tìm thấy sản phẩm '{productName}'.");
            if (matches.Count > 1) return AiToolResult.CreateError($"Tên sản phẩm '{productName}' không duy nhất; vui lòng chỉ rõ sản phẩm.");
            var product = matches[0];
            var confirmation = _confirmations.Create(userContext.UserId, product, newName);
            return AiToolResult.CreateConfirmation($"Đã xác định món '{product.Name}'. Đổi tên thành '{newName.Trim()}'? Vui lòng xác nhận.", confirmation);
        }
        catch (ArgumentException ex) { return AiToolResult.CreateError(AiToolValidator.GetSafeArgumentError(ex)); }
        catch (Exception ex) { return AiToolResult.CreateInternalError(ex); }
    }

    public async Task<AiToolResult> ConfirmAsync(string token, AiUserContext userContext)
    {
        if (userContext.Role != RoleNames.Admin || userContext.UserId == Guid.Empty) return AiToolResult.CreateError("Bạn không có quyền đổi tên món.");
        if (!_confirmations.TryConsume(token, userContext.UserId, out var pending, out var error)) return AiToolResult.CreateError(error);
        var result = await _products.UpdateNameByIdAsync(pending.ProductId, pending.ExpectedCurrentName, pending.ProposedNewName);
        return result.Success ? AiToolResult.CreateSuccess(result.Message) : AiToolResult.CreateError(result.Message);
    }
}
