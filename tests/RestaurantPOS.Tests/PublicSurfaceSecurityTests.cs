using Microsoft.AspNetCore.Mvc;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Infrastructure.Persistence;
using RestaurantPOS.WebAPI.Controllers;

namespace RestaurantPOS.Tests;

public sealed class PublicSurfaceSecurityTests
{
    [Fact]
    public async Task Public_branch_list_returns_only_active_customer_safe_fields()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;
        context.Branches.AddRange(
            new Branch { Id = Guid.NewGuid(), Name = "Public", IsActive = true, BankName = "internal-bank", AccountNumber = "secret" },
            new Branch { Id = Guid.NewGuid(), Name = "Inactive", IsActive = false });
        await context.SaveChangesAsync();

        var result = await new BranchController(context).GetBranches();

        var ok = Assert.IsType<OkObjectResult>(result);
        var branches = Assert.IsAssignableFrom<IEnumerable<BranchPublicDto>>(ok.Value);
        var branch = Assert.Single(branches);
        Assert.Equal("Public", branch.Name);
        Assert.DoesNotContain(typeof(BranchPublicDto).GetProperties(), property => property.Name is "BankName" or "AccountNumber" or "TaxCode");
    }
}
