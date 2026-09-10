using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RestaurantPOS.Application.DTOs.Customers;
using RestaurantPOS.Application.Services;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Infrastructure.Persistence;
using RestaurantPOS.WebAPI.Controllers;
using Xunit;

namespace RestaurantPOS.Tests;

public class CustomerIdorTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Microsoft.Data.Sqlite.SqliteConnection _connection;

    public CustomerIdorTests()
    {
        (_context, _connection) = TestDbContextFactory.Create();
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task GetByPhone_ReturnsForbid_WhenAccessingOtherCustomer()
    {
        // Arrange
        var customerA = new Customer { Id = Guid.NewGuid(), PhoneNumber = "0900111222", FullName = "Customer A" };
        var customerB = new Customer { Id = Guid.NewGuid(), PhoneNumber = "0900333444", FullName = "Customer B" };
        _context.Customers.AddRange(customerA, customerB);
        await _context.SaveChangesAsync();

        var controller = CreateController("customer", customerA.PhoneNumber, customerA.Id);

        // Act
        var result = await controller.GetByPhone(customerB.PhoneNumber);

        // Assert
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task GetByPhone_ReturnsOk_WhenAccessingSelf()
    {
        // Arrange
        var customerA = new Customer { Id = Guid.NewGuid(), PhoneNumber = "0900111222", FullName = "Customer A" };
        _context.Customers.Add(customerA);
        await _context.SaveChangesAsync();

        var controller = CreateController("customer", customerA.PhoneNumber, customerA.Id);

        // Act
        var result = await controller.GetByPhone(customerA.PhoneNumber);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<CustomerResponseDto>(okResult.Value);
        Assert.Equal(customerA.PhoneNumber, dto.PhoneNumber);
    }

    [Fact]
    public async Task GetByPhone_ReturnsOk_ForStaff()
    {
        // Arrange
        var customerA = new Customer { Id = Guid.NewGuid(), PhoneNumber = "0900111222", FullName = "Customer A" };
        _context.Customers.Add(customerA);
        await _context.SaveChangesAsync();

        var controller = CreateController("cashier", "staff_user", Guid.NewGuid());

        // Act
        var result = await controller.GetByPhone(customerA.PhoneNumber);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task CheckExists_IsAnonymousAndReturnsMinimalData()
    {
        // Arrange
        var customerA = new Customer { Id = Guid.NewGuid(), PhoneNumber = "0900111222", FullName = "Customer A", Email = "secret@leak.com" };
        _context.Customers.Add(customerA);
        await _context.SaveChangesAsync();

        var controller = new CustomerController(_context, Mock.Of<ILoyaltyService>(), Mock.Of<IPasswordHasher<Customer>>());

        // Act
        var result = await controller.CheckExists(customerA.PhoneNumber);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<CustomerMinimalDto>(okResult.Value);
        Assert.True(dto.Exists);
        Assert.Equal(customerA.FullName, dto.FullName);
        // Ensure no PII like Email is in the DTO
    }

    [Fact]
    public async Task CreateOrUpdate_ForbidsAnonymousUpdate_OfExistingCustomer()
    {
        // Arrange
        var customerA = new Customer { Id = Guid.NewGuid(), PhoneNumber = "0900111222", FullName = "Customer A" };
        _context.Customers.Add(customerA);
        await _context.SaveChangesAsync();

        var controller = new CustomerController(_context, Mock.Of<ILoyaltyService>(), Mock.Of<IPasswordHasher<Customer>>());
        // No user set (Anonymous)

        // Act
        var result = await controller.CreateOrUpdate(new Customer { PhoneNumber = customerA.PhoneNumber, FullName = "Attacker" });

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task UpdateProfile_ForbidsOtherCustomer()
    {
        // Arrange
        var customerA = new Customer { Id = Guid.NewGuid(), PhoneNumber = "0900111222", FullName = "Customer A" };
        var customerB = new Customer { Id = Guid.NewGuid(), PhoneNumber = "0900333444", FullName = "Customer B" };
        _context.Customers.AddRange(customerA, customerB);
        await _context.SaveChangesAsync();

        var controller = CreateController("customer", customerA.PhoneNumber, customerA.Id);

        // Act
        var result = await controller.UpdateProfile(customerB.PhoneNumber, new CustomerProfileUpdateDto { FullName = "Hacked" });

        // Assert
        Assert.IsType<ForbidResult>(result);
    }

    private CustomerController CreateController(string role, string name, Guid id)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Role, role),
            new(ClaimTypes.Name, name),
            new(ClaimTypes.NameIdentifier, id.ToString())
        };

        var controller = new CustomerController(_context, Mock.Of<ILoyaltyService>(), Mock.Of<IPasswordHasher<Customer>>());
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"))
            }
        };
        return controller;
    }
}
