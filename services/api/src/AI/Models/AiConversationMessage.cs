namespace RestaurantPOS.AI.Models
{
    public class AiConversationMessage
    {
        public string Role { get; set; } = string.Empty; // user, model, tool
        public string Content { get; set; } = string.Empty;
    }
}
