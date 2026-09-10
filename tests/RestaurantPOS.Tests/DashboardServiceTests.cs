using Microsoft.Extensions.Caching.Memory;
using RestaurantPOS.Application.Services;
using RestaurantPOS.Domain.Entities;

namespace RestaurantPOS.Tests;

public sealed class DashboardServiceTests
{
    [Fact]
    public async Task GetSummaryAsync_CalculatesNetProfitFromRevenueExpensesAndCost()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;
        var branch = new Branch { Id = Guid.NewGuid(), Name = "Main", IsActive = true };
        var product = new Product { Id = Guid.NewGuid(), Name = "Coffee", CostPrice = 12m, Price = 30m };
        var order = new Order
        {
            Id = Guid.NewGuid(), Status = "Hoàn thành", PaidAmount = 30m,
            TotalAmount = 30m, CreatedAt = DateTime.UtcNow,
            Details = new List<OrderDetail> { new() { ProductId = product.Id, ProductName = "Coffee", Quantity = 1, UnitPrice = 30m } }
        };
        context.Branches.Add(branch);
        context.Products.Add(product);
        context.Orders.Add(order);
        context.Expenses.Add(new Expense
        {
            Id = Guid.NewGuid(), Description = "Utilities", Amount = 5m,
            Category = "Utilities", PaymentMethod = "Cash", Note = "", BranchId = branch.Id, ExpenseDate = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var summary = await new DashboardService(context, new MemoryCache(new MemoryCacheOptions())).GetSummaryAsync(null);
        var type = summary.GetType();

        Assert.Equal(30m, Convert.ToDecimal(type.GetProperty("todayRevenue")!.GetValue(summary)));
        Assert.Equal(5m, Convert.ToDecimal(type.GetProperty("totalExpenses")!.GetValue(summary)));
        Assert.Equal(12m, Convert.ToDecimal(type.GetProperty("costOfGoodsSold")!.GetValue(summary)));
        Assert.Equal(13m, Convert.ToDecimal(type.GetProperty("netProfit")!.GetValue(summary)));
        Assert.Equal(13d, Convert.ToDouble(type.GetProperty("estimatedProfit")!.GetValue(summary)));
    }
}
