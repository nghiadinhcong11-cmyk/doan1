using System.Data;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using Npgsql;
using RestaurantPOS.Infrastructure.Configuration;

namespace RestaurantPOS.WebAPI.Diagnostics;

/// <summary>
/// Development TEST evidence only. Never serializes credentials or a connection string.
/// </summary>
public static class DatabaseTargetAttestationWriter
{
    public static async Task<(string Status, string Path)> WriteAsync(
        IHostEnvironment environment,
        IConfiguration configuration,
        NpgsqlConnectionStringBuilder effectiveConnection)
    {
        // Recheck the exact connection that EF Core will receive after pool settings.
        SupabaseProjectTargetGuard.Validate(effectiveConnection, configuration, environment.IsDevelopment());

        var process = Process.GetCurrentProcess();
        var executablePath = Environment.ProcessPath ?? string.Empty;
        var assemblyPath = typeof(DatabaseTargetAttestationWriter).Assembly.Location;
        var path = Path.Combine(
            AppContext.BaseDirectory,
            "runtime-attestations",
            $"runtime-attestation-database-{process.Id}-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.json");

        string connectionStatus;
        string? failureType = null;
        JsonElement? observedDatabase = null;
        try
        {
            await using var connection = new NpgsqlConnection(effectiveConnection.ConnectionString);
            await connection.OpenAsync();
            await using var transaction = await connection.BeginTransactionAsync(IsolationLevel.RepeatableRead);
            await using (var readOnly = new NpgsqlCommand("SET TRANSACTION READ ONLY", connection, transaction))
                await readOnly.ExecuteNonQueryAsync();

            const string snapshotSql = """
                SELECT jsonb_build_object(
                    'database', current_database(),
                    'schema', current_schema(),
                    'transactionReadOnly', current_setting('transaction_read_only'),
                    'history', (SELECT COALESCE(jsonb_agg("MigrationId" ORDER BY "MigrationId"), '[]'::jsonb)
                                FROM "__EFMigrationsHistory"),
                    'productBranchAvailabilitiesExists',
                        to_regclass('public."ProductBranchAvailabilities"') IS NOT NULL,
                    'products', (SELECT COUNT(*) FROM "Products"),
                    'branches', (SELECT COUNT(*) FROM "Branches"),
                    'expectedBackfillRows', (SELECT COUNT(*) FROM "Products") * (SELECT COUNT(*) FROM "Branches"),
                    'availabilityStatuses',
                        (SELECT COALESCE(jsonb_agg(jsonb_build_object('status', s.status, 'count', s.count)), '[]'::jsonb)
                         FROM (SELECT "AvailabilityStatus" AS status, COUNT(*) AS count
                               FROM "Products" GROUP BY "AvailabilityStatus") AS s),
                    'maxAvailabilityStatusLength',
                        (SELECT MAX(length("AvailabilityStatus")) FROM "Products"),
                    'productIdType',
                        (SELECT data_type FROM information_schema.columns
                         WHERE table_schema = current_schema() AND table_name = 'Products' AND column_name = 'Id'),
                    'branchIdType',
                        (SELECT data_type FROM information_schema.columns
                         WHERE table_schema = current_schema() AND table_name = 'Branches' AND column_name = 'Id')
                )::text
                """;
            await using var command = new NpgsqlCommand(snapshotSql, connection, transaction);
            command.CommandTimeout = 15;
            var json = (string?)await command.ExecuteScalarAsync();
            if (string.IsNullOrWhiteSpace(json)) throw new InvalidOperationException("Empty database observation.");
            using var document = JsonDocument.Parse(json);
            if (!string.Equals(
                    document.RootElement.GetProperty("database").GetString(),
                    effectiveConnection.Database,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    document.RootElement.GetProperty("transactionReadOnly").GetString(),
                    "on",
                    StringComparison.Ordinal))
                throw new InvalidOperationException("Database observation did not match the guarded read-only target.");
            observedDatabase = document.RootElement.Clone();
            connectionStatus = "READ_ONLY_VERIFIED";
            await transaction.RollbackAsync();
        }
        catch (Exception exception)
        {
            connectionStatus = "BLOCKED";
            failureType = exception.GetType().Name;
        }

        var evidence = new
        {
            schemaVersion = "database-target-attestation-v1",
            processId = process.Id,
            processStartTimeUtc = process.StartTime.ToUniversalTime().ToString("O"),
            generatedAtUtc = DateTimeOffset.UtcNow.ToString("O"),
            environment = environment.EnvironmentName,
            binary = new
            {
                executablePath,
                executableSha256 = HashFile(executablePath),
                assemblyPath,
                assemblySha256 = HashFile(assemblyPath)
            },
            configuredTarget = new
            {
                mode = configuration["Database:TargetMode"],
                host = effectiveConnection.Host,
                port = effectiveConnection.Port,
                database = effectiveConnection.Database,
                projectRef = SupabaseProjectTargetGuard.TestProjectRef,
                projectIdentityVerifiedByGuard = true,
                automaticMigrationDisabled = !configuration.GetValue<bool>("DatabaseSafety:AllowAutomaticMigration")
            },
            connectionStatus,
            failureType,
            observedDatabase
        };

        try
        {
            var directory = Path.GetDirectoryName(path)!;
            Directory.CreateDirectory(directory);
            var temporaryPath = path + ".tmp-" + Guid.NewGuid().ToString("N");
            try
            {
                await File.WriteAllTextAsync(temporaryPath, JsonSerializer.Serialize(evidence, new JsonSerializerOptions { WriteIndented = true }));
                File.Move(temporaryPath, path);
            }
            finally
            {
                if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
            }
            return (connectionStatus, path);
        }
        catch
        {
            // Diagnostics must not prevent the API from starting; do not expose a filesystem exception.
            return ("WRITE_FAILED", "NONE");
        }
    }

    private static string? HashFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return null;
        try
        {
            using var stream = File.OpenRead(path);
            return Convert.ToHexString(SHA256.HashData(stream));
        }
        catch
        {
            return null;
        }
    }
}
