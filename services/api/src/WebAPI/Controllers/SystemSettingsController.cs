using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantPOS.Application.Services;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RestaurantPOS.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "admin")]
public class SystemSettingsController : ControllerBase
{
    private readonly ISystemSettingService _settingService;

    public SystemSettingsController(ISystemSettingService settingService)
    {
        _settingService = settingService;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] Guid? branchId)
    {
        if (!TryBranch(branchId, out var effective)) return Forbid();
        var settings = await _settingService.GetSettingsAsync(effective);
        return Ok(settings);
    }

    [HttpPut]
    public async Task<IActionResult> Put([FromBody] Dictionary<string, string> settings, [FromQuery] Guid? branchId)
    {
        if (!TryBranch(branchId, out var effective)) return Forbid();

        await _settingService.UpdateSettingsAsync(effective, settings);
        return Ok(new { success = true, message = "Settings updated successfully" });
    }

    private bool TryBranch(Guid? requested, out Guid? effective)
    {
        effective = requested;
        var role = User.FindFirstValue(ClaimTypes.Role);

        // Admin được phép truy cập mọi branch (bao gồm global settings - branchId null)
        if (string.Equals(role, "admin", StringComparison.OrdinalIgnoreCase)) return true;

        // This controller is owner-admin only. Keep the explicit branch check so
        // a future authorization change cannot accidentally broaden global scope.
        var branchClaim = User.FindFirst("branchId")?.Value;
        if (!Guid.TryParse(branchClaim, out var branch)) return false;

        // Manager không được truy cập global settings (requested == null) hoặc branch khác
        if (!requested.HasValue || requested != branch) return false;

        effective = branch;
        return true;
    }
}
