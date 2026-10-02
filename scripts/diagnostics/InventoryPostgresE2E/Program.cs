using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;
using RestaurantPOS.Application.DTOs.Inventory;
using RestaurantPOS.Application.Services;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Domain.Finance;
using RestaurantPOS.Domain.Inventory;
using RestaurantPOS.Infrastructure.Persistence;

const string authorizedProjectRef = "qfkgjxwbshjgsxsvkpkp";
const string authorizedHost = "aws-0-ap-southeast-2.pooler.supabase.com";
const int authorizedPort = 5432;
const string authorizedDatabase = "postgres";

var configuration = new ConfigurationBuilder()
    .AddUserSecrets<Program>(optional: true)
    .AddEnvironmentVariables()
    .Build();

if (!string.Equals(Environment.GetEnvironmentVariable("DEV_SUPABASE_PROJECT_REF"), authorizedProjectRef, StringComparison.Ordinal))
    return Fail("TARGET CHECK: FAIL - DEV_SUPABASE_PROJECT_REF is not the authorized DEV project.");

var rawConnection = configuration.GetConnectionString("DefaultConnection") ?? configuration["SUPABASE_CONNECTION_STRING"];
if (string.IsNullOrWhiteSpace(rawConnection))
    return Fail("TARGET CHECK: FAIL - DefaultConnection is missing.");

NpgsqlConnectionStringBuilder connection;
try
{
    connection = NormalizeConnectionString(rawConnection);
}
catch (Exception ex) when (ex is ArgumentException or UriFormatException)
{
    return Fail($"TARGET CHECK: FAIL - invalid connection configuration: {ex.Message}");
}

if (!string.Equals(connection.Host, authorizedHost, StringComparison.OrdinalIgnoreCase) ||
    connection.Port != authorizedPort ||
    !string.Equals(connection.Database, authorizedDatabase, StringComparison.Ordinal) ||
    !(connection.Username ?? string.Empty).Contains($"postgres.{authorizedProjectRef}", StringComparison.OrdinalIgnoreCase) ||
    connection.SslMode == SslMode.Disable)
{
    return Fail("TARGET CHECK: FAIL - configured target is not the authorized DEV Session Pooler.");
}

Console.WriteLine("TARGET CHECK: PASS");
Console.WriteLine($"TARGET PROJECT REF: {authorizedProjectRef}");
Console.WriteLine($"HOST: {authorizedHost}");
Console.WriteLine($"PORT: {authorizedPort}");
Console.WriteLine($"DATABASE: {authorizedDatabase}");
Console.WriteLine("TEST DATA STRATEGY: retain E2E_INV_ records; no production delete API is used.");

await using var probe = new NpgsqlConnection(connection.ConnectionString);
try
{
    await probe.OpenAsync();
    await using var select = new NpgsqlCommand("SELECT 1", probe);
    _ = await select.ExecuteScalarAsync();
    Console.WriteLine("CONNECTION/SELECT 1: PASS");
    await ReportLeftoverE2EDataAsync(connection.ConnectionString);

    await RunE2EAsync(connection.ConnectionString);
    Console.WriteLine("POSTGRES INVENTORY E2E: PASS");
    return 0;
}
catch (DbUpdateException ex)
{
    Console.WriteLine($"{E2EState.CurrentPhase} - FAIL");
    PrintDbUpdateDiagnostics(ex);
    return 1;
}
catch (E2EFailureException ex)
{
    Console.WriteLine($"{E2EState.CurrentPhase} - FAIL");
    Console.WriteLine($"POSTGRES INVENTORY E2E: FAIL - {ex.Message}");
    return 1;
}
catch (InventoryConflictException ex)
{
    Console.WriteLine($"POSTGRES INVENTORY E2E: FAIL - unexpected business conflict: {ex.Message}");
    return 1;
}
catch (Exception ex)
{
    Console.WriteLine($"POSTGRES INVENTORY E2E: FAIL - {ex.GetType().Name}; details suppressed.");
    return 1;
}

