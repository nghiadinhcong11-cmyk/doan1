using Microsoft.EntityFrameworkCore;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Domain.Inventory;
using RestaurantPOS.Infrastructure.Persistence;

namespace RestaurantPOS.Tests;

public sealed class InventoryDomainValidationTests
{
    [Theory]
    [InlineData("kg")]
    [InlineData("g")]
    [InlineData("litre")]
    [InlineData("ml")]
    [InlineData("piece")]
    [InlineData("box")]
    [InlineData("bottle")]
    [InlineData("pack")]
    public void Approved_units_are_valid(string unitCode)
    {
        Assert.True(InventoryUnitCatalog.IsValid(unitCode));
    }

    [Theory]
    [InlineData("")]
    [InlineData("kilogram")]
    [InlineData("unit")]
    [InlineData("kg per bag")]
    public void Arbitrary_units_are_rejected(string unitCode)
    {
        Assert.False(InventoryUnitCatalog.IsValid(unitCode));
    }

    [Theory]
    [InlineData("piece")]
    [InlineData("box")]
    [InlineData("bottle")]
    [InlineData("pack")]
    public void Discrete_units_are_identified(string unitCode)
    {
        Assert.True(InventoryUnitCatalog.IsDiscrete(unitCode));
    }

    [Theory]
    [InlineData("kg", 2.5)]
    [InlineData("litre", 0.750)]
    [InlineData("piece", 2)]
    public void Valid_quantities_are_accepted(string unitCode, decimal quantity)
    {
        Assert.True(InventoryQuantityRules.IsValidPositive(unitCode, quantity));
    }

    [Theory]
    [InlineData("piece", 2.5)]
    [InlineData("box", 0.5)]
    [InlineData("kg", 0)]
    [InlineData("kg", -1)]
    [InlineData("unknown", 1)]
    public void Invalid_quantities_are_rejected(string unitCode, decimal quantity)
    {
        Assert.False(InventoryQuantityRules.IsValidPositive(unitCode, quantity));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2.5)]
    public void Non_negative_balance_values_are_accepted(decimal quantity)
    {
        Assert.True(InventoryQuantityRules.IsValidNonNegative(quantity));
    }

    [Fact]
    public void Negative_balance_values_are_rejected()
    {
        Assert.False(InventoryQuantityRules.IsValidNonNegative(-0.001m));
    }

    [Fact]
    public void Inventory_item_name_normalization_collapses_case_and_whitespace()
    {
        Assert.Equal("CÀ PHÊ", InventoryItem.NormalizeName("  cà   phê "));
    }

    [Fact]
    public void New_documents_default_to_draft()
    {
        Assert.Equal(StockDocumentStatus.Draft, new StockReceipt().Status);
        Assert.Equal(StockDocumentStatus.Draft, new StockIssue().Status);
    }

    [Fact]
    public void Employee_metadata_retains_employee_type()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        using var context = new ApplicationDbContext(options);
        var property = context.Model.FindEntityType(typeof(Employee))?.FindProperty(nameof(Employee.EmployeeType));

        Assert.NotNull(property);
        Assert.Equal(typeof(string), property!.ClrType);
        Assert.False(property.IsNullable);
    }
}
