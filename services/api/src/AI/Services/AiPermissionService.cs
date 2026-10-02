using System;
using System.Threading.Tasks;
using RestaurantPOS.AI.Models;
using RestaurantPOS.AI.Tools;
using RestaurantPOS.AI.Security;

namespace RestaurantPOS.AI.Services
{
    public class AiPermissionService
    {
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
            // Managers are branch-bound. A malformed or incomplete JWT must never
            // turn a manager AI request into an unscoped/global query.
            if (string.Equals(context.Role, "manager", StringComparison.OrdinalIgnoreCase) && !context.BranchId.HasValue)
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
