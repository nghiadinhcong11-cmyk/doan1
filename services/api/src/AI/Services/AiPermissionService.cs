using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestaurantPOS.AI.Models;
using RestaurantPOS.AI.Tools;
using RestaurantPOS.AI.Security;
using RestaurantPOS.Application.Common.Security;

namespace RestaurantPOS.AI.Services
{
    public class AiPermissionService
    {
    private static readonly IReadOnlySet<string> AuthenticatedCustomerTools =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "customer_get_menu",
            "get_my_orders"
        };

        private readonly AiAuthorization _authorization;

        public AiPermissionService(AiAuthorization authorization)
        {
            _authorization = authorization;
        }

        /// <summary>
        /// Kiểm tra tổng thể quyền thực thi Tool của người dùng (Role + RiskLevel)
        /// </summary>
        public async Task<bool> CanExecuteAsync(IAiTool tool, AiUserContext context)
        {
            if (!RoleNames.IsKnownRole(context.Role))
            {
                return false;
            }

            if (context.IsGuest)
            {
                return tool.RiskLevel == ToolRiskLevel.Read &&
                       string.Equals(tool.Name, "customer_get_menu", StringComparison.OrdinalIgnoreCase);
            }

            if (string.Equals(context.Role, "customer", StringComparison.OrdinalIgnoreCase) &&
                !AuthenticatedCustomerTools.Contains(tool.Name))
            {
                return false;
            }

            // Every branch-scoped staff role must remain scoped. Null is never a
            // valid global scope for staff AI queries.
            if (RoleNames.IsBranchScopedStaffRole(context.Role) && !context.BranchId.HasValue)
            {
                return false;
            }

            // 1. Kiểm tra Role có trong AllowedRoles của Tool không
            if (!_authorization.IsRoleAllowed(tool, context))
            {
                return false;
            }

            // 2. Kiểm tra mức độ rủi ro (READ, WRITE, DESTRUCTIVE)
            if (!_authorization.IsRiskLevelAllowed(tool, context))
            {
                return false;
            }

            await Task.CompletedTask;
            return true;
        }
    }
}
