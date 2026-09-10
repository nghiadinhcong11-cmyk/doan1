using System;
using System.Collections.Generic;
using RestaurantPOS.AI.Models;
using System.Linq;

namespace RestaurantPOS.AI.Tools
{
    public class AiToolRegistry
    {
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
        /// Lấy danh sách Schema các Tool hợp lệ cho Role hiện tại để gửi lên Gemini
        /// </summary>
        public object[] GetGeminiToolsDefinition(string role)
        {
            var allowedTools = _tools.Where(t =>
                t.AllowedRoles.Contains(role.ToLower(), StringComparer.OrdinalIgnoreCase)
            ).ToList();

            if (!allowedTools.Any())
            {
                return Array.Empty<object>();
            }

            // Theo cấu trúc Gemini API: tools là một mảng chứa đối tượng có function_declarations
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
