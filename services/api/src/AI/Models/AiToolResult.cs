namespace RestaurantPOS.AI.Models
{
    public class AiToolResult
    {
        public bool Success { get; set; }
        public string Data { get; set; } = string.Empty;
        public string? Error { get; set; }

        public static AiToolResult CreateSuccess(string data) => new() { Success = true, Data = data };
        public static AiToolResult CreateError(string error) => new() { Success = false, Error = error };
    }
}
