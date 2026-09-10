using Microsoft.EntityFrameworkCore;
using RestaurantPOS.Application.Services;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Infrastructure.Persistence;

namespace RestaurantPOS.Tests;

public class OrderFinancialCalculationTests
{
    [Fact]
    public async Task CreateOrder_WithNoVatOrServiceFee_UsesDatabaseSubtotalAsTotal()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;
        var branch = Guid.NewGuid();
        var product = new Product { Id = Guid.NewGuid(), Name = "Americano", Price = 100_000m };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var order = await new OrderService(context).CreateOrUpdateOrderAsync(CreateOrder(branch, product.Id));

        Assert.Equal(100_000m, order.SubTotal);
        Assert.Equal(0m, order.VatPercent);
        Assert.Equal(0m, order.VatAmount);
        Assert.Equal(0m, order.ServiceFeePercent);
        Assert.Equal(0m, order.ServiceFeeAmount);
        Assert.Equal(100_000m, order.TotalAmount);
    }

    [Fact]
    public async Task CreateOrder_CalculatesFinancialSnapshotsFromBranchSettingsAndDatabasePrices()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;
        var branch = Guid.NewGuid();
        var product = new Product { Id = Guid.NewGuid(), Name = "Coffee", Price = 100_000m };
        context.Products.Add(product);
        await SetSettings(context, branch, vatPercent: 8m, serviceFeePercent: 5m);
        await context.SaveChangesAsync();

        var order = await new OrderService(context).CreateOrUpdateOrderAsync(new Order
        {
            BranchId = branch,
            TableName = "Mang về",
            TotalAmount = 1m,
            VatAmount = 0m,
            ServiceFeeAmount = 0m,
            Details = new List<OrderDetail>
            {
                new() { ProductId = product.Id, ProductName = "client supplied", Quantity = 1, UnitPrice = 1m }
            }
        });

        Assert.Equal(100_000m, order.SubTotal);
        Assert.Equal(5m, order.ServiceFeePercent);
        Assert.Equal(5_000m, order.ServiceFeeAmount);
        Assert.Equal(8m, order.VatPercent);
        Assert.Equal(8_400m, order.VatAmount);
        Assert.Equal(113_400m, order.TotalAmount);
        Assert.Equal(100_000m, Assert.Single(order.Details).UnitPrice);
    }

    [Fact]
    public async Task CreateOrder_UsesEachBranchsSettingsAndKeepsSnapshotAfterSettingsChange()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;
        var branchA = Guid.NewGuid();
        var branchB = Guid.NewGuid();
        var product = new Product { Id = Guid.NewGuid(), Name = "Tea", Price = 100_000m };
        context.Products.Add(product);
        await SetSettings(context, branchA, vatPercent: 8m, serviceFeePercent: 0m);
        await SetSettings(context, branchB, vatPercent: 10m, serviceFeePercent: 0m);
        await context.SaveChangesAsync();
        var service = new OrderService(context);

        var orderA = await service.CreateOrUpdateOrderAsync(CreateOrder(branchA, product.Id));
        var orderB = await service.CreateOrUpdateOrderAsync(CreateOrder(branchB, product.Id));
        await SetSettings(context, branchA, vatPercent: 10m, serviceFeePercent: 0m);

        Assert.Equal(8m, orderA.VatPercent);
        Assert.Equal(8_000m, orderA.VatAmount);
        Assert.Equal(108_000m, orderA.TotalAmount);
        Assert.Equal(10m, orderB.VatPercent);
        Assert.Equal(10_000m, orderB.VatAmount);
        Assert.Equal(110_000m, orderB.TotalAmount);
    }

    [Fact]
    public async Task UpdateOrder_RecalculatesUsingDatabasePriceAndRoundsChargesToVnd()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;
        var branch = Guid.NewGuid();
        var product = new Product { Id = Guid.NewGuid(), Name = "Latte", Price = 33_333m };
        context.Products.Add(product);
        await SetSettings(context, branch, vatPercent: 8m, serviceFeePercent: 5m);
        await context.SaveChangesAsync();
        var service = new OrderService(context);
        var order = await service.CreateOrUpdateOrderAsync(CreateOrder(branch, product.Id));

        order.Details.Single().Quantity = 2;
        order.Details.Single().UnitPrice = 1m;
        var updated = await service.CreateOrUpdateOrderAsync(order);

        Assert.Equal(66_666m, updated.SubTotal);
        Assert.Equal(3_333m, updated.ServiceFeeAmount);
        Assert.Equal(5_600m, updated.VatAmount);
        Assert.Equal(75_599m, updated.TotalAmount);
        Assert.Equal(33_333m, updated.Details.Single().UnitPrice);
    }

    [Fact]
    public async Task CreateOrder_UsesAuthoritativeProductOptionAndStandaloneToppingPrices()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;
        var branch = Guid.NewGuid();
        var product = new Product
        {
            Id = Guid.NewGuid(), Name = "Milk tea", Price = 20_000m,
            SizesJson = "[{\"name\":\"L\",\"price\":5000}]",
            ToppingsJson = "[{\"name\":\"Pearl\",\"price\":7000}]"
        };
        var topping = new Topping { Id = Guid.NewGuid(), Name = "Cookie", Price = 4_000m };
        context.AddRange(product, topping);
        await context.SaveChangesAsync();

        var order = await new OrderService(context).CreateOrUpdateOrderAsync(new Order
        {
            BranchId = branch,
            TableName = "Mang về",
            Details = new List<OrderDetail>
            {
                new() { ProductId = product.Id, ProductName = "fake name (L)", Quantity = 1, UnitPrice = 1m, Options = "Pearl, client note" },
                new() { ToppingId = topping.Id, ProductName = "Cookie", Quantity = 2, UnitPrice = 1m }
            }
        });

        Assert.Equal(32_000m, order.Details.Single(d => d.ProductId == product.Id).UnitPrice);
        Assert.Equal(4_000m, order.Details.Single(d => d.ToppingId == topping.Id).UnitPrice);
        Assert.Equal(40_000m, order.TotalAmount);
    }

    [Fact]
    public async Task CreateOrder_FallbackToGlobalSettings()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;
        var branch = Guid.NewGuid();
        var product = new Product { Id = Guid.NewGuid(), Name = "Coffee", Price = 100_000m };
        context.Products.Add(product);

        // Set Global settings (BranchId = null)
        var settingService = new SystemSettingService(context);
        await settingService.UpdateSettingsAsync(null, new Dictionary<string, string>
        {
            ["vatPercent"] = "10",
            ["vatEnabled"] = "true"
        });
        await context.SaveChangesAsync();

        var order = await new OrderService(context).CreateOrUpdateOrderAsync(CreateOrder(branch, product.Id));

        Assert.Equal(10m, order.VatPercent);
        Assert.Equal(10_000m, order.VatAmount);
        Assert.Equal(110_000m, order.TotalAmount);
    }

    private static Order CreateOrder(Guid branchId, Guid productId) => new()
    {
        BranchId = branchId,
        TableName = "Mang về",
        Status = "Đang xử lý",
        Details = new List<OrderDetail>
        {
            new() { ProductId = productId, ProductName = "client name", Quantity = 1, UnitPrice = 1m }
        }
    };

    private static async Task SetSettings(ApplicationDbContext context, Guid branchId, decimal vatPercent, decimal serviceFeePercent)
    {
        var service = new SystemSettingService(context);
        await service.UpdateSettingsAsync(branchId, new Dictionary<string, string>
        {
            ["vatPercent"] = vatPercent.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["serviceFee"] = serviceFeePercent.ToString(System.Globalization.CultureInfo.InvariantCulture)
        });
    }
}
