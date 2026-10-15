using Microsoft.AspNetCore.Identity;
using RestaurantPOS.Application.Common.Security;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Infrastructure.Persistence;

namespace RestaurantPOS.TestSeed;

public sealed record TestSeedData(
    Branch Branch,
    IReadOnlyList<Area> Areas,
    IReadOnlyList<RestaurantTable> Tables,
    Employee Admin,
    IReadOnlyList<Product> Products);

public static class TestSeedPlan
{
    public const string BranchName = "DOAN TEST Branch";
    public const string AdminUsername = "test-admin";
    public const int ExpectedBranchCount = 1;
    public const int ExpectedAreaCount = 3;
    public const int ExpectedTableCount = 7;
    public const int ExpectedAdminCount = 1;
    public const int ExpectedProductCount = 7;

    private static readonly Guid BranchId = Guid.Parse("40000000-0000-4000-8000-000000000001");
    private static readonly Guid AdminId = Guid.Parse("40000000-0000-4000-8000-000000000002");

    private static readonly Guid[] AreaIds =
    [
        Guid.Parse("40000000-0000-4000-8000-000000000011"),
        Guid.Parse("40000000-0000-4000-8000-000000000012"),
        Guid.Parse("40000000-0000-4000-8000-000000000013")
    ];

    private static readonly string[] AreaNames = ["TEST Tầng 1", "TEST Tầng 2", "TEST Sân vườn"];

    private static readonly Guid[] TableIds =
    [
        Guid.Parse("40000000-0000-4000-8000-000000000021"),
        Guid.Parse("40000000-0000-4000-8000-000000000022"),
        Guid.Parse("40000000-0000-4000-8000-000000000023"),
        Guid.Parse("40000000-0000-4000-8000-000000000024"),
        Guid.Parse("40000000-0000-4000-8000-000000000025"),
        Guid.Parse("40000000-0000-4000-8000-000000000026"),
        Guid.Parse("40000000-0000-4000-8000-000000000027")
    ];

    private static readonly string[] TableNames = ["TEST Bàn 01", "TEST Bàn 02", "TEST Bàn 03", "TEST Bàn 11", "TEST Bàn 12", "TEST Bàn VIP", "TEST Bàn SV1"];

    private static readonly (Guid Id, string Code, string Name, string Category, string Group, decimal Price, decimal Cost)[] ProductDefinitions =
    [
        (Guid.Parse("40000000-0000-4000-8000-000000000031"), "TEST-DU001", "Cà phê sữa đá TEST", "Đồ uống", "Cà phê", 29000m, 12000m),
        (Guid.Parse("40000000-0000-4000-8000-000000000032"), "TEST-DU002", "Bạc xỉu TEST", "Đồ uống", "Cà phê", 32000m, 14000m),
        (Guid.Parse("40000000-0000-4000-8000-000000000033"), "TEST-DU003", "Trà đào cam sả TEST", "Đồ uống", "Trà trái cây", 39000m, 15000m),
        (Guid.Parse("40000000-0000-4000-8000-000000000034"), "TEST-DU004", "Trà vải lài TEST", "Đồ uống", "Trà trái cây", 39000m, 15000m),
        (Guid.Parse("40000000-0000-4000-8000-000000000035"), "TEST-DA001", "Mì Ý bò bằm TEST", "Đồ ăn", "Món chính", 65000m, 30000m),
        (Guid.Parse("40000000-0000-4000-8000-000000000036"), "TEST-DA002", "Bò bít tết TEST", "Đồ ăn", "Món chính", 129000m, 65000m),
        (Guid.Parse("40000000-0000-4000-8000-000000000037"), "TEST-DA003", "Khoai tây chiên TEST", "Đồ ăn", "Khai vị", 35000m, 12000m)
    ];

