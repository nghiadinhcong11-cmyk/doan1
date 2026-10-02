using System.Text.Json;
using Moq;
using RestaurantPOS.AI.Models;
using RestaurantPOS.AI.Security;
using RestaurantPOS.AI.Services;
using RestaurantPOS.AI.Tools;
using RestaurantPOS.Application.Services;

namespace RestaurantPOS.Tests;

public sealed class BusinessAdvisorAiTests
{
    [Fact]
    public async Task GetBusinessSummaryTool_CalculatesAovCorrectly()
    {
        var dashboard = new Mock<IDashboardService>();
        dashboard.Setup(x => x.GetBusinessSummaryAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<Guid?>()))
            .ReturnsAsync(new BusinessSummaryDto
            {
                Revenue = 1000000,
                OrderCount = 10,
                AverageOrderValue = 100000,
                NetProfit = 500000
            });

        var tool = new RestaurantPOS.AI.Tools.Admin.GetBusinessSummaryTool(dashboard.Object);
        var args = JsonDocument.Parse("{\"period\":\"today\"}").RootElement;

        var result = await tool.ExecuteAsync(args, new AiUserContext { Role = "admin" });

        Assert.True(result.Success);
        Assert.Contains("1000000", result.Data);
        Assert.Contains("100000", result.Data);
    }

    [Fact]
    public async Task ProcessAsync_AdvisorScenario_ExecutesSummaryAndInfers()
    {
        var gemini = new Mock<IGeminiService>();
        gemini.SetupSequence(x => x.GenerateContentAsync(It.IsAny<object>()))
            .ReturnsAsync("{\"candidates\":[{\"content\":{\"parts\":[{\"functionCall\":{\"name\":\"get_business_summary\",\"args\":{\"period\":\"today\"}}}]}}]}")
            .ReturnsAsync("{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"FACT: Doanh thu 1M. INFERENCE: Kinh doanh tốt. RECOMMENDATION: Tiếp tục phát huy.\"}]}}]}");

        var tool = new Mock<IAiTool>();
        tool.SetupGet(x => x.Name).Returns("get_business_summary");
        tool.SetupGet(x => x.AllowedRoles).Returns(new[] { "admin" });
        tool.SetupGet(x => x.RiskLevel).Returns(ToolRiskLevel.Read);
        tool.Setup(x => x.GetSchema()).Returns(new { name = "get_business_summary" });
        tool.Setup(x => x.ExecuteAsync(It.IsAny<JsonElement>(), It.IsAny<AiUserContext>()))
            .ReturnsAsync(AiToolResult.CreateSuccess("{\"KPIs\":{\"Revenue\":1000000}}"));

        var orchestrator = CreateOrchestrator(gemini.Object, tool.Object);
        var result = await orchestrator.ProcessAsync(new AiRequest { Message = "Phân tích hôm nay" }, new AiUserContext { Role = "admin" });

        Assert.True(result.Success);
        Assert.Contains("FACT:", result.Message);
        Assert.Contains("INFERENCE:", result.Message);
        tool.Verify(x => x.ExecuteAsync(It.IsAny<JsonElement>(), It.IsAny<AiUserContext>()), Times.Once);
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
}
