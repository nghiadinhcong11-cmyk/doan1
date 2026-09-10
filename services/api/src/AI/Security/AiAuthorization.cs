using System;
using System.Linq;
using RestaurantPOS.AI.Models;
using RestaurantPOS.AI.Tools;

namespace RestaurantPOS.AI.Security
{
    public class AiAuthorization
    {
        /// <summary>
        /// Kiểm tra quyền cơ bản dựa trên Role được định nghĩa trong Tool
        /// </summary>
        public bool IsRoleAllowed(IAiTool tool, AiUserContext context)
        {
            if (string.IsNullOrEmpty(context.Role)) return false;

            return tool.AllowedRoles.Any(r =>
                r.Equals(context.Role, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Kiểm tra quyền dựa trên mức độ rủi ro của Tool (READ, WRITE, DESTRUCTIVE)
        /// </summary>
        public bool IsRiskLevelAllowed(IAiTool tool, AiUserContext context)
        {
            var role = context.Role?.ToLowerInvariant() ?? "";

            switch (tool.RiskLevel)
            {
                case ToolRiskLevel.Read:
                    return true;

                case ToolRiskLevel.Write:
                    // Khách hàng chỉ được dùng write nếu tool đó được cấp phép rõ ràng
                    if (role == "customer")
                    {
                        return tool.AllowedRoles.Contains("customer", StringComparer.OrdinalIgnoreCase);
                    }
                    return role == "admin" || role == "manager" || role == "cashier" || role == "kitchen" || role == "employee";

                case ToolRiskLevel.Destructive:
                    // Các thao tác xóa/hủy nguy hiểm chỉ dành cho admin
                    return role == "admin";

                default:
                    return false;
            }
        }

        /// <summary>
        /// Kiểm tra quyền truy cập dữ liệu theo phạm vi (Branch/Ownership)
        /// </summary>
        public bool IsInAuthorizedBranch(AiUserContext context, Guid? dataBranchId)
        {
            if (string.Equals(context.Role, "admin", StringComparison.OrdinalIgnoreCase)) return true;
            if (dataBranchId == null) return true;

            return context.BranchId == dataBranchId;
        }
    }
}
