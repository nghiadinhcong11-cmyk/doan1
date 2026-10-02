using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RestaurantPOS.Application.DTOs.Financial;
using RestaurantPOS.Application.Services;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Infrastructure.Persistence;
using Xunit;

namespace RestaurantPOS.Tests;

public sealed class ProactiveInsightTests
{
    [Fact]
    public async Task ProcessProactiveInsightsAsync_DetectsRevenueDrop()
    {
        // Arrange
        var (context, connection) = TestDbContextFactory.Create();
        _ = connection;

        var branchId = Guid.NewGuid();
        context.Branches.Add(new Branch { Id = branchId, Name = "Test Branch", IsActive = true });
        await context.SaveChangesAsync();

        var financialService = new Mock<IFinancialAnalysisService>();
        financialService.Setup(x => x.GetFinancialAnalysisAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), branchId, It.IsAny<string>()))
            .ReturnsAsync(new FinancialAnalysisDto
            {
                Anomalies = new List<FinancialAnomalyDto>
                {
                    new() { Type = "RevenueSignificantDrop", Severity = "Critical", Description = "Doanh thu giảm mạnh (25.0%)." }
                }
            });

        var logger = new Mock<ILogger<ProactiveInsightService>>();
        var explanationService = new Mock<IInsightExplanationService>();
        var notificationService = new Mock<INotificationService>();
        var service = new ProactiveInsightService(context, financialService.Object, explanationService.Object, notificationService.Object, logger.Object);

        // Act
        await service.ProcessProactiveInsightsAsync();

        // Assert
        var insight = await context.BusinessInsights.FirstOrDefaultAsync(i => i.BranchId == branchId && i.Type == "RevenueSignificantDrop");
        Assert.NotNull(insight);
        Assert.Contains("25.0%", insight.Summary);
    }

    [Fact]
    public async Task ProcessProactiveInsightsAsync_PreventsDuplicateInsights()
    {
        // Arrange
        var (context, connection) = TestDbContextFactory.Create();
        _ = connection;

        var branchId = Guid.NewGuid();
        context.Branches.Add(new Branch { Id = branchId, Name = "Test Branch", IsActive = true });
        await context.SaveChangesAsync();

        var financialService = new Mock<IFinancialAnalysisService>();
        financialService.Setup(x => x.GetFinancialAnalysisAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), branchId, It.IsAny<string>()))
            .ReturnsAsync(new FinancialAnalysisDto
            {
                Anomalies = new List<FinancialAnomalyDto>
                {
                    new() { Type = "RevenueSignificantDrop", Severity = "Critical", Description = "Doanh thu giảm mạnh (25.0%)." }
                }
            });

        var logger = new Mock<ILogger<ProactiveInsightService>>();
        var explanationService = new Mock<IInsightExplanationService>();
        var notificationService = new Mock<INotificationService>();
        var service = new ProactiveInsightService(context, financialService.Object, explanationService.Object, notificationService.Object, logger.Object);

        // Act
        await service.ProcessProactiveInsightsAsync();
        await service.ProcessProactiveInsightsAsync(); // Run second time

        // Assert
        var count = await context.BusinessInsights.CountAsync(i => i.BranchId == branchId && i.Type == "RevenueSignificantDrop");
        Assert.Equal(1, count); // Should be exactly 1 due to deduplication key
    }

    [Fact]
    public async Task ProcessProactiveInsightsAsync_DetectsExpenseSpike()
    {
        // Arrange
        var (context, connection) = TestDbContextFactory.Create();
        _ = connection;

        var branchId = Guid.NewGuid();
        context.Branches.Add(new Branch { Id = branchId, Name = "Test Branch", IsActive = true });
        await context.SaveChangesAsync();

        var financialService = new Mock<IFinancialAnalysisService>();
        financialService.Setup(x => x.GetFinancialAnalysisAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), branchId, It.IsAny<string>()))
            .ReturnsAsync(new FinancialAnalysisDto
            {
                Anomalies = new List<FinancialAnomalyDto>
                {
                    new() { Type = "ExpenseCategorySpike", Severity = "Warning", Description = "Chi phí nhóm 'Nguyên liệu' tăng 60.0%." }
                }
            });

        var logger = new Mock<ILogger<ProactiveInsightService>>();
        var explanationService = new Mock<IInsightExplanationService>();
        var notificationService = new Mock<INotificationService>();
        var service = new ProactiveInsightService(context, financialService.Object, explanationService.Object, notificationService.Object, logger.Object);

        // Act
        await service.ProcessProactiveInsightsAsync();

        // Assert
        var insight = await context.BusinessInsights.FirstOrDefaultAsync(i => i.BranchId == branchId && i.Type == "ExpenseCategorySpike");
        Assert.NotNull(insight);
        Assert.Contains("60.0%", insight.Summary);
    }
}
