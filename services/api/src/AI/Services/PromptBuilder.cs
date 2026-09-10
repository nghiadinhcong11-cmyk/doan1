using RestaurantPOS.AI.Models;
using RestaurantPOS.AI.Prompts;

namespace RestaurantPOS.AI.Services
{
    public class PromptBuilder
    {
        /// <summary>
        /// Xây dựng System Instruction dựa trên Role của người dùng
        /// </summary>
        public string BuildSystemInstruction(AiUserContext context, string contextData)
        {
            return context.Role.ToLower() switch
            {
                "admin" => AdminPrompt.GetPrompt(contextData),
                "manager" => EmployeePrompt.GetPrompt(contextData),
                "employee" => EmployeePrompt.GetPrompt(contextData),
                "cashier" => EmployeePrompt.GetPrompt(contextData),
                "kitchen" => EmployeePrompt.GetPrompt(contextData),
                "customer" => CustomerPrompt.GetPrompt(contextData),
                _ => CustomerPrompt.GetPrompt(contextData) // Mặc định an toàn nhất là Customer
            };
        }
    }
}
