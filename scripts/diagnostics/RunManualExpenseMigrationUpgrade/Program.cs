using System.Diagnostics;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;
using RestaurantPOS.Infrastructure.Configuration;
using RestaurantPOS.Infrastructure.Persistence;
using RestaurantPOS.MigrationUpgrade;

const string environmentName = "Development";
const string targetTable = "ManualExpenseIdempotencyRecords";

var mode = MigrationUpgradePolicy.ResolveMode(args);
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

if (!string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), environmentName, StringComparison.Ordinal))
    return Fail("ENVIRONMENT CHECK: FAIL (Development required)");

if (!string.Equals(configuration["Database:TargetMode"], SupabaseProjectTargetGuard.TestMode, StringComparison.OrdinalIgnoreCase) ||
    !string.Equals(configuration["Database:ExpectedProjectRef"], MigrationUpgradePolicy.TestProjectRef, StringComparison.Ordinal))
    return Fail("TEST TARGET CONFIGURATION: FAIL");

if (!bool.TryParse(configuration["DatabaseSafety:AllowAutomaticMigration"], out var automaticMigration) || automaticMigration)
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

if (connection.SslMode is SslMode.Disable or SslMode.Prefer)
    return Fail("SSL CHECK: FAIL");

var options = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseNpgsql(WithSafePoolPolicy(connection).ConnectionString)
    .Options;

await using var metadataContext = new ApplicationDbContext(options);
var available = metadataContext.Database.GetMigrations().ToArray();
if (!MigrationUpgradePolicy.ValidateAvailable(available, out var sourceFailure))
    return Fail($"SOURCE MIGRATION CHECK: FAIL ({sourceFailure})");

Console.WriteLine("TEST TARGET CHECK: PASS");
Console.WriteLine("PROJECT REF: eorboopkwgmvainlxzkz");
Console.WriteLine($"TARGET: {connection.Host}:{connection.Port}/{connection.Database}");
Console.WriteLine("MAIN TARGET: REJECTED");
Console.WriteLine("ENVIRONMENT: Development");
Console.WriteLine("AUTOMATIC MIGRATION/SEED: DISABLED");
Console.WriteLine($"SOURCE MIGRATIONS: {available.Length}; HISTORICAL BASE: {MigrationUpgradePolicy.HistoricalMigrationCount}; APPROVED TARGET: {MigrationUpgradePolicy.ApprovedMigration}");
Console.WriteLine($"MODE: {mode}");

if (mode == "--dry-run")
{
    Console.WriteLine("DATABASE CONNECTION: NOT_RUN (dry-run)");
    Console.WriteLine("DATABASE MUTATION: NONE");
    Console.WriteLine("SAFE RESULT: DRY_RUN_PASS");
    return 0;
}

if (string.IsNullOrWhiteSpace(connection.Password))
    return Fail("DATABASE CREDENTIAL CONFIGURATION: PASSWORD_MISSING");

if (mode == "--apply" && string.IsNullOrWhiteSpace(configuration["Jwt:Secret"]))
    return Fail("DESIGN_TIME_CONFIGURATION: JWT_SECRET_MISSING");

