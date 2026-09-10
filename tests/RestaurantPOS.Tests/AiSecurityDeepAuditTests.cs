using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RestaurantPOS.AI.Models;
using RestaurantPOS.AI.Security;
using RestaurantPOS.AI.Services;
using RestaurantPOS.AI.Tools;
using RestaurantPOS.AI.Tools.Admin;
using RestaurantPOS.AI.Tools.Employee;
using RestaurantPOS.Application.Services;
using RestaurantPOS.Domain.Entities;

namespace RestaurantPOS.Tests;

public sealed class AiSecurityDeepAuditTests
{
    private static readonly Guid BranchA = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid BranchB = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    [Fact]
    public async Task Manager_A_cannot_see_revenue_of_Branch_B_via_GetRevenueTool()
    {
        var dashboardService = new Mock<IDashboardService>();
        var tool = new GetRevenueTool(dashboardService.Object);
        var userContext = new AiUserContext { Role = "manager", BranchId = BranchA };
        var args = JsonDocument.Parse("{\"period\": \"today\"}").RootElement;

        await tool.ExecuteAsync(args, userContext);

        // Verify that dashboard service is called with Branch A, NOT null or Branch B
        dashboardService.Verify(x => x.GetRevenueByPeriodAsync("today", BranchA), Times.Once);
    }

    [Fact]
    public async Task GetTableSummaryTool_enforces_branch_isolation_for_employee()
    {
        var tableService = new Mock<ITableService>();
        var tool = new GetTableSummaryTool(tableService.Object);
        var userContext = new AiUserContext { Role = "employee", BranchId = BranchA };
        var args = JsonDocument.Parse("{}").RootElement;

        await tool.ExecuteAsync(args, userContext);

        tableService.Verify(x => x.GetTableStatusCountsAsync(BranchA), Times.Once);
    }

    [Fact]
    public async Task AiPermissionService_rejects_manager_from_destructive_tool()
    {
        var auth = new AiAuthorization();
        var permissionService = new AiPermissionService(auth);

        var destructiveTool = new Mock<IAiTool>();
        destructiveTool.Setup(x => x.RiskLevel).Returns(ToolRiskLevel.Destructive);
        destructiveTool.Setup(x => x.AllowedRoles).Returns(new[] { "admin" });

        var context = new AiUserContext { Role = "manager", BranchId = BranchA };

        var canExecute = await permissionService.CanExecuteAsync(destructiveTool.Object, context);

        Assert.False(canExecute);
    }

    [Fact]
    public async Task AiAuthorization_IsInAuthorizedBranch_validates_correctly()
    {
        var auth = new AiAuthorization();
        var context = new AiUserContext { Role = "manager", BranchId = BranchA };

        Assert.True(auth.IsInAuthorizedBranch(context, BranchA));
        Assert.False(auth.IsInAuthorizedBranch(context, BranchB));
        Assert.True(auth.IsInAuthorizedBranch(new AiUserContext { Role = "admin" }, BranchB));
    }

    [Fact]
    public void AiToolRegistry_only_returns_allowed_tools_for_role()
    {
        var adminTool = new Mock<IAiTool>();
        adminTool.Setup(x => x.Name).Returns("admin_tool");
        adminTool.Setup(x => x.AllowedRoles).Returns(new[] { "admin" });
        adminTool.Setup(x => x.GetSchema()).Returns(new { name = "admin_tool" });

        var employeeTool = new Mock<IAiTool>();
        employeeTool.Setup(x => x.Name).Returns("employee_tool");
        employeeTool.Setup(x => x.AllowedRoles).Returns(new[] { "employee", "admin" });
        employeeTool.Setup(x => x.GetSchema()).Returns(new { name = "employee_tool" });

        var registry = new AiToolRegistry(new[] { adminTool.Object, employeeTool.Object });

        var employeeTools = registry.GetGeminiToolsDefinition("employee");
        var adminTools = registry.GetGeminiToolsDefinition("admin");

        // employee should only see employee_tool
        var employeeJson = JsonSerializer.Serialize(employeeTools);
        Assert.Contains("employee_tool", employeeJson);
        Assert.DoesNotContain("admin_tool", employeeJson);

        // admin should see both
        var adminJson = JsonSerializer.Serialize(adminTools);
        Assert.Contains("employee_tool", adminJson);
        Assert.Contains("admin_tool", adminJson);
    }
}
