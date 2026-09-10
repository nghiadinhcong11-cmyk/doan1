using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Infrastructure.Persistence;
using RestaurantPOS.WebAPI.Controllers;

namespace RestaurantPOS.Tests;

public sealed class AttendanceControllerTests
{
    [Fact]
    public async Task Check_in_uses_jwt_employee_and_rejects_duplicate()
    {
        var (db, connection) = TestDbContextFactory.Create();
        await using var _ = connection;
        var branchId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        db.Branches.Add(new Branch { Id = branchId, Name = "A" });
        db.Employees.Add(new Employee { Id = employeeId, FullName = "Nhân viên", BranchId = branchId, IsActive = true });
        await db.SaveChangesAsync();
        var controller = Create(db, employeeId, branchId, "cashier");
        var qr = $"{{\"type\":\"ATTENDANCE_POINT\",\"branchId\":\"{branchId}\"}}";

        var first = await controller.CheckIn(new AttendanceController.CheckInRequest { QrPayload = qr });
        var second = await controller.CheckIn(new AttendanceController.CheckInRequest { QrPayload = qr });

        Assert.IsType<OkObjectResult>(first);
        Assert.IsType<ConflictObjectResult>(second);
        Assert.Single(db.Attendances);
        Assert.Equal(employeeId, db.Attendances.Single().EmployeeId);
    }

    [Fact]
    public async Task Check_in_rejects_wrong_branch_qr()
    {
        var (db, connection) = TestDbContextFactory.Create();
        await using var _ = connection;
        var ownBranch = Guid.NewGuid();
        var otherBranch = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        db.Branches.AddRange(new Branch { Id = ownBranch, Name = "A" }, new Branch { Id = otherBranch, Name = "B" });
        db.Employees.Add(new Employee { Id = employeeId, FullName = "Nhân viên", BranchId = ownBranch, IsActive = true });
        await db.SaveChangesAsync();
        var controller = Create(db, employeeId, ownBranch, "employee");

        var result = await controller.CheckIn(new AttendanceController.CheckInRequest {
            QrPayload = $"{{\"type\":\"ATTENDANCE_POINT\",\"branchId\":\"{otherBranch}\"}}"
        });

        Assert.IsType<ObjectResult>(result);
        Assert.Equal(403, ((ObjectResult)result).StatusCode);
        Assert.Empty(db.Attendances);
    }

    [Fact]
    public async Task Check_out_without_check_in_and_double_check_out_are_rejected()
    {
        var (db, connection) = TestDbContextFactory.Create();
        await using var _ = connection;
        var branchId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        db.Branches.Add(new Branch { Id = branchId, Name = "A" });
        db.Employees.Add(new Employee { Id = employeeId, FullName = "Nhân viên", BranchId = branchId, IsActive = true });
        await db.SaveChangesAsync();
        var controller = Create(db, employeeId, branchId, "cashier");
        var missing = await controller.CheckOut(Guid.NewGuid(), new AttendanceController.CheckInRequest());
        Assert.IsType<NotFoundResult>(missing);

        var attendance = new Attendance { Id = Guid.NewGuid(), EmployeeId = employeeId, BranchId = branchId, CheckInTime = DateTime.UtcNow };
        db.Attendances.Add(attendance);
        await db.SaveChangesAsync();
        var qr = $"{{\"type\":\"ATTENDANCE_POINT\",\"branchId\":\"{branchId}\"}}";
        Assert.IsType<OkObjectResult>(await controller.CheckOut(attendance.Id, new AttendanceController.CheckInRequest { QrPayload = qr }));
        Assert.IsType<ConflictObjectResult>(await controller.CheckOut(attendance.Id, new AttendanceController.CheckInRequest { QrPayload = qr }));
    }

    private static AttendanceController Create(ApplicationDbContext db, Guid userId, Guid branchId, string role)
    {
        var controller = new AttendanceController(db) { ControllerContext = new ControllerContext {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, role), new Claim("branchId", branchId.ToString())
            }, "test"))
        }}};
        return controller;
    }
}
