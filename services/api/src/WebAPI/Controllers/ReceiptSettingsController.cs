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
    private const long MaxLogoBytes = 2 * 1024 * 1024;
    private static readonly HashSet<string> AllowedLogoTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/png", "image/jpeg", "image/webp"
    };

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
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Put([FromBody] ReceiptSettings input, [FromQuery] Guid? branchId)
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (!string.Equals(role, "admin", StringComparison.OrdinalIgnoreCase)) return Forbid();
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

    [HttpPost("logo")]
    [Authorize(Roles = "admin")]
    [RequestSizeLimit(MaxLogoBytes + 1024 * 64)]
    public async Task<IActionResult> UploadLogo([FromQuery] Guid? branchId, [FromForm] IFormFile? logo)
    {
        if (!TryBranch(branchId, out var effective) || !effective.HasValue) return Forbid();
        if (logo == null || logo.Length == 0) return BadRequest(new { message = "Vui lòng chọn tệp logo." });
        if (logo.Length > MaxLogoBytes) return BadRequest(new { message = "Logo không được vượt quá 2 MB." });
        if (!AllowedLogoTypes.Contains(logo.ContentType)) return BadRequest(new { message = "Logo chỉ hỗ trợ PNG, JPG hoặc WebP." });

        await using var stream = logo.OpenReadStream();
        var bytes = new byte[12];
        var read = await stream.ReadAsync(bytes.AsMemory(0, bytes.Length));
        if (!IsSupportedImageSignature(bytes, read, logo.ContentType))
            return BadRequest(new { message = "Nội dung tệp logo không hợp lệ." });

        var branch = await _context.Branches.FirstOrDefaultAsync(x => x.Id == effective.Value);
        if (branch == null) return NotFound();

        stream.Position = 0;
        await using var buffer = new MemoryStream();
        await stream.CopyToAsync(buffer);
        branch.ImageUrl = $"data:{logo.ContentType};base64,{Convert.ToBase64String(buffer.ToArray())}";
        await _context.SaveChangesAsync();
        return Ok(new { branchId = branch.Id, logoUrl = branch.ImageUrl });
    }

    [HttpDelete("logo")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> DeleteLogo([FromQuery] Guid? branchId)
    {
        if (!TryBranch(branchId, out var effective) || !effective.HasValue) return Forbid();
        var branch = await _context.Branches.FirstOrDefaultAsync(x => x.Id == effective.Value);
        if (branch == null) return NotFound();
        branch.ImageUrl = null;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private static bool IsSupportedImageSignature(byte[] bytes, int length, string contentType)
    {
        if (contentType.Equals("image/png", StringComparison.OrdinalIgnoreCase))
            return length >= 8 && bytes.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
        if (contentType.Equals("image/jpeg", StringComparison.OrdinalIgnoreCase))
            return length >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF;
        return contentType.Equals("image/webp", StringComparison.OrdinalIgnoreCase)
            && length >= 12
            && System.Text.Encoding.ASCII.GetString(bytes, 0, 4) == "RIFF"
            && System.Text.Encoding.ASCII.GetString(bytes, 8, 4) == "WEBP";
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
