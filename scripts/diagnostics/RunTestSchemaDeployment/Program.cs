using System.Diagnostics;
using System.Net.Sockets;
using Microsoft.Extensions.Configuration;
using Npgsql;
using RestaurantPOS.Infrastructure.Configuration;

const string latestMigration = "20261005072156_RemoveLegacyTableAreaName";
const string repoRootMarker = "dotnet-tools.json";

var mode = args.Length == 0 ? "--dry-run" : args[0];
if (mode is not ("--dry-run" or "--check-only" or "--inspect-only" or "--apply"))
    return Fail("MODE CHECK: use --dry-run, --check-only, --inspect-only or --apply");

var root = Directory.GetCurrentDirectory();
if (!File.Exists(Path.Combine(root, repoRootMarker)) ||
    !File.Exists(Path.Combine(root, "services", "api", "RestaurantPOS.api.csproj")))
    return Fail("REPOSITORY CHECK: FAIL");

var configuration = new ConfigurationBuilder()
    .AddUserSecrets<Program>(optional: true)
    .AddEnvironmentVariables()
    .Build();

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
if (!string.Equals(environment, "Development", StringComparison.Ordinal))
    return Fail("ENVIRONMENT CHECK: FAIL");

if (!string.Equals(configuration["Database:TargetMode"], SupabaseProjectTargetGuard.TestMode, StringComparison.OrdinalIgnoreCase) ||
    !string.Equals(configuration["Database:ExpectedProjectRef"], SupabaseProjectTargetGuard.TestProjectRef, StringComparison.Ordinal))
    return Fail("TEST TARGET CONFIGURATION: FAIL");

if (!bool.TryParse(configuration["DatabaseSafety:AllowAutomaticMigration"], out var allowAutomaticMigration) || allowAutomaticMigration)
    return Fail("AUTOMATIC MIGRATION CHECK: FAIL");

var rawConnectionString = configuration.GetConnectionString("DefaultConnection")
    ?? configuration["SUPABASE_CONNECTION_STRING"];
if (string.IsNullOrWhiteSpace(rawConnectionString))
    return Fail("CREDENTIAL CONFIGURATION CHECK: FAIL");

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

if (connection.SslMode == SslMode.Disable)
    return Fail("SSL CHECK: FAIL");

connection.MaxPoolSize = 5;
connection.MinPoolSize = 0;
connection.Timeout = 15;
connection.CommandTimeout = 15;

Console.WriteLine("TEST TARGET CHECK: PASS");
Console.WriteLine("PROJECT REF: eorboopkwgmvainlxzkz");
Console.WriteLine($"TARGET: {connection.Host}:{connection.Port}/{connection.Database}");
Console.WriteLine("MAIN TARGET: REJECTED");
Console.WriteLine("AUTOMATIC MIGRATION/SEED: DISABLED");
Console.WriteLine($"MODE: {mode}");

if (mode == "--dry-run")
{
    Console.WriteLine("DATABASE EMPTY CHECK: NOT_RUN (dry-run)");
    Console.WriteLine("MIGRATION EXECUTION: NOT_RUN");
    Console.WriteLine("SAFE RESULT: READY_FOR_SEPARATE_APPROVAL");
    return 0;
}

// The application composition root requires this secret for EF design-time
// service-provider creation. Fail before opening TEST when it is unavailable.
var jwtSecret = configuration["Jwt:Secret"];
if (mode == "--apply" && string.IsNullOrWhiteSpace(jwtSecret))
    return Fail("DESIGN_TIME_CONFIGURATION: JWT_SECRET_MISSING");

