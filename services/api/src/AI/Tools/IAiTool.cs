using System.Text.Json;
using System.Threading.Tasks;
using RestaurantPOS.AI.Models;
using RestaurantPOS.AI.Security;

namespace RestaurantPOS.AI.Tools
{
    public interface IAiTool
    {
        string Name { get; }
        string Description { get; }
        string[] AllowedRoles { get; }
        ToolRiskLevel RiskLevel { get; }

        /// <summary>
        /// Trả về schema JSON cho function declaration của AI contract.
        /// </summary>
        object GetSchema();

        Task<AiToolResult> ExecuteAsync(
            JsonElement arguments,
            AiUserContext userContext);
    }
}
