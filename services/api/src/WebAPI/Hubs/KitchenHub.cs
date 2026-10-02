using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Threading.Tasks;

namespace RestaurantPOS.WebAPI.Hubs
{
    [Authorize(Roles = "admin,manager,employee,cashier,kitchen")]
    public class KitchenHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
            if (string.Equals(role, "admin", StringComparison.OrdinalIgnoreCase))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "kitchen-admin");
            }
            if (!string.IsNullOrWhiteSpace(role))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"role:{role}");
            }
            if (Guid.TryParse(Context.User?.FindFirst("branchId")?.Value, out var branchId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"kitchen-branch:{branchId}");
            }

            if (Guid.TryParse(Context.User?.FindFirst("branchId")?.Value, out var connectedBranchId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"branch:{connectedBranchId}");
                if (!string.IsNullOrWhiteSpace(role))
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"branch:{connectedBranchId}:role:{role}");
                }
            }

            await base.OnConnectedAsync();
        }

    }
}
