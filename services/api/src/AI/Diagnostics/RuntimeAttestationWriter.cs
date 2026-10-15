using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using RestaurantPOS.AI.Security;
using RestaurantPOS.Application.Common.Security;

namespace RestaurantPOS.AI.Diagnostics;

public static class RuntimeAttestationWriter
{
    public const string SchemaVersion = "runtime-attestation-v1";

    public static bool TryWriteTestAdminE2E(
        IConfiguration configuration,
        IHostEnvironment environment,
        string? outputPath,
        string? correlationId,
        string expectedTestProjectRef,
        out string failureCode)
    {
        failureCode = "NONE";
        if (!environment.IsDevelopment()) return Fail("DEVELOPMENT_REQUIRED", out failureCode);
        if (string.IsNullOrWhiteSpace(outputPath) || string.IsNullOrWhiteSpace(correlationId)) return Fail("ATTESTATION_INPUT_MISSING", out failureCode);
        if (!Guid.TryParse(correlationId, out _)) return Fail("CORRELATION_ID_INVALID", out failureCode);
        if (!IsSafeAttestationPath(outputPath, out var normalizedPath)) return Fail("ATTESTATION_PATH_INVALID", out failureCode);

        var targetMode = configuration["Database:TargetMode"];
        var expectedProjectRef = configuration["Database:ExpectedProjectRef"];
        var configuredProjectRef = configuration["DEV_SUPABASE_PROJECT_REF"];
        var testTargetApproved = string.Equals(targetMode, "Test", StringComparison.OrdinalIgnoreCase)
            && string.Equals(expectedProjectRef, expectedTestProjectRef, StringComparison.Ordinal)
            && string.Equals(configuredProjectRef, expectedTestProjectRef, StringComparison.Ordinal);
        var providerMock = string.Equals(configuration["AI:ProviderMode"], "Mock", StringComparison.OrdinalIgnoreCase);
        var migrationsDisabled = bool.TryParse(configuration["DatabaseSafety:AllowAutomaticMigration"], out var migrationEnabled) && !migrationEnabled;
        var insightsDisabled = bool.TryParse(configuration["BackgroundJobs:EnableInsights"], out var insightsEnabled) && !insightsEnabled;
        var allGuardsVerified = testTargetApproved && providerMock && migrationsDisabled && insightsDisabled;

        var process = Process.GetCurrentProcess();
        var generatedAt = DateTimeOffset.UtcNow;
        var evidence = new
        {
            schemaVersion = SchemaVersion,
            status = allGuardsVerified ? "READY" : "BLOCKED",
            correlationId,
            processId = process.Id,
            processStartTimeUtc = process.StartTime.ToUniversalTime().ToString("O"),
            generatedAtUtc = generatedAt.ToString("O"),
            validUntilUtc = generatedAt.AddMinutes(15).ToString("O"),
            databaseTargetMode = targetMode,
            databaseProjectRef = expectedProjectRef,
            safety = new
            {
                environmentDevelopment = true,
                approvedDevelopmentTarget = testTargetApproved,
                mockProvider = providerMock,
                aiReadOnlyEnforced = false,
                adminE2EReadOnly = true,
                automaticMigrationsDisabled = migrationsDisabled,
                backgroundInsightsDisabled = insightsDisabled,
                externalAiDisabled = providerMock,
                runtimeMode = "AdminE2E",
                managerAllowlist = "NOT_APPLICABLE",
                customerAllowlist = "NOT_APPLICABLE"
            }
        };

        return TryPersist(normalizedPath, evidence, allGuardsVerified ? "NONE" : "SAFETY_GUARDS_NOT_VERIFIED", out failureCode);
    }