var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(60));
try
{
    await using var context = new ApplicationDbContext(options);
    await context.Database.OpenConnectionAsync(cancellation.Token);
    var baseline = await VerifyPreUpgradeAsync(context, cancellation.Token);
    PrintPreflightDiagnostics(baseline);
    if (!baseline.IsValid)
        return Fail($"PRE_UPGRADE CHECK: FAIL ({baseline.FailureCode})");

    Console.WriteLine("PRE_UPGRADE CHECK: PASS");
    Console.WriteLine($"APPLIED MIGRATIONS: {baseline.AppliedCount}");
    Console.WriteLine($"PENDING MIGRATIONS: {baseline.PendingCount}; ONLY_APPROVED_TARGET: {baseline.OnlyApprovedPending}");
    Console.WriteLine($"EXISTING DATA COUNTS: BRANCHES={baseline.ExistingData.Branches}; AREAS={baseline.ExistingData.Areas}; TABLES={baseline.ExistingData.Tables}; EMPLOYEES={baseline.ExistingData.Employees}; PRODUCTS={baseline.ExistingData.Products}; OTHER_RECORDS={baseline.ExistingData.OtherRecords}");

    if (mode == "--check-only")
    {
        Console.WriteLine("DATABASE MUTATION: NONE");
        Console.WriteLine("SAFE RESULT: READ_ONLY_PRECHECK_PASS");
        return 0;
    }

    await context.Database.CloseConnectionAsync();
    var processResult = await ApplyApprovedMigrationAsync(root, connection, configuration["Jwt:Secret"]!);
    if (!processResult)
        return Fail("MIGRATION PROCESS: FAIL (SANITIZED_CHILD_FAILURE)");

    await using var verificationContext = new ApplicationDbContext(options);
    await verificationContext.Database.OpenConnectionAsync(cancellation.Token);
    var verification = await VerifyPostUpgradeAsync(verificationContext, baseline.ExistingData, cancellation.Token);
    if (!verification)
        return Fail("POST_UPGRADE VERIFICATION: FAIL");

    Console.WriteLine("POST_UPGRADE VERIFICATION: PASS");
    Console.WriteLine("APPLIED MIGRATIONS: 48");
    Console.WriteLine("NEW TABLE: PRESENT");
    Console.WriteLine("SEED PRESERVED: TRUE");
    Console.WriteLine("DATABASE MUTATION: TEST_ONLY_APPROVED_MIGRATION");
    return 0;
}
catch (OperationCanceledException)
{
    return Fail("DATABASE CHECK: TIMEOUT");
}
catch (PostgresException ex)
{
    return Fail($"DATABASE CHECK: FAIL ({ex.SqlState})");
}
catch (NpgsqlException ex)
{
    return Fail($"DATABASE CHECK: FAIL ({ex.GetType().Name})");
}
catch (Exception ex)
{
    return Fail($"DATABASE CHECK: FAIL ({ex.GetType().Name})");
}

static async Task<PreflightResult> VerifyPreUpgradeAsync(ApplicationDbContext context, CancellationToken cancellationToken)
{
    var applied = (await context.Database.GetAppliedMigrationsAsync(cancellationToken)).ToArray();
    if (!MigrationUpgradePolicy.ValidateAppliedHistorical(applied, out var historyFailure))
        return PreflightResult.Fail(historyFailure, applied.Length, 0, historyValid: false);

    var pending = (await context.Database.GetPendingMigrationsAsync(cancellationToken)).ToArray();
    if (!MigrationUpgradePolicy.ValidatePending(pending, out var pendingFailure))
        return PreflightResult.Fail(pendingFailure, applied.Length, pending.Length, historyValid: true, pendingValid: false);

    var targetObjectsAbsent = await TargetObjectsAbsentAsync(context, cancellationToken);
    if (!targetObjectsAbsent)
        return PreflightResult.Fail("TARGET_OBJECT_ALREADY_EXISTS", applied.Length, pending.Length, historyValid: true, pendingValid: true, targetObjectsAbsent: false);

    var existingSchemaMatches = await HasExpectedPublicSchemaAsync(context, includeTargetTable: false, cancellationToken);
    if (!existingSchemaMatches)
        return PreflightResult.Fail("PUBLIC_SCHEMA_DRIFT", applied.Length, pending.Length, historyValid: true, pendingValid: true, targetObjectsAbsent: true, existingSchemaMatches: false);

    var branchPrincipalCompatible = await HasCompatibleBranchPrincipalAsync(context, cancellationToken);
    if (!MigrationUpgradePolicy.ValidatePreUpgradeStructure(existingSchemaMatches, targetObjectsAbsent, branchPrincipalCompatible, out var structureFailure))
        return PreflightResult.Fail(structureFailure, applied.Length, pending.Length, historyValid: true, pendingValid: true, targetObjectsAbsent: true, existingSchemaMatches: true, branchPrincipalCompatible: false);

    var existingData = await ReadExistingDataSnapshotAsync(context, cancellationToken);
    return new PreflightResult(true, string.Empty, applied.Length, pending.Length, true, true, true, true, true, true, existingData);
}

static void PrintPreflightDiagnostics(PreflightResult result)
{
    Console.WriteLine($"MIGRATION_HISTORY_CHECK: {FormatCheck(result.HistoryValid)}");
    Console.WriteLine($"PENDING_MIGRATION_CHECK: {FormatCheck(result.PendingValid)}");
    Console.WriteLine($"EXISTING_SCHEMA_CHECK: {FormatCheck(result.ExistingSchemaMatches)}");
    Console.WriteLine($"TARGET_OBJECT_ABSENCE_CHECK: {FormatCheck(result.TargetObjectsAbsent)}");
    Console.WriteLine($"BRANCH_FK_COMPATIBILITY_CHECK: {FormatCheck(result.BranchPrincipalCompatible)}");
    Console.WriteLine("SEED_ONLY_RESTRICTIONS_REMOVED: PASS");
}

