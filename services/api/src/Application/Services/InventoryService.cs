using System.Data;
using Microsoft.EntityFrameworkCore;
using RestaurantPOS.Application.DTOs.Inventory;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Domain.Finance;
using RestaurantPOS.Domain.Inventory;
using RestaurantPOS.Infrastructure.Persistence;

namespace RestaurantPOS.Application.Services;

public sealed class InventoryService(ApplicationDbContext context) : IInventoryService
{
    private readonly ApplicationDbContext _context = context;

    public async Task<IReadOnlyCollection<InventoryItemResponse>> GetItemsAsync(bool? activeOnly)
    {
        var query = _context.InventoryItems.AsNoTracking().AsQueryable();
        if (activeOnly.HasValue)
            query = query.Where(item => item.IsActive == activeOnly.Value);

        return await query.OrderBy(item => item.Name)
            .Select(item => new InventoryItemResponse(item.Id, item.Name, item.UnitCode, item.IsActive, item.CreatedAtUtc, item.UpdatedAtUtc))
            .ToListAsync();
    }

    public async Task<InventoryItemResponse> GetItemAsync(Guid id)
    {
        var item = await _context.InventoryItems.AsNoTracking().SingleOrDefaultAsync(value => value.Id == id)
            ?? throw new InventoryNotFoundException("Inventory item was not found.");
        return Map(item);
    }

    public async Task<InventoryItemResponse> CreateItemAsync(CreateInventoryItemRequest request, InventoryUserContext user)
    {
        RequireManagement(user);
        var (name, unitCode, normalizedName) = ValidateItemInput(request.Name, request.UnitCode);
        if (await _context.InventoryItems.AnyAsync(item => item.IsActive && item.NormalizedName == normalizedName))
            throw new InventoryConflictException("An active inventory item with the same name already exists.");

        var item = new InventoryItem
        {
            Id = Guid.NewGuid(),
            Name = name,
            UnitCode = unitCode,
            NormalizedName = normalizedName,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };
        _context.InventoryItems.Add(item);
        await _context.SaveChangesAsync();
        return Map(item);
    }

    public async Task<InventoryItemResponse> UpdateItemAsync(Guid id, UpdateInventoryItemRequest request, InventoryUserContext user)
    {
        RequireManagement(user);
        var item = await _context.InventoryItems.SingleOrDefaultAsync(value => value.Id == id)
            ?? throw new InventoryNotFoundException("Inventory item was not found.");
        var (name, unitCode, normalizedName) = ValidateItemInput(request.Name, request.UnitCode);

        if (request.IsActive && await _context.InventoryItems.AnyAsync(value => value.Id != id && value.IsActive && value.NormalizedName == normalizedName))
            throw new InventoryConflictException("An active inventory item with the same name already exists.");

        item.Name = name;
        item.UnitCode = unitCode;
        item.NormalizedName = normalizedName;
        item.IsActive = request.IsActive;
        item.UpdatedAtUtc = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Map(item);
    }

    public async Task<IReadOnlyCollection<BranchInventoryResponse>> GetBranchInventoryAsync(Guid branchId, InventoryUserContext user)
    {
        EnsureBranchAccess(branchId, user);
        var rows = await _context.BranchInventories.AsNoTracking()
            .Include(row => row.InventoryItem)
            .Where(row => row.BranchId == branchId)
            .OrderBy(row => row.InventoryItem!.Name)
            .ToListAsync();
        return rows.Select(Map).ToList();
    }

