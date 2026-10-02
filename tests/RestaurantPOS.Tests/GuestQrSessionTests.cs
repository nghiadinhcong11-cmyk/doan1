using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RestaurantPOS.Application.Common.Interfaces;
using RestaurantPOS.Application.Common.Security;
using RestaurantPOS.Application.Services;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.WebAPI.Controllers;

namespace RestaurantPOS.Tests;

public sealed class GuestQrSessionTests
{
    [Fact]
    public async Task Bootstrap_resolves_table_and_branch_from_the_qr_token()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;
        var branchId = Guid.NewGuid();
        context.Branches.Add(new Branch { Id = branchId, Name = "Branch A" });
        await context.SaveChangesAsync();
        var table = new RestaurantTable
        {
            Id = Guid.NewGuid(), Name = "A1", AreaName = "Main", BranchId = branchId,
            BranchName = "Branch A", QrToken = QrTokenGenerator.Generate(), IsActive = true
        };
        context.Tables.Add(table);
        await context.SaveChangesAsync();

        var jwt = new Mock<IJwtService>();
        jwt.Setup(service => service.GenerateToken(
                It.IsAny<Guid>(), "guest", "Khách tại bàn", "customer", branchId, null, null, "guest", table.Id))
            .Returns("guest-jwt");
        var controller = new GuestController(context, jwt.Object);

        var result = await controller.Bootstrap(new GuestController.BootstrapRequest { QrToken = table.QrToken });

        Assert.IsType<OkObjectResult>(result);
        jwt.VerifyAll();
    }

    [Fact]
    public async Task Bootstrap_rejects_malformed_and_unknown_tokens_without_issuing_a_jwt()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;
        var jwt = new Mock<IJwtService>();
        var controller = new GuestController(context, jwt.Object);

        Assert.IsType<BadRequestObjectResult>(await controller.Bootstrap(new GuestController.BootstrapRequest { QrToken = "not-a-token" }));
        Assert.IsType<NotFoundObjectResult>(await controller.Bootstrap(new GuestController.BootstrapRequest { QrToken = QrTokenGenerator.Generate() }));
        jwt.VerifyNoOtherCalls();
    }

    [Fact]
    public void Guest_context_fails_closed_for_missing_or_malformed_claims()
    {
        var missingTable = Principal(("customerSessionType", "guest"), ("branchId", Guid.NewGuid().ToString()));
        var malformedBranch = Principal(("customerSessionType", "guest"), ("tableId", Guid.NewGuid().ToString()), ("branchId", "not-a-guid"));

        Assert.False(GuestSessionContext.TryCreate(missingTable, out _));
        Assert.False(GuestSessionContext.TryCreate(malformedBranch, out _));
    }

    [Fact]
    public async Task Guest_order_creation_overwrites_injected_table_and_branch()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;
        var branchA = Guid.NewGuid();
        var branchB = Guid.NewGuid();
        context.Branches.AddRange(new Branch { Id = branchA, Name = "Branch A" }, new Branch { Id = branchB, Name = "Branch B" });
        await context.SaveChangesAsync();
        var tableA = new RestaurantTable { Id = Guid.NewGuid(), Name = "A1", AreaName = "Main", BranchId = branchA, QrToken = QrTokenGenerator.Generate() };
        var tableB = new RestaurantTable { Id = Guid.NewGuid(), Name = "B1", AreaName = "Main", BranchId = branchB, QrToken = QrTokenGenerator.Generate() };
        context.Tables.AddRange(tableA, tableB);
        await context.SaveChangesAsync();

        var orders = new Mock<IOrderService>();
        orders.Setup(service => service.CreateOrUpdateOrderAsync(It.IsAny<Order>())).ReturnsAsync((Order order) => order);
        var controller = CreateOrderController(orders.Object, context, tableA.Id, branchA);
        var request = new Order
        {
            TableName = tableB.Name, BranchId = branchB,
            Details = new List<OrderDetail> { new() { ProductName = "Item", Quantity = 1 } }
        };

        var result = await controller.CreateOrder(request);

        Assert.IsType<OkObjectResult>(result);
        orders.Verify(service => service.CreateOrUpdateOrderAsync(It.Is<Order>(order =>
            order.TableName == tableA.Name && order.BranchId == branchA)), Times.Once);
    }

    [Fact]
    public async Task Guest_cannot_read_an_order_for_another_table_or_branch()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;
        var branchA = Guid.NewGuid();
        var branchB = Guid.NewGuid();
        context.Branches.AddRange(new Branch { Id = branchA, Name = "Branch A" }, new Branch { Id = branchB, Name = "Branch B" });
        await context.SaveChangesAsync();
        var tableA = new RestaurantTable { Id = Guid.NewGuid(), Name = "A1", AreaName = "Main", BranchId = branchA, QrToken = QrTokenGenerator.Generate() };
        context.Tables.Add(tableA);
        await context.SaveChangesAsync();
        var foreignOrder = new Order { Id = Guid.NewGuid(), TableName = "B1", BranchId = branchB };
        var orders = new Mock<IOrderService>();
        orders.Setup(service => service.GetOrderByIdAsync(foreignOrder.Id)).ReturnsAsync(foreignOrder);
        var controller = CreateOrderController(orders.Object, context, tableA.Id, branchA);

        Assert.IsType<ForbidResult>(await controller.GetOrder(foreignOrder.Id));
    }

    private static OrderController CreateOrderController(IOrderService orders, Infrastructure.Persistence.ApplicationDbContext context, Guid tableId, Guid branchId)
    {
        var controller = new OrderController(orders, Mock.Of<IKitchenService>(), Mock.Of<ILoyaltyService>(), context)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = Principal(
                    ("customerSessionType", "guest"),
                    ("tableId", tableId.ToString()),
                    ("branchId", branchId.ToString())) }
            }
        };
        return controller;
    }

    private static ClaimsPrincipal Principal(params (string Type, string Value)[] claims) =>
        new(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "customer") }
            .Concat(claims.Select(claim => new Claim(claim.Type, claim.Value))), "Bearer"));
}
