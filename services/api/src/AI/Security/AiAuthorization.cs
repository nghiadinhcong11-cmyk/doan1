using RestaurantPOS.AI.Models;
using RestaurantPOS.AI.Tools;
using RestaurantPOS.Application.Common.Security;

namespace RestaurantPOS.AI.Security;

public class AiAuthorization
{
    public bool IsRoleAllowed(IAiTool tool, AiUserContext context) =>
        (context.Role == RoleNames.Admin || context.Role == RoleNames.Customer) &&
        tool.AllowedRoles.Contains(context.Role, StringComparer.OrdinalIgnoreCase);

    public bool IsRiskLevelAllowed(IAiTool tool, AiUserContext context) =>
        tool.RiskLevel == ToolRiskLevel.Read ||
        (tool.RiskLevel == ToolRiskLevel.Write && context.Role == RoleNames.Admin &&
         (string.Equals(tool.Name, "update_product_price", StringComparison.OrdinalIgnoreCase) ||
          string.Equals(tool.Name, "update_product_name", StringComparison.OrdinalIgnoreCase) ||
          string.Equals(tool.Name, "create_product", StringComparison.OrdinalIgnoreCase) ||
          string.Equals(tool.Name, "create_topping", StringComparison.OrdinalIgnoreCase)));

    public bool IsInAuthorizedBranch(AiUserContext context, Guid? dataBranchId) =>
        context.Role == RoleNames.Admin || !dataBranchId.HasValue || context.BranchId == dataBranchId;
}
