using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RestaurantPOS.Application.Services;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Infrastructure.Persistence;
using RestaurantPOS.WebAPI.Controllers;

namespace RestaurantPOS.Tests;

public sealed class EmployeeControllerAuthorizationTests
{
    private static readonly Guid BranchA = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid BranchB = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    [Theory]
    [InlineData("employee")]
    [InlineData("cashier")]
    [InlineData("kitchen")]
    [InlineData("customer")]
    public async Task Non_management_role_cannot_enumerate_employees(string role)
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;
        var controller = CreateController(context, role, Guid.NewGuid(), BranchA);

        var result = await controller.GetEmployees(null, null, null, null, null);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task Admin_can_enumerate_employees_globally()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;
        SeedBranches(context);
        context.Employees.AddRange(
            new Employee { Id = Guid.NewGuid(), FullName = "A", BranchId = BranchA, CreatedAt = DateTime.UtcNow },
            new Employee { Id = Guid.NewGuid(), FullName = "B", BranchId = BranchB, CreatedAt = DateTime.UtcNow });
        await context.SaveChangesAsync();
        var controller = CreateController(context, "admin", Guid.NewGuid(), null);

        var result = await controller.GetEmployees(null, null, null, null, null);

        var ok = Assert.IsType<OkObjectResult>(result);
        var employees = Assert.IsAssignableFrom<IEnumerable<Employee>>(ok.Value);
        Assert.Equal(2, employees.Count());
    }

    [Fact]
    public async Task Manager_employee_list_is_limited_to_jwt_branch()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;
        SeedBranches(context);
        context.Employees.AddRange(
            new Employee { Id = Guid.NewGuid(), FullName = "A", BranchId = BranchA, CreatedAt = DateTime.UtcNow },
            new Employee { Id = Guid.NewGuid(), FullName = "B", BranchId = BranchB, CreatedAt = DateTime.UtcNow });
        await context.SaveChangesAsync();
        var controller = CreateController(context, "manager", Guid.NewGuid(), BranchA);

        var result = await controller.GetEmployees(null, null, null, null, BranchB);

        var ok = Assert.IsType<OkObjectResult>(result);
        var employees = Assert.IsAssignableFrom<IEnumerable<Employee>>(ok.Value);
        Assert.Single(employees);
        Assert.Equal(BranchA, employees.Single().BranchId);
    }

    [Fact]
    public async Task Manager_cannot_delete_employee_from_another_branch()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;
        SeedBranches(context);
        var employee = new Employee { Id = Guid.NewGuid(), FullName = "B", BranchId = BranchB };
        context.Employees.Add(employee);
        await context.SaveChangesAsync();
        var controller = CreateController(context, "manager", Guid.NewGuid(), BranchA);

        var result = await controller.DeleteEmployee(employee.Id);

        Assert.IsType<ForbidResult>(result);
        Assert.NotNull(await context.Employees.FindAsync(employee.Id));
    }

    [Fact]
    public async Task Manager_cannot_create_management_role_or_move_branch()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;
        context.Branches.Add(new Branch { Id = BranchA, Name = "A", IsActive = true });
        await context.SaveChangesAsync();
        var controller = CreateController(context, "manager", Guid.NewGuid(), BranchA);

        var result = await controller.CreateEmployee(new Employee
        {
            FullName = "New staff",
            Role = "manager",
            BranchId = BranchB,
            StartDate = DateTime.UtcNow
        });

        var ok = Assert.IsType<OkObjectResult>(result);
        var created = Assert.IsType<Employee>(ok.Value);
        Assert.Equal(BranchA, created.BranchId);
        Assert.Equal("employee", created.Role);
    }

    [Fact]
    public async Task Manager_without_branch_cannot_create_employee()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;
        var controller = CreateController(context, "manager", Guid.NewGuid(), null);

        var result = await controller.CreateEmployee(new Employee
        {
            FullName = "Unscoped employee",
            Role = "employee",
            BranchId = null,
            StartDate = DateTime.UtcNow
        });

        Assert.IsType<ForbidResult>(result);
        Assert.Empty(context.Employees);
    }

    [Fact]
    public async Task Manager_cannot_elevate_staff_during_update()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;
        SeedBranches(context);
        var existing = new Employee
        {
            Id = Guid.NewGuid(), FullName = "Staff", Role = "employee", BranchId = BranchA,
            CreatedAt = DateTime.UtcNow, StartDate = DateTime.UtcNow
        };
        context.Employees.Add(existing);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        var controller = CreateController(context, "manager", Guid.NewGuid(), BranchA);

        var result = await controller.UpdateEmployee(existing.Id, new Employee
        {
            Id = existing.Id, FullName = "Staff updated", Role = "admin", BranchId = BranchB,
            CreatedAt = existing.CreatedAt, StartDate = existing.StartDate
        });

        var ok = Assert.IsType<OkObjectResult>(result);
        var updated = Assert.IsType<Employee>(ok.Value);
        Assert.Equal(BranchA, updated.BranchId);
        Assert.Equal("employee", updated.Role);
    }

    [Fact]
    public async Task Customer_cannot_read_employee_endpoint_even_for_matching_id()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;
        SeedBranches(context);
        var employeeId = Guid.NewGuid();
        context.Employees.Add(new Employee { Id = employeeId, FullName = "Staff", BranchId = BranchA });
        await context.SaveChangesAsync();
        var controller = CreateController(context, "customer", Guid.NewGuid(), null);

        var result = await controller.GetEmployee(employeeId);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task Unauthenticated_context_cannot_read_employee_endpoint()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;
        var controller = CreateController(context, "anonymous", Guid.NewGuid(), null);

        var result = await controller.GetEmployee(Guid.NewGuid());

        Assert.IsType<ForbidResult>(result);
    }

    private static EmployeeController CreateController(ApplicationDbContext context, string role, Guid userId, Guid? branchId)
    {
        var controller = new EmployeeController(
            context,
            new PasswordHasher<Employee>(),
            Mock.Of<IEmployeeService>());
        var claims = new List<Claim>
        {
            new(ClaimTypes.Role, role),
            new(ClaimTypes.NameIdentifier, userId.ToString())
        };
        if (branchId.HasValue) claims.Add(new Claim("branchId", branchId.Value.ToString()));
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"))
            }
        };
        return controller;
    }

    private static void SeedBranches(ApplicationDbContext context)
    {
        context.Branches.AddRange(
            new Branch { Id = BranchA, Name = "Branch A", IsActive = true },
            new Branch { Id = BranchB, Name = "Branch B", IsActive = true });
        context.SaveChanges();
    }
}