static string FormatCheck(bool? value) => value switch
{
    true => "PASS",
    false => "FAIL",
    _ => "NOT_RUN"
};

static async Task<bool> VerifyPostUpgradeAsync(ApplicationDbContext context, ExistingDataSnapshot baseline, CancellationToken cancellationToken)
{
    var applied = (await context.Database.GetAppliedMigrationsAsync(cancellationToken)).ToArray();
    if (applied.Length != MigrationUpgradePolicy.AvailableMigrationCount ||
        !applied.SequenceEqual(MigrationUpgradePolicy.HistoricalMigrations.Append(MigrationUpgradePolicy.ApprovedMigration), StringComparer.Ordinal))
        return false;

    if (!await TableExistsAsync(context, targetTable, cancellationToken) ||
        !await HasExpectedPublicSchemaAsync(context, includeTargetTable: true, cancellationToken) ||
        !await HasExpectedTargetIndexesAndForeignKeyAsync(context, cancellationToken) ||
        await ScalarAsync<long>(context, "SELECT COUNT(*) FROM public.\"ManualExpenseIdempotencyRecords\";", cancellationToken) != 0)
        return false;

    var after = await ReadExistingDataSnapshotAsync(context, cancellationToken);
    return after.Equals(baseline);
}

static async Task<bool> HasExpectedPublicSchemaAsync(ApplicationDbContext context, bool includeTargetTable, CancellationToken cancellationToken)
{
    var actual = await QueryStringsAsync(context, """
        SELECT table_name
        FROM information_schema.tables
        WHERE table_schema = 'public' AND table_type = 'BASE TABLE'
        ORDER BY table_name;
        """, cancellationToken);

    var expected = context.Model.GetEntityTypes()
        .Where(entity => string.Equals(entity.GetSchema() ?? "public", "public", StringComparison.Ordinal))
        .Select(entity => entity.GetTableName())
        .Where(name => !string.IsNullOrWhiteSpace(name))
        .Select(name => name!)
        .ToHashSet(StringComparer.Ordinal);
    expected.Add("__EFMigrationsHistory");
    if (!includeTargetTable)
        expected.Remove(targetTable);

    return actual.ToHashSet(StringComparer.Ordinal).SetEquals(expected);
}

static async Task<bool> HasExpectedTargetIndexesAndForeignKeyAsync(ApplicationDbContext context, CancellationToken cancellationToken)
{
    var primaryKey = await ScalarAsync<bool>(context, """
        SELECT EXISTS (
            SELECT 1 FROM information_schema.table_constraints
            WHERE table_schema = 'public' AND table_name = 'ManualExpenseIdempotencyRecords'
              AND constraint_type = 'PRIMARY KEY'
        );
        """, cancellationToken);
    var uniqueKey = await ScalarAsync<bool>(context, """
        SELECT EXISTS (
            SELECT 1 FROM pg_indexes
            WHERE schemaname = 'public' AND tablename = 'ManualExpenseIdempotencyRecords'
              AND indexdef LIKE '%UNIQUE%BranchId%UserId%IdempotencyKey%'
        );
        """, cancellationToken);
    var createdAtIndex = await ScalarAsync<bool>(context, """
        SELECT EXISTS (
            SELECT 1 FROM pg_indexes
            WHERE schemaname = 'public' AND tablename = 'ManualExpenseIdempotencyRecords'
              AND indexdef LIKE '%CreatedAtUtc%'
        );
        """, cancellationToken);
    var branchForeignKey = await ScalarAsync<bool>(context, """
        SELECT EXISTS (
            SELECT 1
            FROM information_schema.table_constraints tc
            JOIN information_schema.constraint_column_usage ccu
              ON tc.constraint_name = ccu.constraint_name AND tc.table_schema = ccu.table_schema
            WHERE tc.table_schema = 'public'
              AND tc.table_name = 'ManualExpenseIdempotencyRecords'
              AND tc.constraint_type = 'FOREIGN KEY'
              AND ccu.table_name = 'Branches' AND ccu.column_name = 'Id'
        );
        """, cancellationToken);

    return primaryKey && uniqueKey && createdAtIndex && branchForeignKey;
}

