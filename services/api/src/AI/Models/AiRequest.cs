using System.Collections.Generic;

namespace RestaurantPOS.AI.Models
{
    public class AiRequest
    {
        public string Message { get; set; } = string.Empty;
        public List<AiConversationMessage>? History { get; set; }
    }
}
