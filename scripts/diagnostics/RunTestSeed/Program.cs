using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Npgsql;
using RestaurantPOS.Infrastructure.Configuration;
using RestaurantPOS.Infrastructure.Persistence;
using RestaurantPOS.TestSeed;

const string latestMigration = "20261013091926_AddManualExpenseIdempotency";
const int expectedMigrationCount = 48;
const string testEnvironment = "Development";
const string advisoryLockSql = "SELECT pg_advisory_xact_lock(hashtextextended('doan-pos-test-seed', 0));";

var mode = args.Length == 0 ? "--dry-run" : args.Length == 1 ? args[0] : "--invalid";
if (mode is not ("--dry-run" or "--check-only" or "--apply"))
    return Fail("MODE CHECK: use --dry-run, --check-only or --apply");

var root = Directory.GetCurrentDirectory();
if (!File.Exists(Path.Combine(root, "dotnet-tools.json")) ||
    !File.Exists(Path.Combine(root, "services", "api", "RestaurantPOS.api.csproj")))
    return Fail("REPOSITORY CHECK: FAIL");

var configuration = new ConfigurationBuilder()
    .AddUserSecrets<Program>(optional: true)
    .AddEnvironmentVariables()
    .Build();

if (!string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), testEnvironment, StringComparison.Ordinal))
    return Fail("ENVIRONMENT CHECK: FAIL (Development required)");

if (!string.Equals(configuration["Database:TargetMode"], SupabaseProjectTargetGuard.TestMode, StringComparison.OrdinalIgnoreCase) ||
    !string.Equals(configuration["Database:ExpectedProjectRef"], SupabaseProjectTargetGuard.TestProjectRef, StringComparison.Ordinal))
    return Fail("TEST TARGET CONFIGURATION: FAIL");

if (!bool.TryParse(configuration["DatabaseSafety:AllowAutomaticMigration"], out var allowAutomaticMigration) || allowAutomaticMigration)
    return Fail("AUTOMATIC MIGRATION CHECK: FAIL");

var rawConnectionString = configuration.GetConnectionString("DefaultConnection")
    ?? configuration["SUPABASE_CONNECTION_STRING"];
if (string.IsNullOrWhiteSpace(rawConnectionString))
    return Fail("DATABASE CREDENTIAL CONFIGURATION: FAIL");

NpgsqlConnectionStringBuilder connection;
try
{
    connection = NormalizeConnectionString(rawConnectionString);
    SupabaseProjectTargetGuard.Validate(connection, configuration, isDevelopment: true);
}
catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or UriFormatException)
{
    return Fail($"TARGET CHECK: FAIL ({ex.GetType().Name})");
}

if (string.IsNullOrWhiteSpace(connection.Password))
    return Fail("DATABASE CREDENTIAL CONFIGURATION: PASSWORD_MISSING");

connection.MaxPoolSize = 5;
connection.MinPoolSize = 0;
connection.Timeout = 15;
connection.CommandTimeout = 15;

Console.WriteLine("TEST TARGET CHECK: PASS");
Console.WriteLine("PROJECT REF: eorboopkwgmvainlxzkz");
Console.WriteLine($"TARGET: {connection.Host}:{connection.Port}/{connection.Database}");
Console.WriteLine("MAIN TARGET: REJECTED");
Console.WriteLine("ENVIRONMENT: Development");
Console.WriteLine("AUTOMATIC MIGRATION/SEED: DISABLED");
Console.WriteLine($"MODE: {mode}");

if (mode == "--dry-run")
{
    var plan = TestSeedPlan.Create("OfflinePlanPassword!");
    Console.WriteLine($"SEED PLAN: BRANCHES={TestSeedPlan.ExpectedBranchCount}; AREAS={TestSeedPlan.ExpectedAreaCount}; TABLES={TestSeedPlan.ExpectedTableCount}; ADMINS={TestSeedPlan.ExpectedAdminCount}; PRODUCTS={TestSeedPlan.ExpectedProductCount}");
    Console.WriteLine($"QR TOKENS: VALID={TestSeedPlan.HasValidQrTokens(plan.Tables)}");
    Console.WriteLine("DATABASE MUTATION: NONE");
    Console.WriteLine("SAFE RESULT: DRY_RUN_PASS");
    return 0;
}

