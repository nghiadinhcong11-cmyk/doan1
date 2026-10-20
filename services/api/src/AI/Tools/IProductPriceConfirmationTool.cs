using RestaurantPOS.AI.Models;

namespace RestaurantPOS.AI.Tools;

public interface IProductPriceConfirmationTool
{
    Task<AiToolResult> ConfirmAsync(string token, AiUserContext userContext);
}
