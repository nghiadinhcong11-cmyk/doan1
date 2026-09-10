using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RestaurantPOS.Application.Services;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.WebAPI.Controllers;
using RestaurantPOS.Infrastructure.Persistence;

namespace RestaurantPOS.Tests;

public sealed class FullBranchIsolationTests
{
    private static readonly Guid BranchA = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid BranchB = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    [Fact]
    public async Task Manager_A_cannot_see_attendance_of_Branch_B()
    {
        var (context, _) = TestDbContextFactory.Create();
        context.Branches.Add(new Branch { Id = BranchA, Name = "Branch A", IsActive = true });
        context.Branches.Add(new Branch { Id = BranchB, Name = "Branch B", IsActive = true });

        var empBId = Guid.NewGuid();
        context.Employees.Add(new Employee { Id = empBId, FullName = "Staff B", BranchId = BranchB });

        context.Attendances.Add(new Attendance { Id = Guid.NewGuid(), BranchId = BranchB, EmployeeId = empBId, EmployeeName = "Staff B", CheckInTime = DateTime.UtcNow });
        await context.SaveChangesAsync();

        var controller = CreateAttendanceController(context, "manager", BranchA);

        var result = await controller.GetAttendances(null, null, null);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var list = okResult.Value as System.Collections.IEnumerable;
        Assert.NotNull(list);

        int count = 0;
        foreach (var item in list) count++;
        Assert.Equal(0, count);
    }

    [Fact]
    public async Task Manager_A_cannot_see_schedules_of_Branch_B()
    {
        var (context, _) = TestDbContextFactory.Create();
        context.Branches.Add(new Branch { Id = BranchA, Name = "Branch A", IsActive = true });
        context.Branches.Add(new Branch { Id = BranchB, Name = "Branch B", IsActive = true });

        var empBId = Guid.NewGuid();
        context.Employees.Add(new Employee { Id = empBId, FullName = "Staff B", BranchId = BranchB });

        context.WorkSchedules.Add(new WorkSchedule { Id = Guid.NewGuid(), BranchId = BranchB, EmployeeId = empBId, EmployeeName = "Staff B", Date = DateTime.UtcNow });
        await context.SaveChangesAsync();

        var controller = CreateWorkScheduleController(context, "manager", BranchA);

        var result = await controller.GetSchedules(null, null, null, null);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var schedules = okResult.Value as System.Collections.IEnumerable;
        Assert.NotNull(schedules);

        int count = 0;
        foreach (var item in schedules) count++;
        Assert.Equal(0, count);
    }

    [Fact]
    public async Task Manager_A_cannot_see_expenses_of_Branch_B()
    {
        var (context, _) = TestDbContextFactory.Create();
        context.Branches.Add(new Branch { Id = BranchA, Name = "Branch A", IsActive = true });
        context.Branches.Add(new Branch { Id = BranchB, Name = "Branch B", IsActive = true });

        context.Expenses.Add(new Expense { Id = Guid.NewGuid(), BranchId = BranchB, Amount = 100, Description = "B Expense", Category = "Khác", ExpenseDate = DateTime.UtcNow });
        await context.SaveChangesAsync();

        var controller = CreateExpenseController(context, "cashier", BranchA);

        var result = await controller.GetExpenses(null, null, null, null);

        var okResult = Assert.IsType<OkObjectResult>(result);
        // Use reflection to get 'items' from anonymous type
        var itemsProp = okResult.Value!.GetType().GetProperty("items");
        var items = itemsProp!.GetValue(okResult.Value) as System.Collections.IEnumerable;

        int count = 0;
        foreach (var item in items!) count++;
        Assert.Equal(0, count);
    }

    [Fact]
    public async Task Admin_can_see_everything_in_attendance()
    {
        var (context, _) = TestDbContextFactory.Create();
        context.Branches.Add(new Branch { Id = BranchA, Name = "Branch A", IsActive = true });
        context.Branches.Add(new Branch { Id = BranchB, Name = "Branch B", IsActive = true });

        var empAId = Guid.NewGuid();
        var empBId = Guid.NewGuid();
        context.Employees.Add(new Employee { Id = empAId, FullName = "Staff A", BranchId = BranchA });
        context.Employees.Add(new Employee { Id = empBId, FullName = "Staff B", BranchId = BranchB });

        context.Attendances.Add(new Attendance { Id = Guid.NewGuid(), BranchId = BranchA, EmployeeId = empAId, EmployeeName = "Staff A", CheckInTime = DateTime.UtcNow });
        context.Attendances.Add(new Attendance { Id = Guid.NewGuid(), BranchId = BranchB, EmployeeId = empBId, EmployeeName = "Staff B", CheckInTime = DateTime.UtcNow });
        await context.SaveChangesAsync();

        var controller = CreateAttendanceController(context, "admin", null);

        var result = await controller.GetAttendances(null, null, null);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var list = okResult.Value as System.Collections.IEnumerable;
        int count = 0;
        foreach (var item in list!) count++;
        Assert.Equal(2, count);
    }

    private static AttendanceController CreateAttendanceController(ApplicationDbContext context, string role, Guid? branchId)
    {
        var controller = new AttendanceController(context);
        SetUser(controller, role, branchId);
        return controller;
    }

    private static WorkScheduleController CreateWorkScheduleController(ApplicationDbContext context, string role, Guid? branchId)
    {
        var controller = new WorkScheduleController(context);
        SetUser(controller, role, branchId);
        return controller;
    }

    private static ExpenseController CreateExpenseController(ApplicationDbContext context, string role, Guid? branchId)
    {
        var controller = new ExpenseController(context, Mock.Of<IDashboardService>());
        SetUser(controller, role, branchId);
        return controller;
    }

    private static void SetUser(ControllerBase controller, string role, Guid? branchId)
    {
        var claims = new List<Claim> { new(ClaimTypes.Role, role) };
        if (branchId.HasValue)
            claims.Add(new Claim("branchId", branchId.Value.ToString()));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"))
            }
        };
    }
}
