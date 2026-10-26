using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using RestaurantPOS.Application.Common.Security;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Infrastructure.Persistence;

if (!string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), "Development", StringComparison.Ordinal))
    return Fail("ENVIRONMENT CHECK: run this utility only with ASPNETCORE_ENVIRONMENT=Development.");

var rawConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
    ?? Environment.GetEnvironmentVariable("SUPABASE_CONNECTION_STRING");
var password = Environment.GetEnvironmentVariable("RESTAURANTPOS_BOOTSTRAP_ADMIN_PASSWORD");

if (string.IsNullOrWhiteSpace(rawConnectionString))
    return Fail("CREDENTIAL CONFIGURATION CHECK: database connection environment variable is missing.");

var passwordError = PasswordPolicy.Validate(password);
if (passwordError is not null)
    return Fail($"PASSWORD CHECK: {passwordError}");

var connection = new NpgsqlConnectionStringBuilder(rawConnectionString)
{
    SslMode = SslMode.Require,
    MaxPoolSize = 2,
    MinPoolSize = 0,
    Timeout = 15,
    CommandTimeout = 30
};

var options = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseNpgsql(connection.ConnectionString, npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(
        maxRetryCount: 5,
        maxRetryDelay: TimeSpan.FromSeconds(30),
        errorCodesToAdd: null))
    .Options;

await using var db = new ApplicationDbContext(options);
var result = await db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
{
    var existingAdmin = await db.Employees.FirstOrDefaultAsync(employee => employee.Username == "admin");
    if (existingAdmin is not null)
        return (Success: false, Message: "ADMIN CHECK: username 'admin' already exists; refusing to overwrite it.", BranchName: (string?)null);

    var branch = await db.Branches
        .Where(candidate => candidate.IsActive)
        .OrderByDescending(candidate => candidate.IsMain)
        .ThenBy(candidate => candidate.Name)
        .FirstOrDefaultAsync();

    var admin = new Employee
    {
        Id = Guid.NewGuid(),
        EmployeeCode = await CreateEmployeeCodeAsync(db),
        FullName = "Quản Trị Viên",
        Username = "admin",
        Position = "Quản lý",
        Role = "admin",
        Department = "Điều hành",
        BranchId = branch?.Id,
        BranchName = branch?.Name,
        IsActive = true,
        CreatedAt = DateTime.UtcNow,
        StartDate = DateTime.UtcNow
    };

    var hasher = new PasswordHasher<Employee>();
    admin.Password = hasher.HashPassword(admin, password!);
    db.Employees.Add(admin);
    await db.SaveChangesAsync();
    return (Success: true, Message: string.Empty, BranchName: branch?.Name);
});

if (!result.Success)
    return Fail(result.Message);

Console.WriteLine("ADMIN BOOTSTRAP: PASS");
Console.WriteLine("USERNAME: admin");
Console.WriteLine($"BRANCH: {result.BranchName ?? "<global>"}");
Console.WriteLine("PASSWORD: stored as an ASP.NET Identity hash");
return 0;

static async Task<string> CreateEmployeeCodeAsync(ApplicationDbContext db)
{
    const string preferredCode = "NVADMIN";
    if (!await db.Employees.AnyAsync(employee => employee.EmployeeCode == preferredCode))
        return preferredCode;

    return $"NV{Guid.NewGuid():N}"[..10].ToUpperInvariant();
}

static int Fail(string message)
{
    Console.WriteLine(message);
    Console.WriteLine("SAFE RESULT: BLOCKED");
    return 2;
}
