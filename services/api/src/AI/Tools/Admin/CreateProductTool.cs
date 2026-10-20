using System.Text.Json;
using RestaurantPOS.AI.Models;
using RestaurantPOS.AI.Security;
using RestaurantPOS.AI.Utils;
using RestaurantPOS.Application.DTOs.Catalog;
using RestaurantPOS.Application.Services;

namespace RestaurantPOS.AI.Tools.Admin;

public sealed class CreateProductTool : IAiTool, ICatalogCreationConfirmationTool
{
    private readonly IProductService _products;
    private readonly ICatalogCreationConfirmationStore _confirmations;
    private readonly ILogger<CreateProductTool> _logger;
    public CreateProductTool(IProductService products, ICatalogCreationConfirmationStore confirmations, ILogger<CreateProductTool> logger) { _products = products; _confirmations = confirmations; _logger = logger; }
    public string Name => "create_product";
    public string Description => "Tạo món mới sau khi quản trị viên xác nhận bản xem trước.";
    public string[] AllowedRoles => new[] { RestaurantPOS.Application.Common.Security.RoleNames.Admin };
    public RestaurantPOS.AI.Security.ToolRiskLevel RiskLevel => RestaurantPOS.AI.Security.ToolRiskLevel.Write;
    public object GetSchema() => new { name = Name, description = Description, parameters = new { type = "object", properties = new { productName = new { type = "string" }, price = new { type = "number" }, costPrice = new { type = "number" }, category = new { type = "string" }, group = new { type = "string" } }, required = new[] { "productName", "price", "category", "group" } } };

    public async Task<AiToolResult> ExecuteAsync(JsonElement arguments, AiUserContext userContext)
    {
        try
        {
            if (userContext.Role != RestaurantPOS.Application.Common.Security.RoleNames.Admin || userContext.UserId == Guid.Empty) return AiToolResult.CreateError("Bạn không có quyền tạo món.");
            AiToolValidator.ValidateRequired(arguments, "productName", "price", "category", "group");
            var request = new CreateProductRequest(
                AiToolValidator.GetString(arguments, "productName"),
                AiToolValidator.GetDecimal(arguments, "price", min: 1),
                arguments.TryGetProperty("costPrice", out _) ? AiToolValidator.GetDecimal(arguments, "costPrice", min: 0) : 0,
                AiToolValidator.GetString(arguments, "category"), AiToolValidator.GetString(arguments, "group"));
            var preview = _confirmations.CreateProduct(userContext.UserId, request);
            return AiToolResult.CreateConfirmation($"Bản xem trước món mới: {request.Name} — giá {request.Price:N0}đ, giá vốn {request.CostPrice:N0}đ, loại {request.Category}, nhóm {request.Group}. Vui lòng xác nhận để tạo.", preview);
        }
        catch (ArgumentException ex) { return AiToolResult.CreateError(AiToolValidator.GetSafeArgumentError(ex)); }
        catch (Exception ex) { return AiToolResult.CreateInternalError(ex); }
    }

    public async Task<AiToolResult> ConfirmAsync(string token, AiUserContext userContext)
    {
        if (userContext.Role != RestaurantPOS.Application.Common.Security.RoleNames.Admin || userContext.UserId == Guid.Empty) return AiToolResult.CreateError("Bạn không có quyền tạo món.");
        if (!_confirmations.TryConsume(token, userContext.UserId, "product-create", out var pending, out var error)) return AiToolResult.CreateError(error);
        try
        {
            var result = await _products.CreateProductAsync(pending.Product!);
            return result.Success ? AiToolResult.CreateSuccess(result.Message) : AiToolResult.CreateError(result.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI catalog product creation failed after confirmation.");
            return AiToolResult.CreateInternalError(ex);
        }
    }
}