    public static bool TryWrite(
        IConfiguration configuration,
        IHostEnvironment environment,
        AiLiveReadOnlyPolicy policy,
        string? outputPath,
        string? correlationId,
        string? allowlistKey,
        string? expectedAllowlistHmac,
        out string failureCode)
    {
        failureCode = "NONE";
        if (!environment.IsDevelopment()) return Fail("DEVELOPMENT_REQUIRED", out failureCode);
        if (string.IsNullOrWhiteSpace(outputPath) || string.IsNullOrWhiteSpace(correlationId)) return Fail("ATTESTATION_INPUT_MISSING", out failureCode);
        if (!Guid.TryParse(correlationId, out _)) return Fail("CORRELATION_ID_INVALID", out failureCode);
        if (!IsSafeAttestationPath(outputPath, out var normalizedPath)) return Fail("ATTESTATION_PATH_INVALID", out failureCode);

        var providerMock = string.Equals(configuration["AI:ProviderMode"], "Mock", StringComparison.OrdinalIgnoreCase);
        var targetApproved = string.Equals(configuration["DEV_SUPABASE_PROJECT_REF"], QrTokenBackfillTargetGuard.AuthorizedDevProjectRef, StringComparison.Ordinal);
        var migrationsDisabled = bool.TryParse(configuration["DatabaseSafety:AllowAutomaticMigration"], out var migrationEnabled) && !migrationEnabled;
        var insightsDisabled = bool.TryParse(configuration["BackgroundJobs:EnableInsights"], out var insightsEnabled) && !insightsEnabled;
        var readOnlyEnforced = policy.IsConfigurationValid;
        var identities = policy.GetAllowlistIdentitiesForAttestation();
        var actualAllowlistHmac = AiLiveReadOnlyPolicy.ComputeAllowlistHmac(identities, allowlistKey);
        var customerMode = string.Equals(policy.RuntimeMode, "Customer", StringComparison.Ordinal);
        var allowlistShapeValid = customerMode
            ? identities.Count == 1 && string.Equals(identities[0].Role, RoleNames.Customer, StringComparison.OrdinalIgnoreCase)
            : identities.Count == 3 && identities.Count(identity => string.Equals(identity.Role, RoleNames.Manager, StringComparison.OrdinalIgnoreCase)) == 2;
        var allowlistVerified = allowlistShapeValid
            && !string.IsNullOrWhiteSpace(actualAllowlistHmac)
            && CryptographicOperations.FixedTimeEquals(
                Convert.FromHexString(actualAllowlistHmac),
                TryDecodeHex(expectedAllowlistHmac));
        var allGuardsVerified = targetApproved && providerMock && readOnlyEnforced && migrationsDisabled && insightsDisabled && allowlistVerified;
        var process = Process.GetCurrentProcess();
        var generatedAt = DateTimeOffset.UtcNow;
        var evidence = new
        {
            schemaVersion = SchemaVersion,
            status = allGuardsVerified ? "READY" : "BLOCKED",
            correlationId,
            processId = process.Id,
            processStartTimeUtc = process.StartTime.ToUniversalTime().ToString("O"),
            generatedAtUtc = generatedAt.ToString("O"),
            validUntilUtc = generatedAt.AddMinutes(15).ToString("O"),
            safety = new
            {
                environmentDevelopment = true,
                approvedDevelopmentTarget = targetApproved,
                mockProvider = providerMock,
                aiReadOnlyEnforced = readOnlyEnforced,
                automaticMigrationsDisabled = migrationsDisabled,
                backgroundInsightsDisabled = insightsDisabled,
                externalAiDisabled = providerMock && readOnlyEnforced,
                runtimeMode = policy.RuntimeMode,
                managerAllowlist = customerMode ? "NOT_APPLICABLE" : allowlistVerified ? "VERIFIED" : "NOT_VERIFIED",
                customerAllowlist = customerMode ? (allowlistVerified ? "VERIFIED" : "NOT_VERIFIED") : "NOT_APPLICABLE"
            }
        };

        try
        {
            var directory = Path.GetDirectoryName(normalizedPath);
            if (string.IsNullOrWhiteSpace(directory)) return Fail("ATTESTATION_PATH_INVALID", out failureCode);
            if (!TryPersist(normalizedPath, evidence, allGuardsVerified ? "NONE" : "SAFETY_GUARDS_NOT_VERIFIED", out failureCode))
                return false;
            if (!allGuardsVerified) failureCode = "SAFETY_GUARDS_NOT_VERIFIED";
            return allGuardsVerified;
        }
        catch
        {
            return Fail("ATTESTATION_WRITE_FAILED", out failureCode);
        }
    }

    private static bool TryPersist(string normalizedPath, object evidence, string failureCodeOnBlocked, out string failureCode)
    {
        failureCode = "NONE";
        try
        {
            var directory = Path.GetDirectoryName(normalizedPath);
            if (string.IsNullOrWhiteSpace(directory)) return Fail("ATTESTATION_PATH_INVALID", out failureCode);
            Directory.CreateDirectory(directory);
            var temp = normalizedPath + ".tmp-" + Guid.NewGuid().ToString("N");
            try
            {
                using (var stream = new FileStream(temp, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, FileOptions.WriteThrough))
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write(JsonSerializer.Serialize(evidence, new JsonSerializerOptions { WriteIndented = true }));
                    writer.Flush();
                    stream.Flush(true);
                }
                File.Move(temp, normalizedPath, overwrite: false);
            }
            finally
            {
                if (File.Exists(temp)) File.Delete(temp);
            }

            if (failureCodeOnBlocked != "NONE") failureCode = failureCodeOnBlocked;
            return failureCodeOnBlocked == "NONE";
        }
        catch
        {
            return Fail("ATTESTATION_WRITE_FAILED", out failureCode);
        }
    }

    private static byte[] TryDecodeHex(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length != 64) return new byte[32];
        try { return Convert.FromHexString(value); } catch { return new byte[32]; }
    }

    private static bool IsSafeAttestationPath(string path, out string normalizedPath)
    {
        normalizedPath = string.Empty;
        try
        {
            normalizedPath = Path.GetFullPath(path);
            var fileName = Path.GetFileName(normalizedPath);
            var directory = new DirectoryInfo(Path.GetDirectoryName(normalizedPath)!);
            if (!fileName.StartsWith("runtime-attestation-", StringComparison.Ordinal)
                || !fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
                || !directory.Name.Equals("runtime-attestations", StringComparison.OrdinalIgnoreCase))
                return false;
            for (var current = directory; current != null; current = current.Parent)
            {
                if (current.Exists && current.Attributes.HasFlag(FileAttributes.ReparsePoint)) return false;
            }
            var existingFile = new FileInfo(normalizedPath);
            return !existingFile.Exists || !existingFile.Attributes.HasFlag(FileAttributes.ReparsePoint);
        }
        catch
        {
            normalizedPath = string.Empty;
            return false;
        }
    }

    private static bool Fail(string code, out string failureCode)
    {
        failureCode = code;
        return false;
    }
}
