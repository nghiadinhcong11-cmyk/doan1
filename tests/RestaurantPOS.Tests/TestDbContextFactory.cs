using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using RestaurantPOS.Infrastructure.Persistence;

namespace RestaurantPOS.Tests;

internal static class TestDbContextFactory
{
    public static (ApplicationDbContext Context, SqliteConnection Connection) Create()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;
        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();
        return (context, connection);
    }

    public static ApplicationDbContext CreateContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;
        return new ApplicationDbContext(options);
    }
}
