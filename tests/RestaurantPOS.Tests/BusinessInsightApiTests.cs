using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Infrastructure.Persistence;
using RestaurantPOS.WebAPI.Controllers;
using Xunit;

namespace RestaurantPOS.Tests;

public sealed class BusinessInsightApiTests
{
    private static readonly Guid BranchA = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid BranchB = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    [Fact]
    public async Task Admin_CanAccessAll()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context; await using var __ = connection;
        context.BusinessInsights.Add(new BusinessInsight { Id = Guid.NewGuid(), Title = "Admin Test", BranchId = BranchA, DeduplicationKey = "D1", Severity = "Info", Type = "T1", Summary = "S1" });
        await context.SaveChangesAsync();

        var controller = Create(context, "admin", null);
        var result = await controller.Get(null, null);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var resultText = JsonSerializer.Serialize(okResult.Value);
        Assert.Contains("Admin Test", resultText);
    }

    [Fact]
    public async Task Manager_IsBranchIsolated()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context; await using var __ = connection;
        context.BusinessInsights.Add(new BusinessInsight { Id = Guid.NewGuid(), Title = "My Branch", BranchId = BranchA, DeduplicationKey = "D1", Severity = "Info", Type = "T1", Summary = "S1" });
        context.BusinessInsights.Add(new BusinessInsight { Id = Guid.NewGuid(), Title = "Other Branch", BranchId = BranchB, DeduplicationKey = "D2", Severity = "Info", Type = "T1", Summary = "S2" });
        await context.SaveChangesAsync();

        var controller = Create(context, "manager", BranchA);
        var result = await controller.Get(null, null);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var resultText = JsonSerializer.Serialize(okResult.Value);
        Assert.Contains("My Branch", resultText);
        Assert.DoesNotContain("Other Branch", resultText);
    }

    [Fact]
    public async Task MarkRead_UpdatesStatus()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context; await using var __ = connection;
        var id = Guid.NewGuid();
        context.BusinessInsights.Add(new BusinessInsight { Id = id, Title = "Test", BranchId = BranchA, DeduplicationKey = "D1", Severity = "Info", Type = "T1", Summary = "S1", Status = "Unread" });
        await context.SaveChangesAsync();

        var controller = Create(context, "manager", BranchA);
        var result = await controller.MarkRead(id);

        Assert.IsType<NoContentResult>(result);

        var updated = context.BusinessInsights.First(i => i.Id == id);
        Assert.Equal("Read", updated.Status);
    }

    private static BusinessInsightController Create(ApplicationDbContext context, string role, Guid? branch)
    {
        var controller = new BusinessInsightController(context);
        var claims = new List<Claim> { new Claim(ClaimTypes.Role, role) };
        if (branch.HasValue) claims.Add(new Claim("branchId", branch.Value.ToString()));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"))
            }
        };
        return controller;
    }
}
