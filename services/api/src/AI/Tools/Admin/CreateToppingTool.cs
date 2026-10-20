using System.Text.Json;
using RestaurantPOS.AI.Models;
using RestaurantPOS.AI.Security;
using RestaurantPOS.AI.Utils;
using RestaurantPOS.Application.Common.Security;
using RestaurantPOS.Application.DTOs.Catalog;
using RestaurantPOS.Application.Services;

namespace RestaurantPOS.AI.Tools.Admin;

public sealed class CreateToppingTool : IAiTool, ICatalogCreationConfirmationTool
{
    private readonly IToppingService _toppings;
    private readonly ICatalogCreationConfirmationStore _confirmations;
    private readonly ILogger<CreateToppingTool> _logger;
    public CreateToppingTool(IToppingService toppings, ICatalogCreationConfirmationStore confirmations, ILogger<CreateToppingTool> logger) { _toppings = toppings; _confirmations = confirmations; _logger = logger; }
    public string Name => "create_topping";
    public string Description => "Tạo topping mới sau khi quản trị viên xác nhận bản xem trước.";
    public string[] AllowedRoles => new[] { RoleNames.Admin };
    public ToolRiskLevel RiskLevel => ToolRiskLevel.Write;
    public object GetSchema() => new { name = Name, description = Description, parameters = new { type = "object", properties = new { toppingName = new { type = "string" }, price = new { type = "number" }, costPrice = new { type = "number" }, toppingType = new { type = "string" }, category = new { type = "string" }, description = new { type = "string" } }, required = new[] { "toppingName", "price", "toppingType", "category" } } };

    public async Task<AiToolResult> ExecuteAsync(JsonElement arguments, AiUserContext userContext)
    {
        try
        {
            if (userContext.Role != RoleNames.Admin || userContext.UserId == Guid.Empty) return AiToolResult.CreateError("Bạn không có quyền tạo topping.");
            AiToolValidator.ValidateRequired(arguments, "toppingName", "price", "toppingType", "category");
            var request = new CreateToppingRequest(
                AiToolValidator.GetString(arguments, "toppingName"),
                AiToolValidator.GetDecimal(arguments, "price", min: 1),
                arguments.TryGetProperty("costPrice", out _) ? AiToolValidator.GetDecimal(arguments, "costPrice", min: 0) : 0,
                AiToolValidator.GetString(arguments, "toppingType"), AiToolValidator.GetString(arguments, "category"),
                arguments.TryGetProperty("description", out _) ? AiToolValidator.GetString(arguments, "description") : null);
            var preview = _confirmations.CreateTopping(userContext.UserId, request);
            return AiToolResult.CreateConfirmation($"Bản xem trước topping mới: {request.Name} — giá {request.Price:N0}đ, giá vốn {request.CostPrice:N0}đ, loại {request.ToppingType}, nhóm {request.Category}. Vui lòng xác nhận để tạo.", preview);
        }
        catch (ArgumentException ex) { return AiToolResult.CreateError(AiToolValidator.GetSafeArgumentError(ex)); }
        catch (Exception ex) { return AiToolResult.CreateInternalError(ex); }
    }

    public async Task<AiToolResult> ConfirmAsync(string token, AiUserContext userContext)
    {
        if (userContext.Role != RoleNames.Admin || userContext.UserId == Guid.Empty) return AiToolResult.CreateError("Bạn không có quyền tạo topping.");
        if (!_confirmations.TryConsume(token, userContext.UserId, "topping-create", out var pending, out var error)) return AiToolResult.CreateError(error);
        try
        {
            var result = await _toppings.CreateToppingAsync(pending.Topping!);
            return result.Success ? AiToolResult.CreateSuccess(result.Message) : AiToolResult.CreateError(result.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI catalog topping creation failed after confirmation.");
            return AiToolResult.CreateInternalError(ex);
        }
    }
}
