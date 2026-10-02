using System.Text.Json;
using RestaurantPOS.AI.Models;
using RestaurantPOS.AI.Security;
using RestaurantPOS.AI.Tools;
using RestaurantPOS.AI.Tools.Admin;
using RestaurantPOS.AI.Tools.Customer;
using RestaurantPOS.AI.Tools.Employee;
using RestaurantPOS.AI.Utils;
using RestaurantPOS.Application.Services;
using Moq;
using RestaurantPOS.AI.Services;
using RestaurantPOS.Application.DTOs.Financial;
using RestaurantPOS.Application.DTOs.Orders;

namespace RestaurantPOS.Tests;

public sealed class AiSecurityAuditTests
{
    private readonly Mock<IDashboardService> _dashboardMock = new();
    private readonly Mock<IOrderService> _orderMock = new();
    private readonly Mock<IProductService> _productMock = new();
    private readonly Mock<IEmployeeService> _employeeMock = new();
    private readonly Mock<ITableService> _tableMock = new();
    private readonly Mock<IReservationService> _reservationMock = new();
    private readonly Mock<IFinancialAnalysisService> _financialMock = new();

    private readonly AiAuthorization _auth = new();

    [Theory]
    [InlineData("customer", "customer_get_menu", true)]
    [InlineData("customer", "get_my_orders", true)]
    [InlineData("customer", "create_booking", true)]
    [InlineData("customer", "get_revenue", false)]
    [InlineData("cashier", "customer_get_menu", true)]
    [InlineData("cashier", "get_order_list", true)]
    [InlineData("cashier", "update_order_status", true)]
    [InlineData("cashier", "get_revenue", false)]
    [InlineData("kitchen", "get_order_list", true)]
    [InlineData("kitchen", "update_order_status", true)]
    [InlineData("kitchen", "get_revenue", false)]
    [InlineData("employee", "get_my_shift", true)]
    [InlineData("employee", "get_revenue", false)]
    [InlineData("manager", "get_revenue", true)]
    [InlineData("manager", "update_product_price", false)]
    [InlineData("admin", "update_product_price", true)]
    public void Tool_RBAC_ShouldEnforceCorrectRoles(string role, string toolName, bool shouldAllow)
    {
        var tool = GetToolByName(toolName);
        var context = new AiUserContext { Role = role };

        bool isAllowed = _auth.IsRoleAllowed(tool, context);

        Assert.Equal(shouldAllow, isAllowed);
    }

    [Fact]
    public void Validator_ShouldHandleNullArgumentsGracefully()
    {
        var arguments = JsonDocument.Parse("null").RootElement;

        var ex = Assert.Throws<ArgumentException>(() => AiToolValidator.ValidateRequired(arguments, "someProp"));
        Assert.Contains("Thiếu toàn bộ tham số bắt buộc", ex.Message);
    }

    [Fact]
    public void Validator_ShouldHandleEmptyObjectForRequiredProps()
    {
        var arguments = JsonDocument.Parse("{}").RootElement;

        var ex = Assert.Throws<ArgumentException>(() => AiToolValidator.ValidateRequired(arguments, "someProp"));
        Assert.Contains("Thiếu tham số bắt buộc: someProp", ex.Message);
    }

    [Fact]
    public void Validator_ShouldHandleNonObjectJson()
    {
        var arguments = JsonDocument.Parse("[\"item\"]").RootElement;

        var ex = Assert.Throws<ArgumentException>(() => AiToolValidator.ValidateRequired(arguments, "someProp"));
        Assert.Contains("Cấu trúc tham số không hợp lệ", ex.Message);
    }

    [Fact]
    public void Validator_BranchIsolation_ShouldAllowAdmin()
    {
        var context = new AiUserContext { Role = "admin", BranchId = Guid.NewGuid() };
        var otherBranchId = Guid.NewGuid();

        // Should not throw
        AiToolValidator.ValidateBranchIsolation(otherBranchId, context);
    }

    [Fact]
    public void Validator_BranchIsolation_ShouldDenyOtherBranchForManager()
    {
        var myBranch = Guid.NewGuid();
        var context = new AiUserContext { Role = "manager", BranchId = myBranch };
        var otherBranch = Guid.NewGuid();

        Assert.Throws<UnauthorizedAccessException>(() => AiToolValidator.ValidateBranchIsolation(otherBranch, context));
    }

    private IAiTool GetToolByName(string name)
    {
        return name switch
        {
            "get_revenue" => new GetRevenueTool(_dashboardMock.Object),
            "get_active_staff" => new GetActiveStaffTool(_employeeMock.Object),
            "update_product_price" => new UpdateProductPriceTool(_productMock.Object),
            "customer_get_menu" => new GetMenuTool(_productMock.Object),
            "get_my_orders" => new GetMyOrderTool(_orderMock.Object),
            "create_booking" => new CreateBookingTool(_reservationMock.Object),
            "get_my_shift" => new GetMyShiftTool(_employeeMock.Object),
            "employee_get_table_summary" => new GetTableSummaryTool(_tableMock.Object),
            "update_order_status" => new UpdateOrderStatusTool(_orderMock.Object),
            "get_order_list" => new GetOrderListTool(_orderMock.Object),
            "get_best_sellers" => new GetBestSellersTool(_dashboardMock.Object),
            "get_revenue_comparison" => new GetRevenueComparisonTool(_dashboardMock.Object),
            "get_business_summary" => new GetBusinessSummaryTool(_dashboardMock.Object),
            "get_financial_analysis" => new GetFinancialAnalysisTool(_financialMock.Object),
            _ => throw new ArgumentException("Unknown tool")
        };
    }
}