static async Task RunE2EAsync(string connectionString)
{
    var suffix = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
    var prefix = $"E2E_INV_{suffix}";

    Mark("E2E 01", "Resolve DEV branch and test identities");
    await using var setupContext = CreateContext(connectionString);
    var branches = await setupContext.Branches.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.Id).ToListAsync();
    var admin = await setupContext.Employees.AsNoTracking().FirstOrDefaultAsync(x => x.IsActive && x.Role == "admin");
    var manager = await setupContext.Employees.AsNoTracking().FirstOrDefaultAsync(x => x.IsActive && x.Role == "manager" && x.BranchId != null);
    var employee = await setupContext.Employees.AsNoTracking().FirstOrDefaultAsync(x => x.IsActive && x.Role == "employee" && x.BranchId != null);
    if (branches.Count == 0 || admin is null)
        throw new E2EFailureException("an active branch and admin identity are required.");

    var branchA = branches[0].Id;
    var branchB = branches.Skip(1).Select(x => x.Id).FirstOrDefault();
    var adminUser = new InventoryUserContext("admin", admin.Id, null);
    var managerUser = manager?.BranchId is Guid managerBranch
        ? new InventoryUserContext("manager", manager.Id, managerBranch)
        : null;
    var employeeUser = employee?.BranchId is Guid employeeBranch
        ? new InventoryUserContext("employee", employee.Id, employeeBranch)
        : null;
    MarkPass();

    Mark("E2E 02", "Create unique InventoryItems");
    var coffee = await CreateItemAsync(connectionString, adminUser, $"{prefix}_COFFEE", "kg");
    var salt = await CreateItemAsync(connectionString, adminUser, $"{prefix}_SALT", "kg");
    MarkPass();

    Mark("E2E 03", "Create receipt Draft and verify no side effects");
    var receipt = await CreateReceiptAsync(connectionString, adminUser, branchA, coffee.Id, 10m, 100000m, $"{prefix}_RECEIPT");
    await AssertDraftHasNoEffectsAsync(connectionString, receipt.Id, branchA, coffee.Id, "receipt draft");
    Assert(receipt.TotalAmount == 1000000m, "receipt total must be calculated server-side.");
    MarkPass();

    Mark("E2E 04", "Confirm receipt and verify stock, ledger, and Expense");
    await ConfirmReceiptAsync(connectionString, adminUser, receipt.Id);
    var afterReceipt = await GetBalanceAsync(connectionString, branchA, coffee.Id);
    Assert(afterReceipt == 10m, "receipt confirmation must increase stock by 10.");
    await AssertReceiptEffectsAsync(connectionString, receipt.Id, branchA, coffee.Id, 1, "receipt confirmation");
    MarkPass();

    Mark("E2E 05", "Repeat receipt confirmation and test immutability");
    await ConfirmReceiptAsync(connectionString, adminUser, receipt.Id);
    await AssertReceiptEffectsAsync(connectionString, receipt.Id, branchA, coffee.Id, 1, "receipt double confirmation");
    await ExpectConflictAsync(() => UpdateReceiptAsync(connectionString, adminUser, receipt.Id), "confirmed receipt immutability");
    MarkPass();

    Mark("E2E 06", "Create and confirm issue; verify OUT ledger and no Expense");
    var issue = await CreateIssueAsync(connectionString, adminUser, branchA, coffee.Id, 4m, $"{prefix}_ISSUE");
    await AssertDraftHasNoEffectsAsync(connectionString, issue.Id, branchA, coffee.Id, "issue draft");
    await ConfirmIssueAsync(connectionString, adminUser, issue.Id);
    Assert(await GetBalanceAsync(connectionString, branchA, coffee.Id) == 6m, "issue confirmation must reduce stock by 4.");
    await AssertIssueEffectsAsync(connectionString, issue.Id, branchA, coffee.Id, 1, "issue confirmation");
    await ConfirmIssueAsync(connectionString, adminUser, issue.Id);
    await AssertIssueEffectsAsync(connectionString, issue.Id, branchA, coffee.Id, 1, "issue double confirmation");
    await ExpectConflictAsync(() => UpdateIssueAsync(connectionString, adminUser, issue.Id), "confirmed issue immutability");
    MarkPass();

    Mark("E2E 07", "Verify multi-item insufficient-stock atomic rollback");
    var saltReceipt = await CreateReceiptAsync(connectionString, adminUser, branchA, salt.Id, 2m, 10m, $"{prefix}_SALT_RECEIPT");
    await ConfirmReceiptAsync(connectionString, adminUser, saltReceipt.Id);
    var atomicIssue = await CreateMultiItemIssueAsync(connectionString, adminUser, branchA, coffee.Id, 1m, salt.Id, 3m, $"{prefix}_ATOMIC");
    await ExpectConflictAsync(() => ConfirmIssueAsync(connectionString, adminUser, atomicIssue.Id), "multi-item insufficient-stock rollback");
    Assert(await GetBalanceAsync(connectionString, branchA, coffee.Id) == 6m, "atomic failure changed sufficient item stock.");
    Assert(await GetBalanceAsync(connectionString, branchA, salt.Id) == 2m, "atomic failure changed insufficient item stock.");
    await AssertIssueDraftAsync(connectionString, atomicIssue.Id, "atomic issue remains draft");
    MarkPass();

    Mark("E2E 08", "Verify negative-stock rejection");
    var overIssue = await CreateIssueAsync(connectionString, adminUser, branchA, coffee.Id, 7m, $"{prefix}_OVER");
    await ExpectConflictAsync(() => ConfirmIssueAsync(connectionString, adminUser, overIssue.Id), "negative-stock prevention");
    Assert(await GetBalanceAsync(connectionString, branchA, coffee.Id) == 6m, "negative-stock rejection changed stock.");
    MarkPass();

    Mark("E2E 09", "Verify document idempotency and Draft cancellation");
    await AssertIdempotencyAsync(connectionString, adminUser, branchA, coffee.Id, prefix);
    await AssertDraftCancellationAsync(connectionString, adminUser, branchA, coffee.Id, prefix);
    MarkPass();

    Mark("E2E 10", "Verify branch and role authorization");
    if (managerUser is not null)
    {
        var managerBranchId = managerUser.BranchId!.Value;
        await ExpectForbiddenAsync(() => GetBranchInventoryAsync(connectionString, managerUser, managerBranchId == branchA ? branchB : branchA), "manager cross-branch read", branchB != Guid.Empty && managerBranchId != branchB);
        if (managerBranchId == branchA && branchB != Guid.Empty)
        {
            var crossReceipt = await CreateReceiptAsync(connectionString, adminUser, branchB, coffee.Id, 1m, 1m, $"{prefix}_CROSS_RECEIPT");
            await ExpectForbiddenAsync(() => ConfirmReceiptAsync(connectionString, managerUser, crossReceipt.Id), "manager cross-branch receipt confirmation");
            var crossIssue = await CreateIssueAsync(connectionString, adminUser, branchB, coffee.Id, 1m, $"{prefix}_CROSS_ISSUE");
            await ExpectForbiddenAsync(() => ConfirmIssueAsync(connectionString, managerUser, crossIssue.Id), "manager cross-branch issue confirmation");
            Console.WriteLine("BRANCH ISOLATION: PASS (manager read/confirm operations blocked outside assigned branch)");
        }
        if (managerBranchId != branchA)
            Console.WriteLine("ROLE CHECK: manager is assigned to a different branch; cross-branch mutation checks require matching fixture branches.");
    }

    if (employeeUser is not null && employeeUser.BranchId == branchA)
    {
        var employeeIssue = await CreateIssueAsync(connectionString, employeeUser, branchA, coffee.Id, 1m, $"{prefix}_EMPLOYEE");
        await ExpectForbiddenAsync(() => ConfirmIssueAsync(connectionString, employeeUser, employeeIssue.Id), "employee issue confirmation");
        Console.WriteLine("ROLE CHECK: employee draft allowed and confirmation forbidden.");
    }
    else
    {
        Console.WriteLine("ROLE CHECK: employee fixture skipped (no active employee assigned to Branch A).");
    }
    MarkPass();

    Mark("E2E 11", "Verify concurrent 7 + 7 issues from stock 10");
    await RunConcurrencyAsync(connectionString, adminUser, branchA, prefix);
    MarkPass();
    Console.WriteLine($"E2E PREFIX: {prefix}");
    Console.WriteLine("CLEANUP: retained because confirmed documents and ledger are historical/immutable.");
}