var options = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseNpgsql(connection.ConnectionString)
    .Options;

await using var context = new ApplicationDbContext(options);
using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(60));
IDbContextTransaction? transaction = null;
var transactionCommitted = false;
var rollbackReported = false;
var currentStage = "CONNECTION_OPEN";

try
{
    await context.Database.OpenConnectionAsync(timeout.Token);
    Console.WriteLine("STAGE CONNECTION_OPEN: PASS");

    currentStage = "SCHEMA_VALIDATION";
    if (!await VerifyMigrationsAsync(context, timeout.Token))
    {
        Console.WriteLine("STAGE SCHEMA_VALIDATION: FAIL");
        return Fail("MIGRATION CHECK: FAIL");
    }

    Console.WriteLine("STAGE SCHEMA_VALIDATION: PASS");

    if (mode == "--check-only")
    {
        currentStage = "DATASET_RECHECK";
        var state = await ReadSeedStateAsync(context, timeout.Token);
        Console.WriteLine("STAGE DATASET_RECHECK: PASS");
        PrintState(state);
        Console.WriteLine("DATABASE MUTATION: NONE");
        Console.WriteLine("SAFE RESULT: READ_ONLY_CHECK_PASS");
        return 0;
    }

    currentStage = "TRANSACTION_BEGIN";
    transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, timeout.Token);
    Console.WriteLine("STAGE TRANSACTION_BEGIN: PASS");

    currentStage = "ADVISORY_LOCK";
    await context.Database.ExecuteSqlRawAsync(advisoryLockSql, timeout.Token);
    Console.WriteLine("STAGE ADVISORY_LOCK: PASS");

    // Re-read state while holding the transaction lock so two seed processes cannot both write.
    currentStage = "DATASET_RECHECK";
    var stateBeforeSeed = await ReadSeedStateAsync(context, timeout.Token);
    Console.WriteLine("STAGE DATASET_RECHECK: PASS");
    PrintState(stateBeforeSeed);
    if (stateBeforeSeed.IsComplete)
    {
        await transaction.RollbackAsync(timeout.Token);
        rollbackReported = true;
        Console.WriteLine("STAGE TRANSACTION_ROLLBACK: PASS");
        Console.WriteLine("SEED RESULT: ALREADY_SEEDED");
        Console.WriteLine("DATABASE MUTATION: NONE");
        return 0;
    }

    if (!stateBeforeSeed.IsEmpty)
    {
        await transaction.RollbackAsync(timeout.Token);
        rollbackReported = true;
        Console.WriteLine("STAGE TRANSACTION_ROLLBACK: PASS");
        return Fail("SEED STATE: PARTIAL_OR_CONFLICTING_DATASET");
    }

    var adminPassword = configuration["Seed:AdminPassword"]
        ?? configuration["RESTAURANTPOS_SEED_ADMIN_PASSWORD"];
    if (string.IsNullOrWhiteSpace(adminPassword))
    {
        await transaction.RollbackAsync(timeout.Token);
        rollbackReported = true;
        Console.WriteLine("STAGE TRANSACTION_ROLLBACK: PASS");
        return Fail("ADMIN PASSWORD: MISSING_PRIVATE_CONFIGURATION");
    }

    var data = TestSeedPlan.Create(adminPassword);
    TestSeedPlan.AddTo(context, data);
    currentStage = "SEED_SAVE";
    await context.SaveChangesAsync(timeout.Token);
    Console.WriteLine("STAGE SEED_SAVE: PASS");

    currentStage = "TRANSACTION_COMMIT";
    await transaction.CommitAsync(timeout.Token);
    transactionCommitted = true;
    Console.WriteLine("STAGE TRANSACTION_COMMIT: PASS");

    context.ChangeTracker.Clear();
    currentStage = "DATASET_RECHECK";
    var stateAfterSeed = await ReadSeedStateAsync(context, timeout.Token);
    Console.WriteLine("STAGE DATASET_RECHECK: PASS");
    PrintState(stateAfterSeed);
    if (!stateAfterSeed.IsComplete)
        return Fail("POST-SEED VERIFICATION: FAIL");

    Console.WriteLine("SEED RESULT: APPLIED");
    Console.WriteLine("DATABASE MUTATION: TEST_ONLY_SEED");
    return 0;
}
catch (Exception ex)
{
    if (transaction is not null && !transactionCommitted && !rollbackReported)
    {
        try
        {
            await transaction.RollbackAsync(CancellationToken.None);
            Console.WriteLine("STAGE TRANSACTION_ROLLBACK: PASS");
        }
        catch (Exception rollbackException)
        {
            Console.WriteLine($"STAGE TRANSACTION_ROLLBACK: FAIL ({ClassifyException(rollbackException)}; SQLSTATE={GetSqlState(rollbackException) ?? "NOT_AVAILABLE"})");
        }
    }

    return FailException(currentStage, ex);
}
finally
{
    if (transaction is not null)
        await transaction.DisposeAsync();

    try
    {
        await context.Database.CloseConnectionAsync();
    }
    catch
    {
        // Connection-close diagnostics must never expose provider details or replace the primary result.
    }
}

