using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RestaurantPOS.Application.Common.Interfaces;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Infrastructure.Persistence;
using RestaurantPOS.WebAPI.Controllers;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using RestaurantPOS.Application.DTOs.Customers;
using Microsoft.EntityFrameworkCore;

namespace RestaurantPOS.Tests;

public sealed class PasswordHashingTests
{
    [Fact]
    public async Task Employee_Login_UpgradesPlaintextToHash()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;

        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Password = "plaintextpassword",
            FullName = "Test User",
            IsActive = true,
            Position = "Quản lý",
            Role = "manager"
        };
        context.Employees.Add(employee);
        await context.SaveChangesAsync();

        var hasher = new PasswordHasher<Employee>();
        var authController = new AuthController(context, Mock.Of<IJwtService>(), hasher, new PasswordHasher<Customer>());

        // Act
        var result = await authController.Login(new AuthController.LoginRequest
        {
            Username = "testuser",
            Password = "plaintextpassword",
            Mode = "management"
        });

        // Assert
        Assert.IsType<OkObjectResult>(result);

        var updatedEmployee = await context.Employees.AsNoTracking().FirstAsync(e => e.Id == employee.Id);
        Assert.NotEqual("plaintextpassword", updatedEmployee!.Password);
        Assert.Equal(PasswordVerificationResult.Success, hasher.VerifyHashedPassword(updatedEmployee, updatedEmployee.Password!, "plaintextpassword"));

        var secondLogin = await authController.Login(new AuthController.LoginRequest
        {
            Username = "testuser",
            Password = "plaintextpassword",
            Mode = "management"
        });

        Assert.IsType<OkObjectResult>(secondLogin);
    }

    [Fact]
    public async Task Employee_Create_HashesPassword()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;

        var hasher = new PasswordHasher<Employee>();
        var controller = new EmployeeController(context, hasher, Mock.Of<RestaurantPOS.Application.Services.IEmployeeService>());
        SetUser(controller, "admin");

        var newEmployee = new Employee
        {
            Username = "newuser",
            Password = "mypassword",
            FullName = "New User"
        };

        // Act
        var result = await controller.CreateEmployee(newEmployee);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedEmployee = (Employee)okResult.Value!;
        Assert.Null(returnedEmployee.Password); // Should be cleared in response

        var savedEmployee = await context.Employees.AsNoTracking().FirstAsync(e => e.Username == "newuser");
        Assert.NotEqual("mypassword", savedEmployee.Password);
        Assert.Equal(PasswordVerificationResult.Success, hasher.VerifyHashedPassword(savedEmployee, savedEmployee.Password!, "mypassword"));
    }

    [Fact]
    public async Task Customer_UpdateProfile_HashesNewPassword()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;

        var customerId = Guid.NewGuid();
        var customer = new Customer
        {
            Id = customerId,
            PhoneNumber = "0900000000",
            Password = "oldpassword"
        };
        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        var hasher = new PasswordHasher<Customer>();
        var controller = new CustomerController(context, Mock.Of<RestaurantPOS.Application.Services.ILoyaltyService>(), hasher);
        SetUser(controller, "customer", customerId);

        // Act
        var result = await controller.UpdateProfile("0900000000", new CustomerProfileUpdateDto
        {
            NewPassword = "newpassword"
        });

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedCustomer = (CustomerResponseDto)okResult.Value!;
        // PII fields should still be present but Password is not in DTO anyway

        var savedCustomer = await context.Customers.AsNoTracking().FirstAsync(c => c.Id == customerId);
        Assert.NotEqual("newpassword", savedCustomer!.Password);
        Assert.Equal(PasswordVerificationResult.Success, hasher.VerifyHashedPassword(savedCustomer, savedCustomer.Password!, "newpassword"));
    }

    private static void SetUser(ControllerBase controller, string role, Guid? userId = null)
    {
        var claims = new List<Claim> { new Claim(ClaimTypes.Role, role) };
        if (userId.HasValue) claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"))
            }
        };
    }
}