static async Task RunConcurrencyAsync(string cs, InventoryUserContext admin, Guid branchId, string prefix)
{
    var item = await CreateItemAsync(cs, admin, $"{prefix}_CONCURRENCY", "kg");
    var receipt = await CreateReceiptAsync(cs, admin, branchId, item.Id, 10m, 1m, $"{prefix}_CONCURRENCY_RECEIPT");
    await ConfirmReceiptAsync(cs, admin, receipt.Id);
    var issueA = await CreateIssueAsync(cs, admin, branchId, item.Id, 7m, $"{prefix}_CONCURRENCY_A");
    var issueB = await CreateIssueAsync(cs, admin, branchId, item.Id, 7m, $"{prefix}_CONCURRENCY_B");

    var results = await Task.WhenAll(
        ConfirmIssueOutcomeAsync(cs, admin, issueA.Id),
        ConfirmIssueOutcomeAsync(cs, admin, issueB.Id));
    Assert(results.Count(x => x) == 1, "exactly one concurrent 7-unit issue must confirm from stock 10.");
    Assert(await GetBalanceAsync(cs, branchId, item.Id) == 3m, "concurrent final stock must be 3.");
    await using var context = CreateContext(cs);
    var ledgerCount = await context.StockTransactions.CountAsync(x => x.ReferenceType == StockReferenceTypes.StockIssue && (x.ReferenceId == issueA.Id || x.ReferenceId == issueB.Id));
    Assert(ledgerCount == 1, "concurrent confirmation must create exactly one OUT ledger row.");
    Console.WriteLine("CONCURRENCY 7+7 FROM 10: PASS (two independent DbContexts; service uses PostgreSQL FOR UPDATE in deterministic item order)");
}

