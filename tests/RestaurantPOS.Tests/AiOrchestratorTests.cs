using System.Text.Json;
using Moq;
using RestaurantPOS.AI.Models;
using RestaurantPOS.AI.Security;
using RestaurantPOS.AI.Services;
using RestaurantPOS.AI.Tools;

namespace RestaurantPOS.Tests;

public sealed class AiOrchestratorTests
{
    [Fact]
    public async Task ProcessAsync_ReturnsTextResponseWithoutExecutingTool()
    {
        var gemini = new Mock<IGeminiService>();
        gemini.Setup(x => x.GenerateContentAsync(It.IsAny<object>()))
            .ReturnsAsync("{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"Hello\"}]}}]}");
        var orchestrator = CreateOrchestrator(gemini.Object);

        var result = await orchestrator.ProcessAsync(new AiRequest { Message = "Hi" }, UnknownUser());

        Assert.True(result.Success);
        Assert.Equal("Hello", result.Message);
        gemini.Verify(x => x.GenerateContentAsync(It.IsAny<object>()), Times.Once);
    }

    [Fact]
    public async Task ProcessAsync_ReturnsFailureWhenGeminiHasNoCandidates()
    {
        var gemini = new Mock<IGeminiService>();
        gemini.Setup(x => x.GenerateContentAsync(It.IsAny<object>()))
            .ReturnsAsync("{\"candidates\":[]}");

        var result = await CreateOrchestrator(gemini.Object)
            .ProcessAsync(new AiRequest { Message = "Hi" }, UnknownUser());

        Assert.False(result.Success);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public async Task ProcessAsync_ExecutesAllowedToolAndReturnsSecondResponse()
    {
        var tool = new Mock<IAiTool>();
        tool.SetupGet(x => x.Name).Returns("echo");
        tool.SetupGet(x => x.Description).Returns("Echoes input");
        tool.SetupGet(x => x.AllowedRoles).Returns(new[] { "unknown" });
        tool.SetupGet(x => x.RiskLevel).Returns(ToolRiskLevel.Read);
        tool.Setup(x => x.GetSchema()).Returns(new { name = "echo" });
        tool.Setup(x => x.ExecuteAsync(It.IsAny<JsonElement>(), It.IsAny<AiUserContext>()))
            .ReturnsAsync(AiToolResult.CreateSuccess("done"));

        var gemini = new Mock<IGeminiService>();
        gemini.SetupSequence(x => x.GenerateContentAsync(It.IsAny<object>()))
            .ReturnsAsync("{\"candidates\":[{\"content\":{\"parts\":[{\"functionCall\":{\"name\":\"echo\",\"args\":{}}}]}}]}")
            .ReturnsAsync("{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"Tool complete\"}]}}]}");

        var result = await CreateOrchestrator(gemini.Object, tool.Object)
            .ProcessAsync(new AiRequest { Message = "Run echo" }, UnknownUser());

        Assert.True(result.Success);
        Assert.Equal("Tool complete", result.Message);
        tool.Verify(x => x.ExecuteAsync(It.IsAny<JsonElement>(), It.IsAny<AiUserContext>()), Times.Once);
        gemini.Verify(x => x.GenerateContentAsync(It.IsAny<object>()), Times.Exactly(2));
    }

    private static AiOrchestrator CreateOrchestrator(IGeminiService gemini, params IAiTool[] tools)
    {
        var (context, connection) = TestDbContextFactory.Create();
        // The unknown role follows the no-database branch in AiContextService.
        // Keep the connection alive for the lifetime of the orchestrator call.
        _ = connection;
        var contextService = new AiContextService(context);
        var registry = new AiToolRegistry(tools);
        var permission = new AiPermissionService(new AiAuthorization());
        return new AiOrchestrator(gemini, contextService, registry, permission, new PromptBuilder());
    }

    private static AiUserContext UnknownUser() => new() { Role = "unknown" };
}
