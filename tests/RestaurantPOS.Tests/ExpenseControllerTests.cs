using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RestaurantPOS.Application.DTOs.Expenses;
using RestaurantPOS.Application.Services;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Infrastructure.Persistence;
using RestaurantPOS.WebAPI.Controllers;

namespace RestaurantPOS.Tests;

public sealed class ExpenseControllerTests
{
    private static readonly Guid BranchA = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid BranchB = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    [Fact]
    public async Task Employee_can_create_expense_only_for_own_branch()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context; await using var __ = connection;
        context.Branches.Add(new Branch { Id = BranchA, Name = "A", IsActive = true });
        await context.SaveChangesAsync();
        var controller = Create(context, "employee", BranchA);

        var result = await controller.CreateExpense(new ExpenseCreateDto { BranchId = BranchA, Category = "Nguyên liệu", Description = "Mua đường", Amount = 500000, ExpenseDate = DateTime.UtcNow });

        Assert.IsType<CreatedAtActionResult>(result);
        Assert.Single(context.Expenses);
    }

    [Fact]
    public async Task Employee_cannot_create_or_read_expense_from_another_branch()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context; await using var __ = connection;
        context.Branches.AddRange(new Branch { Id = BranchA, Name = "A", IsActive = true }, new Branch { Id = BranchB, Name = "B", IsActive = true });
        await context.SaveChangesAsync();
        var controller = Create(context, "employee", BranchA);

        Assert.IsType<ForbidResult>(await controller.CreateExpense(new ExpenseCreateDto { BranchId = BranchB, Category = "Gas", Description = "Gas", Amount = 100, ExpenseDate = DateTime.UtcNow }));
        Assert.IsType<ForbidResult>(await controller.GetExpenses(BranchB, null, null, null));
    }

    [Theory]
    [InlineData(0, "Amount must be greater than zero.")]
    [InlineData(-1, "Amount must be greater than zero.")]
    public async Task Invalid_amount_is_rejected(decimal amount, string message)
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context; await using var __ = connection;
        context.Branches.Add(new Branch { Id = BranchA, Name = "A", IsActive = true }); await context.SaveChangesAsync();
        var result = await Create(context, "employee", BranchA).CreateExpense(new ExpenseCreateDto { BranchId = BranchA, Category = "Gas", Description = "Gas", Amount = amount, ExpenseDate = DateTime.UtcNow });
        var bad = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(message, bad.Value);
    }

    private static ExpenseController Create(ApplicationDbContext context, string role, Guid branch)
    {
        var controller = new ExpenseController(context, Mock.Of<IDashboardService>());
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, role), new Claim("branchId", branch.ToString()) }, "Bearer")) } };
        return controller;
    }
}
