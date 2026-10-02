using Microsoft.Extensions.Configuration;
using Npgsql;

const string authorizedDevProjectRef = "qfkgjxwbshjgsxsvkpkp";

var configuration = new ConfigurationBuilder()
    .AddUserSecrets<Program>(optional: true)
    .AddEnvironmentVariables()
    .Build();

var configuredProjectRef = Environment.GetEnvironmentVariable("DEV_SUPABASE_PROJECT_REF");
if (!string.Equals(configuredProjectRef, authorizedDevProjectRef, StringComparison.Ordinal))
{
    Console.WriteLine("TARGET CHECK: FAIL");
    Console.WriteLine("DEV_SUPABASE_PROJECT_REF does not match the authorized DEV project reference.");
    return 2;
}

var rawConnectionString = configuration.GetConnectionString("DefaultConnection")
    ?? configuration["SUPABASE_CONNECTION_STRING"];

if (string.IsNullOrWhiteSpace(rawConnectionString))
{
    Console.WriteLine("TARGET CHECK: FAIL");
    Console.WriteLine("ConnectionStrings:DefaultConnection is missing.");
    return 2;
}

NpgsqlConnectionStringBuilder connectionString;
try
{
    connectionString = NormalizeConnectionString(rawConnectionString);
}
catch (Exception ex) when (ex is ArgumentException or UriFormatException)
{
    Console.WriteLine("TARGET CHECK: FAIL");
    Console.WriteLine($"Connection configuration is invalid: {ex.Message}");
    return 2;
}

var configuredTargetIdentity = $"{connectionString.Host}|{connectionString.Username}";
if (!configuredTargetIdentity.Contains(authorizedDevProjectRef, StringComparison.OrdinalIgnoreCase))
{
    Console.WriteLine("TARGET CHECK: FAIL");
    Console.WriteLine("DefaultConnection host/username does not identify the authorized DEV project.");
    return 2;
}

if (connectionString.SslMode == SslMode.Disable)
{
    Console.WriteLine("TARGET CHECK: FAIL");
    Console.WriteLine("SSL Mode=Disable is not permitted for this diagnostic.");
    return 2;
}

Console.WriteLine("TARGET CHECK: PASS");
Console.WriteLine($"TARGET PROJECT REF: {authorizedDevProjectRef}");
Console.WriteLine($"CONNECTION TARGET: {connectionString.Host}:{connectionString.Port}/{connectionString.Database}");
Console.WriteLine("CONNECTION OPEN: START");

await using var connection = new NpgsqlConnection(connectionString.ConnectionString);
try
{
    await connection.OpenAsync();
    Console.WriteLine("CONNECTION OPEN: PASS");

    var selectOne = await ExecuteScalarAsync<int>(connection, "SELECT 1");
    Console.WriteLine($"SELECT 1: {(selectOne == 1 ? "PASS" : "FAIL")}");

    await ReportMigrationHistoryAsync(connection);
    await ReportSchemaAsync(connection);
    await ReportSeedAsync(connection);

    Console.WriteLine("DATABASE MUTATION: NONE");
    Console.WriteLine("MIGRATIONS/SEED: NOT EXECUTED BY DIAGNOSTIC");
    return 0;
}
catch (PostgresException ex)
{
    Console.WriteLine("POST-MIGRATION VALIDATION: FAIL");
    Console.WriteLine($"POSTGRES SQLSTATE: {ex.SqlState}");
    Console.WriteLine("SANITIZED ERROR: PostgreSQL read-only validation query failed.");
    return 1;
}
catch (NpgsqlException ex)
{
    Console.WriteLine("POST-MIGRATION VALIDATION: FAIL");
    Console.WriteLine("NPGSQL ERROR: " + ex.GetType().Name);
    return 1;
}

static async Task ReportMigrationHistoryAsync(NpgsqlConnection connection)
{
    Console.WriteLine("--- MIGRATION HISTORY ---");
    var historyCount = await ExecuteScalarAsync<long>(connection, "SELECT COUNT(*) FROM public.\"__EFMigrationsHistory\"");
    var latestMigration = await ExecuteScalarAsync<string?>(connection, "SELECT \"MigrationId\" FROM public.\"__EFMigrationsHistory\" ORDER BY \"MigrationId\" DESC LIMIT 1");
    Console.WriteLine($"MIGRATION HISTORY COUNT: {historyCount}");
    Console.WriteLine($"LATEST MIGRATION: {latestMigration ?? "NONE"}");

    foreach (var migration in new[]
    {
        "20260910052615_AddAnalyticsIndexes",
        "20260910063419_AddBusinessInsight",
        "20260919075618_RemovePayrollAndRepairReceiptSettings"
    })
    {
        var present = await ExecuteScalarAsync<bool>(connection, """
            SELECT EXISTS (
                SELECT 1 FROM public."__EFMigrationsHistory" WHERE "MigrationId" = $1
            )
            """, migration);
        Console.WriteLine($"MIGRATION {migration}: {(present ? "PRESENT" : "MISSING")}");
    }
}

