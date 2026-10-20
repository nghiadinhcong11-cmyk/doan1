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

    }
}