    public async Task<InventoryOverviewResponse> GetOverviewAsync(Guid branchId, InventoryUserContext user)
    {
        EnsureBranchAccess(branchId, user);

        var periodStartUtc = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var periodEndUtc = periodStartUtc.AddMonths(1);

        // Include active master items without a balance row as zero stock, matching the
        // branch inventory creation semantics used by receipt confirmation.
        var activeRows = await _context.InventoryItems.AsNoTracking()
            .Where(item => item.IsActive)
            .GroupJoin(
                _context.BranchInventories.AsNoTracking().Where(row => row.BranchId == branchId),
                item => item.Id,
                row => row.InventoryItemId,
                (item, rows) => new
                {
                    item.Id,
                    item.Name,
                    item.UnitCode,
                    Balance = rows.Select(row => new { row.CurrentQuantity, row.MinimumStock }).FirstOrDefault()
                })
            .Select(value => new
            {
                value.Id,
                value.Name,
                value.UnitCode,
                CurrentQuantity = value.Balance == null ? 0m : value.Balance.CurrentQuantity,
                MinimumStock = value.Balance == null ? 0m : value.Balance.MinimumStock
            })
            .ToListAsync();

        var outOfStockCount = activeRows.Count(row => row.CurrentQuantity == 0m);
        var lowStockRows = activeRows
            .Where(row => row.CurrentQuantity > 0m && row.MinimumStock > 0m && row.CurrentQuantity <= row.MinimumStock)
            .OrderBy(row => row.CurrentQuantity)
            .ThenBy(row => row.Name)
            .Take(10)
            .ToList();

        var receivedByUnit = await GetQuantityByUnitAsync(branchId, StockTransactionType.IN, periodStartUtc, periodEndUtc);
        var issuedByUnit = await GetQuantityByUnitAsync(branchId, StockTransactionType.OUT, periodStartUtc, periodEndUtc);
        var importCostQuery = _context.StockReceipts.AsNoTracking()
            .Where(receipt => receipt.BranchId == branchId &&
                receipt.Status == StockDocumentStatus.Confirmed &&
                receipt.ConfirmedAtUtc.HasValue &&
                receipt.ConfirmedAtUtc.Value >= periodStartUtc &&
                receipt.ConfirmedAtUtc.Value < periodEndUtc);
        var importCost = IsSqlite
            ? (await _context.StockReceipts.AsNoTracking()
                .Where(receipt => receipt.BranchId == branchId && receipt.Status == StockDocumentStatus.Confirmed && receipt.ConfirmedAtUtc.HasValue)
                .Select(receipt => new { receipt.ConfirmedAtUtc, receipt.TotalAmount })
                .ToListAsync())
                .Where(receipt => receipt.ConfirmedAtUtc!.Value >= periodStartUtc && receipt.ConfirmedAtUtc.Value < periodEndUtc)
                .Sum(receipt => receipt.TotalAmount)
            : await importCostQuery.Select(receipt => (decimal?)receipt.TotalAmount).SumAsync() ?? 0m;

        return new InventoryOverviewResponse(
            branchId,
            periodStartUtc,
            periodEndUtc,
            activeRows.Count,
            activeRows.Count(row => row.CurrentQuantity > 0m && row.MinimumStock > 0m && row.CurrentQuantity <= row.MinimumStock),
            outOfStockCount,
            receivedByUnit,
            issuedByUnit,
            importCost,
            lowStockRows.Select(row => new InventoryLowStockItemResponse(
                row.Id, row.Name, row.UnitCode, row.CurrentQuantity, row.MinimumStock, false)).ToList());
    }

    private async Task<IReadOnlyCollection<InventoryQuantityByUnitResponse>> GetQuantityByUnitAsync(
        Guid branchId, StockTransactionType type, DateTime periodStartUtc, DateTime periodEndUtc)
    {
        var query = _context.StockTransactions.AsNoTracking()
            .Where(transaction => transaction.BranchId == branchId && transaction.Type == type &&
                transaction.CreatedAtUtc >= periodStartUtc && transaction.CreatedAtUtc < periodEndUtc)
            .Join(_context.InventoryItems.AsNoTracking(), transaction => transaction.InventoryItemId, item => item.Id,
                (transaction, item) => new { item.UnitCode, transaction.Quantity });

        // SQLite's test provider cannot translate SUM(decimal), while PostgreSQL does.
        // Keep production aggregation server-side and use a bounded monthly fallback only for tests.
        if (IsSqlite)
        {
            var rows = await query.ToListAsync();
            return rows.GroupBy(value => value.UnitCode)
                .Select(group => new InventoryQuantityByUnitResponse(group.Key, group.Sum(value => value.Quantity)))
                .OrderBy(value => value.UnitCode)
                .ToList();
        }

        var grouped = await query
            .GroupBy(value => value.UnitCode)
            .Select(group => new { UnitCode = group.Key, Quantity = group.Sum(value => value.Quantity) })
            .OrderBy(value => value.UnitCode)
            .ToListAsync();

        return grouped.Select(value => new InventoryQuantityByUnitResponse(value.UnitCode, value.Quantity)).ToList();
    }

