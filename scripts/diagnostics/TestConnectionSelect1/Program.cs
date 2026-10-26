using Microsoft.Extensions.Configuration;
using Npgsql;
using RestaurantPOS.Infrastructure.Configuration;

const string environmentName = "Development";

var configuration = new ConfigurationBuilder()
    .AddUserSecrets<Program>(optional: true)
    .AddEnvironmentVariables()
    .Build();

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
if (!string.Equals(environment, environmentName, StringComparison.Ordinal))
    return Fail("ENVIRONMENT CHECK: FAIL");

var targetMode = configuration["Database:TargetMode"]?.Trim();
var expectedProjectRef = configuration["Database:ExpectedProjectRef"]?.Trim();
var allowAutomaticMigration = configuration["DatabaseSafety:AllowAutomaticMigration"];

if (!string.Equals(targetMode, SupabaseProjectTargetGuard.TestMode, StringComparison.OrdinalIgnoreCase))
    return Fail("TARGET MODE CHECK: FAIL");

if (!string.Equals(expectedProjectRef, SupabaseProjectTargetGuard.TestProjectRef, StringComparison.Ordinal))
    return Fail("PROJECT REFERENCE CHECK: FAIL");

if (!bool.TryParse(allowAutomaticMigration, out var automaticMigration) || automaticMigration)
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

// Keep this diagnostic within the approved Development pool budget.
connection.MaxPoolSize = PostgresConnectionPoolPolicy.DefaultMaxPoolSize;
connection.MinPoolSize = PostgresConnectionPoolPolicy.DefaultMinPoolSize;
connection.Timeout = 15;
connection.CommandTimeout = 15;

Console.WriteLine("TARGET CHECK: PASS");
Console.WriteLine($"TARGET PROJECT REF: {SupabaseProjectTargetGuard.TestProjectRef}");
Console.WriteLine($"TARGET: {connection.Host}:{connection.Port}/{connection.Database}");
Console.WriteLine("SSL: REQUIRED");
Console.WriteLine("AUTOMATIC MIGRATION/SEED: DISABLED");
Console.WriteLine("DATABASE MUTATION: NONE");

await using var db = new NpgsqlConnection(connection.ConnectionString);
using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));

try
{
    await db.OpenAsync(timeout.Token);
    await using var command = new NpgsqlCommand("SELECT 1", db)
    {
        CommandTimeout = 15
    };
    var result = await command.ExecuteScalarAsync(timeout.Token);
    if (result is not int value || value != 1)
        return Fail("SELECT 1: FAIL");

    Console.WriteLine("CONNECTION OPEN: PASS");
    Console.WriteLine("SELECT 1: PASS");
    Console.WriteLine("CONNECTION CLOSED: PASS");
    return 0;
}
catch (OperationCanceledException)
{
    return Fail("CONNECTION/SELECT: TIMEOUT");
}
catch (PostgresException ex)
{
    return Fail($"POSTGRES CHECK: FAIL ({ex.SqlState})");
}
catch (NpgsqlException ex)
{
    return Fail($"NPGSQL CHECK: FAIL ({ex.GetType().Name})");
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

    var builder = new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.Port > 0 ? uri.Port : 5432,
        Database = uri.AbsolutePath.Trim('/'),
        Username = Uri.UnescapeDataString(userInfo[0]),
        Password = Uri.UnescapeDataString(userInfo[1]),
        SslMode = SslMode.Require
    };
    return builder;
}