    public static TestSeedData Create(string adminPassword)
    {
        var passwordError = PasswordPolicy.Validate(adminPassword);
        if (passwordError is not null)
            throw new InvalidOperationException("TEST admin password does not satisfy the application password policy.");

        var branch = new Branch
        {
            Id = BranchId,
            Name = BranchName,
            IsMain = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var areas = new[]
        {
            new Area { Id = AreaIds[0], BranchId = BranchId, Name = "TEST Tầng 1", DisplayOrder = 1 },
            new Area { Id = AreaIds[1], BranchId = BranchId, Name = "TEST Tầng 2", DisplayOrder = 2 },
            new Area { Id = AreaIds[2], BranchId = BranchId, Name = "TEST Sân vườn", DisplayOrder = 3 }
        };

        var tables = new[]
        {
            CreateTable(0, areas[0], "TEST Bàn 01", 4),
            CreateTable(1, areas[0], "TEST Bàn 02", 4),
            CreateTable(2, areas[0], "TEST Bàn 03", 6),
            CreateTable(3, areas[1], "TEST Bàn 11", 4),
            CreateTable(4, areas[1], "TEST Bàn 12", 8),
            CreateTable(5, areas[1], "TEST Bàn VIP", 10),
            CreateTable(6, areas[2], "TEST Bàn SV1", 4)
        };

        var admin = new Employee
        {
            Id = AdminId,
            EmployeeCode = "TEST-ADMIN-001",
            EmployeeType = "FullTime",
            FullName = "DOAN TEST Admin",
            Position = "Quản lý TEST",
            Department = "TEST",
            BranchId = BranchId,
            BranchName = BranchName,
            IsActive = true,
            Role = "admin",
            Username = AdminUsername,
            CreatedAt = DateTime.UtcNow
        };
        admin.Password = new PasswordHasher<Employee>().HashPassword(admin, adminPassword);

        var products = ProductDefinitions.Select(definition => new Product
        {
            Id = definition.Id,
            Code = definition.Code,
            Name = definition.Name,
            Category = definition.Category,
            Group = definition.Group,
            Price = definition.Price,
            CostPrice = definition.Cost,
            IsActive = true,
            AvailabilityStatus = "Available",
            CreatedAt = DateTime.UtcNow
        }).ToArray();

        return new TestSeedData(branch, areas, tables, admin, products);
    }

    public static void AddTo(ApplicationDbContext context, TestSeedData data)
    {
        context.Branches.Add(data.Branch);
        context.Areas.AddRange(data.Areas);
        context.Tables.AddRange(data.Tables);
        context.Employees.Add(data.Admin);
        context.Products.AddRange(data.Products);
    }

    public static bool HasValidQrTokens(IEnumerable<RestaurantTable> tables)
    {
        var values = tables.Select(table => table.QrToken).ToArray();
        return values.Length == ExpectedTableCount &&
               values.All(token => token is not null && token.Length == QrTokenGenerator.EncodedLength) &&
               values.Distinct(StringComparer.Ordinal).Count() == ExpectedTableCount;
    }

    public static bool MatchesExpectedBranch(Branch? branch) =>
        branch is not null && branch.Id == BranchId && branch.Name == BranchName && branch.IsMain && branch.IsActive;

    public static bool MatchesExpectedAreas(IEnumerable<Area> areas)
    {
        var values = areas.ToArray();
        return values.Length == ExpectedAreaCount &&
               values.Select(area => area.Id).ToHashSet().SetEquals(AreaIds) &&
               values.All(area => area.BranchId == BranchId && area.Name is not null && AreaNames.Contains(area.Name, StringComparer.Ordinal));
    }

    public static bool MatchesExpectedTables(IEnumerable<RestaurantTable> tables)
    {
        var values = tables.ToArray();
        var expectedNames = TableIds.Zip(TableNames).ToDictionary(pair => pair.First, pair => pair.Second);
        return values.Length == ExpectedTableCount &&
               values.Select(table => table.Id).ToHashSet().SetEquals(TableIds) &&
               values.All(table => table.BranchId == BranchId && table.IsActive && table.Status == "Trống" &&
                                   expectedNames.TryGetValue(table.Id, out var name) && table.Name == name) &&
               HasValidQrTokens(values);
    }

    public static bool MatchesExpectedAdmin(Employee? admin) =>
        admin is not null && admin.Id == AdminId && admin.Username == AdminUsername &&
        admin.Role == "admin" && admin.IsActive && admin.BranchId == BranchId &&
        admin.Password is { Length: > 0 };

    public static bool MatchesExpectedProducts(IEnumerable<Product> products)
    {
        var values = products.ToArray();
        var expected = ProductDefinitions.ToDictionary(definition => definition.Id);
        return values.Length == ExpectedProductCount &&
               values.All(product => expected.TryGetValue(product.Id, out var definition) &&
                                     product.Code == definition.Code && product.Name == definition.Name &&
                                     product.Category == definition.Category && product.Group == definition.Group &&
                                     product.Price == definition.Price && product.CostPrice == definition.Cost &&
                                     product.IsActive && product.AvailabilityStatus == "Available");
    }

    private static RestaurantTable CreateTable(int index, Area area, string name, int seats) => new()
    {
        Id = TableIds[index],
        Name = name,
        AreaId = area.Id,
        SeatCount = seats,
        IsActive = true,
        Status = "Trống",
        BranchId = BranchId,
        BranchName = BranchName,
        QrToken = QrTokenGenerator.Generate()
    };
}