    public async Task<BranchInventoryResponse> UpdateMinimumStockAsync(Guid branchId, Guid inventoryItemId, UpdateMinimumStockRequest request, InventoryUserContext user)
    {
        EnsureBranchAccess(branchId, user);
        if (request.MinimumStock < 0)
            throw new InventoryValidationException("MinimumStock must be greater than or equal to zero.");

        var item = await _context.InventoryItems.SingleOrDefaultAsync(value => value.Id == inventoryItemId)
            ?? throw new InventoryNotFoundException("Inventory item was not found.");
        var row = await _context.BranchInventories
            .Include(value => value.InventoryItem)
            .SingleOrDefaultAsync(value => value.BranchId == branchId && value.InventoryItemId == inventoryItemId);
        if (row is null)
        {
            row = new BranchInventory
            {
                Id = Guid.NewGuid(), BranchId = branchId, InventoryItemId = inventoryItemId,
                CurrentQuantity = 0, MinimumStock = request.MinimumStock, UpdatedAtUtc = DateTime.UtcNow,
                InventoryItem = item
            };
            _context.BranchInventories.Add(row);
        }
        else
        {
            row.MinimumStock = request.MinimumStock;
            row.UpdatedAtUtc = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return Map(row);
    }

    public async Task<StockReceiptResponse> GetReceiptAsync(Guid id, InventoryUserContext user)
    {
        var receipt = await LoadReceiptAsync(id);
        EnsureBranchAccess(receipt.BranchId, user);
        return Map(receipt);
    }

    public async Task<InventoryPage<StockReceiptResponse>> GetReceiptsAsync(Guid? branchId, StockDocumentStatus? status, DateTime? fromDate, DateTime? toDate, int page, int pageSize, InventoryUserContext user)
    {
        ValidatePage(page, pageSize);
        var query = _context.StockReceipts.AsNoTracking().Include(receipt => receipt.Items).ThenInclude(item => item.InventoryItem).AsQueryable();
        query = ApplyBranchFilter(query, branchId, user);
        if (status.HasValue) query = query.Where(receipt => receipt.Status == status.Value);
        if (fromDate.HasValue) query = query.Where(receipt => receipt.CreatedAtUtc >= NormalizeUtc(fromDate.Value));
        if (toDate.HasValue) query = query.Where(receipt => receipt.CreatedAtUtc <= NormalizeUtc(toDate.Value));
        var total = await query.CountAsync();
        var items = await query.OrderByDescending(receipt => receipt.CreatedAtUtc).ThenByDescending(receipt => receipt.Id)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return new InventoryPage<StockReceiptResponse>(items.Select(Map).ToList(), total, page, pageSize);
    }

    public async Task<StockReceiptResponse> CreateReceiptAsync(CreateStockReceiptRequest request, InventoryUserContext user)
    {
        RequireManagement(user);
        var branchId = ResolveBranch(request.BranchId, user);
        var items = await ValidateReceiptItemsAsync(request.Items);
        var idempotencyKey = NormalizeKey(request.IdempotencyKey);
        if (idempotencyKey is not null)
        {
            var existing = await _context.StockReceipts.AsNoTracking().Include(receipt => receipt.Items).ThenInclude(item => item.InventoryItem)
                .SingleOrDefaultAsync(receipt => receipt.BranchId == branchId && receipt.IdempotencyKey == idempotencyKey);
            if (existing is not null) return Map(existing);
        }

        var receipt = new StockReceipt
        {
            Id = Guid.NewGuid(), BranchId = branchId, SupplierName = TrimOrNull(request.SupplierName),
            PurchaseDate = NormalizeNullableUtc(request.PurchaseDate), Note = TrimOrNull(request.Note),
            Status = StockDocumentStatus.Draft, CreatedBy = RequireUserId(user), CreatedAtUtc = DateTime.UtcNow,
            IdempotencyKey = idempotencyKey, Items = items
        };
        receipt.TotalAmount = CalculateReceiptTotal(items);
        _context.StockReceipts.Add(receipt);
        await _context.SaveChangesAsync();
        return Map(receipt);
    }

    public async Task<StockReceiptResponse> UpdateReceiptAsync(Guid id, UpdateStockReceiptRequest request, InventoryUserContext user)
    {
        RequireManagement(user);
        var receipt = await LoadReceiptAsync(id);
        EnsureBranchAccess(receipt.BranchId, user);
        EnsureDraft(receipt.Status, "receipt");
        var items = await ValidateReceiptItemsAsync(request.Items);
        _context.StockReceiptItems.RemoveRange(receipt.Items);
        receipt.Items.Clear();
        foreach (var item in items) receipt.Items.Add(item);
        receipt.SupplierName = TrimOrNull(request.SupplierName);
        receipt.PurchaseDate = NormalizeNullableUtc(request.PurchaseDate);
        receipt.Note = TrimOrNull(request.Note);
        receipt.TotalAmount = CalculateReceiptTotal(items);
        await _context.SaveChangesAsync();
        return Map(receipt);
    }

    public async Task<StockReceiptResponse> ConfirmReceiptAsync(Guid id, InventoryUserContext user)
    {
        RequireManagement(user);
        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
        var receipt = await LoadReceiptAsync(id, tracking: true);
        EnsureBranchAccess(receipt.BranchId, user);
        if (receipt.Status == StockDocumentStatus.Confirmed) return Map(receipt);
        EnsureDraft(receipt.Status, "receipt");
        var items = await ValidateReceiptItemsAsync(receipt.Items);
        var inventoryItems = items.Select(item => item.InventoryItem!).ToDictionary(item => item.Id);
        var rows = await LockBranchInventoryRowsAsync(receipt.BranchId, inventoryItems.Keys, createMissing: true);
        var now = DateTime.UtcNow;

        receipt.TotalAmount = CalculateReceiptTotal(items);
        foreach (var item in items)
        {
            var row = rows[item.InventoryItemId];
            var before = row.CurrentQuantity;
            row.CurrentQuantity += item.Quantity;
            row.UpdatedAtUtc = now;
            _context.StockTransactions.Add(new StockTransaction
            {
                Id = Guid.NewGuid(), BranchId = receipt.BranchId, InventoryItemId = item.InventoryItemId,
                Type = StockTransactionType.IN, Quantity = item.Quantity, BeforeQuantity = before,
                AfterQuantity = row.CurrentQuantity, ReferenceType = StockReferenceTypes.StockReceipt,
                ReferenceId = receipt.Id, CreatedBy = RequireUserId(user), CreatedAtUtc = now,
                Note = "Stock receipt confirmation"
            });
        }

        var existingExpense = await _context.Expenses.SingleOrDefaultAsync(expense => expense.StockReceiptId == receipt.Id);
        if (existingExpense is not null)
            throw new InventoryConflictException("This receipt already has an Expense record.");

        _context.Expenses.Add(new Expense
        {
            Id = Guid.NewGuid(), BranchId = receipt.BranchId, Category = "Nh\u1EADp h\u00E0ng",
            Description = BuildReceiptDescription(receipt), Amount = receipt.TotalAmount,
            ExpenseDate = receipt.PurchaseDate ?? now, PaymentMethod = ExpensePaymentMethods.Cash,
            Note = BuildReceiptNote(receipt), CreatedBy = RequireUserId(user), CreatedAt = now,
            StockReceiptId = receipt.Id
        });
        receipt.Status = StockDocumentStatus.Confirmed;
        receipt.ConfirmedBy = RequireUserId(user);
        receipt.ConfirmedAtUtc = now;
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        return Map(receipt);
    }

    public async Task<StockReceiptResponse> CancelReceiptAsync(Guid id, InventoryUserContext user)
    {
        RequireManagement(user);
        var receipt = await LoadReceiptAsync(id);
        EnsureBranchAccess(receipt.BranchId, user);
        EnsureDraft(receipt.Status, "receipt");
        receipt.Status = StockDocumentStatus.Cancelled;
        await _context.SaveChangesAsync();
        return Map(receipt);
    }

    public async Task<StockIssueResponse> GetIssueAsync(Guid id, InventoryUserContext user)
    {
        var issue = await LoadIssueAsync(id);
        EnsureBranchAccess(issue.BranchId, user);
        return Map(issue);
    }

    public async Task<InventoryPage<StockIssueResponse>> GetIssuesAsync(Guid? branchId, StockDocumentStatus? status, DateTime? fromDate, DateTime? toDate, int page, int pageSize, InventoryUserContext user)
    {
        ValidatePage(page, pageSize);
        var query = _context.StockIssues.AsNoTracking().Include(issue => issue.Items).ThenInclude(item => item.InventoryItem).AsQueryable();
        query = ApplyBranchFilter(query, branchId, user);
        if (status.HasValue) query = query.Where(issue => issue.Status == status.Value);
        if (fromDate.HasValue) query = query.Where(issue => issue.CreatedAtUtc >= NormalizeUtc(fromDate.Value));
        if (toDate.HasValue) query = query.Where(issue => issue.CreatedAtUtc <= NormalizeUtc(toDate.Value));
        var total = await query.CountAsync();
        var items = await query.OrderByDescending(issue => issue.CreatedAtUtc).ThenByDescending(issue => issue.Id)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return new InventoryPage<StockIssueResponse>(items.Select(Map).ToList(), total, page, pageSize);
    }

    public async Task<StockIssueResponse> CreateIssueAsync(CreateStockIssueRequest request, InventoryUserContext user)
    {
        RequireOperational(user);
        var branchId = ResolveBranch(request.BranchId, user);
        var items = await ValidateIssueItemsAsync(request.Items);
        var reason = RequiredText(request.Reason, "Reason");
        var idempotencyKey = NormalizeKey(request.IdempotencyKey);
        if (idempotencyKey is not null)
        {
            var existing = await _context.StockIssues.AsNoTracking().Include(issue => issue.Items).ThenInclude(item => item.InventoryItem)
                .SingleOrDefaultAsync(issue => issue.BranchId == branchId && issue.IdempotencyKey == idempotencyKey);
            if (existing is not null) return Map(existing);
        }

        var issue = new StockIssue
        {
            Id = Guid.NewGuid(), BranchId = branchId, Reason = reason, Note = TrimOrNull(request.Note),
            Status = StockDocumentStatus.Draft, CreatedBy = RequireUserId(user), CreatedAtUtc = DateTime.UtcNow,
            IdempotencyKey = idempotencyKey, Items = items
        };
        _context.StockIssues.Add(issue);
        await _context.SaveChangesAsync();
        return Map(issue);
    }

    public async Task<StockIssueResponse> UpdateIssueAsync(Guid id, UpdateStockIssueRequest request, InventoryUserContext user)
    {
        RequireOperational(user);
        var issue = await LoadIssueAsync(id);
        EnsureBranchAccess(issue.BranchId, user);
        EnsureDraft(issue.Status, "issue");
        var items = await ValidateIssueItemsAsync(request.Items);
        _context.StockIssueItems.RemoveRange(issue.Items);
        issue.Items.Clear();
        foreach (var item in items) issue.Items.Add(item);
        issue.Reason = RequiredText(request.Reason, "Reason");
        issue.Note = TrimOrNull(request.Note);
        await _context.SaveChangesAsync();
        return Map(issue);
    }

    public async Task<StockIssueResponse> ConfirmIssueAsync(Guid id, InventoryUserContext user)
    {
        RequireManagement(user);
        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
        var issue = await LoadIssueAsync(id, tracking: true);
        EnsureBranchAccess(issue.BranchId, user);
        if (issue.Status == StockDocumentStatus.Confirmed) return Map(issue);
        EnsureDraft(issue.Status, "issue");
        var items = await ValidateIssueItemsAsync(issue.Items);
        var rows = await LockBranchInventoryRowsAsync(issue.BranchId, items.Select(item => item.InventoryItemId), createMissing: false);
        var insufficient = items.Select(item =>
        {
            var available = rows.TryGetValue(item.InventoryItemId, out var row) ? row.CurrentQuantity : 0m;
            return (item, available);
        }).FirstOrDefault(value => value.available < value.item.Quantity);
        if (insufficient.item is not null)
        {
            var itemName = insufficient.item.InventoryItem?.Name ?? insufficient.item.InventoryItemId.ToString();
            throw new InventoryConflictException($"Insufficient stock for {itemName}. Available: {insufficient.available}, requested: {insufficient.item.Quantity}.");
        }

        var now = DateTime.UtcNow;
        foreach (var item in items)
        {
            var row = rows[item.InventoryItemId];
            var before = row.CurrentQuantity;
            row.CurrentQuantity -= item.Quantity;
            row.UpdatedAtUtc = now;
            _context.StockTransactions.Add(new StockTransaction
            {
                Id = Guid.NewGuid(), BranchId = issue.BranchId, InventoryItemId = item.InventoryItemId,
                Type = StockTransactionType.OUT, Quantity = item.Quantity, BeforeQuantity = before,
                AfterQuantity = row.CurrentQuantity, ReferenceType = StockReferenceTypes.StockIssue,
                ReferenceId = issue.Id, CreatedBy = RequireUserId(user), CreatedAtUtc = now,
                Note = issue.Reason
            });
        }

        issue.Status = StockDocumentStatus.Confirmed;
        issue.ConfirmedBy = RequireUserId(user);
        issue.ConfirmedAtUtc = now;
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        return Map(issue);
    }

    public async Task<StockIssueResponse> CancelIssueAsync(Guid id, InventoryUserContext user)
    {
        RequireOperational(user);
        var issue = await LoadIssueAsync(id);
        EnsureBranchAccess(issue.BranchId, user);
        EnsureDraft(issue.Status, "issue");
        issue.Status = StockDocumentStatus.Cancelled;
        await _context.SaveChangesAsync();
        return Map(issue);
    }

    public async Task<InventoryPage<StockTransactionResponse>> GetTransactionsAsync(Guid? branchId, Guid? inventoryItemId, StockTransactionType? type, DateTime? fromDate, DateTime? toDate, int page, int pageSize, InventoryUserContext user)
    {
        ValidatePage(page, pageSize);
        var query = _context.StockTransactions.AsNoTracking().Include(transaction => transaction.InventoryItem).AsQueryable();
        query = ApplyBranchFilter(query, branchId, user);
        if (inventoryItemId.HasValue) query = query.Where(transaction => transaction.InventoryItemId == inventoryItemId.Value);
        if (type.HasValue) query = query.Where(transaction => transaction.Type == type.Value);
        if (fromDate.HasValue) query = query.Where(transaction => transaction.CreatedAtUtc >= NormalizeUtc(fromDate.Value));
        if (toDate.HasValue) query = query.Where(transaction => transaction.CreatedAtUtc <= NormalizeUtc(toDate.Value));
        var total = await query.CountAsync();
        var items = await query.OrderByDescending(transaction => transaction.CreatedAtUtc).ThenByDescending(transaction => transaction.Id)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return new InventoryPage<StockTransactionResponse>(items.Select(Map).ToList(), total, page, pageSize);
    }

    private async Task<StockReceipt> LoadReceiptAsync(Guid id, bool tracking = false)
    {
        var query = _context.StockReceipts.Include(receipt => receipt.Items).ThenInclude(item => item.InventoryItem).AsQueryable();
        if (!tracking) query = query.AsNoTracking();
        return await query.SingleOrDefaultAsync(receipt => receipt.Id == id)
            ?? throw new InventoryNotFoundException("Stock receipt was not found.");
    }

    private async Task<StockIssue> LoadIssueAsync(Guid id, bool tracking = false)
    {
        var query = _context.StockIssues.Include(issue => issue.Items).ThenInclude(item => item.InventoryItem).AsQueryable();
        if (!tracking) query = query.AsNoTracking();
        return await query.SingleOrDefaultAsync(issue => issue.Id == id)
            ?? throw new InventoryNotFoundException("Stock issue was not found.");
    }

    private async Task<List<StockReceiptItem>> ValidateReceiptItemsAsync(IReadOnlyCollection<StockDocumentItemRequest>? requests)
    {
        if (requests is null || requests.Count == 0) throw new InventoryValidationException("At least one receipt item is required.");
        if (requests.Any(item => item.UnitPrice is null)) throw new InventoryValidationException("UnitPrice is required for every receipt item.");
        var duplicate = requests.GroupBy(item => item.InventoryItemId).FirstOrDefault(group => group.Count() > 1);
        if (duplicate is not null) throw new InventoryValidationException("An inventory item may appear only once per receipt.");
        var ids = requests.Select(item => item.InventoryItemId).ToArray();
        var inventoryItems = await _context.InventoryItems.Where(item => ids.Contains(item.Id)).ToDictionaryAsync(item => item.Id);
        if (inventoryItems.Count != ids.Length) throw new InventoryValidationException("One or more inventory items were not found.");

        var result = new List<StockReceiptItem>();
        foreach (var request in requests)
        {
            var inventoryItem = inventoryItems[request.InventoryItemId];
            if (!inventoryItem.IsActive) throw new InventoryValidationException($"Inventory item '{inventoryItem.Name}' is inactive.");
            if (!InventoryQuantityRules.IsValidPositive(inventoryItem.UnitCode, request.Quantity))
                throw new InventoryValidationException($"Quantity for '{inventoryItem.Name}' is invalid.");
            if (request.UnitPrice < 0) throw new InventoryValidationException("UnitPrice must be greater than or equal to zero.");
            result.Add(new StockReceiptItem { Id = Guid.NewGuid(), InventoryItemId = inventoryItem.Id, InventoryItem = inventoryItem, Quantity = request.Quantity, UnitPrice = request.UnitPrice!.Value });
        }
        return result;
    }

    private async Task<List<StockReceiptItem>> ValidateReceiptItemsAsync(IEnumerable<StockReceiptItem> items)
    {
        var requests = items.Select(item => new StockDocumentItemRequest(item.InventoryItemId, item.Quantity, item.UnitPrice)).ToArray();
        return await ValidateReceiptItemsAsync(requests);
    }

    private async Task<List<StockIssueItem>> ValidateIssueItemsAsync(IReadOnlyCollection<StockDocumentItemRequest>? requests)
    {
        if (requests is null || requests.Count == 0) throw new InventoryValidationException("At least one issue item is required.");
        var duplicate = requests.GroupBy(item => item.InventoryItemId).FirstOrDefault(group => group.Count() > 1);
        if (duplicate is not null) throw new InventoryValidationException("An inventory item may appear only once per issue.");
        var ids = requests.Select(item => item.InventoryItemId).ToArray();
        var inventoryItems = await _context.InventoryItems.Where(item => ids.Contains(item.Id)).ToDictionaryAsync(item => item.Id);
        if (inventoryItems.Count != ids.Length) throw new InventoryValidationException("One or more inventory items were not found.");

        var result = new List<StockIssueItem>();
        foreach (var request in requests)
        {
            var inventoryItem = inventoryItems[request.InventoryItemId];
            if (!inventoryItem.IsActive) throw new InventoryValidationException($"Inventory item '{inventoryItem.Name}' is inactive.");
            if (!InventoryQuantityRules.IsValidPositive(inventoryItem.UnitCode, request.Quantity))
                throw new InventoryValidationException($"Quantity for '{inventoryItem.Name}' is invalid.");
            result.Add(new StockIssueItem { Id = Guid.NewGuid(), InventoryItemId = inventoryItem.Id, InventoryItem = inventoryItem, Quantity = request.Quantity });
        }
        return result;
    }

    private async Task<List<StockIssueItem>> ValidateIssueItemsAsync(IEnumerable<StockIssueItem> items)
    {
        var requests = items.Select(item => new StockDocumentItemRequest(item.InventoryItemId, item.Quantity)).ToArray();
        return await ValidateIssueItemsAsync(requests);
    }

    private async Task<Dictionary<Guid, BranchInventory>> LockBranchInventoryRowsAsync(Guid branchId, IEnumerable<Guid> inventoryItemIds, bool createMissing)
    {
        var rows = new Dictionary<Guid, BranchInventory>();
        foreach (var inventoryItemId in inventoryItemIds.Distinct().OrderBy(id => id))
        {
            if (IsPostgres)
            {
                if (createMissing)
                {
                    await _context.Database.ExecuteSqlInterpolatedAsync($"""
                        INSERT INTO "BranchInventories" ("Id", "BranchId", "InventoryItemId", "CurrentQuantity", "MinimumStock", "UpdatedAtUtc")
                        VALUES ({Guid.NewGuid()}, {branchId}, {inventoryItemId}, 0, 0, {DateTime.UtcNow})
                        ON CONFLICT ("BranchId", "InventoryItemId") DO NOTHING
                        """);
                }

                var locked = await _context.BranchInventories.FromSqlInterpolated($"""
                    SELECT * FROM "BranchInventories"
                    WHERE "BranchId" = {branchId} AND "InventoryItemId" = {inventoryItemId}
                    FOR UPDATE
                    """).SingleOrDefaultAsync();
                if (locked is not null) rows[inventoryItemId] = locked;
            }
            else
            {
                var row = await _context.BranchInventories.SingleOrDefaultAsync(value => value.BranchId == branchId && value.InventoryItemId == inventoryItemId);
                if (row is null && createMissing)
                {
                    row = new BranchInventory { Id = Guid.NewGuid(), BranchId = branchId, InventoryItemId = inventoryItemId, UpdatedAtUtc = DateTime.UtcNow };
                    _context.BranchInventories.Add(row);
                }
                if (row is not null) rows[inventoryItemId] = row;
            }
        }
        return rows;
    }

    private static IQueryable<StockReceipt> ApplyBranchFilter(IQueryable<StockReceipt> query, Guid? branchId, InventoryUserContext user)
    {
        if (user.IsAdmin)
        {
            return branchId.HasValue ? query.Where(receipt => receipt.BranchId == branchId.Value) : query;
        }
        if (!user.BranchId.HasValue) throw new InventoryForbiddenException("Your account is not assigned to a branch.");
        if (branchId.HasValue && branchId.Value != user.BranchId.Value) throw new InventoryForbiddenException("You cannot access another branch.");
        return query.Where(receipt => receipt.BranchId == user.BranchId.Value);
    }

    private static IQueryable<StockIssue> ApplyBranchFilter(IQueryable<StockIssue> query, Guid? branchId, InventoryUserContext user)
    {
        if (user.IsAdmin) return branchId.HasValue ? query.Where(issue => issue.BranchId == branchId.Value) : query;
        if (!user.BranchId.HasValue) throw new InventoryForbiddenException("Your account is not assigned to a branch.");
        if (branchId.HasValue && branchId.Value != user.BranchId.Value) throw new InventoryForbiddenException("You cannot access another branch.");
        return query.Where(issue => issue.BranchId == user.BranchId.Value);
    }

    private static IQueryable<StockTransaction> ApplyBranchFilter(IQueryable<StockTransaction> query, Guid? branchId, InventoryUserContext user)
    {
        if (user.IsAdmin) return branchId.HasValue ? query.Where(transaction => transaction.BranchId == branchId.Value) : query;
        if (!user.BranchId.HasValue) throw new InventoryForbiddenException("Your account is not assigned to a branch.");
        if (branchId.HasValue && branchId.Value != user.BranchId.Value) throw new InventoryForbiddenException("You cannot access another branch.");
        return query.Where(transaction => transaction.BranchId == user.BranchId.Value);
    }

    private bool IsPostgres => _context.Database.ProviderName?.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) == true;
    private bool IsSqlite => _context.Database.ProviderName?.Contains("Sqlite", StringComparison.OrdinalIgnoreCase) == true;

