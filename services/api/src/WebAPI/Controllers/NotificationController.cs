using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantPOS.Application.Services;

namespace RestaurantPOS.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notifications;
    public NotificationController(INotificationService notifications) => _notifications = notifications;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] Guid? branchId, [FromQuery] string? readState = "unread", [FromQuery] string? type = null, [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        if (!TryGetUserId(out var userId)) return Forbid();
        if (!TryResolveBranch(role, branchId, out var effectiveBranchId)) return Forbid();
        return Ok(await _notifications.GetForUserAsync(role, userId, effectiveBranchId, readState, type, from, to, page, pageSize));
    }

    [HttpGet("count")]
    public async Task<IActionResult> Count([FromQuery] Guid? branchId)
    {
        var role = Role();
        if (!TryGetUserId(out var userId)) return Forbid();
        if (!TryResolveBranch(role, branchId, out var effectiveBranchId)) return Forbid();
        return Ok(new { count = await _notifications.GetUnreadCountAsync(role, userId, effectiveBranchId) });
    }

    [HttpPatch("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id, [FromQuery] Guid? branchId)
    {
        if (!TryGetUserId(out var userId)) return Forbid();
        if (!TryResolveBranch(Role(), branchId, out var effectiveBranchId)) return Forbid();
        var result = await _notifications.MarkReadAsync(id, Role(), userId, effectiveBranchId);
        return result ? NoContent() : NotFound();
    }

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllRead([FromQuery] Guid? branchId)
    {
        if (!TryGetUserId(out var userId)) return Forbid();
        if (!TryResolveBranch(Role(), branchId, out var effectiveBranchId)) return Forbid();
        var cutoff = DateTime.UtcNow;
        return Ok(new { count = await _notifications.MarkAllReadAsync(Role(), userId, effectiveBranchId, cutoff) });
    }

    private string Role() => User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
    private bool TryGetUserId(out Guid userId) => Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out userId) && userId != Guid.Empty;

    private bool TryResolveBranch(string role, Guid? requestedBranchId, out Guid? effectiveBranchId)
    {
        if (string.Equals(role, "admin", StringComparison.OrdinalIgnoreCase))
        {
            effectiveBranchId = requestedBranchId;
            return true;
        }

        if (!Guid.TryParse(User.FindFirst("branchId")?.Value, out var claimBranchId))
        {
            effectiveBranchId = null;
            return false;
        }
        if (requestedBranchId.HasValue && requestedBranchId.Value != claimBranchId)
        {
            effectiveBranchId = null;
            return false;
        }
        effectiveBranchId = claimBranchId;
        return true;
    }
}
