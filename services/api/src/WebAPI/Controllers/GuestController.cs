using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantPOS.Application.Common.Interfaces;
using RestaurantPOS.Infrastructure.Persistence;

namespace RestaurantPOS.WebAPI.Controllers;

[ApiController]
[Route("api/guest")]
[Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("strict-limiter")]
public sealed class GuestController : ControllerBase
{
    private static readonly Regex QrTokenPattern = new("^[A-Za-z0-9_-]{43}$", RegexOptions.CultureInvariant);
    private readonly ApplicationDbContext _context;
    private readonly IJwtService _jwtService;

    public GuestController(ApplicationDbContext context, IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public sealed class BootstrapRequest
    {
        public string QrToken { get; init; } = string.Empty;
    }

    [AllowAnonymous]
    [HttpPost("bootstrap")]
    public async Task<IActionResult> Bootstrap([FromBody] BootstrapRequest? request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.QrToken) || !QrTokenPattern.IsMatch(request.QrToken))
            return BadRequest(new { message = "Mã QR không hợp lệ." });

        var table = await _context.Tables.AsNoTracking()
            .FirstOrDefaultAsync(candidate => candidate.QrToken == request.QrToken && candidate.IsActive);
        if (table is null || !table.BranchId.HasValue)
            return NotFound(new { message = "Mã QR không hợp lệ." });

        // Orders historically persist TableName, not TableId. Do not establish a
        // guest session when that legacy representation could be ambiguous.
        var matchingTableCount = await _context.Tables.AsNoTracking()
            .CountAsync(candidate => candidate.IsActive && candidate.BranchId == table.BranchId && candidate.Name == table.Name);
        if (matchingTableCount != 1)
            return NotFound(new { message = "Mã QR không hợp lệ." });

        var token = _jwtService.GenerateToken(
            Guid.NewGuid(),
            "guest",
            "Khách tại bàn",
            "customer",
            table.BranchId,
            customerSessionType: "guest",
            tableId: table.Id);

        return Ok(new
        {
            token,
            table = new { table.Name, table.AreaName, table.SeatCount },
            branch = new { id = table.BranchId, name = table.BranchName }
        });
    }
}
