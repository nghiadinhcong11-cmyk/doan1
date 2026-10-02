using RestaurantPOS.Application.DTOs.Orders;
using RestaurantPOS.Application.Common.Security;
using RestaurantPOS.Application.Services;
using RestaurantPOS.Domain.Entities;

namespace RestaurantPOS.Tests;

public sealed class OrderServiceTests
{
    private const string InProgress = "Đang xử lý";
    private const string TakeAway = "Mang về";

    [Fact]
    public async Task CreateOrUpdateOrderAsync_CreatesOrderAndInitializesDetails()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;
        var service = new OrderService(context);
        var order = new Order
        {
            TableName = "Table 1",
            Status = "Pending",
            Details = new List<OrderDetail>
            {
                new() { ProductName = "Coffee", Quantity = 2, SentQuantity = 99, UnitPrice = 3 }
            }
        };

        var result = await service.CreateOrUpdateOrderAsync(order);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.NotEqual(Guid.Empty, result.Details[0].Id);
        Assert.Equal(0, result.Details[0].SentQuantity);
        Assert.NotNull(result.InvoiceCode);
        Assert.Same(result, await service.GetOrderByIdAsync(result.Id));
    }

    [Fact]
    public async Task GetOrdersAsync_AppliesStatusFilterAndEnrichesCustomer()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;
        var phone = "0900000000";
        context.Customers.Add(new Customer { Id = Guid.NewGuid(), PhoneNumber = phone, FullName = "Alice", Email = "alice@example.com" });
        context.Orders.AddRange(
            new Order { Id = Guid.NewGuid(), InvoiceCode = "A", CustomerPhone = phone, Status = "Completed", CreatedAt = DateTime.UtcNow },
            new Order { Id = Guid.NewGuid(), InvoiceCode = "B", Status = "Cancelled", CreatedAt = DateTime.UtcNow });
        await context.SaveChangesAsync();

        var result = await new OrderService(context).GetOrdersAsync(new OrderQueryFilter { Status = "Completed" });

        var order = Assert.Single(result);
        Assert.Equal("Alice", order.CustomerName);
        Assert.Equal("alice@example.com", order.CustomerEmail);
    }

    [Fact]
    public async Task UpdateOrderStatusByCodeAsync_RejectsEmployeeFromAnotherBranch()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;
        var branch = Guid.NewGuid();
        context.Orders.Add(new Order { Id = Guid.NewGuid(), InvoiceCode = "HD-1", BranchId = branch, Status = "Pending" });
        await context.SaveChangesAsync();

        var result = await new OrderService(context).UpdateOrderStatusByCodeAsync("HD-1", "Completed", Guid.NewGuid(), "employee");

        Assert.False(result.Success);
        Assert.Equal("Pending", (await context.Orders.SingleAsync()).Status);
    }

    [Fact]
    public async Task GetOrderByIdAsync_UpdateOrderStatusAsync_AndDeleteOrderAsync_ReturnExpectedResultsForMissingOrder()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;
        var service = new OrderService(context);
        var missingId = Guid.NewGuid();

        Assert.Null(await service.GetOrderByIdAsync(missingId));
        Assert.Null(await service.UpdateOrderStatusAsync(missingId, new OrderUpdateDto { Status = "Completed" }));
        Assert.False(await service.DeleteOrderAsync(missingId));
    }

    [Fact]
    public async Task UpdateOrderStatusByCodeAsync_AllowsEmployeeFromSameBranch()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;
        var branch = Guid.NewGuid();
        context.Orders.Add(new Order { Id = Guid.NewGuid(), InvoiceCode = "HD-SAME", BranchId = branch, Status = "Pending" });
        await context.SaveChangesAsync();

        var result = await new OrderService(context).UpdateOrderStatusByCodeAsync("HD-SAME", "Completed", branch, "employee");

        Assert.True(result.Success);
        Assert.Equal("Completed", (await context.Orders.SingleAsync()).Status);
    }

    [Fact]
    public async Task UpdateOrderStatusByCodeAsync_AllowsAdminAcrossBranches()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;
        context.Orders.Add(new Order { Id = Guid.NewGuid(), InvoiceCode = "HD-ADMIN", BranchId = Guid.NewGuid(), Status = "Pending" });
        await context.SaveChangesAsync();

        var result = await new OrderService(context).UpdateOrderStatusByCodeAsync("HD-ADMIN", "Completed", Guid.NewGuid(), "admin");

        Assert.True(result.Success);
        Assert.Equal("Completed", (await context.Orders.SingleAsync()).Status);
    }

    [Fact]
    public async Task GetOrdersAsync_SupportsMultipleStatusBranchTableCustomerAndDateFilters()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;
        var branch = Guid.NewGuid();
        var phone = "0900111222";
        var today = DateTime.UtcNow.Date;
        context.Orders.AddRange(
            new Order { Id = Guid.NewGuid(), InvoiceCode = "MATCH", BranchId = branch, TableName = "Table 7", CustomerPhone = phone, Status = "Pending", CreatedAt = today.AddHours(8) },
            new Order { Id = Guid.NewGuid(), InvoiceCode = "WRONG-BRANCH", BranchId = Guid.NewGuid(), TableName = "Table 7", CustomerPhone = phone, Status = "Completed", CreatedAt = today.AddHours(9) },
            new Order { Id = Guid.NewGuid(), InvoiceCode = "WRONG-DATE", BranchId = branch, TableName = "Table 7", CustomerPhone = phone, Status = "Completed", CreatedAt = today.AddDays(-1).AddHours(9) });
        await context.SaveChangesAsync();

        var result = await new OrderService(context).GetOrdersAsync(new OrderQueryFilter
        {
            Status = "Pending, Completed",
            BranchId = branch,
            TableName = "Table 7",
            CustomerPhone = phone,
            FromDate = today.ToString("yyyy-MM-dd"),
            ToDate = today.ToString("yyyy-MM-dd")
        });

        var order = Assert.Single(result);
        Assert.Equal("MATCH", order.InvoiceCode);
    }

    [Fact]
    public async Task CreateOrUpdateOrderAsync_MergesExistingOpenTableOrderAndRemovesUnsentDetail()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;
        var branch = Guid.NewGuid();
        var existing = new Order
        {
            Id = Guid.NewGuid(), TableName = "Table 2", BranchId = branch, BranchName = "Main", Status = InProgress,
            Details = new List<OrderDetail>
            {
                new() { Id = Guid.NewGuid(), ProductName = "Unsent", Quantity = 2, SentQuantity = 0, UnitPrice = 10, Options = "A" },
                new() { Id = Guid.NewGuid(), ProductName = "Sent", Quantity = 5, SentQuantity = 3, UnitPrice = 20, Options = "B" }
            }
        };
        context.Orders.Add(existing);
        await context.SaveChangesAsync();
        await using var serviceContext = TestDbContextFactory.CreateContext(connection);

        var result = await new OrderService(serviceContext).CreateOrUpdateOrderAsync(new Order
        {
            TableName = "Table 2", BranchId = branch, Status = "Completed", TotalAmount = 100,
            Details = new List<OrderDetail>
            {
                new() { ProductName = "Sent", Quantity = 1, UnitPrice = 25, Options = "B" }
            }
        });

        Assert.Equal(existing.Id, result.Id);
        Assert.Equal(InProgress, result.Status);
        Assert.Single(result.Details);
        var sent = Assert.Single(result.Details, d => d.Options == "B");
        Assert.Equal(3, sent.Quantity);
        Assert.Equal(0, sent.UnitPrice);
        Assert.DoesNotContain(result.Details, d => d.Options == "A");
    }

    [Fact]
    public async Task CreateOrUpdateOrderAsync_DoesNotDeleteOrReduceAlreadySentDetail()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;
        var existing = new Order
        {
            Id = Guid.NewGuid(), TableName = "Table 3", BranchId = Guid.NewGuid(), Status = InProgress,
            Details = new List<OrderDetail> { new() { Id = Guid.NewGuid(), ProductName = "Sent", Quantity = 5, SentQuantity = 3, UnitPrice = 20, Options = "S" } }
        };
        context.Orders.Add(existing);
        await context.SaveChangesAsync();
        await using var serviceContext = TestDbContextFactory.CreateContext(connection);

        var result = await new OrderService(serviceContext).CreateOrUpdateOrderAsync(new Order
        {
            TableName = "Table 3", BranchId = existing.BranchId, Status = InProgress,
            Details = new List<OrderDetail>()
        });

        var detail = Assert.Single(result.Details);
        Assert.Equal(3, detail.Quantity);
        Assert.Equal(3, detail.SentQuantity);
    }

    [Fact]
    public async Task CreateOrUpdateOrderAsync_ClampsQuantityToSentQuantityWhenIncomingQuantityIsLower()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;
        var branch = Guid.NewGuid();
        var existing = new Order
        {
            Id = Guid.NewGuid(), TableName = "Table 4", BranchId = branch, Status = InProgress,
            Details = new List<OrderDetail> { new() { Id = Guid.NewGuid(), ProductName = "Sent", Quantity = 5, SentQuantity = 4, UnitPrice = 20, Options = "S" } }
        };
        context.Orders.Add(existing);
        await context.SaveChangesAsync();
        await using var serviceContext = TestDbContextFactory.CreateContext(connection);

        var result = await new OrderService(serviceContext).CreateOrUpdateOrderAsync(new Order
        {
            TableName = "Table 4", BranchId = branch, Status = InProgress,
            Details = new List<OrderDetail> { new() { ProductName = "Sent", Quantity = 1, UnitPrice = 22, Options = "S" } }
        });

        var detail = Assert.Single(result.Details);
        Assert.Equal(4, detail.Quantity);
        Assert.Equal(4, detail.SentQuantity);
        Assert.Equal(0, detail.UnitPrice);
    }

    [Fact]
    public async Task CreateOrUpdateOrderAsync_TakeAwayOrderDoesNotMergeExistingOrder()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;
        var branch = Guid.NewGuid();
        context.Orders.Add(new Order { Id = Guid.NewGuid(), TableName = TakeAway, BranchId = branch, Status = InProgress });
        await context.SaveChangesAsync();

        var result = await new OrderService(context).CreateOrUpdateOrderAsync(new Order
        {
            TableName = TakeAway, BranchId = branch, Status = "Completed"
        });

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(2, await context.Orders.CountAsync());
    }

    [Fact]
    public async Task CreateOrUpdateOrderAsync_AddsOnlyNewItemsAfterExistingItemsWereSent()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;
        var branch = Guid.NewGuid();
        var existing = new Order
        {
            Id = Guid.NewGuid(), TableName = "Table batch", BranchId = branch, Status = InProgress,
            Details = new List<OrderDetail>
            {
                new() { Id = Guid.NewGuid(), ProductName = "Coffee", Quantity = 2, SentQuantity = 2, UnitPrice = 3, Options = "coffee" },
                new() { Id = Guid.NewGuid(), ProductName = "Tea", Quantity = 1, SentQuantity = 1, UnitPrice = 2, Options = "tea" }
            }
        };
        context.Orders.Add(existing);
        await context.SaveChangesAsync();
        await using var serviceContext = TestDbContextFactory.CreateContext(connection);

        var result = await new OrderService(serviceContext).CreateOrUpdateOrderAsync(new Order
        {
            TableName = existing.TableName, BranchId = branch, Status = InProgress,
            Details = new List<OrderDetail>
            {
                new() { ProductName = "Coffee", Quantity = 2, UnitPrice = 3, Options = "coffee" },
                new() { ProductName = "Tea", Quantity = 1, UnitPrice = 2, Options = "tea" },
                new() { ProductName = "Cake", Quantity = 1, UnitPrice = 4, Options = "cake" },
                new() { ProductName = "Juice", Quantity = 2, UnitPrice = 5, Options = "juice" }
            }
        });

        Assert.Equal(4, result.Details.Count);
        Assert.Equal(2, result.Details.Single(d => d.Options == "coffee").SentQuantity);
        Assert.Equal(1, result.Details.Single(d => d.Options == "tea").SentQuantity);
        Assert.All(result.Details.Where(d => d.Options == "cake" || d.Options == "juice"), d => Assert.Equal(0, d.SentQuantity));
    }

    [Fact]
    public async Task AcceptWebOrder_ReusesTheSameOrderAndPreservesDetails()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;
        var branch = Guid.NewGuid();
        var table = new RestaurantTable { Id = Guid.NewGuid(), BranchId = branch, Name = "Table Accept", AreaName = "Main", Status = "Trống", QrToken = QrTokenGenerator.Generate() };
        context.Branches.Add(new Branch { Id = branch, Name = "Main" });
        var order = new Order
        {
            Id = Guid.NewGuid(), BranchId = branch, TableName = "Bàn QR", Status = InProgress,
            CustomerName = "Web guest", TotalAmount = 50,
            Details = new List<OrderDetail> { new() { ProductName = "Coffee", Quantity = 2, UnitPrice = 25, Options = "Less ice" } }
        };
        context.Tables.Add(table);
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var result = await new OrderService(context).AcceptWebOrderAsync(order.Id, table.Id, branch, "Cashier");

        Assert.Equal(WebOrderAcceptResult.Accepted, result.Result);
        Assert.Equal(order.Id, result.Order!.Id);
        Assert.Equal(table.Name, result.Order.TableName);
        Assert.Equal("Cashier", result.Order.CreatedBy);
        Assert.Single(result.Order.Details);
        context.ChangeTracker.Clear();
        Assert.Equal(1, await context.Orders.CountAsync());
        Assert.Equal("Có khách", (await context.Tables.SingleAsync()).Status);
    }

    [Fact]
    public async Task AcceptWebOrder_SecondAttemptReturnsConflictWithoutCreatingOrder()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;
        var branch = Guid.NewGuid();
        var order = new Order { Id = Guid.NewGuid(), BranchId = branch, TableName = TakeAway, Status = InProgress, TotalAmount = 20 };
        context.Orders.Add(order);
        await context.SaveChangesAsync();
        var service = new OrderService(context);

        var first = await service.AcceptWebOrderAsync(order.Id, null, branch, "Admin");
        var second = await service.AcceptWebOrderAsync(order.Id, null, branch, "Cashier");

        Assert.Equal(WebOrderAcceptResult.Accepted, first.Result);
        Assert.Equal(WebOrderAcceptResult.Conflict, second.Result);
        context.ChangeTracker.Clear();
        Assert.Equal(1, await context.Orders.CountAsync());
        Assert.Equal("Admin", (await context.Orders.SingleAsync()).CreatedBy);
    }

    [Fact]
    public async Task AcceptWebOrder_ConcurrentAttemptsAllowExactlyOneSuccess()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;
        var branch = Guid.NewGuid();
        var order = new Order { Id = Guid.NewGuid(), BranchId = branch, TableName = TakeAway, Status = InProgress };
        context.Orders.Add(order);
        await context.SaveChangesAsync();
        await using var firstContext = TestDbContextFactory.CreateContext(connection);
        await using var secondContext = TestDbContextFactory.CreateContext(connection);

        var results = await Task.WhenAll(
            new OrderService(firstContext).AcceptWebOrderAsync(order.Id, null, branch, "Admin"),
            new OrderService(secondContext).AcceptWebOrderAsync(order.Id, null, branch, "Cashier"));

        Assert.Single(results, result => result.Result == WebOrderAcceptResult.Accepted);
        Assert.Single(results, result => result.Result == WebOrderAcceptResult.Conflict);
        context.ChangeTracker.Clear();
        Assert.Equal(1, await context.Orders.CountAsync());
        Assert.NotNull((await context.Orders.SingleAsync()).CreatedBy);
    }

    [Fact]
    public async Task AcceptWebOrder_RejectsWrongBranchWithoutChangingOrder()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;
        var ownerBranch = Guid.NewGuid();
        var otherBranch = Guid.NewGuid();
        var order = new Order { Id = Guid.NewGuid(), BranchId = ownerBranch, TableName = TakeAway, Status = InProgress };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var result = await new OrderService(context).AcceptWebOrderAsync(order.Id, null, otherBranch, "Cashier");

        Assert.Equal(WebOrderAcceptResult.Forbidden, result.Result);
        context.ChangeTracker.Clear();
        var persisted = await context.Orders.SingleAsync();
        Assert.Equal(InProgress, persisted.Status);
        Assert.Null(persisted.CreatedBy);
    }

    [Fact]
    public async Task PayOrderAsync_PaysAnUnpaidOrderOnlyOnce()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;
        var order = new Order { Id = Guid.NewGuid(), BranchId = Guid.NewGuid(), Status = InProgress, TotalAmount = 100m, PaidAmount = 0m };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var service = new OrderService(context);
        var first = await service.PayOrderAsync(order.Id, 100m, "Tiền mặt");
        var second = await service.PayOrderAsync(order.Id, 100m, "Tiền mặt");

        Assert.Equal(PaymentAttemptResult.Paid, first.Result);
        Assert.Equal(PaymentAttemptResult.AlreadyPaid, second.Result);
        context.ChangeTracker.Clear();
        var persisted = await context.Orders.SingleAsync();
        Assert.Equal(100m, persisted.PaidAmount);
        Assert.Equal("Tiền mặt", persisted.PaymentMethod);
        Assert.Equal("Hoàn thành", persisted.Status);
    }

    [Fact]
    public async Task PayOrderAsync_ConcurrentAttemptsAllowExactlyOneSuccess()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;
        var order = new Order { Id = Guid.NewGuid(), BranchId = Guid.NewGuid(), Status = InProgress, TotalAmount = 100m, PaidAmount = 0m };
        context.Orders.Add(order);
        await context.SaveChangesAsync();
        await using var firstContext = TestDbContextFactory.CreateContext(connection);
        await using var secondContext = TestDbContextFactory.CreateContext(connection);

        var results = await Task.WhenAll(
            new OrderService(firstContext).PayOrderAsync(order.Id, 100m, "Tiền mặt"),
            new OrderService(secondContext).PayOrderAsync(order.Id, 100m, "Tiền mặt"));

        Assert.Single(results, r => r.Result == PaymentAttemptResult.Paid);
        Assert.Single(results, r => r.Result == PaymentAttemptResult.AlreadyPaid);
        context.ChangeTracker.Clear();
        var persisted = await context.Orders.SingleAsync();
        Assert.Equal(100m, persisted.PaidAmount);
        Assert.Equal("Hoàn thành", persisted.Status);
    }

    [Theory]
    [InlineData("Completed")]
    [InlineData("Cancelled")]
    public async Task TerminalOrderCannotBeChangedOrDeleted(string status)
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;
        var order = new Order { Id = Guid.NewGuid(), Status = status };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var service = new OrderService(context);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UpdateOrderStatusAsync(order.Id, new OrderUpdateDto { Status = "Pending" }));
        Assert.False(await service.DeleteOrderAsync(order.Id));
        Assert.NotNull(await context.Orders.FindAsync(order.Id));
    }
}
