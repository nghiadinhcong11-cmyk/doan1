using Moq;
using RestaurantPOS.Application.Common.Interfaces;
using RestaurantPOS.Application.Services;
using RestaurantPOS.Domain.Entities;
using System.Reflection;

namespace RestaurantPOS.Tests;

public sealed class KitchenServiceTests
{
    private const string InProgress = "Đang xử lý";

    [Fact]
    public async Task SendToKitchenAsync_CreatesRequestForOnlyUnsentQuantityAndNotifies()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;
        var notifier = new Mock<IKitchenNotifier>();
        var order = new Order
        {
            Id = Guid.NewGuid(), TableName = "Table 1", Status = "Đang xử lý",
            Details = new List<OrderDetail> { new() { Id = Guid.NewGuid(), ProductName = "Coffee", Quantity = 3, SentQuantity = 1, UnitPrice = 3 } }
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var result = await new KitchenService(context, notifier.Object).SendToKitchenAsync(order.Id);

        var item = Assert.Single(result.Items);
        Assert.Equal(2, item.Quantity);
        Assert.Equal(3, (await context.OrderDetails.SingleAsync()).SentQuantity);
        notifier.Verify(x => x.NotifyNewOrderRequestAsync(It.IsAny<object>()), Times.Once);
    }

    [Fact]
    public async Task SendToKitchenAsync_ThrowsWhenOrderIsNotPending()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;
        var notifier = new Mock<IKitchenNotifier>();
        var order = new Order { Id = Guid.NewGuid(), Status = "Completed", Details = new List<OrderDetail> { new() { ProductName = "Coffee", Quantity = 1 } } };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(() => new KitchenService(context, notifier.Object).SendToKitchenAsync(order.Id));
        notifier.Verify(x => x.NotifyNewOrderRequestAsync(It.IsAny<object>()), Times.Never);
    }

    [Fact]
    public async Task UpdateRequestStatusAsync_UpdatesAndNotifies()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;
        var notifier = new Mock<IKitchenNotifier>();
        var order = new Order { Id = Guid.NewGuid(), Status = "Đang xử lý" };
        var request = new OrderRequest { Id = Guid.NewGuid(), OrderId = order.Id, Status = "Pending" };
        context.Orders.Add(order);
        context.OrderRequests.Add(request);
        await context.SaveChangesAsync();

        var result = await new KitchenService(context, notifier.Object).UpdateRequestStatusAsync(request.Id, "Ready");

        Assert.Equal("Ready", result!.Status);
        notifier.Verify(x => x.NotifyRequestStatusUpdatedAsync(request.Id, "Ready"), Times.Once);
    }

    [Fact]
    public async Task SendToKitchenAsync_ThrowsForMissingOrderWithoutPersistingOrNotifying()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;
        var notifier = new Mock<IKitchenNotifier>();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new KitchenService(context, notifier.Object).SendToKitchenAsync(Guid.NewGuid()));

        Assert.Empty(await context.OrderRequests.ToListAsync());
        notifier.Verify(x => x.NotifyNewOrderRequestAsync(It.IsAny<object>()), Times.Never);
    }

    [Theory]
    [InlineData("Completed")]
    [InlineData("Cancelled")]
    [InlineData("Draft")]
    public async Task SendToKitchenAsync_RejectsEveryNonInProgressStatus(string status)
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;
        var notifier = new Mock<IKitchenNotifier>();
        var order = new Order
        {
            Id = Guid.NewGuid(), Status = status,
            Details = new List<OrderDetail> { new() { ProductName = "Meal", Quantity = 2, SentQuantity = 0 } }
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new KitchenService(context, notifier.Object).SendToKitchenAsync(order.Id));

        Assert.Empty(await context.OrderRequests.ToListAsync());
        Assert.Equal(0, (await context.OrderDetails.SingleAsync()).SentQuantity);
        notifier.Verify(x => x.NotifyNewOrderRequestAsync(It.IsAny<object>()), Times.Never);
    }

    [Fact]
    public async Task SendToKitchenAsync_ThrowsWhenAllQuantitiesHaveAlreadyBeenSent()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;
        var notifier = new Mock<IKitchenNotifier>();
        var order = new Order
        {
            Id = Guid.NewGuid(), Status = InProgress,
            Details = new List<OrderDetail> { new() { ProductName = "Meal", Quantity = 5, SentQuantity = 5 } }
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new KitchenService(context, notifier.Object).SendToKitchenAsync(order.Id));

        Assert.Empty(await context.OrderRequests.ToListAsync());
        Assert.Equal(5, (await context.OrderDetails.SingleAsync()).SentQuantity);
        notifier.Verify(x => x.NotifyNewOrderRequestAsync(It.IsAny<object>()), Times.Never);
    }

    [Fact]
    public async Task SendToKitchenAsync_FirstRequestSendsAllFiveAndUsesRequestNumberOne()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;
        var notifier = new Mock<IKitchenNotifier>();
        var order = new Order
        {
            Id = Guid.NewGuid(), Status = InProgress,
            Details = new List<OrderDetail> { new() { ProductName = "Meal", Quantity = 5, SentQuantity = 0 } }
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var result = await new KitchenService(context, notifier.Object).SendToKitchenAsync(order.Id);

        Assert.Equal(1, result.RequestNumber);
        Assert.Equal(5, Assert.Single(result.Items).Quantity);
        Assert.Equal(5, (await context.OrderDetails.SingleAsync()).SentQuantity);
        Assert.Equal(1, await context.OrderRequests.CountAsync());
        notifier.Verify(x => x.NotifyNewOrderRequestAsync(It.IsAny<object>()), Times.Once);
    }

    [Fact]
    public async Task SendToKitchenAsync_SecondRequestSendsOnlyUnsentQuantityAndIncrementsNumber()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;
        var notifier = new Mock<IKitchenNotifier>();
        var order = new Order
        {
            Id = Guid.NewGuid(), Status = InProgress,
            Details = new List<OrderDetail> { new() { ProductName = "Meal", Quantity = 5, SentQuantity = 3 } }
        };
        context.Orders.Add(order);
        context.OrderRequests.Add(new OrderRequest { Id = Guid.NewGuid(), OrderId = order.Id, RequestNumber = 4, Status = "Ready" });
        await context.SaveChangesAsync();

        var result = await new KitchenService(context, notifier.Object).SendToKitchenAsync(order.Id);

        Assert.Equal(5, result.RequestNumber);
        Assert.Equal(2, Assert.Single(result.Items).Quantity);
        Assert.Equal(5, (await context.OrderDetails.SingleAsync()).SentQuantity);
        Assert.Equal(2, await context.OrderRequests.CountAsync());
        notifier.Verify(x => x.NotifyNewOrderRequestAsync(It.IsAny<object>()), Times.Once);
    }

    [Fact]
    public async Task SendToKitchenAsync_ProcessesMultipleDetailsIndependently()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;
        var notifier = new Mock<IKitchenNotifier>();
        var order = new Order
        {
            Id = Guid.NewGuid(), Status = InProgress,
            Details = new List<OrderDetail>
            {
                new() { ProductName = "Burger", Quantity = 2, SentQuantity = 0, Options = "burger" },
                new() { ProductName = "Pizza", Quantity = 3, SentQuantity = 0, Options = "pizza" },
                new() { ProductName = "Drink", Quantity = 4, SentQuantity = 0, Options = "drink" }
            }
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var result = await new KitchenService(context, notifier.Object).SendToKitchenAsync(order.Id);

        Assert.Equal(3, result.Items.Count);
        Assert.Equal(new Dictionary<string, int> { ["Burger"] = 2, ["Pizza"] = 3, ["Drink"] = 4 },
            result.Items.ToDictionary(i => i.ProductName, i => i.Quantity));
        Assert.All(await context.OrderDetails.ToListAsync(), d => Assert.Equal(d.Quantity, d.SentQuantity));
        notifier.Verify(x => x.NotifyNewOrderRequestAsync(It.IsAny<object>()), Times.Once);
    }

    [Fact]
    public async Task SendToKitchenAsync_SendsOnlyPartiallyUnsentDetails()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;
        var notifier = new Mock<IKitchenNotifier>();
        var order = new Order
        {
            Id = Guid.NewGuid(), Status = InProgress,
            Details = new List<OrderDetail>
            {
                new() { ProductName = "Burger", Quantity = 5, SentQuantity = 2 },
                new() { ProductName = "Pizza", Quantity = 3, SentQuantity = 3 }
            }
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var result = await new KitchenService(context, notifier.Object).SendToKitchenAsync(order.Id);

        var item = Assert.Single(result.Items);
        Assert.Equal("Burger", item.ProductName);
        Assert.Equal(3, item.Quantity);
        var details = await context.OrderDetails.OrderBy(d => d.ProductName).ToListAsync();
        Assert.Equal(new[] { 5, 3 }, details.Select(d => d.SentQuantity));
        notifier.Verify(x => x.NotifyNewOrderRequestAsync(It.IsAny<object>()), Times.Once);
    }

    [Fact]
    public async Task UpdateRequestStatusAsync_ReturnsNullAndDoesNotNotifyForMissingRequest()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;
        var notifier = new Mock<IKitchenNotifier>();

        var result = await new KitchenService(context, notifier.Object)
            .UpdateRequestStatusAsync(Guid.NewGuid(), "Ready");

        Assert.Null(result);
        Assert.Empty(await context.OrderRequests.ToListAsync());
        notifier.Verify(x => x.NotifyRequestStatusUpdatedAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetActiveKitchenRequestsAsync_ExcludesCompletedAndCancelledRequests()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;
        var order = new Order { Id = Guid.NewGuid(), TableName = "Table 1", Status = InProgress };
        context.Orders.Add(order);
        context.OrderRequests.AddRange(
            new OrderRequest { Id = Guid.NewGuid(), OrderId = order.Id, RequestNumber = 1, Status = "Pending" },
            new OrderRequest { Id = Guid.NewGuid(), OrderId = order.Id, RequestNumber = 2, Status = "Preparing" },
            new OrderRequest { Id = Guid.NewGuid(), OrderId = order.Id, RequestNumber = 3, Status = "Completed" },
            new OrderRequest { Id = Guid.NewGuid(), OrderId = order.Id, RequestNumber = 4, Status = "Cancelled" });
        await context.SaveChangesAsync();

        var result = await new KitchenService(context, Mock.Of<IKitchenNotifier>()).GetActiveKitchenRequestsAsync(null);

        Assert.Equal(2, result.Count);
        Assert.DoesNotContain(result, x => (string)x.GetType().GetProperty("Status")!.GetValue(x)! == "Completed");
        Assert.DoesNotContain(result, x => (string)x.GetType().GetProperty("Status")!.GetValue(x)! == "Cancelled");
    }

    [Fact]
    public async Task GetActiveKitchenRequestsAsync_FiltersByBranch()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;
        var branch = Guid.NewGuid();
        var otherBranch = Guid.NewGuid();
        var matchingOrder = new Order { Id = Guid.NewGuid(), BranchId = branch, Status = InProgress };
        var otherOrder = new Order { Id = Guid.NewGuid(), BranchId = otherBranch, Status = InProgress };
        context.Orders.AddRange(matchingOrder, otherOrder);
        context.OrderRequests.AddRange(
            new OrderRequest { Id = Guid.NewGuid(), OrderId = matchingOrder.Id, RequestNumber = 1, Status = "Pending" },
            new OrderRequest { Id = Guid.NewGuid(), OrderId = otherOrder.Id, RequestNumber = 1, Status = "Pending" });
        await context.SaveChangesAsync();

        var result = await new KitchenService(context, Mock.Of<IKitchenNotifier>()).GetActiveKitchenRequestsAsync(branch);

        var orderIds = result.Select(x => (Guid)x.GetType().GetProperty("OrderId")!.GetValue(x)!).ToList();
        Assert.Single(orderIds);
        Assert.Equal(matchingOrder.Id, orderIds[0]);
    }
}
