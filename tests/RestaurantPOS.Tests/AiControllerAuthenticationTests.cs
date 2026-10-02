using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RestaurantPOS.AI.Controllers;
using RestaurantPOS.AI.Models;
using RestaurantPOS.AI.Services;
using Microsoft.AspNetCore.Authorization;

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
    public void Ai_chat_requires_an_authenticated_token()
    {
        var action = typeof(AiController).GetMethod(nameof(AiController.Chat));
        Assert.NotNull(action);
        Assert.NotNull(typeof(AiController).GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true).OfType<AuthorizeAttribute>().SingleOrDefault());
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
        Assert.Equal(string.Empty, capturedContext!.Role);
        Assert.NotEqual("admin", capturedContext.Role);
    }

    [Fact]
    public async Task Guest_customer_session_is_denied_ai_access()
    {
        var orchestrator = new Mock<IAiOrchestrator>();
        var principal = AuthenticatedUser("customer", Guid.NewGuid());
        ((ClaimsIdentity)principal.Identity!).AddClaim(new Claim("customerSessionType", "guest"));
        var controller = CreateController(orchestrator, principal);

        var result = await controller.Chat(new AiRequest { Message = "menu" });

        Assert.IsType<ForbidResult>(result);
        orchestrator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Registered_customer_ai_context_uses_jwt_identity_and_phone()
    {
        var customerId = Guid.NewGuid();
        var principal = AuthenticatedUser("customer", Guid.NewGuid());
        var identity = (ClaimsIdentity)principal.Identity!;
        identity.RemoveClaim(identity.FindFirst(ClaimTypes.NameIdentifier)!);
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, customerId.ToString()));
        identity.AddClaim(new Claim(ClaimTypes.Name, "0900000000"));
        identity.AddClaim(new Claim("customerSessionType", "registered"));
        var orchestrator = new Mock<IAiOrchestrator>();
        AiUserContext? captured = null;
        orchestrator.Setup(x => x.ProcessAsync(It.IsAny<AiRequest>(), It.IsAny<AiUserContext>()))
            .Callback<AiRequest, AiUserContext>((_, context) => captured = context)
            .ReturnsAsync(new AiResponse { Message = "ok" });

        var result = await CreateController(orchestrator, principal).Chat(new AiRequest { Message = "orders" });

        Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(captured);
        Assert.Equal(customerId, captured!.CustomerId);
        Assert.Equal("0900000000", captured.PhoneNumber);
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
