namespace RestaurantPOS.AI.Models
{
    public class AiResponse
    {
        public string Message { get; set; } = string.Empty;
        public bool Success { get; set; } = true;
        public string? Error { get; set; }
    }
}
