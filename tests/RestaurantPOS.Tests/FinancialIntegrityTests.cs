using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RestaurantPOS.Application.Services;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Infrastructure.Persistence;
using Xunit;

namespace RestaurantPOS.Tests;

public sealed class FinancialIntegrityTests
{
    [Fact]
    public async Task CreateOrUpdateOrderAsync_IgnoresClientSuppliedFinancials()
    {
        var (context, _) = TestDbContextFactory.Create();
        var product = new Product { Id = Guid.NewGuid(), Name = "Cafe", Price = 50000, IsActive = true };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var service = new OrderService(context);
        var order = new Order
        {
            BranchId = Guid.NewGuid(),
            Details = new List<OrderDetail>
            {
                new OrderDetail { ProductId = product.Id, ProductName = "Cafe", Quantity = 1, UnitPrice = 1 } // Malicious price
            },
            TotalAmount = 1, // Malicious total
            PaidAmount = 50000, // Malicious paid amount
            Status = "Hoàn thành" // Malicious status
        };

        var result = await service.CreateOrUpdateOrderAsync(order);

        Assert.Equal(50000, result.TotalAmount);
        Assert.Equal(50000, result.Details.First().UnitPrice);
        Assert.Equal(0, result.PaidAmount);
        Assert.Equal("Đang xử lý", result.Status);
    }

    [Fact]
    public async Task CreateOrUpdateOrderAsync_RejectsLargeQuantity()
    {
        var (context, _) = TestDbContextFactory.Create();
        var service = new OrderService(context);
        var order = new Order
        {
            Details = new List<OrderDetail>
            {
                new OrderDetail { ProductId = Guid.NewGuid(), Quantity = 9999 }
            }
        };

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateOrUpdateOrderAsync(order));
    }

    [Fact]
    public async Task PayOrderAsync_UsesPersistedTotalAmount()
    {
        var (context, _) = TestDbContextFactory.Create();
        var branchId = Guid.NewGuid();
        var order = new Order { Id = Guid.NewGuid(), BranchId = branchId, TotalAmount = 50000, Status = "Đang xử lý" };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var service = new OrderService(context);

        // Attempt to pay 1 VND
        var result = await service.PayOrderAsync(order.Id, 1, "Tiền mặt");

        Assert.Equal(PaymentAttemptResult.NotPayable, result.Result);

        var dbOrder = await context.Orders.FindAsync(order.Id);
        Assert.Equal(0, dbOrder!.PaidAmount);
        Assert.Equal("Đang xử lý", dbOrder.Status);
    }

    [Fact]
    public async Task RedeemPointsAsync_PreventsNegativeBalance()
    {
        var (context, _) = TestDbContextFactory.Create();
        var customer = new Customer { Id = Guid.NewGuid(), LoyaltyPoints = 10, IsActive = true };
        var order = new Order { Id = Guid.NewGuid(), TotalAmount = 50000, Status = "Đang xử lý" };
        context.Customers.Add(customer);
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var loyaltyService = new LoyaltyService(context, new SystemSettingService(context));

        // Try to redeem 100 points while having only 10
        var result = await loyaltyService.RedeemPointsAsync(customer.Id, order.Id, 100);

        Assert.Null(result);
        var dbCustomer = await context.Customers.FindAsync(customer.Id);
        Assert.Equal(10, dbCustomer!.LoyaltyPoints);
    }
}
