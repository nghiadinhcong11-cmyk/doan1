using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RestaurantPOS.AI.Controllers;
using RestaurantPOS.AI.Models;
using RestaurantPOS.AI.Services;

namespace RestaurantPOS.Tests;

public sealed class AiControllerAuthenticationTests
{
    [Fact]
    public async Task Authenticated_employee_uses_jwt_role_even_when_query_role_is_admin()
    {
        var orchestrator = new Mock<IAiOrchestrator>();
        AiUserContext? capturedContext = null;
        orchestrator
            .Setup(x => x.ProcessAsync(It.IsAny<AiRequest>(), It.IsAny<AiUserContext>()))
            .Callback<AiRequest, AiUserContext>((_, context) => capturedContext = context)
            .ReturnsAsync(new AiResponse { Message = "ok" });
        var controller = CreateController(orchestrator, AuthenticatedUser("employee", Guid.NewGuid()));

        controller.HttpContext.Request.QueryString = new QueryString("?role=admin");
        await controller.Chat(new AiRequest { Message = "test" });

        Assert.NotNull(capturedContext);
        Assert.Equal("employee", capturedContext!.Role);
    }

    [Fact]
    public async Task Anonymous_request_remains_customer_even_when_query_role_is_admin()
    {
        var orchestrator = new Mock<IAiOrchestrator>();
        AiUserContext? capturedContext = null;
        orchestrator
            .Setup(x => x.ProcessAsync(It.IsAny<AiRequest>(), It.IsAny<AiUserContext>()))
            .Callback<AiRequest, AiUserContext>((_, context) => capturedContext = context)
            .ReturnsAsync(new AiResponse { Message = "ok" });
        var controller = CreateController(orchestrator, new ClaimsPrincipal(new ClaimsIdentity()));

        controller.HttpContext.Request.QueryString = new QueryString("?role=admin");
        await controller.Chat(new AiRequest { Message = "test" });

        Assert.NotNull(capturedContext);
        Assert.Equal("customer", capturedContext!.Role);
    }

    [Fact]
    public async Task Authenticated_token_without_role_claim_does_not_default_to_admin()
    {
        var orchestrator = new Mock<IAiOrchestrator>();
        AiUserContext? capturedContext = null;
        orchestrator
            .Setup(x => x.ProcessAsync(It.IsAny<AiRequest>(), It.IsAny<AiUserContext>()))
            .Callback<AiRequest, AiUserContext>((_, context) => capturedContext = context)
            .ReturnsAsync(new AiResponse { Message = "ok" });
        var controller = CreateController(orchestrator, AuthenticatedUser(null, Guid.NewGuid()));

        controller.HttpContext.Request.QueryString = new QueryString("?role=admin");
        await controller.Chat(new AiRequest { Message = "test" });

        Assert.NotNull(capturedContext);
        Assert.Equal("customer", capturedContext!.Role);
        Assert.NotEqual("admin", capturedContext.Role);
    }

    private static AiController CreateController(Mock<IAiOrchestrator> orchestrator, ClaimsPrincipal user)
    {
        var controller = new AiController(orchestrator.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            }
        };
        return controller;
    }

    private static ClaimsPrincipal AuthenticatedUser(string? role, Guid branchId)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new("branchId", branchId.ToString())
        };
        if (role is not null)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"));
    }
}
