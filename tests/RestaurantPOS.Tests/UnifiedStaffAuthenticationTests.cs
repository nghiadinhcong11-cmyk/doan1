using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RestaurantPOS.Application.Common.Interfaces;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Infrastructure.Persistence;
using RestaurantPOS.WebAPI.Controllers;

namespace RestaurantPOS.Tests;

public sealed class UnifiedStaffAuthenticationTests
{
    [Theory]
    [InlineData("admin")]
    [InlineData("manager")]
    public async Task Management_login_issues_the_stored_management_role(string storedRole)
    {
        var (controller, jwt, employee, context, connection) = await CreateAsync(storedRole);
        await using var _ = context;
        await using var __ = connection;

        var result = await controller.Login(new AuthController.LoginRequest { Username = employee.Username!, Password = "valid-password", Mode = "management" });

        Assert.IsType<OkObjectResult>(result);
        jwt.Verify(service => service.GenerateToken(employee.Id, It.IsAny<string>(), employee.FullName, storedRole, employee.BranchId, employee.BranchName, employee.Position, null, null), Times.Once);
    }

    [Theory]
    [InlineData("employee")]
    [InlineData("cashier")]
    [InlineData("kitchen")]
    public async Task Staff_login_issues_the_stored_operational_role(string storedRole)
    {
        var (controller, jwt, employee, context, connection) = await CreateAsync(storedRole);
        await using var _ = context;
        await using var __ = connection;

        var result = await controller.Login(new AuthController.LoginRequest { Username = employee.Username!, Password = "valid-password", Mode = "staff" });

        Assert.IsType<OkObjectResult>(result);
        jwt.Verify(service => service.GenerateToken(employee.Id, It.IsAny<string>(), employee.FullName, storedRole, employee.BranchId, employee.BranchName, employee.Position, null, null), Times.Once);
    }

    [Theory]
    [InlineData("employee", "management")]
    [InlineData("cashier", "management")]
    [InlineData("kitchen", "management")]
    [InlineData("admin", "staff")]
    [InlineData("manager", "staff")]
    public async Task Login_group_cannot_elevate_or_transform_a_stored_role(string storedRole, string loginGroup)
    {
        var (controller, jwt, employee, context, connection) = await CreateAsync(storedRole);
        await using var _ = context;
        await using var __ = connection;

        var result = await controller.Login(new AuthController.LoginRequest { Username = employee.Username!, Password = "valid-password", Mode = loginGroup });

        Assert.IsType<BadRequestObjectResult>(result);
        jwt.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Login_rejects_missing_or_unknown_group_before_issuing_a_token()
    {
        var (controller, jwt, employee, context, connection) = await CreateAsync("employee");
        await using var _ = context;
        await using var __ = connection;

        var missing = await controller.Login(new AuthController.LoginRequest { Username = employee.Username!, Password = "valid-password" });
        var unknown = await controller.Login(new AuthController.LoginRequest { Username = employee.Username!, Password = "valid-password", Mode = "admin" });

        Assert.IsType<BadRequestObjectResult>(missing);
        Assert.IsType<BadRequestObjectResult>(unknown);
        jwt.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Client_branch_input_does_not_override_the_authenticated_employees_branch_claim()
    {
        var (controller, jwt, employee, context, connection) = await CreateAsync("employee");
        await using var _ = context;
        await using var __ = connection;
#pragma warning disable CS0618
        var result = await controller.Login(new AuthController.LoginRequest { Username = employee.Username!, Password = "valid-password", Mode = "staff", BranchId = Guid.NewGuid() });
#pragma warning restore CS0618

        Assert.IsType<OkObjectResult>(result);
        jwt.Verify(service => service.GenerateToken(employee.Id, It.IsAny<string>(), employee.FullName, "employee", employee.BranchId, employee.BranchName, employee.Position, null, null), Times.Once);
    }

    private static async Task<(AuthController Controller, Mock<IJwtService> Jwt, Employee Employee, ApplicationDbContext Context, Microsoft.Data.Sqlite.SqliteConnection Connection)> CreateAsync(string role)
    {
        var (context, connection) = TestDbContextFactory.Create();
        var branchId = Guid.NewGuid();
        context.Branches.Add(new Branch { Id = branchId, Name = "Test Branch", IsActive = true });
        var employee = new Employee { Id = Guid.NewGuid(), Username = $"{role}-{Guid.NewGuid():N}", FullName = "Test Employee", Role = role, IsActive = true, BranchId = branchId, BranchName = "Test Branch", Position = "Test" };
        employee.Password = new PasswordHasher<Employee>().HashPassword(employee, "valid-password");
        context.Employees.Add(employee);
        await context.SaveChangesAsync();
        var jwt = new Mock<IJwtService>(MockBehavior.Strict);
        jwt.Setup(service => service.GenerateToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<Guid?>())).Returns("test-token");
        return (new AuthController(context, jwt.Object, new PasswordHasher<Employee>(), new PasswordHasher<Customer>()), jwt, employee, context, connection);
    }
}