static async Task<bool> VerifyMigrationsAsync(ApplicationDbContext context, CancellationToken cancellationToken)
{
    var available = context.Database.GetMigrations().ToArray();
    var applied = (await context.Database.GetAppliedMigrationsAsync(cancellationToken)).ToArray();
    var exact = available.Length == expectedMigrationCount &&
                applied.Length == expectedMigrationCount &&
                available.ToHashSet(StringComparer.Ordinal).SetEquals(applied) &&
                string.Equals(applied.OrderBy(value => value, StringComparer.Ordinal).LastOrDefault(), latestMigration, StringComparison.Ordinal);

    Console.WriteLine($"MIGRATIONS: EXPECTED={expectedMigrationCount}; AVAILABLE={available.Length}; APPLIED={applied.Length}; COMPLETE={exact}");
    return exact;
}

static async Task<SeedState> ReadSeedStateAsync(ApplicationDbContext context, CancellationToken cancellationToken)
{
    var branches = await context.Branches.AsNoTracking().ToListAsync(cancellationToken);
    var areas = await context.Areas.AsNoTracking().ToListAsync(cancellationToken);
    var tables = await context.Tables.AsNoTracking().ToListAsync(cancellationToken);
    var employees = await context.Employees.AsNoTracking().ToListAsync(cancellationToken);
    var products = await context.Products.AsNoTracking().ToListAsync(cancellationToken);

    var otherRecordCount =
        await context.Customers.LongCountAsync(cancellationToken) +
        await context.Orders.LongCountAsync(cancellationToken) +
        await context.OrderDetails.LongCountAsync(cancellationToken) +
        await context.Expenses.LongCountAsync(cancellationToken) +
        await context.Attendances.LongCountAsync(cancellationToken) +
        await context.WorkSchedules.LongCountAsync(cancellationToken) +
        await context.Shifts.LongCountAsync(cancellationToken) +
        await context.Reservations.LongCountAsync(cancellationToken) +
        await context.Toppings.LongCountAsync(cancellationToken) +
        await context.Promotions.LongCountAsync(cancellationToken) +
        await context.OrderRequests.LongCountAsync(cancellationToken) +
        await context.OrderRequestItems.LongCountAsync(cancellationToken) +
        await context.Notifications.LongCountAsync(cancellationToken) +
        await context.ReceiptSettings.LongCountAsync(cancellationToken) +
        await context.SystemSettings.LongCountAsync(cancellationToken) +
        await context.LoyaltyTransactions.LongCountAsync(cancellationToken) +
        await context.BusinessInsights.LongCountAsync(cancellationToken) +
        await context.InventoryItems.LongCountAsync(cancellationToken) +
        await context.BranchInventories.LongCountAsync(cancellationToken) +
        await context.StockReceipts.LongCountAsync(cancellationToken) +
        await context.StockReceiptItems.LongCountAsync(cancellationToken) +
        await context.StockIssues.LongCountAsync(cancellationToken) +
        await context.StockIssueItems.LongCountAsync(cancellationToken) +
        await context.StockTransactions.LongCountAsync(cancellationToken);

    var branch = branches.Count == TestSeedPlan.ExpectedBranchCount ? branches[0] : null;
    var admin = employees.Count == TestSeedPlan.ExpectedAdminCount ? employees[0] : null;
    var expected = branches.Count == TestSeedPlan.ExpectedBranchCount &&
                   areas.Count == TestSeedPlan.ExpectedAreaCount &&
                   tables.Count == TestSeedPlan.ExpectedTableCount &&
                   employees.Count == TestSeedPlan.ExpectedAdminCount &&
                   products.Count == TestSeedPlan.ExpectedProductCount &&
                   otherRecordCount == 0 &&
                   TestSeedPlan.MatchesExpectedBranch(branch) &&
                   TestSeedPlan.MatchesExpectedAreas(areas) &&
                   TestSeedPlan.MatchesExpectedTables(tables) &&
                   TestSeedPlan.MatchesExpectedAdmin(admin) &&
                   TestSeedPlan.MatchesExpectedProducts(products);

    var seedRowsEmpty = branches.Count == 0 && areas.Count == 0 && tables.Count == 0 && employees.Count == 0 && products.Count == 0 && otherRecordCount == 0;
    return new SeedState(branches.Count, areas.Count, tables.Count, employees.Count, products.Count, otherRecordCount, seedRowsEmpty, expected);
}

