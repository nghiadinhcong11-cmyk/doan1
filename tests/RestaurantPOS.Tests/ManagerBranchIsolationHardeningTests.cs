using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RestaurantPOS.AI.Models;
using RestaurantPOS.AI.Security;
using RestaurantPOS.AI.Services;
using RestaurantPOS.AI.Tools;
using RestaurantPOS.Application.Services;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Infrastructure.Persistence;
using RestaurantPOS.WebAPI.Controllers;

namespace RestaurantPOS.Tests;

public sealed class ManagerBranchIsolationHardeningTests
{
    private static readonly Guid BranchA = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid BranchB = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    [Fact]
    public async Task Dashboard_manager_uses_JWT_branch_instead_of_requested_branch()
    {
        var service = new Mock<IDashboardService>();
        service.Setup(x => x.GetSummaryAsync(BranchA.ToString())).ReturnsAsync(new object());
        var controller = WithUser(new DashboardController(service.Object), "manager", BranchA);

        var result = await controller.GetSummary(BranchB.ToString());

        Assert.IsType<OkObjectResult>(result);
        service.Verify(x => x.GetSummaryAsync(BranchA.ToString()), Times.Once);
        service.Verify(x => x.GetSummaryAsync(BranchB.ToString()), Times.Never);
    }

    [Fact]
    public async Task Dashboard_manager_without_valid_branch_fails_closed()
    {
        var service = new Mock<IDashboardService>();
        var controller = WithUser(new DashboardController(service.Object), "manager", null);

        var result = await controller.GetSummary(BranchB.ToString());

        Assert.IsType<ForbidResult>(result);
        service.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Work_schedule_manager_cannot_schedule_employee_from_another_branch()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;
        SeedBranches(context);
        var employeeB = new Employee { Id = Guid.NewGuid(), FullName = "Staff B", BranchId = BranchB };
        context.Employees.Add(employeeB);
        await context.SaveChangesAsync();
        var controller = WithUser(new WorkScheduleController(context), "manager", BranchA);

        var result = await controller.CreateSchedule(new WorkSchedule
        {
            EmployeeId = employeeB.Id,
            BranchId = BranchA,
            Date = DateTime.UtcNow
        });

        Assert.IsType<ForbidResult>(result);
        Assert.Empty(context.WorkSchedules);
    }

    [Fact]
    public async Task Work_schedule_manager_uses_trusted_employee_branch_and_names()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;
        SeedBranches(context);
        var employeeA = new Employee
        {
            Id = Guid.NewGuid(), FullName = "Trusted Staff", BranchId = BranchA, BranchName = "Branch A"
        };
        context.Employees.Add(employeeA);
        await context.SaveChangesAsync();
        var controller = WithUser(new WorkScheduleController(context), "manager", BranchA);

        var result = await controller.CreateSchedule(new WorkSchedule
        {
            EmployeeId = employeeA.Id,
            EmployeeName = "Spoofed",
            BranchId = BranchB,
            BranchName = "Spoofed Branch",
            Date = DateTime.UtcNow
        });

