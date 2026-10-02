using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;
using RestaurantPOS.Application.Common.Security;
using RestaurantPOS.Infrastructure.Persistence;

var execute = args.Contains("--execute", StringComparer.Ordinal);
var configuration = new ConfigurationBuilder()
    .AddUserSecrets<Program>(optional: true)
    .AddEnvironmentVariables()
    .Build();
var rawConnectionString = configuration.GetConnectionString("DefaultConnection")
    ?? configuration["SUPABASE_CONNECTION_STRING"];

if (string.IsNullOrWhiteSpace(rawConnectionString))
    return Fail("ConnectionStrings:DefaultConnection is missing.");

var connectionString = new NpgsqlConnectionStringBuilder(rawConnectionString);
if (!QrTokenBackfillTargetGuard.IsAuthorized(
        Environment.GetEnvironmentVariable("DEV_SUPABASE_PROJECT_REF"),
        connectionString.Host,
        connectionString.Username))
    return Fail("Target guard rejected this configuration. No write was attempted.");

var options = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseNpgsql(connectionString.ConnectionString)
    .Options;
await using var context = new ApplicationDbContext(options);

var migrationApplied = await context.Database.SqlQueryRaw<bool>("""
    SELECT EXISTS (
        SELECT 1 FROM public."__EFMigrationsHistory"
        WHERE "MigrationId" = '20261001094205_AddRestaurantTableQrToken') AS "Value"
    """).SingleAsync();
if (!migrationApplied)
    return Fail("QrToken Migration A is not present. No write was attempted.");

var schemaValid = await context.Database.SqlQueryRaw<bool>("""
    SELECT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'public' AND table_name = 'Tables' AND column_name = 'QrToken'
          AND is_nullable = 'YES' AND data_type = 'character varying' AND character_maximum_length = 43) AS "Value"
    """).SingleAsync();
if (!schemaValid)
    return Fail("QrToken column does not match Migration A. No write was attempted.");

var indexValid = await context.Database.SqlQueryRaw<bool>("""
    SELECT EXISTS (
        SELECT 1 FROM pg_index ix
        JOIN pg_class i ON i.oid = ix.indexrelid
        JOIN pg_class t ON t.oid = ix.indrelid
        JOIN pg_namespace n ON n.oid = t.relnamespace
        WHERE n.nspname = 'public' AND t.relname = 'Tables'
          AND i.relname = 'IX_Tables_QrToken' AND ix.indisunique
          AND pg_get_expr(ix.indpred, ix.indrelid) = '("QrToken" IS NOT NULL)') AS "Value"
    """).SingleAsync();
if (!indexValid)
    return Fail("QrToken unique filtered index is missing or incorrect. No write was attempted.");

var tables = await context.Tables
    .AsNoTracking()
    .Select(table => new { table.Id, table.QrToken })
    .ToListAsync();
var assignments = QrTokenBackfillPreparation.CreateAssignments(
    tables.Select(table => new RestaurantPOS.Domain.Entities.RestaurantTable { Id = table.Id, QrToken = table.QrToken }));
Console.WriteLine($"MODE={(execute ? "EXECUTE" : "DRY_RUN")}");
Console.WriteLine($"TABLES_TOTAL={tables.Count}");
Console.WriteLine($"QRTOKEN_NULL={assignments.Count}");
Console.WriteLine($"QRTOKEN_NON_NULL={tables.Count - assignments.Count}");

if (!execute)
{
    Console.WriteLine("ROWS_WRITTEN=0");
    Console.WriteLine("RESULT=DRY_RUN_COMPLETE");
    return 0;
}

await using var transaction = await context.Database.BeginTransactionAsync();
try
{
    var written = 0;
    foreach (var assignment in assignments)
    {
        var affected = await context.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE "Tables"
            SET "QrToken" = {assignment.Token}
            WHERE "Id" = {assignment.TableId} AND "QrToken" IS NULL
            """);
        if (affected != 1)
            throw new InvalidOperationException("A Table row changed concurrently; transaction rolled back without overwriting a token.");
        written++;
    }

    await transaction.CommitAsync();
    Console.WriteLine($"ROWS_WRITTEN={written}");
    await ReportTokenStateAsync(connectionString.ConnectionString);
    Console.WriteLine("RESULT=EXECUTE_COMPLETE");
    return 0;
}
catch (PostgresException exception) when (exception.SqlState == PostgresErrorCodes.UniqueViolation)
{
    await transaction.RollbackAsync();
    return Fail("A QrToken uniqueness conflict occurred; transaction rolled back.");
}
catch
{
    await transaction.RollbackAsync();
    throw;
}

static int Fail(string message)
{
    Console.Error.WriteLine($"RESULT=ABORTED: {message}");
    return 2;
}

static async Task ReportTokenStateAsync(string connectionString)
{
    await using var connection = new NpgsqlConnection(connectionString);
    await connection.OpenAsync();
    await using var command = new NpgsqlCommand("""
        SELECT
            COUNT(*) AS total,
            COUNT(*) FILTER (WHERE "QrToken" IS NULL) AS null_count,
            COUNT(*) FILTER (WHERE "QrToken" IS NOT NULL) AS non_null_count,
            COUNT(DISTINCT "QrToken") FILTER (WHERE "QrToken" IS NOT NULL) AS distinct_count,
            COUNT(*) FILTER (WHERE "QrToken" IS NOT NULL AND char_length("QrToken") <> 43) AS invalid_length_count,
            COUNT(*) FILTER (WHERE "QrToken" IS NOT NULL AND "QrToken" !~ '^[A-Za-z0-9_-]{43}$') AS invalid_base64url_count
        FROM public."Tables"
        """, connection);
    await using var reader = await command.ExecuteReaderAsync();
    await reader.ReadAsync();
    Console.WriteLine($"POST_TOTAL={reader.GetInt64(0)}");
    Console.WriteLine($"POST_NULL={reader.GetInt64(1)}");
    Console.WriteLine($"POST_NON_NULL={reader.GetInt64(2)}");
    Console.WriteLine($"POST_DISTINCT={reader.GetInt64(3)}");
    Console.WriteLine($"POST_INVALID_LENGTH={reader.GetInt64(4)}");
    Console.WriteLine($"POST_INVALID_BASE64URL={reader.GetInt64(5)}");
}
