using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantPOS.Application.DTOs.Expenses;
using RestaurantPOS.Application.Services;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Domain.Finance;
using RestaurantPOS.Infrastructure.Persistence;

namespace RestaurantPOS.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "admin,manager")]
public class ExpenseController : ControllerBase
{
    private static readonly string[] Categories = { "Nguyên liệu", "Bao bì", "Điện nước", "Vệ sinh", "Gas", "Vận hành", "Khác" };
    private readonly ApplicationDbContext _context;
    private readonly IDashboardService _dashboardService;

    public ExpenseController(ApplicationDbContext context, IDashboardService dashboardService)
    { _context = context; _dashboardService = dashboardService; }

    [HttpGet]
    public async Task<IActionResult> GetExpenses([FromQuery] Guid? branchId, [FromQuery] string? category,
        [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        if (!TryResolveBranch(branchId, out var effectiveBranchId)) return Forbid();
        if (page < 1 || pageSize is < 1 or > 200) return BadRequest("Page must be >= 1 and pageSize must be between 1 and 200.");
        if (fromDate.HasValue && toDate.HasValue && fromDate > toDate) return BadRequest("fromDate must be before toDate.");

        var query = _context.Expenses.AsNoTracking().AsQueryable();
        if (effectiveBranchId.HasValue) query = query.Where(e => e.BranchId == effectiveBranchId.Value);
        if (!string.IsNullOrWhiteSpace(category)) query = query.Where(e => e.Category == category.Trim());
        if (fromDate.HasValue) query = query.Where(e => e.ExpenseDate >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(e => e.ExpenseDate <= toDate.Value);
        var total = await query.CountAsync();
        var items = await query.OrderByDescending(e => e.ExpenseDate).ThenByDescending(e => e.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return Ok(new { items, total, page, pageSize, totalAmount = items.Sum(e => e.Amount) });
    }

    [HttpPost]
    public async Task<IActionResult> CreateExpense([FromBody] ExpenseCreateDto request)
    {
        var validation = await ValidateRequest(request);
        if (validation != null) return validation;
        if (!HasBranchAccess(request.BranchId)) return Forbid();
        var expense = new Expense { Id = Guid.NewGuid(), BranchId = request.BranchId, Category = request.Category!.Trim(),
            Description = request.Description!.Trim(), Amount = request.Amount, ExpenseDate = NormalizeDate(request.ExpenseDate),
            PaymentMethod = request.PaymentMethod?.Trim() ?? ExpensePaymentMethods.Cash, Note = request.Note?.Trim() ?? string.Empty, CreatedBy = CurrentUserId(), CreatedAt = DateTime.UtcNow };
        _context.Expenses.Add(expense); await _context.SaveChangesAsync(); _dashboardService.InvalidateSummaryCache();
        return CreatedAtAction(nameof(GetExpenses), new { id = expense.Id }, expense);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateExpense(Guid id, [FromBody] ExpenseUpdateDto request)
    {
        var expense = await _context.Expenses.FindAsync(id);
        if (expense == null) return NotFound();
        if (!HasBranchAccess(expense.BranchId) || expense.BranchId != request.BranchId) return Forbid();
        if (expense.StockReceiptId.HasValue)
            return Conflict(new { message = "Inventory-linked expenses are immutable." });
        var validation = await ValidateRequest(request);
        if (validation != null) return validation;
        expense.Category = request.Category!.Trim(); expense.Description = request.Description!.Trim(); expense.Amount = request.Amount;
        expense.ExpenseDate = NormalizeDate(request.ExpenseDate); expense.PaymentMethod = request.PaymentMethod?.Trim() ?? ExpensePaymentMethods.Cash;
        expense.Note = request.Note?.Trim() ?? string.Empty; expense.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(); _dashboardService.InvalidateSummaryCache(); return Ok(expense);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteExpense(Guid id)
    {
        var expense = await _context.Expenses.FindAsync(id); if (expense == null) return NotFound();
        if (!HasBranchAccess(expense.BranchId)) return Forbid();
        if (expense.StockReceiptId.HasValue)
            return Conflict(new { message = "Inventory-linked expenses cannot be deleted." });
        _context.Expenses.Remove(expense); await _context.SaveChangesAsync(); _dashboardService.InvalidateSummaryCache(); return NoContent();
    }

    private async Task<IActionResult?> ValidateRequest(ExpenseCreateDto request)
    {
        if (request.BranchId == Guid.Empty) return BadRequest("BranchId is required.");
        if (string.IsNullOrWhiteSpace(request.Description)) return BadRequest("Description is required.");
        if (request.Amount <= 0) return BadRequest("Amount must be greater than zero.");
        if (request.ExpenseDate == default) return BadRequest("ExpenseDate is required.");
        if (string.IsNullOrWhiteSpace(request.Category) || !Categories.Contains(request.Category.Trim(), StringComparer.OrdinalIgnoreCase))
            return BadRequest(new { message = "Invalid category.", allowedCategories = Categories });
        if (!await _context.Branches.AnyAsync(b => b.Id == request.BranchId && b.IsActive)) return BadRequest("Branch is invalid or inactive.");
        return null;
    }

    private bool TryResolveBranch(Guid? requested, out Guid? effective)
    {
        effective = requested; if (IsAdmin()) return true;
        if (!Guid.TryParse(User.FindFirst("branchId")?.Value, out var ownBranch)) return false;
        if (requested.HasValue && requested.Value != ownBranch) return false; effective = ownBranch; return true;
    }
    private bool HasBranchAccess(Guid branchId) => TryResolveBranch(branchId, out var effective) && effective == branchId;
    private bool IsAdmin() => string.Equals(User.FindFirst(ClaimTypes.Role)?.Value, "admin", StringComparison.OrdinalIgnoreCase);
    private Guid? CurrentUserId() => Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : null;
    private static DateTime NormalizeDate(DateTime value) => value.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(value, DateTimeKind.Utc) : value.ToUniversalTime();
}
