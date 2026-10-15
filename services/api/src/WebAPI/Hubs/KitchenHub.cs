using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using RestaurantPOS.Application.Services;
using System;
using System.Security.Claims;
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
            if (Guid.TryParse(Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId) && userId != Guid.Empty)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, NotificationRecipientRouting.UserGroup(userId));
            }
            var requestedBranchId = Context.GetHttpContext()?.Request.Query["branchId"].ToString();
            var isAdmin = string.Equals(role, "admin", StringComparison.OrdinalIgnoreCase);
            var hasRequestedBranch = Guid.TryParse(requestedBranchId, out var requestedBranchIdValue);
            var hasClaimBranch = Guid.TryParse(Context.User?.FindFirst("branchId")?.Value, out var claimBranchId);
            var connectedBranchId = isAdmin && hasRequestedBranch ? requestedBranchIdValue : (hasClaimBranch ? claimBranchId : Guid.Empty);

            if (connectedBranchId != Guid.Empty)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"kitchen-branch:{connectedBranchId}");
            }

            if (connectedBranchId != Guid.Empty)
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
