using System.Threading.Tasks;
using RestaurantPOS.AI.Models;

namespace RestaurantPOS.AI.Services
{
    public interface IAiOrchestrator
    {
        /// <summary>
        /// Quy trình xử lý yêu cầu AI tổng thể
        /// </summary>
        Task<AiResponse> ProcessAsync(AiRequest request, AiUserContext userContext);
    }
}
