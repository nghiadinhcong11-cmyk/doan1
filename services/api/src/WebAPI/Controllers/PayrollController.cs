using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantPOS.Application.Services;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Infrastructure.Persistence;
using System.Security.Claims;

namespace RestaurantPOS.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PayrollController : ControllerBase
    {
        private readonly IPayrollService _service;
        private readonly ApplicationDbContext _db;
        public PayrollController(IPayrollService service, ApplicationDbContext db) { _service = service; _db = db; }
        private Guid? UserId => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
        private bool IsAdmin => string.Equals(User.FindFirst(ClaimTypes.Role)?.Value, "admin", StringComparison.OrdinalIgnoreCase);
        private bool IsManager => string.Equals(User.FindFirst(ClaimTypes.Role)?.Value, "manager", StringComparison.OrdinalIgnoreCase);
        private Guid? ClaimBranch => Guid.TryParse(User.FindFirst("branchId")?.Value, out var id) ? id : null;
        private Guid? ScopeBranch(Guid? requested) => IsManager ? ClaimBranch : requested;

        [HttpGet]
        public async Task<IActionResult> List([FromQuery] int? month, [FromQuery] int? year, [FromQuery] Guid? branchId, [FromQuery] string? status, [FromQuery] Guid? employeeId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (!IsAdmin && !IsManager) return Forbid();
            var m = month ?? DateTime.UtcNow.Month; var y = year ?? DateTime.UtcNow.Year; page = Math.Max(1, page); pageSize = Math.Clamp(pageSize, 1, 100);
            var scoped = ScopeBranch(branchId); var result = await _service.ListAsync(m, y, scoped, status, employeeId, page, pageSize);
            var employees = await _db.Employees.AsNoTracking().Where(e => result.Items.Select(p => p.EmployeeId).Contains(e.Id)).ToDictionaryAsync(e => e.Id, e => e.FullName);
            return Ok(new { items = result.Items.Select(p => new { p.Id, p.EmployeeId, employeeName = employees.GetValueOrDefault(p.EmployeeId), p.BranchId, p.Month, p.Year, p.EmployeeType, p.StandardWorkDays, p.ActualWorkDays, p.TotalHours, p.RegularHours, p.OvertimeHours, p.BaseSalary, p.HourlyRate, p.RegularPay, p.OvertimePay, p.Allowance, p.Bonus, p.Deduction, p.GrossSalary, p.NetSalary, p.Status, p.CalculatedAt, p.LockedAt, p.PaidAt }), page, pageSize, totalItems = result.Total, totalPages = (int)Math.Ceiling(result.Total / (double)pageSize) });
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Detail(Guid id)
        {
            if (!IsAdmin && !IsManager) return Forbid();
            var p = await _service.GetAsync(id); if (p == null) return NotFound();
            if (IsManager && p.BranchId != ClaimBranch) return Forbid();
            var e = await _db.Employees.FindAsync(p.EmployeeId); var b = await _db.Branches.FindAsync(p.BranchId); return Ok(new { p, employeeName = e?.FullName, branchName = b?.Name });
        }

        public record CalculateRequest(Guid EmployeeId, Guid BranchId, int Month, int Year, decimal? OvertimeHours);
        [HttpPost("calculate")]
        public async Task<IActionResult> Calculate(CalculateRequest request)
        {
            if (!IsAdmin && !IsManager) return Forbid();
            try { if (IsManager && request.BranchId != ClaimBranch) return Forbid(); return Ok(await _service.CalculateAsync(request.EmployeeId, request.BranchId, request.Month, request.Year, request.OvertimeHours)); } catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }
        [HttpPost("{id:guid}/lock")] public async Task<IActionResult> Lock(Guid id) => await Change(id, true);
        [HttpPost("{id:guid}/unlock")] public async Task<IActionResult> Unlock(Guid id) => await Change(id, false);
        private async Task<IActionResult> Change(Guid id, bool lockIt)
        {
            if (!IsAdmin && !IsManager) return Forbid();
            try { var p = await _service.GetAsync(id); if (p == null) return NotFound(); if (IsManager && p.BranchId != ClaimBranch) return Forbid(); if (lockIt) await _service.LockAsync(id); else await _service.UnlockAsync(id); return Ok(await _service.GetAsync(id)); } catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        public record AdjustmentRequest(string Type, decimal Amount, string Reason);
        [HttpPost("{id:guid}/adjustments")]
        public async Task<IActionResult> Adjustment(Guid id, AdjustmentRequest request)
        {
            if (!IsAdmin && !IsManager) return Forbid();
            try { var p = await _service.GetAsync(id); if (p == null) return NotFound(); if (IsManager && p.BranchId != ClaimBranch) return Forbid(); return Ok(await _service.AddAdjustmentAsync(id, request.Type, request.Amount, request.Reason, UserId)); } catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpGet("employees/{employeeId:guid}/salary")]
        public async Task<IActionResult> Salary(Guid employeeId, [FromQuery] Guid branchId)
        {
            if (!IsAdmin && !IsManager) return Forbid();
            if (IsManager && branchId != ClaimBranch) return Forbid();
            return Ok(await _service.GetSalaryAsync(employeeId, branchId) ?? new EmployeeSalaryProfile { EmployeeId = employeeId, BranchId = branchId });
        }
        [HttpPut("employees/{employeeId:guid}/salary")]
        public async Task<IActionResult> SaveSalary(Guid employeeId, [FromQuery] Guid branchId, EmployeeSalaryProfile input)
        {
            if (!IsAdmin && !IsManager) return Forbid();
            try { if (IsManager && branchId != ClaimBranch) return Forbid(); return Ok(await _service.SaveSalaryAsync(employeeId, branchId, input)); } catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }
        [HttpGet("settings")] public async Task<IActionResult> Settings([FromQuery] Guid? branchId)
        {
            if (!IsAdmin && !IsManager) return Forbid();
            return Ok(await _service.GetSettingsAsync(ScopeBranch(branchId)));
        }
        [HttpPut("settings")] public async Task<IActionResult> SaveSettings([FromQuery] Guid? branchId, PayrollSettings input)
        {
            if (!IsAdmin && !IsManager) return Forbid();
            try { var scoped = ScopeBranch(branchId); return Ok(await _service.SaveSettingsAsync(scoped, input)); } catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }
    }
}
