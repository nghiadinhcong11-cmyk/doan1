using Microsoft.EntityFrameworkCore;
using RestaurantPOS.Application.DTOs.Inventory;
using RestaurantPOS.Application.Services;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Domain.Finance;
using RestaurantPOS.Domain.Inventory;
using RestaurantPOS.Infrastructure.Persistence;

namespace RestaurantPOS.Tests;

public sealed class InventoryServiceTests
{
    private static readonly Guid BranchA = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid BranchB = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private static readonly Guid AdminId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid ManagerId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    [Fact]
    public async Task Inventory_item_validates_unit_normalizes_name_and_rejects_duplicate()
    {
        await using var fixture = await Fixture.CreateAsync();
        var service = new InventoryService(fixture.Context);
        var admin = new InventoryUserContext("admin", AdminId, null);

        var item = await service.CreateItemAsync(new CreateInventoryItemRequest("  Cà phê  ", " KG "), admin);

        Assert.Equal("Cà phê", item.Name);
        Assert.Equal("kg", item.UnitCode);
        await Assert.ThrowsAsync<InventoryConflictException>(() => service.CreateItemAsync(new CreateInventoryItemRequest("cà   phê", "kg"), admin));
        await Assert.ThrowsAsync<InventoryValidationException>(() => service.CreateItemAsync(new CreateInventoryItemRequest("Salt", "bag"), admin));
    }

    [Fact]
    public async Task Manager_cannot_access_another_branch_inventory()
    {
        await using var fixture = await Fixture.CreateAsync();
        var service = new InventoryService(fixture.Context);
        var manager = new InventoryUserContext("manager", ManagerId, BranchA);

        await Assert.ThrowsAsync<InventoryForbiddenException>(() => service.GetBranchInventoryAsync(BranchB, manager));
        await Assert.ThrowsAsync<InventoryForbiddenException>(() => service.GetOverviewAsync(BranchB, manager));
    }

    [Fact]
    public async Task Receipt_draft_has_no_side_effects_and_confirmation_is_idempotent()
    {
        await using var fixture = await Fixture.CreateAsync();
        var item = await fixture.AddItemAsync("Coffee", "kg");
        var service = new InventoryService(fixture.Context);
        var manager = new InventoryUserContext("manager", ManagerId, BranchA);
        var request = new CreateStockReceiptRequest(BranchA, "Supplier", null, null, "receipt-1", [new StockDocumentItemRequest(item.Id, 5m, 120m)]);

        var draft = await service.CreateReceiptAsync(request, manager);
        Assert.Equal(StockDocumentStatus.Draft, draft.Status);
        Assert.Empty(fixture.Context.BranchInventories);
        Assert.Empty(fixture.Context.StockTransactions);
        Assert.Empty(fixture.Context.Expenses);

        var confirmed = await service.ConfirmReceiptAsync(draft.Id, manager);
        var repeated = await service.ConfirmReceiptAsync(draft.Id, manager);

        Assert.Equal(StockDocumentStatus.Confirmed, confirmed.Status);
        Assert.Equal(confirmed.TotalAmount, repeated.TotalAmount);
        Assert.Equal(5m, await fixture.Context.BranchInventories.Select(row => row.CurrentQuantity).SingleAsync());
        Assert.Single(fixture.Context.StockTransactions);
        Assert.Single(fixture.Context.Expenses);
        Assert.Equal(600m, fixture.Context.Expenses.Single().Amount);
        Assert.Equal("Nhập hàng", fixture.Context.Expenses.Single().Category);
        Assert.Equal(ExpensePaymentMethods.Cash, fixture.Context.Expenses.Single().PaymentMethod);
        Assert.Contains(draft.Id.ToString(), fixture.Context.Expenses.Single().Note);
        Assert.False(string.IsNullOrWhiteSpace(fixture.Context.Expenses.Single().Description));
        Assert.Equal(draft.Id, fixture.Context.Expenses.Single().StockReceiptId);
    }