static async Task ReportSchemaAsync(NpgsqlConnection connection)
{
    Console.WriteLine("--- SCHEMA ---");
    foreach (var table in new[]
    {
        "BusinessInsights", "ReceiptSettings", "Payrolls", "PayrollSettings",
        "PayrollAdjustments", "EmployeeSalaryProfiles", "Employees", "Orders",
        "OrderItems", "OrderDetails", "OrderRequests", "OrderRequestItems",
        "Customers", "Branches", "Areas", "Tables", "Products", "Notifications"
    })
    {
        var exists = await TableExistsAsync(connection, table);
        var payroll = table is "Payrolls" or "PayrollSettings" or "PayrollAdjustments" or "EmployeeSalaryProfiles";
        var result = payroll ? (exists ? "PRESENT (UNEXPECTED)" : "ABSENT") : (exists ? "EXISTS" : "MISSING");
        Console.WriteLine($"TABLE {table}: {result}");
    }

    var receiptTableCount = await ExecuteScalarAsync<long>(connection, """
        SELECT COUNT(*) FROM information_schema.tables
        WHERE table_schema = 'public' AND table_name = 'ReceiptSettings'
        """);
    Console.WriteLine($"RECEIPT SETTINGS TABLE COUNT: {receiptTableCount} (expected 1)");

    foreach (var column in new[] { "BasicSalary", "EmployeeType" })
    {
        var exists = await ColumnExistsAsync(connection, "Employees", column);
        Console.WriteLine($"Employees.{column}: {(exists ? "EXISTS" : "MISSING")}");
    }

    foreach (var column in new[] { "PaymentAt", "PaymentMethod", "PaidAmount", "TotalAmount" })
    {
        var exists = await ColumnExistsAsync(connection, "Orders", column);
        Console.WriteLine($"Orders.{column} (payment schema): {(exists ? "EXISTS" : "MISSING")}");
    }

    Console.WriteLine("KITCHEN SCHEMA: OrderDetails, OrderRequests, OrderRequestItems, Notifications checked above");
    Console.WriteLine("PAYMENT SCHEMA: Orders payment columns checked above; current source has no Payments DbSet/table expectation");
}

static async Task ReportSeedAsync(NpgsqlConnection connection)
{
    Console.WriteLine("--- SEED ---");
    var checks = new (string Label, string Sql)[]
    {
        ("Branch", "SELECT COUNT(*) FROM public.\"Branches\""),
        ("Area", "SELECT COUNT(*) FROM public.\"Areas\""),
        ("Table", "SELECT COUNT(*) FROM public.\"Tables\""),
        ("Product", "SELECT COUNT(*) FROM public.\"Products\""),
        ("Development admin", "SELECT COUNT(*) FROM public.\"Employees\" WHERE \"Role\" = 'admin' AND \"Username\" = 'admin'")
    };

    var allPresent = true;
    foreach (var check in checks)
    {
        var count = await ExecuteScalarAsync<long>(connection, check.Sql);
        var present = count > 0;
        allPresent &= present;
        Console.WriteLine($"SEED {check.Label}: {(present ? "PRESENT" : "MISSING")}");
    }

    Console.WriteLine($"SEED: {(allPresent ? "PASS" : "PARTIAL/FAIL")}");
}

static async Task<bool> TableExistsAsync(NpgsqlConnection connection, string tableName)
{
    return await ExecuteScalarAsync<bool>(connection, """
        SELECT EXISTS (
            SELECT 1 FROM information_schema.tables
            WHERE table_schema = 'public' AND table_name = $1
        )
        """, tableName);
}

static async Task<bool> ColumnExistsAsync(NpgsqlConnection connection, string tableName, string columnName)
{
    return await ExecuteScalarAsync<bool>(connection, """
        SELECT EXISTS (
            SELECT 1 FROM information_schema.columns
            WHERE table_schema = 'public' AND table_name = $1 AND column_name = $2
        )
        """, tableName, columnName);
}

static async Task<T> ExecuteScalarAsync<T>(NpgsqlConnection connection, string sql, params object?[] parameters)
{
    await using var command = new NpgsqlCommand(sql, connection);
    for (var index = 0; index < parameters.Length; index++)
        command.Parameters.AddWithValue(parameters[index] ?? DBNull.Value);

    var value = await command.ExecuteScalarAsync();
    if (value is null || value is DBNull)
        return default!;
    return (T)Convert.ChangeType(value, typeof(T));
}

static NpgsqlConnectionStringBuilder NormalizeConnectionString(string raw)
{
    var value = raw.Trim();
    if (value.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase) ||
        value.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase))
    {
        var uri = new Uri(value);
        var userInfo = uri.UserInfo.Split(':', 2);
        if (userInfo.Length != 2 || string.IsNullOrWhiteSpace(uri.Host))
            throw new UriFormatException("The PostgreSQL connection URI is invalid.");

        return new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.IsDefaultPort ? 5432 : uri.Port,
            Database = uri.AbsolutePath.Trim('/'),
            Username = Uri.UnescapeDataString(userInfo[0]),
            Password = Uri.UnescapeDataString(userInfo[1]),
            SslMode = SslMode.Require
        };
    }

    return new NpgsqlConnectionStringBuilder(value);
}
