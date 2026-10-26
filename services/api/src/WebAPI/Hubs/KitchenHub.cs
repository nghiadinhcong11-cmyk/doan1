using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using RestaurantPOS.Application.Services;
using RestaurantPOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RestaurantPOS.WebAPI.Hubs
{
    [Authorize(Roles = "admin,manager,employee,cashier,kitchen")]
    public class KitchenHub : Hub
    {
        private readonly ApplicationDbContext _context;

        public KitchenHub(ApplicationDbContext context)
        {
            _context = context;
        }

        //xac thuc va tham gia nhom
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
            else if (isAdmin && !hasRequestedBranch)
            {
                // Global admins receive branch-scoped notifications from every
                // active branch. Managers and staff never enter this path.
                var branchIds = await _context.Branches.AsNoTracking()
                    .Where(branch => branch.IsActive)
                    .Select(branch => branch.Id)
                    .ToListAsync();
                foreach (var branchId in branchIds)
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"branch:{branchId}");
            }

            await base.OnConnectedAsync();
        }

    }
}