    [Fact]
    public async Task Receipt_confirm_is_atomic_when_expense_link_already_exists()
    {
        await using var fixture = await Fixture.CreateAsync();
        var item = await fixture.AddItemAsync("Coffee", "kg");
        var service = new InventoryService(fixture.Context);
        var manager = new InventoryUserContext("manager", ManagerId, BranchA);
        var draft = await service.CreateReceiptAsync(new CreateStockReceiptRequest(BranchA, null, null, null, null, [new StockDocumentItemRequest(item.Id, 2m, 100m)]), manager);
        fixture.Context.Expenses.Add(new Expense { Id = Guid.NewGuid(), BranchId = BranchA, Description = "pre-existing", Category = "Khác", Amount = 1m, ExpenseDate = DateTime.UtcNow, StockReceiptId = draft.Id });
        await fixture.Context.SaveChangesAsync();

        await Assert.ThrowsAsync<InventoryConflictException>(() => service.ConfirmReceiptAsync(draft.Id, manager));
        Assert.Empty(fixture.Context.BranchInventories);
        Assert.Empty(fixture.Context.StockTransactions);
        Assert.Equal(StockDocumentStatus.Draft, await fixture.Context.StockReceipts.Select(receipt => receipt.Status).SingleAsync());
    }

    [Fact]
    public async Task Issue_confirm_rejects_all_items_when_one_item_is_insufficient()
    {
        await using var fixture = await Fixture.CreateAsync();
        var coffee = await fixture.AddItemAsync("Coffee", "kg");
        var salt = await fixture.AddItemAsync("Salt", "kg");
        fixture.Context.BranchInventories.AddRange(
            new BranchInventory { Id = Guid.NewGuid(), BranchId = BranchA, InventoryItemId = coffee.Id, CurrentQuantity = 5m },
            new BranchInventory { Id = Guid.NewGuid(), BranchId = BranchA, InventoryItemId = salt.Id, CurrentQuantity = 1m });
        await fixture.Context.SaveChangesAsync();
        Assert.Equal(2, await fixture.Context.BranchInventories.CountAsync(row => row.BranchId == BranchA));
        var service = new InventoryService(fixture.Context);
        var manager = new InventoryUserContext("manager", ManagerId, BranchA);
        var issue = await service.CreateIssueAsync(new CreateStockIssueRequest(BranchA, "Kitchen use", null, null, [new StockDocumentItemRequest(coffee.Id, 4m), new StockDocumentItemRequest(salt.Id, 2m)]), manager);
        Assert.Equal(2, await fixture.Context.BranchInventories.CountAsync(row => row.BranchId == BranchA));

        await Assert.ThrowsAsync<InventoryConflictException>(() => service.ConfirmIssueAsync(issue.Id, manager));
        fixture.Context.ChangeTracker.Clear();
        var persistedRows = await fixture.Context.BranchInventories.AsNoTracking().Where(row => row.BranchId == BranchA).OrderBy(row => row.InventoryItemId).ToListAsync();
        Assert.Equal(2, persistedRows.Count);
        Assert.Equal(5m, persistedRows.Single(row => row.InventoryItemId == coffee.Id).CurrentQuantity);
        Assert.Equal(1m, persistedRows.Single(row => row.InventoryItemId == salt.Id).CurrentQuantity);
        Assert.Empty(fixture.Context.StockTransactions);
        Assert.Equal(StockDocumentStatus.Draft, await fixture.Context.StockIssues.Select(value => value.Status).SingleAsync());
    }

    [Fact]
    public async Task Issue_confirm_decreases_stock_without_creating_expense()
    {
        await using var fixture = await Fixture.CreateAsync();
        var item = await fixture.AddItemAsync("Coffee", "kg");
        fixture.Context.BranchInventories.Add(new BranchInventory { Id = Guid.NewGuid(), BranchId = BranchA, InventoryItemId = item.Id, CurrentQuantity = 5m });
        await fixture.Context.SaveChangesAsync();
        var service = new InventoryService(fixture.Context);
        var manager = new InventoryUserContext("manager", ManagerId, BranchA);
        var issue = await service.CreateIssueAsync(new CreateStockIssueRequest(BranchA, "Kitchen use", null, null, [new StockDocumentItemRequest(item.Id, 3m)]), manager);

        await service.ConfirmIssueAsync(issue.Id, manager);

        Assert.Equal(2m, await fixture.Context.BranchInventories.Select(row => row.CurrentQuantity).SingleAsync());
        Assert.Equal(StockTransactionType.OUT, await fixture.Context.StockTransactions.Select(transaction => transaction.Type).SingleAsync());
        Assert.Empty(fixture.Context.Expenses);
    }

