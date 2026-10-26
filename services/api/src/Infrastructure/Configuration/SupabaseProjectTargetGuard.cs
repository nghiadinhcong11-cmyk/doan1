using Microsoft.Extensions.Configuration;
using Npgsql;

namespace RestaurantPOS.Infrastructure.Configuration;

/// <summary>
/// Validates an explicitly selected Development database target without opening a connection.
/// The project reference is checked from the tenant-qualified Supabase username because the
/// Session Pooler hostname alone does not identify the tenant.
/// </summary>
public static class SupabaseProjectTargetGuard
{
    public const string TestMode = "Test";
    public const string MainMode = "Main";
    public const string TestProjectRef = "lizpcvayxcuwjgrxsvfe";
    public const string TestPoolerHost = "aws-0-ap-northeast-1.pooler.supabase.com";
    public const int TestPoolerPort = 5432;
    public const string TestDatabase = "postgres";
    public const string TestUsername = "postgres.lizpcvayxcuwjgrxsvfe";

    public static void Validate(NpgsqlConnectionStringBuilder connection, IConfiguration configuration, bool isDevelopment)
    {
        var mode = configuration["Database:TargetMode"]?.Trim();
        if (string.IsNullOrWhiteSpace(mode))
            throw new InvalidOperationException("Database:TargetMode must be explicitly selected; refusing an implicit database target.");

        if (!isDevelopment)
            throw new InvalidOperationException("Database target mode is permitted only in Development.");

        if (configuration.GetValue<bool>("DatabaseSafety:AllowAutomaticMigration"))
            throw new InvalidOperationException("Explicit database target mode requires automatic migrations and seed to remain disabled.");

        if (string.Equals(mode, TestMode, StringComparison.OrdinalIgnoreCase))
        {
            ValidateExpectedProject(configuration, TestProjectRef);
            if (!string.Equals(connection.Host, TestPoolerHost, StringComparison.OrdinalIgnoreCase) ||
                connection.Port != TestPoolerPort ||
                !string.Equals(connection.Database, TestDatabase, StringComparison.Ordinal) ||
                !string.Equals(connection.Username, TestUsername, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Explicit TEST database target does not match the approved Development Supabase target.");
            }
        }
        else if (string.Equals(mode, MainMode, StringComparison.OrdinalIgnoreCase))
        {
            // The former development project is retained only as a negative-test
            // identity. It is not a permitted application or diagnostic target.
            throw new InvalidOperationException("The legacy MAIN database target is retired and cannot be selected.");
        }
        else
        {
            throw new InvalidOperationException("Database:TargetMode must be Test or Main when specified.");
        }

        if (connection.SslMode == SslMode.Disable)
            throw new InvalidOperationException("Database target requires SSL.");
    }

    private static void ValidateExpectedProject(IConfiguration configuration, string expectedProjectRef)
    {
        if (!string.Equals(configuration["Database:ExpectedProjectRef"], expectedProjectRef, StringComparison.Ordinal))
            throw new InvalidOperationException("Database target project reference does not match the selected target mode.");
    }

}
