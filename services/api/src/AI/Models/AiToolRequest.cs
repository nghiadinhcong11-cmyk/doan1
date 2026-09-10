using System.Text.Json;

namespace RestaurantPOS.AI.Models
{
    public class AiToolRequest
    {
        public string ToolName { get; set; } = string.Empty;
        public JsonElement Arguments { get; set; }
    }
}