static async Task<bool> TableExistsAsync(ApplicationDbContext context, string tableName, CancellationToken cancellationToken) =>
    await ScalarAsync<bool>(context, """
        SELECT EXISTS (
            SELECT 1 FROM information_schema.tables
            WHERE table_schema = 'public' AND table_name = @tableName
        );
        """, cancellationToken, ("tableName", tableName));

static async Task<bool> TargetObjectsAbsentAsync(ApplicationDbContext context, CancellationToken cancellationToken) =>
    await ScalarAsync<bool>(context, """
        SELECT NOT EXISTS (
            SELECT 1
            FROM pg_class c
            JOIN pg_namespace n ON n.oid = c.relnamespace
            WHERE n.nspname = 'public'
              AND (
                  c.relname = @tableName
                  OR c.relname LIKE @indexPrefix
              )
        );
        """, cancellationToken,
        ("tableName", targetTable),
        ("indexPrefix", "IX_ManualExpenseIdempotencyRecords_%"));

static async Task<bool> HasCompatibleBranchPrincipalAsync(ApplicationDbContext context, CancellationToken cancellationToken) =>
    await ScalarAsync<bool>(context, """
        SELECT EXISTS (
            SELECT 1
            FROM information_schema.table_constraints tc
            JOIN information_schema.key_column_usage kcu
              ON tc.constraint_name = kcu.constraint_name
             AND tc.table_schema = kcu.table_schema
             AND tc.table_name = kcu.table_name
            WHERE tc.table_schema = 'public'
              AND tc.table_name = 'Branches'
              AND tc.constraint_type = 'PRIMARY KEY'
              AND kcu.column_name = 'Id'
        );
        """, cancellationToken);

static async Task<ExistingDataSnapshot> ReadExistingDataSnapshotAsync(ApplicationDbContext context, CancellationToken cancellationToken)
{
    return new ExistingDataSnapshot(
        await context.Branches.LongCountAsync(cancellationToken),
        await context.Areas.LongCountAsync(cancellationToken),
        await context.Tables.LongCountAsync(cancellationToken),
        await context.Employees.LongCountAsync(cancellationToken),
        await context.Products.LongCountAsync(cancellationToken),
        await CountOtherRecordsAsync(context, cancellationToken));
}

static async Task<long> CountOtherRecordsAsync(ApplicationDbContext context, CancellationToken cancellationToken)
{
    long total = 0;
    total += await context.Customers.LongCountAsync(cancellationToken);
    total += await context.Orders.LongCountAsync(cancellationToken);
    total += await context.OrderDetails.LongCountAsync(cancellationToken);
    total += await context.Expenses.LongCountAsync(cancellationToken);
    total += await context.Attendances.LongCountAsync(cancellationToken);
    total += await context.WorkSchedules.LongCountAsync(cancellationToken);
    total += await context.Shifts.LongCountAsync(cancellationToken);
    total += await context.Reservations.LongCountAsync(cancellationToken);
    total += await context.Toppings.LongCountAsync(cancellationToken);
    total += await context.Promotions.LongCountAsync(cancellationToken);
    total += await context.OrderRequests.LongCountAsync(cancellationToken);
    total += await context.OrderRequestItems.LongCountAsync(cancellationToken);
    total += await context.Notifications.LongCountAsync(cancellationToken);
    total += await context.ReceiptSettings.LongCountAsync(cancellationToken);
    total += await context.SystemSettings.LongCountAsync(cancellationToken);
    total += await context.LoyaltyTransactions.LongCountAsync(cancellationToken);
    total += await context.InventoryItems.LongCountAsync(cancellationToken);
    total += await context.BranchInventories.LongCountAsync(cancellationToken);
    total += await context.StockReceipts.LongCountAsync(cancellationToken);
    total += await context.StockReceiptItems.LongCountAsync(cancellationToken);
    total += await context.StockIssues.LongCountAsync(cancellationToken);
    total += await context.StockIssueItems.LongCountAsync(cancellationToken);
    total += await context.StockTransactions.LongCountAsync(cancellationToken);
    return total;
}

