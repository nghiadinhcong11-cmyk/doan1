using Microsoft.Extensions.Configuration;
using Npgsql;

namespace RestaurantPOS.Infrastructure.Configuration;

/// <summary>
/// Validates the explicitly selected Development database target without opening a connection.
/// Local PostgreSQL is the only supported application target in this repository.
/// </summary>
public static class DatabaseTargetGuard
{
    public const string LocalMode = "Local";

    public static void Validate(
        NpgsqlConnectionStringBuilder connection,
        IConfiguration configuration,
        bool isDevelopment)
    {
        var mode = configuration["Database:TargetMode"]?.Trim();
        if (!string.Equals(mode, LocalMode, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Database:TargetMode must be Local.");

        if (!isDevelopment)
            throw new InvalidOperationException("Local database target mode is permitted only in Development.");

        if (string.IsNullOrWhiteSpace(connection.Host))
            throw new InvalidOperationException("Local PostgreSQL host must be configured.");

        if (connection.Port is < 1 or > 65535)
            throw new InvalidOperationException("Local PostgreSQL port is invalid.");
    }
}
