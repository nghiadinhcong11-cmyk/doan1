using System.Text.Json;
using Moq;
using RestaurantPOS.AI.Models;
using RestaurantPOS.AI.Security;
using RestaurantPOS.AI.Tools.Admin;
using RestaurantPOS.Application.DTOs.Financial;
using RestaurantPOS.Application.Services;
using Xunit;

namespace RestaurantPOS.Tests;

public sealed class FinancialIntelligenceTests
{
    [Fact]
    public async Task GetFinancialAnalysisTool_CalculatesGrowthAndBreakdown()
    {
        var financialService = new Mock<IFinancialAnalysisService>();
        var branchId = Guid.NewGuid();

        financialService.Setup(x => x.GetFinancialAnalysisAsync(
            It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<Guid?>(), It.IsAny<string>()))
            .ReturnsAsync(new FinancialAnalysisDto
            {
                Current = new BusinessSummaryDto
                {
                    Revenue = 1200000,
                    TotalExpenses = 300000,
                    NetProfit = 700000,
                    ProfitMargin = 58.33m,
                    ExpenseBreakdown = new List<ExpenseCategoryDto>
                    {
                        new() { Category = "Nguyên liệu", Amount = 200000, Percentage = 66.67m },
                        new() { Category = "Điện nước", Amount = 100000, Percentage = 33.33m }
                    }
                },
                Growth = new FinancialGrowthDto
                {
                    RevenueGrowthPercent = 20,
                    ProfitGrowthPercent = 15
                },
                Anomalies = new List<FinancialAnomalyDto>
                {
                    new() { Type = "RevenueSignificantIncrease", Severity = "Info", Description = "Doanh thu tăng trưởng tốt (20%)." }
                }
            });

        var tool = new GetFinancialAnalysisTool(financialService.Object);
        var args = JsonDocument.Parse("{\"period\":\"this_month\"}").RootElement;
        var userContext = new AiUserContext { Role = "admin", BranchId = branchId };

        var result = await tool.ExecuteAsync(args, userContext);

        Assert.True(result.Success);
        Assert.Contains("1200000", result.Data);
        Assert.Contains("20", result.Data); // Growth
        Assert.Contains("200000", result.Data); // Amount in breakdown
        Assert.Contains("RevenueSignificantIncrease", result.Data); // Anomaly
    }

    [Fact]
    public async Task FinancialAnalysisService_DetectsRevenueDropAnomaly()
    {
        var dashboard = new Mock<IDashboardService>();
        var now = DateTime.UtcNow;
        var prev = now.AddDays(-7);

        dashboard.Setup(x => x.GetBusinessSummaryAsync(now, It.IsAny<DateTime>(), It.IsAny<Guid?>()))
            .ReturnsAsync(new BusinessSummaryDto { Revenue = 80, NetProfit = 20 });
        dashboard.Setup(x => x.GetBusinessSummaryAsync(prev, It.IsAny<DateTime>(), It.IsAny<Guid?>()))
            .ReturnsAsync(new BusinessSummaryDto { Revenue = 100, NetProfit = 30 });

        var service = new FinancialAnalysisService(dashboard.Object);
        var result = await service.GetFinancialAnalysisAsync(now, now.AddDays(1), prev, prev.AddDays(1), null, "today");

        Assert.Equal(-20, result.Growth.RevenueGrowthPercent);
        Assert.Contains(result.Anomalies, a => a.Type == "RevenueSignificantDrop");
    }

    [Fact]
    public async Task FinancialAnalysisService_DetectsExpenseSpike()
    {
        var dashboard = new Mock<IDashboardService>();
        dashboard.SetupSequence(x => x.GetBusinessSummaryAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<Guid?>()))
            .ReturnsAsync(new BusinessSummaryDto
            {
                TotalExpenses = 150,
                ExpenseBreakdown = new List<ExpenseCategoryDto> { new() { Category = "Marketing", Amount = 150 } }
            })
            .ReturnsAsync(new BusinessSummaryDto
            {
                TotalExpenses = 100,
                ExpenseBreakdown = new List<ExpenseCategoryDto> { new() { Category = "Marketing", Amount = 100 } }
            });

        var service = new FinancialAnalysisService(dashboard.Object);
        var result = await service.GetFinancialAnalysisAsync(DateTime.UtcNow, DateTime.UtcNow, DateTime.UtcNow, DateTime.UtcNow, null, "today");

        Assert.Contains(result.Anomalies, a => a.Type == "ExpenseCategorySpike");
    }

    [Fact]
    public async Task FinancialAnalysisTool_RespectsBranchIsolation()
    {
        var branchId = Guid.NewGuid();
        var financialService = new Mock<IFinancialAnalysisService>();
        financialService.Setup(x => x.GetFinancialAnalysisAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), branchId, It.IsAny<string>()))
            .ReturnsAsync(new FinancialAnalysisDto { Period = "today" });

        var tool = new GetFinancialAnalysisTool(financialService.Object);
        var userContext = new AiUserContext { Role = "manager", BranchId = branchId };
        var args = JsonDocument.Parse("{\"period\":\"today\"}").RootElement;

        await tool.ExecuteAsync(args, userContext);

        financialService.Verify(x => x.GetFinancialAnalysisAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), branchId, "today"), Times.Once);
    }
}
