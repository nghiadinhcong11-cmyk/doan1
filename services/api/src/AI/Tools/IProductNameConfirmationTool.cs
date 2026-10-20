using RestaurantPOS.AI.Models;

namespace RestaurantPOS.AI.Tools;

public interface IProductNameConfirmationTool
{
    Task<AiToolResult> ConfirmAsync(string token, AiUserContext userContext);
}