static async Task<bool> ConfirmIssueOutcomeAsync(string cs, InventoryUserContext user, Guid id)
{
    try { await ConfirmIssueAsync(cs, user, id); return true; }
    catch (InventoryConflictException) { return false; }
}

static async Task<InventoryItemResponse> CreateItemAsync(string cs, InventoryUserContext user, string name, string unit)
{
    await using var context = CreateContext(cs);
    return await new InventoryService(context).CreateItemAsync(new CreateInventoryItemRequest(name, unit), user);
}

static async Task<StockReceiptResponse> CreateReceiptAsync(string cs, InventoryUserContext user, Guid branch, Guid item, decimal quantity, decimal price, string key)
{
    await using var context = CreateContext(cs);
    return await new InventoryService(context).CreateReceiptAsync(new CreateStockReceiptRequest(branch, "E2E supplier", null, "E2E_INV", key, new[] { new StockDocumentItemRequest(item, quantity, price) }), user);
}

static async Task<StockIssueResponse> CreateIssueAsync(string cs, InventoryUserContext user, Guid branch, Guid item, decimal quantity, string key)
{
    await using var context = CreateContext(cs);
    return await new InventoryService(context).CreateIssueAsync(new CreateStockIssueRequest(branch, "E2E inventory validation", "E2E_INV", key, new[] { new StockDocumentItemRequest(item, quantity) }), user);
}

static async Task<StockIssueResponse> CreateMultiItemIssueAsync(string cs, InventoryUserContext user, Guid branch, Guid itemA, decimal quantityA, Guid itemB, decimal quantityB, string key)
{
    await using var context = CreateContext(cs);
    return await new InventoryService(context).CreateIssueAsync(new CreateStockIssueRequest(branch, "E2E atomic validation", "E2E_INV", key, new[] { new StockDocumentItemRequest(itemA, quantityA), new StockDocumentItemRequest(itemB, quantityB) }), user);
}

