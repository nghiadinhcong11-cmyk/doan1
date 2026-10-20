using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantPOS.Application.Common.Interfaces;
using RestaurantPOS.Application.Common.Security;
using RestaurantPOS.Domain.Entities;
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

    public sealed class DirectSessionRequest
    {
        public Guid BranchId { get; init; }
        public Guid? TableId { get; init; }
    }
// kiểm tra tài khoản và nâng cấp mật khẩu trong luồng đăng nhập nhân sự.
    [AllowAnonymous]
    [HttpPost("bootstrap")]
    public async Task<IActionResult> Bootstrap([FromBody] BootstrapRequest? request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.QrToken) || !QrTokenPattern.IsMatch(request.QrToken))
            return BadRequest(new { message = "Mã QR không hợp lệ." });

        var table = await _context.Tables.AsNoTracking()
            .Include(candidate => candidate.Area)
            .Include(candidate => candidate.Branch)
            .FirstOrDefaultAsync(candidate => candidate.QrToken == request.QrToken && candidate.IsActive);
        if (table is null || table.BranchId == Guid.Empty || table.Area is null || table.Branch is null)
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
            table = new { id = table.Id, name = table.Name, AreaName = table.Area.Name, table.SeatCount },
            // Branch.Name is authoritative; RestaurantTable.BranchName is a legacy display snapshot.
            branch = new { id = table.BranchId, name = table.Branch.Name }
        });
    }

    /// <summary>
    /// Creates a short-lived guest session for a direct menu visitor. The selected
    /// branch/table are only a request; the server verifies both before issuing claims.
    /// </summary>
    // Guest sessions must originate from a verified table QR token. The old
    // branch-selected direct session is intentionally no longer routable.
    [NonAction]
    [HttpPost("direct-session")]
    public async Task<IActionResult> DirectSession([FromBody] DirectSessionRequest? request)
    {
        if (request is null || request.BranchId == Guid.Empty)
            return BadRequest(new { message = "Vui lòng chọn chi nhánh phục vụ." });

        var branch = await _context.Branches.AsNoTracking()
            .Where(candidate => candidate.Id == request.BranchId && candidate.IsActive)
            .Select(candidate => new { candidate.Id, candidate.Name })
            .FirstOrDefaultAsync();
        if (branch is null)
            return NotFound(new { message = "Chi nhánh không tồn tại hoặc không hoạt động." });

        RestaurantTable? table = null;
        if (request.TableId.HasValue)
        {
            table = await _context.Tables.AsNoTracking()
                .FirstOrDefaultAsync(candidate => candidate.Id == request.TableId.Value &&
                    candidate.BranchId == branch.Id && candidate.IsActive);
            if (table is null)
                return NotFound(new { message = "Bàn không tồn tại hoặc không hoạt động." });

            var sameNameCount = await _context.Tables.AsNoTracking()
                .CountAsync(candidate => candidate.IsActive && candidate.BranchId == branch.Id && candidate.Name == table.Name);
            if (sameNameCount != 1)
                return Conflict(new { message = "Không thể xác định duy nhất bàn phục vụ." });
        }

        var token = _jwtService.GenerateDirectGuestToken(branch.Id, table?.Id, TimeSpan.FromMinutes(30));

        return Ok(new
        {
            token,
            branch = new { id = branch.Id, name = branch.Name },
            table = table is null ? null : new { table.Id, table.Name }
        });
    }
}
