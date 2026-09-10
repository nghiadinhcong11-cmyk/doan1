using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using RestaurantPOS.Application.Services;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.WebAPI.Controllers;

namespace RestaurantPOS.Tests;

public sealed class TableReservationHardeningTests
{
    private static readonly Guid BranchA = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid BranchB = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    #region DiningTable Tests

    [Fact]
    public async Task Manager_A_cannot_see_tables_from_Branch_B()
    {
        var tableService = new Mock<ITableService>();
        var controller = CreateTableController(tableService, "manager", BranchA);

        var result = await controller.GetTables(null, null, null, BranchB);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task Admin_can_see_all_tables()
    {
        var tableService = new Mock<ITableService>();
        var controller = CreateTableController(tableService, "admin", null);

        var result = await controller.GetTables(null, null, null, null);

        Assert.IsType<OkObjectResult>(result);
        tableService.Verify(x => x.GetTablesAsync(null, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool?>()), Times.Once);
    }

    [Fact]
    public async Task Guest_can_see_Branch_A_tables_if_providing_BranchId()
    {
        var tableService = new Mock<ITableService>();
        var controller = CreateTableController(tableService, null, null); // Anonymous

        var result = await controller.GetTables(null, null, null, BranchA);

        Assert.IsType<OkObjectResult>(result);
        tableService.Verify(x => x.GetTablesAsync(BranchA, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool?>()), Times.Once);
    }

    [Fact]
    public async Task Guest_cannot_see_all_tables_globally()
    {
        var tableService = new Mock<ITableService>();
        var controller = CreateTableController(tableService, null, null);

        var result = await controller.GetTables(null, null, null, null);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Manager_A_cannot_create_table_for_Branch_B()
    {
        var tableService = new Mock<ITableService>();
        var controller = CreateTableController(tableService, "manager", BranchA);
        var table = new RestaurantTable { BranchId = BranchB, Name = "Table B1" };

        var result = await controller.CreateTable(table);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task Manager_A_cannot_delete_Branch_B_table()
    {
        var tableService = new Mock<ITableService>();
        tableService.Setup(x => x.DeleteTableAsync(It.IsAny<Guid>(), BranchA)).ReturnsAsync(false);
        var controller = CreateTableController(tableService, "manager", BranchA);
        var tableId = Guid.NewGuid();

        var result = await controller.DeleteTable(tableId);

        Assert.IsType<NotFoundResult>(result); // Because service returns false due to branch mismatch or not found
    }

    #endregion

    #region Reservation Tests

    [Fact]
    public async Task Manager_A_cannot_see_reservations_of_Branch_B()
    {
        var resService = new Mock<IReservationService>();
        var controller = CreateReservationController(resService, "manager", BranchA);

        var result = await controller.GetReservations(BranchB, null, null, null, null);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task Customer_can_only_see_their_own_reservations()
    {
        var resService = new Mock<IReservationService>();
        var customerPhone = "0900111222";
        var controller = CreateCustomerReservationController(resService, customerPhone);

        var result = await controller.GetReservations(null, null, "0900999999", null, null);

        Assert.IsType<OkObjectResult>(result);
        resService.Verify(x => x.GetReservationsAsync(It.Is<ReservationQueryFilter>(f => f.CustomerPhone == customerPhone)), Times.Once);
    }

    [Fact]
    public async Task Guest_can_create_reservation_but_status_is_forced_to_Pending()
    {
        var resService = new Mock<IReservationService>();
        var controller = CreateReservationController(resService, null, null);
        var res = new Reservation { CustomerName = "Guest", Status = "Confirmed", BranchId = BranchA };

        await controller.CreateReservation(res);

        resService.Verify(x => x.CreateReservationAsync(It.Is<Reservation>(r => r.Status == "Pending")), Times.Once);
    }

    [Fact]
    public async Task Staff_cannot_spoof_BranchId_in_CreateReservation()
    {
        var resService = new Mock<IReservationService>();
        var controller = CreateReservationController(resService, "manager", BranchA);
        var res = new Reservation { BranchId = BranchB, CustomerName = "Spoofed" };

        var result = await controller.CreateReservation(res);

        Assert.IsType<ForbidResult>(result);
        resService.Verify(x => x.CreateReservationAsync(It.IsAny<Reservation>()), Times.Never);
    }

    [Fact]
    public async Task Manager_A_cannot_update_Branch_B_reservation()
    {
        var resService = new Mock<IReservationService>();
        var controller = CreateReservationController(resService, "manager", BranchA);
        var resId = Guid.NewGuid();
        var update = new Reservation { Id = resId, BranchId = BranchB };

        resService.Setup(x => x.UpdateReservationAsync(resId, It.IsAny<Reservation>(), BranchA, null))
                  .ReturnsAsync((Reservation?)null);

        var result = await controller.UpdateReservation(resId, update);

        Assert.IsType<NotFoundResult>(result);
    }

    #endregion

    #region ReservationService Branch Validation Tests

    [Fact]
    public async Task ReservationService_CreateReservation_Throws_If_Table_Branch_Mismatches()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;

        context.Branches.AddRange(
            new Branch { Id = BranchA, Name = "Branch A" },
            new Branch { Id = BranchB, Name = "Branch B" }
        );
        var table = new RestaurantTable { Id = Guid.NewGuid(), BranchId = BranchB, Name = "Table B", AreaName = "Main" };
        context.Tables.Add(table);
        await context.SaveChangesAsync();

        var service = new ReservationService(context);
        var res = new Reservation
        {
            BranchId = BranchA,
            TableId = table.Id,
            ReservationTime = DateTime.UtcNow.AddDays(1)
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateReservationAsync(res));
    }

    [Fact]
    public async Task ReservationService_UpdateReservation_Throws_If_Table_Branch_Mismatches()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;

        context.Branches.AddRange(
            new Branch { Id = BranchA, Name = "Branch A" },
            new Branch { Id = BranchB, Name = "Branch B" }
        );
        var tableB = new RestaurantTable { Id = Guid.NewGuid(), BranchId = BranchB, Name = "Table B", AreaName = "Main" };
        var resA = new Reservation { Id = Guid.NewGuid(), BranchId = BranchA, ReservationTime = DateTime.UtcNow.AddDays(1) };

        context.Tables.Add(tableB);
        context.Reservations.Add(resA);
        await context.SaveChangesAsync();

        var service = new ReservationService(context);
        var update = new Reservation { TableId = tableB.Id };

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateReservationAsync(resA.Id, update, null, null));
    }

    [Fact]
    public async Task ReservationService_CreateReservation_Automatically_Sets_Branch_From_Table()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;

        context.Branches.Add(new Branch { Id = BranchB, Name = "Branch B" });
        var table = new RestaurantTable { Id = Guid.NewGuid(), BranchId = BranchB, Name = "Table B", BranchName = "Branch B", AreaName = "Main" };
        context.Tables.Add(table);
        await context.SaveChangesAsync();

        var service = new ReservationService(context);
        var res = new Reservation
        {
            TableId = table.Id,
            ReservationTime = DateTime.UtcNow.AddDays(1)
        };

        var result = await service.CreateReservationAsync(res);

        Assert.Equal(BranchB, result.BranchId);
        Assert.Equal("Branch B", result.BranchName);
    }

    #endregion

    private static TableController CreateTableController(Mock<ITableService> tableService, string? role, Guid? branchId)
    {
        var claims = new List<Claim>();
        if (role != null) claims.Add(new Claim(ClaimTypes.Role, role));
        if (branchId.HasValue) claims.Add(new Claim("branchId", branchId.Value.ToString()));

        var identity = role != null ? new ClaimsIdentity(claims, "Bearer") : new ClaimsIdentity();

        return new TableController(tableService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(identity)
                }
            }
        };
    }

    private static ReservationController CreateReservationController(Mock<IReservationService> resService, string? role, Guid? branchId)
    {
        var claims = new List<Claim>();
        if (role != null) claims.Add(new Claim(ClaimTypes.Role, role));
        if (branchId.HasValue) claims.Add(new Claim("branchId", branchId.Value.ToString()));

        var identity = role != null ? new ClaimsIdentity(claims, "Bearer") : new ClaimsIdentity();

        return new ReservationController(resService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(identity)
                }
            }
        };
    }

    private static ReservationController CreateCustomerReservationController(Mock<IReservationService> resService, string phone)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Role, "customer"),
            new(ClaimTypes.Name, phone)
        };
        return new ReservationController(resService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"))
                }
            }
        };
    }
}