await using var db = new NpgsqlConnection(connection.ConnectionString);
using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
try
{
    await db.OpenAsync(timeout.Token);

    if (mode == "--inspect-only")
    {
        await InspectDatabaseAsync(db, timeout.Token);
        Console.WriteLine("DATABASE MUTATION: NONE");
        Console.WriteLine("SAFE RESULT: READ_ONLY_INSPECTION_PASS");
        return 0;
    }

    var publicTableCount = await ScalarAsync<long>(db, """
        SELECT COUNT(*)
        FROM information_schema.tables
        WHERE table_schema = 'public' AND table_type = 'BASE TABLE';
        """, timeout.Token);

    if (publicTableCount != 0)
        return Fail("DATABASE EMPTY CHECK: FAIL (public tables already exist)");

    var migrationHistoryExists = await ScalarAsync<bool>(db, """
        SELECT EXISTS (
            SELECT 1 FROM information_schema.tables
            WHERE table_schema = 'public' AND table_name = '__EFMigrationsHistory'
        );
        """, timeout.Token);

    if (migrationHistoryExists)
        return Fail("DATABASE EMPTY CHECK: FAIL (migration history already exists)");

    Console.WriteLine("DATABASE EMPTY CHECK: PASS");
}
catch (OperationCanceledException)
{
    return Fail("DATABASE EMPTY CHECK: TIMEOUT");
}
catch (PostgresException ex)
{
    return Fail($"DATABASE EMPTY CHECK: FAIL ({ex.SqlState})");
}
catch (NpgsqlException ex)
{
    return Fail($"DATABASE EMPTY CHECK: FAIL ({ex.GetType().Name})");
}
catch (SocketException)
{
    return Fail("DATABASE INSPECTION: NETWORK_FAILURE");
}
catch (Exception ex)
{
    return Fail($"DATABASE INSPECTION: FAIL ({ex.GetType().Name})");
}

if (mode == "--check-only")
{
    Console.WriteLine("MIGRATION EXECUTION: NOT_RUN");
    Console.WriteLine("SAFE RESULT: READ_ONLY_PRECHECK_PASS");
    return 0;
}

var ef = new ProcessStartInfo("dotnet")
{
    WorkingDirectory = root,
    RedirectStandardOutput = true,
    RedirectStandardError = true,
    UseShellExecute = false,
    CreateNoWindow = true
};

foreach (var argument in new[]
{
    "tool", "run", "dotnet-ef", "database", "update", latestMigration,
    "--project", "services/api/RestaurantPOS.api.csproj",
    "--startup-project", "services/api/RestaurantPOS.api.csproj",
    "--context", "ApplicationDbContext", "--configuration", "Release", "--no-build"
})
    ef.ArgumentList.Add(argument);

ef.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";
ef.Environment["Database__TargetMode"] = "Test";
ef.Environment["Database__ExpectedProjectRef"] = SupabaseProjectTargetGuard.TestProjectRef;
ef.Environment["DatabaseSafety__AllowAutomaticMigration"] = "false";
ef.Environment["ConnectionStrings__DefaultConnection"] = connection.ConnectionString;
ef.Environment["Jwt__Secret"] = jwtSecret;

using var process = Process.Start(ef);
if (process is null)
    return Fail("MIGRATION PROCESS: START FAILED");

var stdout = process.StandardOutput.ReadToEndAsync();
var stderr = process.StandardError.ReadToEndAsync();
using var processTimeout = new CancellationTokenSource(TimeSpan.FromMinutes(10));
try
{
    await process.WaitForExitAsync(processTimeout.Token);
}
catch (OperationCanceledException)
{
    try { process.Kill(entireProcessTree: true); } catch { }
    return Fail("MIGRATION PROCESS: TIMEOUT");
}

var standardOutput = await stdout;
var errorOutput = await stderr;
Console.WriteLine($"MIGRATION PROCESS EXIT CODE: {process.ExitCode}");
if (process.ExitCode == 0)
    return 0;

