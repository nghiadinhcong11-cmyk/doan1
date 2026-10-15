using Microsoft.Extensions.Configuration;
using Npgsql;

namespace RestaurantPOS.Infrastructure.Configuration;

/// <summary>
/// Applies a conservative client-side pool budget for the Supabase Session Pooler.
/// The default assumes one Backend instance and leaves room for other approved
/// Development consumers under the observed 15-session pool limit.
/// </summary>
public static class PostgresConnectionPoolPolicy
{
    public const int DefaultMaxPoolSize = 5;
    public const int DefaultMinPoolSize = 0;
    public const int ObservedSessionPoolLimit = 15;

    public static string Apply(string connectionString, IConfiguration configuration)
    {
        var maxPoolSize = configuration.GetValue(
            "Database:MaxPoolSize",
            DefaultMaxPoolSize);
        var minPoolSize = configuration.GetValue(
            "Database:MinPoolSize",
            DefaultMinPoolSize);

        if (maxPoolSize < 1 || maxPoolSize > ObservedSessionPoolLimit)
            throw new InvalidOperationException(
                $"Database:MaxPoolSize must be between 1 and {ObservedSessionPoolLimit}.");

        if (minPoolSize < 0 || minPoolSize > maxPoolSize)
            throw new InvalidOperationException(
                "Database:MinPoolSize must be between 0 and Database:MaxPoolSize.");

        var builder = new NpgsqlConnectionStringBuilder(connectionString);

        // Transaction-pooler connections are deliberately handled elsewhere and
        // may have pooling disabled during migration-related diagnostics.
        if (!builder.Pooling)
            return builder.ConnectionString;

        builder.MaxPoolSize = maxPoolSize;
        builder.MinPoolSize = minPoolSize;
        return builder.ConnectionString;
    }
}
