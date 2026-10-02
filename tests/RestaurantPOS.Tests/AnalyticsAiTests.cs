using System.Text.Json;
using Moq;
using RestaurantPOS.AI.Models;
using RestaurantPOS.AI.Security;
using RestaurantPOS.AI.Services;
using RestaurantPOS.AI.Tools;
using RestaurantPOS.Application.Services;

namespace RestaurantPOS.Tests;

public sealed class AnalyticsAiTests
{
    [Fact]
    public async Task ProcessAsync_SequentialToolCalling_ExecutesMultipleTools()
    {
        var toolA = new Mock<IAiTool>();
        toolA.SetupGet(x => x.Name).Returns("tool_a");
        toolA.SetupGet(x => x.AllowedRoles).Returns(new[] { "admin" });
        toolA.SetupGet(x => x.RiskLevel).Returns(ToolRiskLevel.Read);
        toolA.Setup(x => x.GetSchema()).Returns(new { name = "tool_a" });
        toolA.Setup(x => x.ExecuteAsync(It.IsAny<JsonElement>(), It.IsAny<AiUserContext>()))
            .ReturnsAsync(AiToolResult.CreateSuccess("result_a"));

        var toolB = new Mock<IAiTool>();
        toolB.SetupGet(x => x.Name).Returns("tool_b");
        toolB.SetupGet(x => x.AllowedRoles).Returns(new[] { "admin" });
        toolB.SetupGet(x => x.RiskLevel).Returns(ToolRiskLevel.Read);
        toolB.Setup(x => x.GetSchema()).Returns(new { name = "tool_b" });
        toolB.Setup(x => x.ExecuteAsync(It.IsAny<JsonElement>(), It.IsAny<AiUserContext>()))
            .ReturnsAsync(AiToolResult.CreateSuccess("result_b"));

        var gemini = new Mock<IGeminiService>();
        // 1. Model asks for tool_a
        // 2. Model asks for tool_b (after receiving result_a)
        // 3. Model gives final answer
        gemini.SetupSequence(x => x.GenerateContentAsync(It.IsAny<object>()))
            .ReturnsAsync("{\"candidates\":[{\"content\":{\"parts\":[{\"functionCall\":{\"name\":\"tool_a\",\"args\":{}}}]}}]}")
            .ReturnsAsync("{\"candidates\":[{\"content\":{\"parts\":[{\"functionCall\":{\"name\":\"tool_b\",\"args\":{}}}]}}]}")
            .ReturnsAsync("{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"All tools done\"}]}}]}");

        var orchestrator = CreateOrchestrator(gemini.Object, toolA.Object, toolB.Object);
        var result = await orchestrator.ProcessAsync(new AiRequest { Message = "Run both" }, AdminUser());

        Assert.True(result.Success);
        Assert.Equal("All tools done", result.Message);
        toolA.Verify(x => x.ExecuteAsync(It.IsAny<JsonElement>(), It.IsAny<AiUserContext>()), Times.Once);
        toolB.Verify(x => x.ExecuteAsync(It.IsAny<JsonElement>(), It.IsAny<AiUserContext>()), Times.Once);
        gemini.Verify(x => x.GenerateContentAsync(It.IsAny<object>()), Times.Exactly(3));
    }

    [Fact]
    public async Task ProcessAsync_SequentialToolCalling_EnforcesMaxIterations()
    {
        var tool = new Mock<IAiTool>();
        tool.SetupGet(x => x.Name).Returns("loop");
        tool.SetupGet(x => x.AllowedRoles).Returns(new[] { "admin" });
        tool.SetupGet(x => x.RiskLevel).Returns(ToolRiskLevel.Read);
        tool.Setup(x => x.GetSchema()).Returns(new { name = "loop" });
        tool.Setup(x => x.ExecuteAsync(It.IsAny<JsonElement>(), It.IsAny<AiUserContext>()))
            .ReturnsAsync(AiToolResult.CreateSuccess("looping"));

        var gemini = new Mock<IGeminiService>();
        gemini.Setup(x => x.GenerateContentAsync(It.IsAny<object>()))
            .ReturnsAsync("{\"candidates\":[{\"content\":{\"parts\":[{\"functionCall\":{\"name\":\"loop\",\"args\":{}}}]}}]}");

        var orchestrator = CreateOrchestrator(gemini.Object, tool.Object);
        var result = await orchestrator.ProcessAsync(new AiRequest { Message = "Infinite loop" }, AdminUser());

        Assert.False(result.Success);
        Assert.Contains("vượt quá giới hạn", result.Error);
        gemini.Verify(x => x.GenerateContentAsync(It.IsAny<object>()), Times.Exactly(5));
    }

    private static AiOrchestrator CreateOrchestrator(IGeminiService gemini, params IAiTool[] tools)
    {
        var (context, connection) = TestDbContextFactory.Create();
        _ = connection;
        var contextService = new AiContextService(context);
        var registry = new AiToolRegistry(tools);
        var permission = new AiPermissionService(new AiAuthorization());
        return new AiOrchestrator(gemini, contextService, registry, permission, new PromptBuilder());
    }

    private static AiUserContext AdminUser() => new() { Role = "admin" };
}
