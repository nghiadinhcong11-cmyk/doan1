using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using RestaurantPOS.AI.Services;
using RestaurantPOS.Application.Services;
using RestaurantPOS.Domain.Entities;
using Xunit;

namespace RestaurantPOS.Tests;

public sealed class ProactiveInsightAiTests
{
    [Fact]
    public async Task InsightExplanationService_PopulatesAiFields()
    {
        // Arrange
        var gemini = new Mock<IGeminiService>();
        var response = new
        {
            candidates = new[]
            {
                new
                {
                    content = new
                    {
                        parts = new[]
                        {
                            new { text = "{\"explanation\": \"Test Explanation\", \"recommendation\": \"Test Recommendation\"}" }
                        }
                    }
                }
            }
        };
        gemini.Setup(x => x.GenerateContentAsync(It.IsAny<object>()))
            .ReturnsAsync(JsonSerializer.Serialize(response));

        var logger = new Mock<ILogger<InsightExplanationService>>();
        var service = new InsightExplanationService(gemini.Object, logger.Object);
        var insight = new BusinessInsight
        {
            Type = "RevenueSignificantDrop",
            Summary = "Revenue dropped 25%",
            EvidenceJson = "{}"
        };

        // Act
        await service.ExplainInsightAsync(insight);

        // Assert
        Assert.Equal("Test Explanation", insight.AiExplanation);
        Assert.Equal("Test Recommendation", insight.AiRecommendation);
    }

    [Fact]
    public async Task InsightExplanationService_HandlesGeminiFailureGracefully()
    {
        // Arrange
        var gemini = new Mock<IGeminiService>();
        gemini.Setup(x => x.GenerateContentAsync(It.IsAny<object>()))
            .ThrowsAsync(new Exception("API Error"));

        var logger = new Mock<ILogger<InsightExplanationService>>();
        var service = new InsightExplanationService(gemini.Object, logger.Object);
        var insight = new BusinessInsight { Type = "Test" };

        // Act
        await service.ExplainInsightAsync(insight);

        // Assert
        Assert.Null(insight.AiExplanation);
        Assert.Null(insight.AiRecommendation);
    }

    [Fact]
    public async Task ProactiveInsightService_CallsExplanationServiceForNewInsights()
    {
        // Arrange
        var (context, connection) = TestDbContextFactory.Create();
        _ = connection;

        var branchId = Guid.NewGuid();
        context.Branches.Add(new Branch { Id = branchId, Name = "Test Branch", IsActive = true });
        await context.SaveChangesAsync();

        var financialService = new Mock<IFinancialAnalysisService>();
        financialService.Setup(x => x.GetFinancialAnalysisAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), branchId, It.IsAny<string>()))
            .ReturnsAsync(new Application.DTOs.Financial.FinancialAnalysisDto
            {
                Anomalies = new List<Application.DTOs.Financial.FinancialAnomalyDto>
                {
                    new() { Type = "RevenueSignificantDrop", Severity = "Critical", Description = "Drop" }
                }
            });

        var explanationService = new Mock<IInsightExplanationService>();
        explanationService.Setup(x => x.ExplainInsightAsync(It.IsAny<BusinessInsight>()))
            .Callback<BusinessInsight>(i => { i.AiExplanation = "Explained"; });

        var notificationService = new Mock<INotificationService>();
        var logger = new Mock<ILogger<ProactiveInsightService>>();
        var service = new ProactiveInsightService(context, financialService.Object, explanationService.Object, notificationService.Object, logger.Object);

        // Act
        await service.ProcessProactiveInsightsAsync();

        // Assert
        var insight = await context.BusinessInsights.FirstOrDefaultAsync();
        Assert.NotNull(insight);
        Assert.Equal("Explained", insight!.AiExplanation);
        explanationService.Verify(x => x.ExplainInsightAsync(It.IsAny<BusinessInsight>()), Times.Once);
    }
}
