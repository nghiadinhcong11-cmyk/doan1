using System;
using System.Collections.Generic;
using RestaurantPOS.AI.Models;
using System.Linq;
using RestaurantPOS.Application.Common.Security;
using RestaurantPOS.AI.Security;

namespace RestaurantPOS.AI.Tools
{
    public class AiToolRegistry
    {
        private static readonly IReadOnlySet<string> AuthenticatedCustomerTools =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "customer_get_menu",
                "get_my_orders"
            };

        private readonly IEnumerable<IAiTool> _tools;

        public AiToolRegistry(IEnumerable<IAiTool> tools)
        {
            _tools = tools;
        }

        /// <summary>
        /// Tìm Tool theo tên định danh
        /// </summary>
        public IAiTool? GetTool(string name)
        {
            return _tools.FirstOrDefault(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Lấy tất cả Tool hiện có trong hệ thống
        /// </summary>
        public IEnumerable<IAiTool> GetAllTools()
        {
            return _tools;
        }

        /// <summary>
        /// Lấy danh sách schema các Tool hợp lệ cho Role hiện tại để gửi cho AI.
        /// </summary>
        public object[] GetToolsDefinition(
            string role,
            bool readOnlyOnly = false,
            IReadOnlySet<string>? allowedToolNames = null)
        {
            if (!RoleNames.IsKnownRole(role))
            {
                return Array.Empty<object>();
            }

            var normalizedRole = role.ToLowerInvariant();
            var allowedTools = _tools.Where(t =>
                t.AllowedRoles.Contains(normalizedRole, StringComparer.OrdinalIgnoreCase) &&
                (!readOnlyOnly || t.RiskLevel == ToolRiskLevel.Read) &&
                (normalizedRole != "customer" || AuthenticatedCustomerTools.Contains(t.Name)) &&
                (allowedToolNames is null || allowedToolNames.Contains(t.Name))
            ).ToList();

            if (!allowedTools.Any())
            {
                return Array.Empty<object>();
            }

            // The existing function-calling contract uses a function_declarations array.
            return new object[]
            {
                new
                {
                    function_declarations = allowedTools.Select(t => t.GetSchema()).ToArray()
                }
            };
        }
    }
}
