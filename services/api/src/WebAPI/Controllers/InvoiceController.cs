using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantPOS.Infrastructure.Persistence;
using System.Security.Claims;

namespace RestaurantPOS.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "admin,manager,cashier,employee")]
public class InvoiceController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public InvoiceController(ApplicationDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? search, [FromQuery] string? fromDate,
        [FromQuery] string? toDate, [FromQuery] string? paymentStatus, [FromQuery] string? paymentMethod,
        [FromQuery] Guid? branchId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var query = _context.Orders.Include(o => o.Details).AsNoTracking().AsQueryable();

        var role = User.FindFirstValue(ClaimTypes.Role);
        var claimBranch = Guid.TryParse(User.FindFirst("branchId")?.Value, out var userBranch) ? userBranch : (Guid?)null;
        if (!string.Equals(role, "admin", StringComparison.OrdinalIgnoreCase))
        {
            if (!claimBranch.HasValue) return Forbid();
            query = query.Where(o => o.BranchId == claimBranch.Value);
        }
        else if (branchId.HasValue)
        {
            query = query.Where(o => o.BranchId == branchId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            var hasOrderId = Guid.TryParse(term, out var searchedOrderId);
            query = query.Where(o => (o.InvoiceCode != null && o.InvoiceCode.Contains(term)) ||
                (o.TableName != null && o.TableName.Contains(term)) ||
                (o.CustomerName != null && o.CustomerName.Contains(term)) ||
                (o.CreatedBy != null && o.CreatedBy.Contains(term)) ||
                (hasOrderId && o.Id == searchedOrderId));
        }

        if (DateTime.TryParse(fromDate, out var from)) query = query.Where(o => o.CreatedAt >= DateTime.SpecifyKind(from.Date, DateTimeKind.Utc));
        if (DateTime.TryParse(toDate, out var to)) query = query.Where(o => o.CreatedAt < DateTime.SpecifyKind(to.Date.AddDays(1), DateTimeKind.Utc));
        if (!string.IsNullOrWhiteSpace(paymentMethod)) query = query.Where(o => o.PaymentMethod == paymentMethod);
        if (!string.IsNullOrWhiteSpace(paymentStatus))
        {
            query = paymentStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase) || paymentStatus == "Hoàn thành"
                ? query.Where(o => o.Status == "Hoàn thành" && o.PaidAmount >= o.TotalAmount && o.TotalAmount > 0)
                : query.Where(o => (o.Status != "Hoàn thành" && o.Status != "Đã hủy") || (o.Status == "Hoàn thành" && o.PaidAmount < o.TotalAmount));
        }

        var totalItems = await query.CountAsync();
        var items = await query.OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return Ok(new { items, page, pageSize, totalItems, totalPages = (int)Math.Ceiling(totalItems / (double)pageSize) });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Detail(Guid id)
    {
        var order = await _context.Orders.Include(o => o.Details).AsNoTracking().FirstOrDefaultAsync(o => o.Id == id);
        if (order == null) return NotFound();
        var role = User.FindFirstValue(ClaimTypes.Role);
        var branch = Guid.TryParse(User.FindFirst("branchId")?.Value, out var branchId) ? branchId : (Guid?)null;
        if (!string.Equals(role, "admin", StringComparison.OrdinalIgnoreCase) && order.BranchId != branch) return Forbid();
        return Ok(order);
    }
}
