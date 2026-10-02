using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantPOS.Application.DTOs.Inventory;
using RestaurantPOS.Application.Services;
using RestaurantPOS.Domain.Inventory;

namespace RestaurantPOS.WebAPI.Controllers;

[ApiController]
[Route("api/inventory")]
[Authorize(Roles = "admin,manager,employee,cashier,kitchen")]
public sealed class InventoryController(IInventoryService inventoryService) : ControllerBase
{
    private readonly IInventoryService _inventoryService = inventoryService;

    [HttpGet("items")]
    [Authorize(Roles = "admin,manager")]
    public Task<IActionResult> GetItems([FromQuery] bool? activeOnly) => Execute(() => _inventoryService.GetItemsAsync(activeOnly));

    [HttpGet("items/{id:guid}")]
    [Authorize(Roles = "admin,manager")]
    public Task<IActionResult> GetItem(Guid id) => Execute(() => _inventoryService.GetItemAsync(id));

    [HttpPost("items")]
    [Authorize(Roles = "admin,manager")]
    public Task<IActionResult> CreateItem([FromBody] CreateInventoryItemRequest request) => Execute(() => _inventoryService.CreateItemAsync(request, CurrentUser()));

    [HttpPut("items/{id:guid}")]
    [Authorize(Roles = "admin,manager")]
    public Task<IActionResult> UpdateItem(Guid id, [FromBody] UpdateInventoryItemRequest request) => Execute(() => _inventoryService.UpdateItemAsync(id, request, CurrentUser()));

    [HttpGet("branches/{branchId:guid}")]
    public Task<IActionResult> GetBranchInventory(Guid branchId) => Execute(() => _inventoryService.GetBranchInventoryAsync(branchId, CurrentUser()));

    [HttpGet("overview/{branchId:guid}")]
    public Task<IActionResult> GetOverview(Guid branchId) => Execute(() => _inventoryService.GetOverviewAsync(branchId, CurrentUser()));

    [HttpPut("branches/{branchId:guid}/items/{inventoryItemId:guid}/minimum-stock")]
    [Authorize(Roles = "admin,manager")]
    public Task<IActionResult> UpdateMinimumStock(Guid branchId, Guid inventoryItemId, [FromBody] UpdateMinimumStockRequest request) =>
        Execute(() => _inventoryService.UpdateMinimumStockAsync(branchId, inventoryItemId, request, CurrentUser()));

    [HttpGet("receipts")]
    public Task<IActionResult> GetReceipts([FromQuery] Guid? branchId, [FromQuery] StockDocumentStatus? status, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate, [FromQuery] int page = 1, [FromQuery] int pageSize = 50) =>
        Execute(() => _inventoryService.GetReceiptsAsync(branchId, status, fromDate, toDate, page, pageSize, CurrentUser()));

    [HttpGet("receipts/{id:guid}")]
    public Task<IActionResult> GetReceipt(Guid id) => Execute(() => _inventoryService.GetReceiptAsync(id, CurrentUser()));

    [HttpPost("receipts")]
    [Authorize(Roles = "admin,manager")]
    public Task<IActionResult> CreateReceipt([FromBody] CreateStockReceiptRequest request) => Execute(() => _inventoryService.CreateReceiptAsync(request, CurrentUser()));

    [HttpPut("receipts/{id:guid}")]
    [Authorize(Roles = "admin,manager")]
    public Task<IActionResult> UpdateReceipt(Guid id, [FromBody] UpdateStockReceiptRequest request) => Execute(() => _inventoryService.UpdateReceiptAsync(id, request, CurrentUser()));

    [HttpPost("receipts/{id:guid}/confirm")]
    [Authorize(Roles = "admin,manager")]
    public Task<IActionResult> ConfirmReceipt(Guid id) => Execute(() => _inventoryService.ConfirmReceiptAsync(id, CurrentUser()));

    [HttpPost("receipts/{id:guid}/cancel")]
    [Authorize(Roles = "admin,manager")]
    public Task<IActionResult> CancelReceipt(Guid id) => Execute(() => _inventoryService.CancelReceiptAsync(id, CurrentUser()));

    [HttpGet("issues")]
    public Task<IActionResult> GetIssues([FromQuery] Guid? branchId, [FromQuery] StockDocumentStatus? status, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate, [FromQuery] int page = 1, [FromQuery] int pageSize = 50) =>
        Execute(() => _inventoryService.GetIssuesAsync(branchId, status, fromDate, toDate, page, pageSize, CurrentUser()));

    [HttpGet("issues/{id:guid}")]
    public Task<IActionResult> GetIssue(Guid id) => Execute(() => _inventoryService.GetIssueAsync(id, CurrentUser()));

    [HttpPost("issues")]
    public Task<IActionResult> CreateIssue([FromBody] CreateStockIssueRequest request) => Execute(() => _inventoryService.CreateIssueAsync(request, CurrentUser()));

    [HttpPut("issues/{id:guid}")]
    public Task<IActionResult> UpdateIssue(Guid id, [FromBody] UpdateStockIssueRequest request) => Execute(() => _inventoryService.UpdateIssueAsync(id, request, CurrentUser()));

    [HttpPost("issues/{id:guid}/confirm")]
    [Authorize(Roles = "admin,manager")]
    public Task<IActionResult> ConfirmIssue(Guid id) => Execute(() => _inventoryService.ConfirmIssueAsync(id, CurrentUser()));

    [HttpPost("issues/{id:guid}/cancel")]
    public Task<IActionResult> CancelIssue(Guid id) => Execute(() => _inventoryService.CancelIssueAsync(id, CurrentUser()));

    [HttpGet("transactions")]
    public Task<IActionResult> GetTransactions([FromQuery] Guid? branchId, [FromQuery] Guid? inventoryItemId, [FromQuery] StockTransactionType? type, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate, [FromQuery] int page = 1, [FromQuery] int pageSize = 50) =>
        Execute(() => _inventoryService.GetTransactionsAsync(branchId, inventoryItemId, type, fromDate, toDate, page, pageSize, CurrentUser()));

    private async Task<IActionResult> Execute<T>(Func<Task<T>> operation)
    {
        try
        {
            return Ok(await operation());
        }
        catch (InventoryValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InventoryForbiddenException)
        {
            return Forbid();
        }
        catch (InventoryNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InventoryConflictException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    private InventoryUserContext CurrentUser()
    {
        var role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
        var userId = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var parsedUserId) ? parsedUserId : Guid.Empty;
        var branchId = Guid.TryParse(User.FindFirstValue("branchId"), out var parsedBranchId) ? parsedBranchId : (Guid?)null;
        return new InventoryUserContext(role, userId, branchId);
    }
}
