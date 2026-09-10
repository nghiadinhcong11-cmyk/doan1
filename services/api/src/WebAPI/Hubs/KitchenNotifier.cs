using System;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.AspNetCore.SignalR;
using RestaurantPOS.Application.Common.Interfaces;

namespace RestaurantPOS.WebAPI.Hubs
{
    public class KitchenNotifier : IKitchenNotifier
    {
        private readonly IHubContext<KitchenHub> _hubContext;

        public KitchenNotifier(IHubContext<KitchenHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task NotifyNewOrderRequestAsync(object payload)
        {
            var branchId = payload.GetType().GetProperty("branchId", BindingFlags.Public | BindingFlags.Instance)?.GetValue(payload)?.ToString();
            if (Guid.TryParse(branchId, out var parsedBranchId))
                await _hubContext.Clients.Groups("kitchen-admin", $"kitchen-branch:{parsedBranchId}").SendAsync("NewOrderRequest", payload);
            else
                await _hubContext.Clients.Group("kitchen-admin").SendAsync("NewOrderRequest", payload);
        }

        public async Task NotifyRequestStatusUpdatedAsync(Guid requestId, string status)
        {
            await _hubContext.Clients.Group("kitchen-admin").SendAsync("RequestStatusUpdated", new { id = requestId, status });
        }

        public async Task NotifyRequestStatusUpdatedAsync(Guid requestId, string status, Guid? branchId)
        {
            var payload = new { id = requestId, status };
            if (branchId.HasValue)
                await _hubContext.Clients.Groups("kitchen-admin", $"kitchen-branch:{branchId.Value}").SendAsync("RequestStatusUpdated", payload);
            else
                await _hubContext.Clients.Group("kitchen-admin").SendAsync("RequestStatusUpdated", payload);
        }
    }
}
