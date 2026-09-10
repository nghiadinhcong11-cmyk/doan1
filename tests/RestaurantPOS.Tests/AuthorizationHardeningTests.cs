using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RestaurantPOS.Application.DTOs.Orders;
using RestaurantPOS.Application.Services;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.WebAPI.Controllers;

namespace RestaurantPOS.Tests;

public sealed class AuthorizationHardeningTests
{
    [Fact]
    public void Protected_endpoints_declare_expected_role_requirements()
    {
        var dashboard = typeof(DashboardController).GetMethod(nameof(DashboardController.GetSummary));
        var orderStatus = typeof(OrderController).GetMethod(nameof(OrderController.UpdateStatus));
        var kitchenRequests = typeof(OrderController).GetMethod(nameof(OrderController.GetActiveKitchenRequests));

        Assert.Equal("admin,manager", GetAuthorizeRoles(dashboard!));
        Assert.Equal("admin,manager,employee,cashier,kitchen", GetAuthorizeRoles(orderStatus!));
        Assert.Equal("admin,manager,kitchen", GetAuthorizeRoles(kitchenRequests!));
    }

    [Fact]
    public void Ai_chat_no_longer_accepts_a_role_query_parameter()
    {
        var method = typeof(RestaurantPOS.AI.Controllers.AiController).GetMethod("Chat");

        Assert.NotNull(method);
        Assert.DoesNotContain(method!.GetParameters(), parameter => parameter.Name == "role");
    }

    [Fact]
    public async Task Employee_branch_a_cannot_bypass_scope_with_branch_b_filter()
    {
        var orderService = new Mock<IOrderService>();
        orderService.Setup(x => x.GetOrdersAsync(It.IsAny<OrderQueryFilter>()))
            .ReturnsAsync(new List<Order>());
        var controller = CreateOrderController(orderService, "employee", BranchA);

        var result = await controller.GetOrders(null, null, null, BranchB, null, null, null);

        Assert.IsType<ForbidResult>(result);
        orderService.Verify(x => x.GetOrdersAsync(It.IsAny<OrderQueryFilter>()), Times.Never);
    }

    [Fact]
    public async Task Employee_branch_a_uses_jwt_branch_when_filter_is_omitted()
    {
        var orderService = new Mock<IOrderService>();
        orderService.Setup(x => x.GetOrdersAsync(It.IsAny<OrderQueryFilter>()))
            .ReturnsAsync(new List<Order>());
        var controller = CreateOrderController(orderService, "employee", BranchA);

        var result = await controller.GetOrders(null, null, null, null, null, null, null);

        Assert.IsType<OkObjectResult>(result);
        orderService.Verify(x => x.GetOrdersAsync(It.Is<OrderQueryFilter>(filter => filter.BranchId == BranchA)), Times.Once);
    }

    [Fact]
    public async Task Admin_can_request_cross_branch_order_filter()
    {
        var orderService = new Mock<IOrderService>();
        orderService.Setup(x => x.GetOrdersAsync(It.IsAny<OrderQueryFilter>()))
            .ReturnsAsync(new List<Order>());
        var controller = CreateOrderController(orderService, "admin", null);

        var result = await controller.GetOrders(null, null, null, BranchB, null, null, null);

        Assert.IsType<OkObjectResult>(result);
        orderService.Verify(x => x.GetOrdersAsync(It.Is<OrderQueryFilter>(filter => filter.BranchId == BranchB)), Times.Once);
    }

    [Fact]
    public async Task Customer_list_ignores_client_phone_and_branch_filters()
    {
        var orderService = new Mock<IOrderService>();
        orderService.Setup(x => x.GetOrdersAsync(It.IsAny<OrderQueryFilter>()))
            .ReturnsAsync(new List<Order>());
        var controller = CreateCustomerController(orderService, "0900111222");

        var result = await controller.GetOrders(null, null, "0900999999", BranchB, null, null, null);

        Assert.IsType<OkObjectResult>(result);
        orderService.Verify(x => x.GetOrdersAsync(It.Is<OrderQueryFilter>(f =>
            f.CustomerPhone == "0900111222" && f.BranchId == BranchB)), Times.Once);
    }

    [Fact]
    public async Task Customer_cannot_read_another_customers_order_by_id()
    {
        var orderService = new Mock<IOrderService>();
        var order = new Order { Id = Guid.NewGuid(), CustomerPhone = "0900999999" };
        orderService.Setup(x => x.GetOrderByIdAsync(order.Id)).ReturnsAsync(order);
        var controller = CreateCustomerController(orderService, "0900111222");

        var result = await controller.GetOrder(order.Id);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task Employee_cannot_read_order_from_another_branch_by_id()
    {
        var orderService = new Mock<IOrderService>();
        var order = new Order { Id = Guid.NewGuid(), BranchId = BranchB };
        orderService.Setup(x => x.GetOrderByIdAsync(order.Id)).ReturnsAsync(order);
        var controller = CreateOrderController(orderService, "employee", BranchA);

        var result = await controller.GetOrder(order.Id);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task Employee_cannot_move_order_to_another_branch_in_status_update()
    {
        var orderService = new Mock<IOrderService>();
        var order = new Order { Id = Guid.NewGuid(), BranchId = BranchA, Status = "Pending" };
        orderService.Setup(x => x.GetOrderByIdAsync(order.Id)).ReturnsAsync(order);
        var controller = CreateOrderController(orderService, "employee", BranchA);

        var result = await controller.UpdateStatus(order.Id, new OrderUpdateDto { Status = "Completed", BranchId = BranchB });

        Assert.IsType<ForbidResult>(result);
        orderService.Verify(x => x.UpdateOrderStatusAsync(It.IsAny<Guid>(), It.IsAny<OrderUpdateDto>()), Times.Never);
    }

    [Fact]
    public async Task Kitchen_cannot_update_request_from_another_branch()
    {
        var orderService = new Mock<IOrderService>();
        var kitchen = new Mock<IKitchenService>();
        var requestId = Guid.NewGuid();
        kitchen.Setup(x => x.GetOrderRequestOrderAsync(requestId))
            .ReturnsAsync(new Order { BranchId = BranchB });
        var controller = CreateOrderController(orderService, "kitchen", BranchA);
        controller = new OrderController(orderService.Object, kitchen.Object, Mock.Of<ILoyaltyService>())
        {
            ControllerContext = controller.ControllerContext
        };

        var result = await controller.UpdateRequestStatus(requestId, "Ready");

        Assert.IsType<ForbidResult>(result);
        kitchen.Verify(x => x.UpdateRequestStatusAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
    }

    private static OrderController CreateOrderController(Mock<IOrderService> orderService, string role, Guid? branchId)
    {
        var claims = new List<Claim> { new(ClaimTypes.Role, role) };
        if (branchId.HasValue)
            claims.Add(new Claim("branchId", branchId.Value.ToString()));

        return new OrderController(orderService.Object, Mock.Of<IKitchenService>(), Mock.Of<ILoyaltyService>())
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

    private static OrderController CreateCustomerController(Mock<IOrderService> orderService, string phone)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Role, "customer"),
            new(ClaimTypes.Name, phone),
            new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
        };
        return new OrderController(orderService.Object, Mock.Of<IKitchenService>(), Mock.Of<ILoyaltyService>())
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

    private static string? GetAuthorizeRoles(MethodInfo method) =>
        method.GetCustomAttribute<AuthorizeAttribute>()?.Roles ??
        method.DeclaringType?.GetCustomAttribute<AuthorizeAttribute>()?.Roles;

    private static readonly Guid BranchA = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid BranchB = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
}
