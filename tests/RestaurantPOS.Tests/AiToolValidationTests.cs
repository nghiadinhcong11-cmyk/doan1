using System;
using System.Text.Json;
using System.Threading.Tasks;
using Moq;
using RestaurantPOS.AI.Models;
using RestaurantPOS.AI.Security;
using RestaurantPOS.AI.Tools.Admin;
using RestaurantPOS.AI.Tools.Employee;
using RestaurantPOS.AI.Utils;
using RestaurantPOS.Application.Services;
using Xunit;

namespace RestaurantPOS.Tests;

public class AiToolValidationTests
{
    [Fact]
    public void ValidateRequired_Throws_WhenMissingProperty()
    {
        var json = "{\"name\": \"test\"}";
        var doc = JsonDocument.Parse(json);

        Assert.Throws<ArgumentException>(() =>
            AiToolValidator.ValidateRequired(doc.RootElement, "age"));
    }

    [Fact]
    public void GetString_Throws_WhenEmptyAndRequired()
    {
        var json = "{\"name\": \"\"}";
        var doc = JsonDocument.Parse(json);

        Assert.Throws<ArgumentException>(() =>
            AiToolValidator.GetString(doc.RootElement, "name"));
    }

    [Fact]
    public void GetDecimal_Throws_WhenWrongType()
    {
        var json = "{\"price\": \"100\"}";
        var doc = JsonDocument.Parse(json);

        Assert.Throws<ArgumentException>(() =>
            AiToolValidator.GetDecimal(doc.RootElement, "price"));
    }

    [Fact]
    public void GetDecimal_Throws_WhenOutOfRange()
    {
        var json = "{\"price\": -10}";
        var doc = JsonDocument.Parse(json);

        Assert.Throws<ArgumentException>(() =>
            AiToolValidator.GetDecimal(doc.RootElement, "price", min: 0));
    }

    [Fact]
    public void GetGuid_Throws_WhenInvalidFormat()
    {
        var json = "{\"id\": \"not-a-guid\"}";
        var doc = JsonDocument.Parse(json);

        Assert.Throws<ArgumentException>(() =>
            AiToolValidator.GetGuid(doc.RootElement, "id"));
    }

    [Fact]
    public void ValidateBranchIsolation_Throws_WhenBranchMismatch()
    {
        var userContext = new AiUserContext { Role = "employee", BranchId = Guid.NewGuid() };
        var otherBranch = Guid.NewGuid();

        Assert.Throws<UnauthorizedAccessException>(() =>
            AiToolValidator.ValidateBranchIsolation(otherBranch, userContext));
    }

    [Fact]
    public void ValidateBranchIsolation_Allows_ForAdmin()
    {
        var userContext = new AiUserContext { Role = "admin", BranchId = Guid.NewGuid() };
        var otherBranch = Guid.NewGuid();

        // Should not throw
        AiToolValidator.ValidateBranchIsolation(otherBranch, userContext);
    }

    [Fact]
    public async Task UpdateProductPriceTool_ReturnsError_OnInvalidInput()
    {
        // Arrange
        var productService = new Mock<IProductService>();
        var tool = new UpdateProductPriceTool(productService.Object);
        var json = "{\"productName\": \"\", \"newPrice\": -1}";
        var doc = JsonDocument.Parse(json);
        var userContext = new AiUserContext { Role = "admin" };

        // Act
        var result = await tool.ExecuteAsync(doc.RootElement, userContext);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("không được để trống", result.Error);
    }

    [Fact]
    public async Task UpdateOrderStatusTool_ReturnsError_OnMissingOrderCode()
    {
        // Arrange
        var orderService = new Mock<IOrderService>();
        var tool = new UpdateOrderStatusTool(orderService.Object);
        var json = "{\"newStatus\": \"Completed\"}";
        var doc = JsonDocument.Parse(json);
        var userContext = new AiUserContext { Role = "employee", BranchId = Guid.NewGuid() };

        // Act
        var result = await tool.ExecuteAsync(doc.RootElement, userContext);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Thiếu tham số bắt buộc: orderCode", result.Error);
    }
}
