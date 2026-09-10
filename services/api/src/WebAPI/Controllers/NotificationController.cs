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
    public async Task<IActionResult> Get([FromQuery] bool unreadOnly = false)
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        var userId = Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : (Guid?)null;
        var branchId = Guid.TryParse(User.FindFirst("branchId")?.Value, out var branch) ? branch : (Guid?)null;
        return Ok(await _notifications.GetForUserAsync(role, userId, branchId, unreadOnly));
    }

    [HttpPatch("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id)
    {
        var result = await _notifications.MarkReadAsync(id, Role(), UserId(), BranchId());
        return result ? NoContent() : NotFound();
    }

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllRead() => Ok(new { count = await _notifications.MarkAllReadAsync(Role(), UserId(), BranchId()) });

    private string Role() => User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
    private Guid? UserId() => Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : null;
    private Guid? BranchId() => Guid.TryParse(User.FindFirst("branchId")?.Value, out var id) ? id : null;
}
