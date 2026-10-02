using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantPOS.Application.DTOs.Financial;
using RestaurantPOS.Infrastructure.Persistence;

namespace RestaurantPOS.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "admin,manager")]
public class BusinessInsightController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public BusinessInsightController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? status, [FromQuery] string? severity, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var branchId = GetUserBranchId();
        var isAdmin = IsAdmin();

        var query = _context.BusinessInsights.AsNoTracking().AsQueryable();

        if (!isAdmin)
        {
            if (!branchId.HasValue) return Forbid();
            query = query.Where(i => i.BranchId == branchId.Value);
        }

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(i => i.Status == status);
        }

        if (!string.IsNullOrEmpty(severity))
        {
            query = query.Where(i => i.Severity == severity);
        }

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(i => i.DetectedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(i => new BusinessInsightListDto
            {
                Id = i.Id,
                BranchId = i.BranchId,
                Type = i.Type,
                Severity = i.Severity,
                Title = i.Title,
                Summary = i.Summary,
                Status = i.Status,
                DetectedAt = i.DetectedAt,
                CreatedAt = i.CreatedAt
            })
            .ToListAsync();

        return Ok(new { items, total, page, pageSize });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var branchId = GetUserBranchId();
        var isAdmin = IsAdmin();
        if (!isAdmin && !branchId.HasValue) return Forbid();

        var insight = await _context.BusinessInsights.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);
        if (insight == null) return NotFound();

        if (!isAdmin && insight.BranchId != branchId) return Forbid();

        object? evidence = null;
        try
        {
            if (!string.IsNullOrEmpty(insight.EvidenceJson))
            {
                evidence = JsonSerializer.Deserialize<object>(insight.EvidenceJson);
            }
        }
        catch { }

        var dto = new BusinessInsightDetailDto
        {
            Id = insight.Id,
            BranchId = insight.BranchId,
            Type = insight.Type,
            Severity = insight.Severity,
            Title = insight.Title,
            Summary = insight.Summary,
            AiExplanation = insight.AiExplanation,
            AiRecommendation = insight.AiRecommendation,
            Evidence = evidence,
            Status = insight.Status,
            DetectedAt = insight.DetectedAt,
            CreatedAt = insight.CreatedAt,
            PeriodStart = insight.PeriodStart,
            PeriodEnd = insight.PeriodEnd,
            ComparisonPeriodStart = insight.ComparisonPeriodStart,
            ComparisonPeriodEnd = insight.ComparisonPeriodEnd
        };

        return Ok(dto);
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var branchId = GetUserBranchId();
        var isAdmin = IsAdmin();

        var query = _context.BusinessInsights.Where(i => i.Status == "Unread");

        if (!isAdmin)
        {
            if (!branchId.HasValue) return Forbid();
            query = query.Where(i => i.BranchId == branchId.Value);
        }

        var count = await query.CountAsync();
        return Ok(new { count });
    }

    [HttpPost("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id)
    {
        var branchId = GetUserBranchId();
        var isAdmin = IsAdmin();
        if (!isAdmin && !branchId.HasValue) return Forbid();

        var insight = await _context.BusinessInsights.FirstOrDefaultAsync(i => i.Id == id);
        if (insight == null) return NotFound();

        if (!isAdmin && insight.BranchId != branchId) return Forbid();

        if (insight.Status == "Unread")
        {
            insight.Status = "Read";
            await _context.SaveChangesAsync();
        }

        return NoContent();
    }

    [HttpPost("{id:guid}/resolve")]
    public async Task<IActionResult> Resolve(Guid id)
    {
        var branchId = GetUserBranchId();
        var isAdmin = IsAdmin();
        if (!isAdmin && !branchId.HasValue) return Forbid();

        var insight = await _context.BusinessInsights.FirstOrDefaultAsync(i => i.Id == id);
        if (insight == null) return NotFound();

        if (!isAdmin && insight.BranchId != branchId) return Forbid();

        insight.Status = "Resolved";
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private Guid? GetUserBranchId() => Guid.TryParse(User.FindFirst("branchId")?.Value, out var id) ? id : null;
    private bool IsAdmin() => User.IsInRole("admin");
}