    private static (string Name, string UnitCode, string NormalizedName) ValidateItemInput(string? rawName, string? rawUnitCode)
    {
        var name = RequiredText(rawName, "Name");
        var unitCode = RequiredText(rawUnitCode, "UnitCode").ToLowerInvariant();
        if (!InventoryUnitCatalog.IsValid(unitCode)) throw new InventoryValidationException("UnitCode is not supported.");
        var normalizedName = InventoryItem.NormalizeName(name);
        if (string.IsNullOrWhiteSpace(normalizedName)) throw new InventoryValidationException("Name is required.");
        return (name, unitCode, normalizedName);
    }

    private static decimal CalculateReceiptTotal(IEnumerable<StockReceiptItem> items) => items.Sum(item => item.Quantity * item.UnitPrice);

    private static string BuildReceiptDescription(StockReceipt receipt) =>
        string.IsNullOrWhiteSpace(receipt.SupplierName) ? "Nh\u1EADp h\u00E0ng" : $"Nh\u1EADp h\u00E0ng - {receipt.SupplierName.Trim()}";

    private static string BuildReceiptNote(StockReceipt receipt) =>
        string.IsNullOrWhiteSpace(receipt.Note) ? $"Phiếu nhập kho {receipt.Id}" : receipt.Note.Trim();

