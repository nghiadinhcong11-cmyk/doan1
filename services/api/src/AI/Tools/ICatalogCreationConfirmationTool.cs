using RestaurantPOS.AI.Models;

namespace RestaurantPOS.AI.Tools;

public interface ICatalogCreationConfirmationTool
{
    Task<AiToolResult> ConfirmAsync(string token, AiUserContext userContext);
}