static void PrintState(SeedState state)
{
    Console.WriteLine($"SEED COUNTS: BRANCHES={state.BranchCount}; AREAS={state.AreaCount}; TABLES={state.TableCount}; ADMINS={state.EmployeeCount}; PRODUCTS={state.ProductCount}");
    Console.WriteLine($"NON_SEED_RECORDS_PRESENT: {state.OtherRecordCount > 0}");
    Console.WriteLine($"EXPECTED_DATASET: {state.IsComplete}");
}

static int Fail(string message)
{
    Console.WriteLine(message);
    Console.WriteLine("SAFE RESULT: BLOCKED");
    return 2;
}

static int FailException(string stage, Exception exception)
{
    Console.WriteLine($"FAILURE_STAGE: {stage}");
    Console.WriteLine($"NPGSQL_EXCEPTION_CLASSIFICATION: {ClassifyException(exception)}");
    Console.WriteLine($"SQLSTATE: {GetSqlState(exception) ?? "NOT_AVAILABLE"}");
    Console.WriteLine("SAFE RESULT: BLOCKED");
    return 2;
}

static string ClassifyException(Exception exception)
{
    if (FindException<PostgresException>(exception) is not null)
        return "POSTGRES_SERVER_ERROR";
    if (FindException<NpgsqlException>(exception) is not null)
        return "NPGSQL_CONNECTION_OR_PROTOCOL";
    if (FindException<OperationCanceledException>(exception) is not null)
        return "TIMEOUT_OR_CANCELED";
    if (FindException<InvalidOperationException>(exception) is not null)
        return "EF_OR_TRANSACTION_STATE";
    return $"UNCLASSIFIED_{exception.GetType().Name}";
}

static string? GetSqlState(Exception exception)
    => FindException<PostgresException>(exception)?.SqlState;

static TException? FindException<TException>(Exception exception)
    where TException : Exception
{
    for (var current = exception; current is not null; current = current.InnerException)
    {
        if (current is TException match)
            return match;
    }

    return null;
}

static NpgsqlConnectionStringBuilder NormalizeConnectionString(string raw)
{
    var value = raw.Trim();
    if (!value.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase) &&
        !value.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase))
        return new NpgsqlConnectionStringBuilder(value);

    var uri = new Uri(value);
    var userInfo = uri.UserInfo.Split(':', 2);
    if (userInfo.Length != 2 || string.IsNullOrWhiteSpace(uri.Host))
        throw new UriFormatException("The PostgreSQL connection URI is invalid.");

    return new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.Port > 0 ? uri.Port : 5432,
        Database = uri.AbsolutePath.Trim('/'),
        Username = Uri.UnescapeDataString(userInfo[0]),
        Password = Uri.UnescapeDataString(userInfo[1]),
        SslMode = SslMode.Require
    };
}

public sealed record SeedState(
    int BranchCount,
    int AreaCount,
    int TableCount,
    int EmployeeCount,
    int ProductCount,
    long OtherRecordCount,
    bool IsEmpty,
    bool IsComplete);
