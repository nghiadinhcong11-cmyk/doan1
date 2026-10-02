using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RestaurantPOS.Application.Common.Interfaces;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Infrastructure.Persistence;
using RestaurantPOS.WebAPI.Controllers;
using Microsoft.AspNetCore.Identity;

namespace RestaurantPOS.Tests;

public sealed class ChangePasswordSecurityTests
{
    [Fact]
    public void ChangePassword_requires_authorization()
    {
        var method = typeof(AuthController).GetMethod(nameof(AuthController.ChangePassword));

        Assert.NotNull(method);
        Assert.NotNull(method!.GetCustomAttribute<AuthorizeAttribute>());
    }

    [Fact]
    public async Task Authenticated_employee_can_change_only_own_password()
    {
        var employeeId = Guid.NewGuid();
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;
        context.Employees.Add(new Employee { Id = employeeId, FullName = "Employee", Password = "old" });
        await context.SaveChangesAsync();
        var controller = CreateController(context, employeeId, "employee");

        var result = await controller.ChangePassword(new AuthController.ChangePasswordRequest
        {
            Id = employeeId,
            Type = "Employee",
            OldPassword = "old",
            NewPassword = "new-password"
        });

        Assert.IsType<OkObjectResult>(result);
        var updated = await context.Employees.FindAsync(employeeId);
        Assert.NotEqual("new", updated!.Password);
        Assert.Equal(PasswordVerificationResult.Success, new PasswordHasher<Employee>().VerifyHashedPassword(updated, updated.Password!, "new-password"));
    }

    [Fact]
    public async Task Authenticated_user_cannot_change_another_account_password()
    {
        var ownId = Guid.NewGuid();
        var otherId = Guid.NewGuid();
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;
        context.Employees.AddRange(
            new Employee { Id = ownId, FullName = "Own", Password = "own-old" },
            new Employee { Id = otherId, FullName = "Other", Password = "other-old" });
        await context.SaveChangesAsync();
        var controller = CreateController(context, ownId, "employee");

        var result = await controller.ChangePassword(new AuthController.ChangePasswordRequest
        {
            Id = otherId,
            Type = "Employee",
            OldPassword = "other-old",
            NewPassword = "attacker-value"
        });

        Assert.IsType<ForbidResult>(result);
        Assert.Equal("other-old", (await context.Employees.FindAsync(otherId))!.Password);
    }

    [Fact]
    public async Task Admin_cannot_reset_another_account_through_self_change_endpoint()
    {
        var adminId = Guid.NewGuid();
        var otherId = Guid.NewGuid();
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;
        context.Employees.Add(new Employee { Id = otherId, FullName = "Other", Password = "old" });
        await context.SaveChangesAsync();
        var controller = CreateController(context, adminId, "admin");

        var result = await controller.ChangePassword(new AuthController.ChangePasswordRequest
        {
            Id = otherId,
            Type = "Employee",
            OldPassword = "old",
            NewPassword = "new-password"
        });

        Assert.IsType<ForbidResult>(result);
        Assert.Equal("old", (await context.Employees.FindAsync(otherId))!.Password);
    }

    [Fact]
    public async Task Wrong_current_password_does_not_change_password()
    {
        var employeeId = Guid.NewGuid();
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;
        context.Employees.Add(new Employee { Id = employeeId, FullName = "Employee", Password = "old" });
        await context.SaveChangesAsync();
        var controller = CreateController(context, employeeId, "employee");

        var result = await controller.ChangePassword(new AuthController.ChangePasswordRequest
        {
            Id = employeeId,
            Type = "Employee",
            OldPassword = "wrong",
            NewPassword = "new-password"
        });

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("old", (await context.Employees.FindAsync(employeeId))!.Password);
    }

    [Fact]
    public async Task Missing_identity_claim_is_rejected_without_database_change()
    {
        var employeeId = Guid.NewGuid();
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;
        context.Employees.Add(new Employee { Id = employeeId, FullName = "Employee", Password = "old" });
        await context.SaveChangesAsync();
        var controller = CreateController(context, null, "employee");

        var result = await controller.ChangePassword(new AuthController.ChangePasswordRequest
        {
            Id = employeeId,
            Type = "Employee",
            OldPassword = "old",
            NewPassword = "new-password"
        });

        Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("old", (await context.Employees.FindAsync(employeeId))!.Password);
    }

    [Fact]
    public async Task Customer_can_change_own_customer_password_but_cannot_use_employee_type()
    {
        var customerId = Guid.NewGuid();
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;
        context.Customers.Add(new Customer { Id = customerId, FullName = "Customer", Password = "old" });
        await context.SaveChangesAsync();
        var controller = CreateController(context, customerId, "customer");

        var forbidden = await controller.ChangePassword(new AuthController.ChangePasswordRequest
        {
            Id = customerId,
            Type = "Employee",
            OldPassword = "old",
            NewPassword = "new-password"
        });
        Assert.IsType<ForbidResult>(forbidden);

        var allowed = await controller.ChangePassword(new AuthController.ChangePasswordRequest
        {
            Id = customerId,
            Type = "Customer",
            OldPassword = "old",
            NewPassword = "new-password"
        });
        Assert.IsType<OkObjectResult>(allowed);
        var updated = await context.Customers.FindAsync(customerId);
        Assert.NotEqual("new", updated!.Password);
        Assert.Equal(PasswordVerificationResult.Success, new PasswordHasher<Customer>().VerifyHashedPassword(updated, updated.Password!, "new-password"));
    }

    private static AuthController CreateController(ApplicationDbContext context, Guid? userId, string role)
    {
        var claims = new List<Claim>();
        if (userId.HasValue)
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()));
        claims.Add(new Claim(ClaimTypes.Role, role));

        return new AuthController(context, Mock.Of<IJwtService>(), new PasswordHasher<Employee>(), new PasswordHasher<Customer>())
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"))
                }
            }
        };
    }
}
