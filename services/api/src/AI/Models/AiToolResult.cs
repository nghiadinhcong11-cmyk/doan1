namespace RestaurantPOS.AI.Models
{
    public class AiToolResult
    {
        public bool Success { get; set; }
        public string Data { get; set; } = string.Empty;
        public string? Error { get; set; }
        public AiConfirmation? Confirmation { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public Exception? InternalException { get; set; }

        public static AiToolResult CreateSuccess(string data) => new() { Success = true, Data = data };
        public static AiToolResult CreateError(string error) => new() { Success = false, Error = error };
        public static AiToolResult CreateConfirmation(string data, AiConfirmation confirmation) => new() { Success = true, Data = data, Confirmation = confirmation };
        public static AiToolResult CreateInternalError(Exception exception) => new()
        {
            Success = false,
            Error = "Không thể hoàn tất thao tác AI lúc này. Vui lòng thử lại sau.",
            InternalException = exception
        };
    }
}