    [Fact]
    public async Task Employee_can_create_issue_draft_but_cannot_confirm()
    {
        await using var fixture = await Fixture.CreateAsync();
        var item = await fixture.AddItemAsync("Coffee", "kg");
        var service = new InventoryService(fixture.Context);
        var employee = new InventoryUserContext("employee", ManagerId, BranchA);
        var issue = await service.CreateIssueAsync(new CreateStockIssueRequest(BranchA, "Use", null, null, [new StockDocumentItemRequest(item.Id, 1m)]), employee);

        await Assert.ThrowsAsync<InventoryForbiddenException>(() => service.ConfirmIssueAsync(issue.Id, employee));
        Assert.Equal(StockDocumentStatus.Draft, await fixture.Context.StockIssues.Select(value => value.Status).SingleAsync());
    }

    [Fact]
    public async Task Overview_groups_monthly_movements_by_unit_and_keeps_stock_cards_exclusive()
    {
        await using var fixture = await Fixture.CreateAsync();
        var coffee = await fixture.AddItemAsync("Coffee", "kg");
        var sugar = await fixture.AddItemAsync("Sugar", "kg");
        var cake = await fixture.AddItemAsync("Cake", "piece");
        fixture.Context.BranchInventories.AddRange(
            new BranchInventory { Id = Guid.NewGuid(), BranchId = BranchA, InventoryItemId = coffee.Id, CurrentQuantity = 2m, MinimumStock = 5m, UpdatedAtUtc = DateTime.UtcNow },
            new BranchInventory { Id = Guid.NewGuid(), BranchId = BranchA, InventoryItemId = sugar.Id, CurrentQuantity = 3m, MinimumStock = 1m, UpdatedAtUtc = DateTime.UtcNow },
            new BranchInventory { Id = Guid.NewGuid(), BranchId = BranchA, InventoryItemId = cake.Id, CurrentQuantity = 0m, MinimumStock = 5m, UpdatedAtUtc = DateTime.UtcNow });

        var now = DateTime.UtcNow;
        var periodStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var periodEnd = periodStart.AddMonths(1);
        var receiptId = Guid.NewGuid();
        fixture.Context.StockReceipts.Add(new StockReceipt
        {
            Id = receiptId, BranchId = BranchA, Status = StockDocumentStatus.Confirmed, TotalAmount = 123m,
            CreatedBy = ManagerId, CreatedAtUtc = now, ConfirmedBy = ManagerId, ConfirmedAtUtc = now
        });
        fixture.Context.StockTransactions.AddRange(
            new StockTransaction { Id = Guid.NewGuid(), BranchId = BranchA, InventoryItemId = coffee.Id, Type = StockTransactionType.IN, Quantity = 5m, BeforeQuantity = 0m, AfterQuantity = 5m, ReferenceType = StockReferenceTypes.StockReceipt, ReferenceId = receiptId, CreatedBy = ManagerId, CreatedAtUtc = now },
            new StockTransaction { Id = Guid.NewGuid(), BranchId = BranchA, InventoryItemId = sugar.Id, Type = StockTransactionType.IN, Quantity = 3m, BeforeQuantity = 0m, AfterQuantity = 3m, ReferenceType = StockReferenceTypes.StockReceipt, ReferenceId = receiptId, CreatedBy = ManagerId, CreatedAtUtc = now },
            new StockTransaction { Id = Guid.NewGuid(), BranchId = BranchA, InventoryItemId = cake.Id, Type = StockTransactionType.IN, Quantity = 10m, BeforeQuantity = 0m, AfterQuantity = 10m, ReferenceType = StockReferenceTypes.StockReceipt, ReferenceId = receiptId, CreatedBy = ManagerId, CreatedAtUtc = now },
            new StockTransaction { Id = Guid.NewGuid(), BranchId = BranchA, InventoryItemId = coffee.Id, Type = StockTransactionType.OUT, Quantity = 2m, BeforeQuantity = 4m, AfterQuantity = 2m, ReferenceType = StockReferenceTypes.StockIssue, ReferenceId = Guid.NewGuid(), CreatedBy = ManagerId, CreatedAtUtc = now },
            new StockTransaction { Id = Guid.NewGuid(), BranchId = BranchA, InventoryItemId = coffee.Id, Type = StockTransactionType.ADJUSTMENT, Quantity = 99m, BeforeQuantity = 2m, AfterQuantity = 2m, ReferenceType = StockReferenceTypes.StockAdjustment, ReferenceId = Guid.NewGuid(), CreatedBy = ManagerId, CreatedAtUtc = now },
            new StockTransaction { Id = Guid.NewGuid(), BranchId = BranchA, InventoryItemId = coffee.Id, Type = StockTransactionType.IN, Quantity = 100m, BeforeQuantity = 0m, AfterQuantity = 100m, ReferenceType = StockReferenceTypes.StockReceipt, ReferenceId = Guid.NewGuid(), CreatedBy = ManagerId, CreatedAtUtc = periodStart.AddTicks(-1) },
            new StockTransaction { Id = Guid.NewGuid(), BranchId = BranchA, InventoryItemId = coffee.Id, Type = StockTransactionType.IN, Quantity = 100m, BeforeQuantity = 0m, AfterQuantity = 100m, ReferenceType = StockReferenceTypes.StockReceipt, ReferenceId = Guid.NewGuid(), CreatedBy = ManagerId, CreatedAtUtc = periodEnd },
            new StockTransaction { Id = Guid.NewGuid(), BranchId = BranchB, InventoryItemId = coffee.Id, Type = StockTransactionType.IN, Quantity = 200m, BeforeQuantity = 0m, AfterQuantity = 200m, ReferenceType = StockReferenceTypes.StockReceipt, ReferenceId = Guid.NewGuid(), CreatedBy = ManagerId, CreatedAtUtc = now });
        await fixture.Context.SaveChangesAsync();

        var result = await new InventoryService(fixture.Context).GetOverviewAsync(BranchA, new InventoryUserContext("manager", ManagerId, BranchA));

        Assert.Equal(3, result.ActiveItemCount);
        Assert.Equal(1, result.LowStockItemCount);
        Assert.Equal(1, result.OutOfStockItemCount);
        Assert.Equal(123m, result.ImportCostThisMonth);
        Assert.Equal(8m, Assert.Single(result.ReceivedThisMonthByUnit, value => value.UnitCode == "kg").Quantity);
        Assert.Equal(10m, Assert.Single(result.ReceivedThisMonthByUnit, value => value.UnitCode == "piece").Quantity);
        Assert.Equal(2m, Assert.Single(result.IssuedThisMonthByUnit, value => value.UnitCode == "kg").Quantity);
        Assert.Single(result.LowStockItems);
        Assert.Equal(coffee.Id, result.LowStockItems.Single().InventoryItemId);
    }