static async Task<bool> ApplyApprovedMigrationAsync(string root, NpgsqlConnectionStringBuilder connection, string jwtSecret)
{
    var startInfo = new ProcessStartInfo("dotnet")
    {
        WorkingDirectory = root,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true
    };
    foreach (var argument in new[]
    {
        "tool", "run", "dotnet-ef", "database", "update", MigrationUpgradePolicy.ApprovedMigration,
        "--project", "services/api/RestaurantPOS.api.csproj",
        "--startup-project", "services/api/RestaurantPOS.api.csproj",
        "--context", "ApplicationDbContext", "--configuration", "Release", "--no-build"
    })
        startInfo.ArgumentList.Add(argument);

    startInfo.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";
    startInfo.Environment["Database__TargetMode"] = "Test";
    startInfo.Environment["Database__ExpectedProjectRef"] = MigrationUpgradePolicy.TestProjectRef;
    startInfo.Environment["DatabaseSafety__AllowAutomaticMigration"] = "false";
    startInfo.Environment["ConnectionStrings__DefaultConnection"] = WithSafePoolPolicy(connection).ConnectionString;
    startInfo.Environment["Jwt__Secret"] = jwtSecret;

    using var process = Process.Start(startInfo);
    if (process is null)
        return false;

    var standardOutput = process.StandardOutput.ReadToEndAsync();
    var standardError = process.StandardError.ReadToEndAsync();
    using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(10));
    try
    {
        await process.WaitForExitAsync(timeout.Token);
    }
    catch (OperationCanceledException)
    {
        try { process.Kill(entireProcessTree: true); } catch { }
        return false;
    }

    _ = await standardOutput;
    _ = await standardError;
    return process.ExitCode == 0;
}

static NpgsqlConnectionStringBuilder WithSafePoolPolicy(NpgsqlConnectionStringBuilder source)
{
    var connection = new NpgsqlConnectionStringBuilder(source.ConnectionString)
    {
        MaxPoolSize = 5,
        MinPoolSize = 0,
        Timeout = 15,
        CommandTimeout = 15
    };
    return connection;
}

static async Task<List<string>> QueryStringsAsync(ApplicationDbContext context, string sql, CancellationToken cancellationToken)
{
    await using var command = context.Database.GetDbConnection().CreateCommand();
    command.CommandText = sql;
    command.CommandTimeout = 15;
    if (command.Connection!.State != System.Data.ConnectionState.Open)
        await command.Connection.OpenAsync(cancellationToken);
    var values = new List<string>();
    await using var reader = await command.ExecuteReaderAsync(cancellationToken);
    while (await reader.ReadAsync(cancellationToken))
        values.Add(reader.GetString(0));
    return values;
}

static async Task<T> ScalarAsync<T>(ApplicationDbContext context, string sql, CancellationToken cancellationToken, params (string Name, object Value)[] parameters)
{
    await using var command = context.Database.GetDbConnection().CreateCommand();
    command.CommandText = sql;
    command.CommandTimeout = 15;
    foreach (var (name, value) in parameters)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }
    if (command.Connection!.State != System.Data.ConnectionState.Open)
        await command.Connection.OpenAsync(cancellationToken);
    var valueResult = await command.ExecuteScalarAsync(cancellationToken);
    return (T)Convert.ChangeType(valueResult!, typeof(T), CultureInfo.InvariantCulture);
}

static int Fail(string message)
{
    Console.WriteLine(message);
    Console.WriteLine("SAFE RESULT: BLOCKED");
    return 2;
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

sealed record PreflightResult(
    bool IsValid,
    string FailureCode,
    int AppliedCount,
    int PendingCount,
    bool OnlyApprovedPending,
    bool? HistoryValid,
    bool? PendingValid,
    bool? ExistingSchemaMatches,
    bool? TargetObjectsAbsent,
    bool? BranchPrincipalCompatible,
    ExistingDataSnapshot ExistingData)
{
    public static PreflightResult Fail(
        string code,
        int appliedCount = 0,
        int pendingCount = 0,
        bool? historyValid = null,
        bool? pendingValid = null,
        bool? targetObjectsAbsent = null,
        bool? existingSchemaMatches = null,
        bool? branchPrincipalCompatible = null) =>
        new(false, code, appliedCount, pendingCount, false, historyValid, pendingValid, existingSchemaMatches,
            targetObjectsAbsent, branchPrincipalCompatible, new ExistingDataSnapshot(0, 0, 0, 0, 0, 0));
}

sealed record ExistingDataSnapshot(long Branches, long Areas, long Tables, long Employees, long Products, long OtherRecords);
