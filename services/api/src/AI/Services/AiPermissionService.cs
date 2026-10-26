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
            "customer_get_best_sellers",
            "get_my_orders"
        };

        private readonly AiAuthorization _authorization;
        private static readonly IReadOnlySet<string> AdminTools =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { "get_revenue", "get_business_summary", "get_revenue_comparison", "get_best_sellers", "get_order_list", "customer_get_menu", "customer_get_best_sellers", "update_product_price", "update_product_name", "create_product", "create_topping" };

        public AiPermissionService(AiAuthorization authorization)
        {
            _authorization = authorization;
        }

        /// <summary>
        /// Kiểm tra tổng thể quyền thực thi Tool của người dùng (Role + RiskLevel)
        /// </summary>
        public async Task<bool> CanExecuteAsync(IAiTool tool, AiUserContext context)
        {
            if ((tool.RiskLevel != ToolRiskLevel.Read && tool.RiskLevel != ToolRiskLevel.Write) ||
                (tool.RiskLevel == ToolRiskLevel.Write && context.Role != RoleNames.Admin) ||
                (context.Role != RoleNames.Admin && context.Role != RoleNames.Customer))
            {
                return false;
            }

            if (context.IsGuest)
            {
                return context.HasVerifiedGuestSession && context.BranchId.HasValue &&
                       tool.RiskLevel == ToolRiskLevel.Read &&
                       _authorization.IsRoleAllowed(tool, context) &&
                       (string.Equals(tool.Name, "customer_get_menu", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(tool.Name, "customer_get_best_sellers", StringComparison.OrdinalIgnoreCase));
            }

            if (string.Equals(context.Role, "customer", StringComparison.OrdinalIgnoreCase) &&
                (!context.CustomerId.HasValue || context.CustomerId == Guid.Empty ||
                 !AuthenticatedCustomerTools.Contains(tool.Name) || tool.RiskLevel != ToolRiskLevel.Read))
            {
                return false;
            }

            if (string.Equals(context.Role, RoleNames.Admin, StringComparison.OrdinalIgnoreCase) &&
                !AdminTools.Contains(tool.Name)) return false;

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