static async Task ConfirmReceiptAsync(string cs, InventoryUserContext user, Guid id)
{
    await using var context = CreateContext(cs);
    await new InventoryService(context).ConfirmReceiptAsync(id, user);
}

static async Task ConfirmIssueAsync(string cs, InventoryUserContext user, Guid id)
{
    await using var context = CreateContext(cs);
    await new InventoryService(context).ConfirmIssueAsync(id, user);
}

static async Task UpdateReceiptAsync(string cs, InventoryUserContext user, Guid id)
{
    await using var context = CreateContext(cs);
    await new InventoryService(context).UpdateReceiptAsync(id, new UpdateStockReceiptRequest("changed", null, "changed", new[] { new StockDocumentItemRequest(Guid.Empty, 1m, 1m) }), user);
}

static async Task UpdateIssueAsync(string cs, InventoryUserContext user, Guid id)
{
    await using var context = CreateContext(cs);
    await new InventoryService(context).UpdateIssueAsync(id, new UpdateStockIssueRequest("changed", "changed", new[] { new StockDocumentItemRequest(Guid.Empty, 1m) }), user);
}

static async Task<decimal> GetBalanceAsync(string cs, Guid branch, Guid item)
{
    await using var context = CreateContext(cs);
    return await context.BranchInventories.Where(x => x.BranchId == branch && x.InventoryItemId == item).Select(x => (decimal?)x.CurrentQuantity).SingleOrDefaultAsync() ?? 0m;
}

static async Task AssertDraftHasNoEffectsAsync(string cs, Guid documentId, Guid branch, Guid item, string label)
{
    await using var context = CreateContext(cs);
    var balance = await context.BranchInventories.Where(x => x.BranchId == branch && x.InventoryItemId == item).Select(x => (decimal?)x.CurrentQuantity).SingleOrDefaultAsync() ?? 0m;
    var ledger = await context.StockTransactions.CountAsync(x => x.ReferenceId == documentId);
    var expense = await context.Expenses.CountAsync(x => x.StockReceiptId == documentId);
    Assert(balance == 0m || label != "receipt draft", $"{label}: unexpected balance mutation.");
    Assert(ledger == 0, $"{label}: ledger was created before confirmation.");
    Assert(expense == 0, $"{label}: Expense was created before confirmation.");
}

static async Task AssertReceiptEffectsAsync(string cs, Guid id, Guid branch, Guid item, int expectedCount, string label)
{
    await using var context = CreateContext(cs);
    var ledger = await context.StockTransactions.CountAsync(x => x.ReferenceType == StockReferenceTypes.StockReceipt && x.ReferenceId == id && x.Type == StockTransactionType.IN);
    var receipt = await context.StockReceipts.AsNoTracking().SingleAsync(x => x.Id == id);
    var expenses = await context.Expenses.AsNoTracking().Where(x => x.StockReceiptId == id).ToListAsync();
    Assert(ledger == expectedCount, $"{label}: expected {expectedCount} IN ledger row(s).");
    Assert(expenses.Count == expectedCount, $"{label}: expected {expectedCount} linked Expense row(s).");
    if (expectedCount == 1)
    {
        var expense = expenses[0];
        Assert(expense.PaymentMethod == ExpensePaymentMethods.Cash, $"{label}: Expense PaymentMethod is not the canonical non-null cash value.");
        Assert(!string.IsNullOrWhiteSpace(expense.Note), $"{label}: Expense Note is null or empty.");
        Assert(expense.Category == "Nhập hàng", $"{label}: Expense category is incorrect.");
        Assert(expense.Amount == receipt.TotalAmount, $"{label}: Expense amount does not match receipt total.");
        Assert(expense.StockReceiptId == id, $"{label}: Expense link is incorrect.");
    }
    Assert(await context.StockReceipts.AnyAsync(x => x.Id == id && x.Status == StockDocumentStatus.Confirmed && x.BranchId == branch), $"{label}: receipt is not confirmed.");
}