        var ok = Assert.IsType<OkObjectResult>(result);
        var schedule = Assert.IsType<WorkSchedule>(ok.Value);
        Assert.Equal(BranchA, schedule.BranchId);
        Assert.Equal("Branch A", schedule.BranchName);
        Assert.Equal("Trusted Staff", schedule.EmployeeName);
    }

    [Fact]
    public async Task Shift_manager_cannot_open_branch_A_shift_for_branch_B_employee()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;
        SeedBranches(context);
        var employeeB = new Employee { Id = Guid.NewGuid(), FullName = "Staff B", BranchId = BranchB };
        context.Employees.Add(employeeB);
        await context.SaveChangesAsync();
        var controller = WithUser(new ShiftController(context), "manager", BranchA);

        var result = await controller.OpenShift(new ShiftController.OpenShiftRequest
        {
            EmployeeId = employeeB.Id,
            EmployeeName = "Spoofed",
            BranchId = BranchA,
            BranchName = "Branch A"
        });

        Assert.IsType<ForbidResult>(result);
        Assert.Empty(context.Shifts);
    }

    [Fact]
    public async Task Shift_close_revenue_uses_shift_branch_as_well_as_employee_identity()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;
        SeedBranches(context);
        var employee = new Employee { Id = Guid.NewGuid(), FullName = "Same Name", BranchId = BranchA };
        var shift = new Shift
        {
            Id = Guid.NewGuid(), EmployeeId = employee.Id, EmployeeName = employee.FullName,
            BranchId = BranchA, StartTime = DateTime.UtcNow.AddHours(-1), Status = "Open", Note = ""
        };
        context.Employees.Add(employee);
        context.Shifts.Add(shift);
        context.Orders.AddRange(
            new Order
            {
                Id = Guid.NewGuid(), BranchId = BranchA, CreatedAt = DateTime.UtcNow,
                CreatedBy = employee.FullName, Status = "Hoàn thành", PaymentMethod = "Tiền mặt", PaidAmount = 100m
            },
            new Order
            {
                Id = Guid.NewGuid(), BranchId = BranchB, CreatedAt = DateTime.UtcNow,
                CreatedBy = employee.FullName, Status = "Hoàn thành", PaymentMethod = "Tiền mặt", PaidAmount = 999m
            });
        await context.SaveChangesAsync();
        var controller = WithUser(new ShiftController(context), "manager", BranchA);

        var result = await controller.CloseShift(shift.Id, new ShiftController.CloseShiftRequest { EndingCash = 100m });

        Assert.IsType<OkObjectResult>(result);
        Assert.Equal(100m, shift.CashRevenue);
        Assert.Equal(100m, shift.TotalRevenue);
    }

    [Fact]
    public async Task Table_detail_manager_without_branch_does_not_issue_unscoped_lookup()
    {
        var service = new Mock<ITableService>();
        var controller = WithUser(new TableController(service.Object), "manager", null);

        var result = await controller.GetTable(Guid.NewGuid());

        Assert.IsType<ForbidResult>(result);
        service.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Kitchen_request_detail_manager_without_branch_does_not_issue_unscoped_lookup()
    {
        var kitchen = new Mock<IKitchenService>();
        var controller = WithUser(new OrderController(
            Mock.Of<IOrderService>(), kitchen.Object, Mock.Of<ILoyaltyService>()), "manager", null);

        var result = await controller.GetKitchenRequestDetail(Guid.NewGuid());

        Assert.IsType<ForbidResult>(result);
        kitchen.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Business_insight_manager_without_branch_cannot_read_global_insight_by_id()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;
        var insight = new BusinessInsight
        {
            Id = Guid.NewGuid(), BranchId = null, Title = "Global", Summary = "Global",
            Type = "Risk", Severity = "Info", DeduplicationKey = "global"
        };
        context.BusinessInsights.Add(insight);
        await context.SaveChangesAsync();
        var controller = WithUser(new BusinessInsightController(context), "manager", null);

        var result = await controller.GetById(insight.Id);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task AI_manager_without_branch_cannot_execute_otherwise_allowed_read_tool()
    {
        var tool = new Mock<IAiTool>();
        tool.Setup(x => x.AllowedRoles).Returns(new[] { "manager" });
        tool.Setup(x => x.RiskLevel).Returns(ToolRiskLevel.Read);
        var service = new AiPermissionService(new AiAuthorization());

        var allowed = await service.CanExecuteAsync(tool.Object, new AiUserContext { Role = "manager" });

        Assert.False(allowed);
    }

    [Fact]
    public async Task AI_manager_with_branch_retains_allowed_read_access()
    {
        var tool = new Mock<IAiTool>();
        tool.Setup(x => x.AllowedRoles).Returns(new[] { "manager" });
        tool.Setup(x => x.RiskLevel).Returns(ToolRiskLevel.Read);
        var service = new AiPermissionService(new AiAuthorization());

        var allowed = await service.CanExecuteAsync(tool.Object, new AiUserContext { Role = "manager", BranchId = BranchA });

        Assert.True(allowed);
    }

    private static T WithUser<T>(T controller, string role, Guid? branchId) where T : ControllerBase
    {
        var claims = new List<Claim> { new(ClaimTypes.Role, role) };
        if (branchId.HasValue) claims.Add(new Claim("branchId", branchId.Value.ToString()));
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"))
            }
        };
        return controller;
    }

    private static void SeedBranches(ApplicationDbContext context)
    {
        context.Branches.AddRange(
            new Branch { Id = BranchA, Name = "Branch A", IsActive = true },
            new Branch { Id = BranchB, Name = "Branch B", IsActive = true });
        context.SaveChanges();
    }
}