    [Fact]
    public async Task Overview_returns_empty_quantity_groups_when_branch_has_no_transactions()
    {
        await using var fixture = await Fixture.CreateAsync();
        var result = await new InventoryService(fixture.Context)
            .GetOverviewAsync(BranchA, new InventoryUserContext("admin", AdminId, null));

        Assert.Empty(result.ReceivedThisMonthByUnit);
        Assert.Empty(result.IssuedThisMonthByUnit);
        Assert.Equal(0m, result.ImportCostThisMonth);
    }

    private sealed class Fixture : IAsyncDisposable
    {
        private readonly Microsoft.Data.Sqlite.SqliteConnection _connection;
        public ApplicationDbContext Context { get; }

        private Fixture(ApplicationDbContext context, Microsoft.Data.Sqlite.SqliteConnection connection)
        {
            Context = context;
            _connection = connection;
        }

        public static async Task<Fixture> CreateAsync()
        {
            var connection = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source=inventory-{Guid.NewGuid():N};Mode=Memory;Cache=Shared");
            await connection.OpenAsync();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options;
            var context = new ApplicationDbContext(options);
            await context.Database.EnsureCreatedAsync();
            context.Branches.AddRange(new Branch { Id = BranchA, Name = "A", IsActive = true }, new Branch { Id = BranchB, Name = "B", IsActive = true });
            await context.SaveChangesAsync();
            return new Fixture(context, connection);
        }

        public async Task<InventoryItem> AddItemAsync(string name, string unitCode)
        {
            var item = new InventoryItem { Id = Guid.NewGuid(), Name = name, NormalizedName = InventoryItem.NormalizeName(name), UnitCode = unitCode, IsActive = true, CreatedAtUtc = DateTime.UtcNow };
            Context.InventoryItems.Add(item);
            await Context.SaveChangesAsync();
            return item;
        }

        public async ValueTask DisposeAsync()
        {
            await Context.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }
}