static async Task AssertIssueEffectsAsync(string cs, Guid id, Guid branch, Guid item, int expectedCount, string label)
{
    await using var context = CreateContext(cs);
    var ledger = await context.StockTransactions.CountAsync(x => x.ReferenceType == StockReferenceTypes.StockIssue && x.ReferenceId == id && x.Type == StockTransactionType.OUT);
    var issueExpense = await context.Expenses.CountAsync(x => x.Note == "E2E_INV" && x.StockReceiptId == null && x.Description.Contains(id.ToString()));
    Assert(ledger == expectedCount, $"{label}: expected {expectedCount} OUT ledger row(s).");
    Assert(issueExpense == 0, $"{label}: issue created an Expense.");
    Assert(await context.StockIssues.AnyAsync(x => x.Id == id && x.Status == StockDocumentStatus.Confirmed && x.BranchId == branch), $"{label}: issue is not confirmed.");
}

static async Task AssertIssueDraftAsync(string cs, Guid id, string label)
{
    await using var context = CreateContext(cs);
    Assert(await context.StockIssues.AnyAsync(x => x.Id == id && x.Status == StockDocumentStatus.Draft), $"{label}: issue changed status.");
    Assert(!await context.StockTransactions.AnyAsync(x => x.ReferenceId == id), $"{label}: issue created a ledger row.");
}

static async Task AssertIdempotencyAsync(string cs, InventoryUserContext user, Guid branch, Guid item, string prefix)
{
    var key = $"{prefix}_IDEMPOTENT";
    var first = await CreateReceiptAsync(cs, user, branch, item, 1m, 1m, key);
    var second = await CreateReceiptAsync(cs, user, branch, item, 1m, 1m, key);
    Assert(first.Id == second.Id, "duplicate receipt IdempotencyKey created two documents.");
    var issueKey = $"{prefix}_ISSUE_IDEMPOTENT";
    var firstIssue = await CreateIssueAsync(cs, user, branch, item, 1m, issueKey);
    var secondIssue = await CreateIssueAsync(cs, user, branch, item, 1m, issueKey);
    Assert(firstIssue.Id == secondIssue.Id, "duplicate issue IdempotencyKey created two documents.");
    Console.WriteLine("DOCUMENT IDEMPOTENCY: PASS");
}

static async Task AssertDraftCancellationAsync(string cs, InventoryUserContext user, Guid branch, Guid item, string prefix)
{
    await using (var context = CreateContext(cs))
        await new InventoryService(context).CancelReceiptAsync((await CreateReceiptAsync(cs, user, branch, item, 1m, 1m, $"{prefix}_CANCEL_RECEIPT")).Id, user);
    await using (var context = CreateContext(cs))
        await new InventoryService(context).CancelIssueAsync((await CreateIssueAsync(cs, user, branch, item, 1m, $"{prefix}_CANCEL_ISSUE")).Id, user);
    Console.WriteLine("DRAFT CANCELLATION: PASS");
}

static async Task ReportLeftoverE2EDataAsync(string cs)
{
    await using var context = CreateContext(cs);
    var itemCount = await context.InventoryItems.CountAsync(x => x.Name.StartsWith("E2E_INV_"));
    var receiptCount = await context.StockReceipts.CountAsync(x => x.Note == "E2E_INV");
    var issueCount = await context.StockIssues.CountAsync(x => x.Note == "E2E_INV");
    var transactionCount = await context.StockTransactions.CountAsync(x => x.Note == "E2E_INV");
    var expenseCount = await context.Expenses.CountAsync(x => x.Note == "E2E_INV");
    var total = itemCount + receiptCount + issueCount + transactionCount + expenseCount;
    Console.WriteLine($"LEFTOVER E2E DATA: {(total > 0 ? "YES" : "NO")}");
    Console.WriteLine($"LEFTOVER COUNTS: InventoryItems={itemCount}; StockReceipts={receiptCount}; StockIssues={issueCount}; StockTransactions={transactionCount}; Expenses={expenseCount}");
}

static void Mark(string phase, string description)
{
    E2EState.CurrentPhase = $"[{phase}] {description}";
    Console.WriteLine($"{E2EState.CurrentPhase} - START");
}