    private static Guid RequireUserId(InventoryUserContext user) => user.UserId == Guid.Empty ? throw new InventoryForbiddenException("Authenticated user identity is required.") : user.UserId;
    private static void RequireManagement(InventoryUserContext user) { if (!user.IsAdmin && !user.IsManager) throw new InventoryForbiddenException("Inventory management requires admin or manager access."); }
    private static void RequireOperational(InventoryUserContext user) { if (!user.IsOperational) throw new InventoryForbiddenException("Inventory access is not available for this role."); }
    private static void EnsureBranchAccess(Guid branchId, InventoryUserContext user) { if (!user.IsAdmin && user.BranchId != branchId) throw new InventoryForbiddenException("You cannot access another branch."); }
    private static Guid ResolveBranch(Guid branchId, InventoryUserContext user) { if (branchId == Guid.Empty) throw new InventoryValidationException("BranchId is required."); EnsureBranchAccess(branchId, user); return branchId; }
    private static void EnsureDraft(StockDocumentStatus status, string document) { if (status != StockDocumentStatus.Draft) throw new InventoryConflictException($"The {document} is no longer editable in its current status."); }
    private static string RequiredText(string? value, string field) { if (string.IsNullOrWhiteSpace(value)) throw new InventoryValidationException($"{field} is required."); return value.Trim(); }
    private static string? TrimOrNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static string? NormalizeKey(string? value) => TrimOrNull(value);
    private static DateTime NormalizeUtc(DateTime value) => value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();
    private static DateTime? NormalizeNullableUtc(DateTime? value) => value.HasValue ? NormalizeUtc(value.Value) : null;
    private static void ValidatePage(int page, int pageSize) { if (page < 1 || pageSize is < 1 or > 200) throw new InventoryValidationException("Page must be >= 1 and pageSize must be between 1 and 200."); }

