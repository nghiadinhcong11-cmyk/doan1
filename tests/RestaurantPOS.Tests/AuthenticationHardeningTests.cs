using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RestaurantPOS.Application.Common.Interfaces;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Infrastructure.Persistence;
using RestaurantPOS.WebAPI.Controllers;
using System.Security.Claims;

namespace RestaurantPOS.Tests;

public sealed class AuthenticationHardeningTests
{
    [Fact]
    public async Task Inactive_employee_cannot_login()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;

        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            Username = "inactive-user",
            FullName = "Inactive",
            Role = "manager",
            IsActive = false
        };
        employee.Password = new PasswordHasher<Employee>().HashPassword(employee, "valid-password");
        context.Employees.Add(employee);
        await context.SaveChangesAsync();

        var controller = new AuthController(
            context,
            Mock.Of<IJwtService>(),
            new PasswordHasher<Employee>(),
            new PasswordHasher<Customer>());

        var result = await controller.Login(new AuthController.LoginRequest
        {
            Username = "inactive-user",
            Password = "valid-password",
            Mode = "management"
        });

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task Inactive_customer_cannot_receive_registered_customer_token()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;
        context.Customers.Add(new Customer
        {
            Id = Guid.NewGuid(),
            PhoneNumber = "0900000001",
            FullName = "Inactive Customer",
            IsActive = false
        });
        await context.SaveChangesAsync();

        var controller = new AuthController(
            context,
            Mock.Of<IJwtService>(),
            new PasswordHasher<Employee>(),
            new PasswordHasher<Customer>());

        var result = await controller.GetCustomerToken(new AuthController.CustomerTokenRequest
        {
            PhoneNumber = "0900000001"
        });

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task Customer_token_endpoint_does_not_issue_an_unbound_guest_session()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;

        var controller = new AuthController(
            context,
            Mock.Of<IJwtService>(),
            new PasswordHasher<Employee>(),
            new PasswordHasher<Customer>());

        var result = await controller.GetCustomerToken(new AuthController.CustomerTokenRequest
        {
            PhoneNumber = string.Empty,
            FullName = "Guest"
        });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Employee_creation_rejects_short_password_before_persistence()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;

        var controller = new EmployeeController(
            context,
            new PasswordHasher<Employee>(),
            Mock.Of<RestaurantPOS.Application.Services.IEmployeeService>());
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    new[] { new Claim(ClaimTypes.Role, "admin") }, "Bearer"))
            }
        };

        var result = await controller.CreateEmployee(new Employee
        {
            Username = "short-password",
            Password = "short",
            FullName = "Test User"
        });

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Empty(context.Employees);
    }
}