static void MarkPass() => Console.WriteLine($"{E2EState.CurrentPhase} - PASS");

static void PrintDbUpdateDiagnostics(DbUpdateException exception)
{
    Console.WriteLine("DB ERROR TYPE: DbUpdateException");
    foreach (var entry in exception.Entries)
        Console.WriteLine($"FAILED ENTITY: {entry.Metadata.ClrType.Name} - {entry.State}");

    var postgres = FindPostgresException(exception);
    if (postgres is not null)
    {
        Console.WriteLine("INNER DB ERROR TYPE: PostgresException");
        Console.WriteLine($"SQLSTATE: {postgres.SqlState}");
        Console.WriteLine($"SEVERITY: {postgres.Severity}");
        Console.WriteLine($"CONSTRAINT: {postgres.ConstraintName ?? "<none>"}");
        Console.WriteLine($"TABLE: {postgres.TableName ?? "<none>"}");
        Console.WriteLine($"COLUMN: {postgres.ColumnName ?? "<none>"}");
        Console.WriteLine($"DATA TYPE: {postgres.DataTypeName ?? "<none>"}");
        Console.WriteLine($"SCHEMA: {postgres.SchemaName ?? "<none>"}");
        Console.WriteLine($"MESSAGE: {postgres.MessageText}");
    }
    else
    {
        Console.WriteLine($"INNER ERROR TYPES: {GetInnerExceptionTypes(exception)}");
        Console.WriteLine($"SAFE MESSAGE: {exception.Message}");
    }
}

static PostgresException? FindPostgresException(Exception exception)
{
    for (var current = exception; current is not null; current = current.InnerException)
        if (current is PostgresException postgres) return postgres;
    return null;
}

static string GetInnerExceptionTypes(Exception exception)
{
    var types = new List<string>();
    for (var current = exception; current is not null; current = current.InnerException)
        types.Add(current.GetType().Name);
    return string.Join(" -> ", types);
}

static async Task GetBranchInventoryAsync(string cs, InventoryUserContext user, Guid branch)
{
    await using var context = CreateContext(cs);
    await new InventoryService(context).GetBranchInventoryAsync(branch, user);
}

static async Task ExpectConflictAsync(Func<Task> action, string label)
{
    try { await action(); throw new E2EFailureException($"{label}: expected conflict but operation succeeded."); }
    catch (InventoryConflictException) { }
}

static async Task ExpectForbiddenAsync(Func<Task> action, string label, bool enabled = true)
{
    if (!enabled) { Console.WriteLine($"{label}: SKIPPED (no second branch fixture)."); return; }
    try { await action(); throw new E2EFailureException($"{label}: expected forbidden but operation succeeded."); }
    catch (InventoryForbiddenException) { }
}

static void Assert(bool condition, string message)
{
    if (!condition) throw new E2EFailureException(message);
}

static ApplicationDbContext CreateContext(string connectionString)
{
    var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(connectionString).Options;
    return new ApplicationDbContext(options);
}

static int Fail(string message)
{
    Console.WriteLine(message);
    return 2;
}

static NpgsqlConnectionStringBuilder NormalizeConnectionString(string raw)
{
    var value = raw.Trim();
    if (value.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase) || value.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase))
    {
        var uri = new Uri(value);
        var userInfo = uri.UserInfo.Split(':', 2);
        if (userInfo.Length != 2 || string.IsNullOrWhiteSpace(uri.Host)) throw new UriFormatException("PostgreSQL URI is invalid.");
        return new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host, Port = uri.IsDefaultPort ? 5432 : uri.Port, Database = uri.AbsolutePath.Trim('/'),
            Username = Uri.UnescapeDataString(userInfo[0]), Password = Uri.UnescapeDataString(userInfo[1]), SslMode = SslMode.Require
        };
    }
    return new NpgsqlConnectionStringBuilder(value);
}

file sealed class E2EFailureException(string message) : Exception(message);

file static class E2EState
{
    public static string CurrentPhase { get; set; } = "[E2E 00] target validation";
}