    private static InventoryItemResponse Map(InventoryItem item) => new(item.Id, item.Name, item.UnitCode, item.IsActive, item.CreatedAtUtc, item.UpdatedAtUtc);
    private static BranchInventoryResponse Map(BranchInventory row) => new(row.BranchId, row.InventoryItemId, row.InventoryItem?.Name ?? string.Empty, row.InventoryItem?.UnitCode ?? string.Empty, row.CurrentQuantity, row.MinimumStock, row.MinimumStock > 0 && row.CurrentQuantity <= row.MinimumStock, row.CurrentQuantity <= 0, row.UpdatedAtUtc);
    private static StockReceiptResponse Map(StockReceipt receipt) => new(receipt.Id, receipt.BranchId, receipt.SupplierName, receipt.PurchaseDate, receipt.TotalAmount, receipt.Status, receipt.Note, receipt.CreatedBy, receipt.CreatedAtUtc, receipt.ConfirmedBy, receipt.ConfirmedAtUtc, receipt.Items.Select(Map).ToList());
    private static StockIssueResponse Map(StockIssue issue) => new(issue.Id, issue.BranchId, issue.Reason, issue.Note, issue.Status, issue.CreatedBy, issue.CreatedAtUtc, issue.ConfirmedBy, issue.ConfirmedAtUtc, issue.Items.Select(Map).ToList());
    private static StockDocumentItemResponse Map(StockReceiptItem item) => new(item.Id, item.InventoryItemId, item.InventoryItem?.Name ?? string.Empty, item.InventoryItem?.UnitCode ?? string.Empty, item.Quantity, item.UnitPrice);
    private static StockDocumentItemResponse Map(StockIssueItem item) => new(item.Id, item.InventoryItemId, item.InventoryItem?.Name ?? string.Empty, item.InventoryItem?.UnitCode ?? string.Empty, item.Quantity, null);
    private static StockTransactionResponse Map(StockTransaction transaction) => new(transaction.Id, transaction.BranchId, transaction.InventoryItemId, transaction.InventoryItem?.Name ?? string.Empty, transaction.InventoryItem?.UnitCode ?? string.Empty, transaction.Type, transaction.Quantity, transaction.BeforeQuantity, transaction.AfterQuantity, transaction.ReferenceType, transaction.ReferenceId, transaction.CreatedBy, transaction.CreatedAtUtc, transaction.Note);
}
