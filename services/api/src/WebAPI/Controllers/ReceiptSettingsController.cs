using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Infrastructure.Persistence;
using System.Security.Claims;

namespace RestaurantPOS.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "admin,manager,cashier,employee")]
public class ReceiptSettingsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    public ReceiptSettingsController(ApplicationDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] Guid? branchId)
    {
        if (!TryBranch(branchId, out var effective)) return Forbid();
        var settings = await _context.ReceiptSettings.AsNoTracking().FirstOrDefaultAsync(x => x.BranchId == effective);
        return Ok(settings ?? new ReceiptSettings { BranchId = effective });
    }

    [HttpPut]
    public async Task<IActionResult> Put([FromBody] ReceiptSettings input, [FromQuery] Guid? branchId)
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (!string.Equals(role, "admin", StringComparison.OrdinalIgnoreCase) && !string.Equals(role, "manager", StringComparison.OrdinalIgnoreCase)) return Forbid();
        if (!TryBranch(branchId, out var effective)) return Forbid();
        var settings = await _context.ReceiptSettings.FirstOrDefaultAsync(x => x.BranchId == effective);
        if (settings == null)
        {
            settings = new ReceiptSettings { Id = Guid.NewGuid(), BranchId = effective };
            _context.ReceiptSettings.Add(settings);
        }
        settings.PaperWidth = input.PaperWidth == 58 ? 58 : 80;
        settings.ShowLogo = input.ShowLogo; settings.ShowAddress = input.ShowAddress; settings.ShowPhone = input.ShowPhone;
        settings.ShowStaff = input.ShowStaff; settings.ShowPaymentMethod = input.ShowPaymentMethod;
        settings.ShowOrderNote = input.ShowOrderNote; settings.ShowThankYou = input.ShowThankYou;
        settings.ThankYouText = string.IsNullOrWhiteSpace(input.ThankYouText) ? "Cảm ơn quý khách và hẹn gặp lại!" : input.ThankYouText.Trim();
        settings.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok(settings);
    }

    private bool TryBranch(Guid? requested, out Guid? effective)
    {
        effective = requested;
        if (string.Equals(User.FindFirstValue(ClaimTypes.Role), "admin", StringComparison.OrdinalIgnoreCase)) return true;
        if (!Guid.TryParse(User.FindFirst("branchId")?.Value, out var branch)) return false;
        if (requested.HasValue && requested != branch) return false;
        effective = branch; return true;
    }
}