Console.WriteLine("MIGRATION PROCESS: FAIL");
Console.WriteLine($"EF_FAILURE_STAGE: {ClassifyEfFailureStage(standardOutput, errorOutput)}");
Console.WriteLine($"EF_FAILURE_SIGNATURE: {ClassifyEfFailureSignature(standardOutput, errorOutput)}");
Console.WriteLine("EF_OUTPUT: SANITIZED_SUMMARY_ONLY");
Console.WriteLine("SAFE RESULT: BLOCKED");
return 2;

static async Task InspectDatabaseAsync(NpgsqlConnection connection, CancellationToken cancellationToken)
{
    var historyExists = await ScalarAsync<bool>(connection, """
        SELECT EXISTS (
            SELECT 1 FROM information_schema.tables
            WHERE table_schema = 'public' AND table_name = '__EFMigrationsHistory'
        );
        """, cancellationToken);
    Console.WriteLine($"MIGRATION HISTORY TABLE: {(historyExists ? "PRESENT" : "ABSENT")}");

    if (historyExists)
    {
        var count = await ScalarAsync<long>(connection, """
            SELECT COUNT(*) FROM public."__EFMigrationsHistory";
            """, cancellationToken);
        var latest = await ScalarAsync<string?>(connection, """
            SELECT "MigrationId"
            FROM public."__EFMigrationsHistory"
            ORDER BY "MigrationId" DESC
            LIMIT 1;
            """, cancellationToken);
        Console.WriteLine($"MIGRATION HISTORY COUNT: {count}");
        Console.WriteLine($"LATEST APPLIED MIGRATION: {latest ?? "NONE"}");
    }
    else
    {
        Console.WriteLine("MIGRATION HISTORY COUNT: 0");
        Console.WriteLine("LATEST APPLIED MIGRATION: NONE");
    }

    var publicTableCount = await ScalarAsync<long>(connection, """
        SELECT COUNT(*)
        FROM information_schema.tables
        WHERE table_schema = 'public' AND table_type = 'BASE TABLE';
        """, cancellationToken);
    Console.WriteLine($"PUBLIC BASE TABLE COUNT: {publicTableCount}");
    Console.WriteLine($"PUBLIC TABLES: {(publicTableCount == 0 ? "NONE" : "PRESENT_NAMES_REDACTED_FROM_LOG")}");
}

static string ClassifyEfFailureStage(string stdout, string stderr)
{
    var text = $"{stdout}\n{stderr}".ToLowerInvariant();
    if (text.Contains("finding dbcontext") || text.Contains("application service provider") ||
        text.Contains("design-time") || text.Contains("startup assembly"))
        return "DESIGN_TIME_STARTUP_OR_DBCONTEXT";
    if (text.Contains("npgsql") || text.Contains("postgres") || text.Contains("connection") ||
        text.Contains("timeout"))
        return "DATABASE_CONNECTION_OR_SQL";
    return "UNKNOWN_BEFORE_SQL_EXECUTION";
}

static string ClassifyEfFailureSignature(string stdout, string stderr)
{
    var text = $"{stdout}\n{stderr}".ToLowerInvariant();
    if (text.Contains("jwt:secret") || text.Contains("jwt__secret"))
        return "REQUIRED_RUNTIME_SECRET_MISSING";
    if (text.Contains("password") || text.Contains("connection string") || text.Contains("host="))
        return "SENSITIVE_CONFIGURATION_REDACTED";
    if (text.Contains("npgsql"))
        return "NPGSQL_ERROR";
    if (text.Contains("postgres"))
        return "POSTGRES_ERROR";
    if (text.Contains("dbcontext"))
        return "DBCONTEXT_CREATION_ERROR";
    return "UNCLASSIFIED_ERROR";
}

static async Task<T> ScalarAsync<T>(NpgsqlConnection connection, string sql, CancellationToken cancellationToken)
{
    await using var command = new NpgsqlCommand(sql, connection) { CommandTimeout = 15 };
    var value = await command.ExecuteScalarAsync(cancellationToken);
    return (T)Convert.ChangeType(value!, typeof(T));
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
