using System.Threading;
using System.Threading.Tasks;
using RestaurantPOS.AI.Models;

namespace RestaurantPOS.AI.Services
{
    public interface IAiOrchestrator
    {
        /// <summary>
        /// Quy trình xử lý yêu cầu AI tổng thể
        /// </summary>
        Task<AiResponse> ProcessAsync(AiRequest request, AiUserContext userContext, CancellationToken cancellationToken = default);

        Task<AiResponse> ProcessReadOnlyAsync(AiRequest request, AiUserContext userContext, CancellationToken cancellationToken = default);

        Task<AiResponse> ProcessGuestReadOnlyAsync(AiRequest request, CancellationToken cancellationToken = default);
    }
}
