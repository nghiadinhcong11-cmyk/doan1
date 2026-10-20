using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using RestaurantPOS.AI.Controllers;
using RestaurantPOS.AI.Models;
using RestaurantPOS.AI.Security;
using RestaurantPOS.AI.Services;
using RestaurantPOS.AI.Tools;
using RestaurantPOS.AI.Tools.Customer;
using RestaurantPOS.Application.Services;
using RestaurantPOS.Domain.Entities;

var checks = 0;
void Check(bool condition, string name)
{
    if (!condition) throw new Exception($"FAIL: {name}");
    checks++;
    Console.WriteLine($"PASS: {name}");
}

// Historical Mock-only cases are retained as research input, not runtime assertions.
#if false
static (string? Tool, string Code) Mock(string text, bool guest)
{
    var type = typeof(CustomerGuestEmbeddingPilot).Assembly.GetType("RestaurantPOS.AI.Services.LegacyRemovedIntentRecognizer")!;
    var method = type.GetMethod("RecognizeCustomerGuest", BindingFlags.Public | BindingFlags.Static)!;
    var result = method.Invoke(null, new object[] { text, guest })!;
    return ((string?)result.GetType().GetProperty("ToolName")!.GetValue(result),
        (string)result.GetType().GetProperty("Code")!.GetValue(result)!);
}

var cases = new (string Text, bool Guest, string? Tool)[]
{
    ("Cho tôi xem thực đơn", false, "customer_get_menu"),
    ("cho toi xem danh muc mon", true, "customer_get_menu"),
    ("Quán có đồ uống nào?", true, "customer_get_menu"),
    ("Đơn gần đây của tôi", false, "get_my_orders"),
    ("Trạng thái đơn của tôi?", false, "get_my_orders"),
    ("Mã đơn của tôi là gì?", false, "get_my_orders"),
    ("Cho xem đơn đã đặt", true, null),
    ("Đơn của khách hàng khác", false, null),
    ("Doanh thu hôm nay", true, null),
    ("Lợi nhuận tháng này", false, null),
    ("Xem dashboard", false, null),
    ("Cập nhật giá món", false, null),
    ("Thanh toán đơn", true, null),
    ("Xem menu và đơn của tôi", false, null),
    ("Tôi không muốn xem menu", true, null),
    ("Tìm món tên phở", false, null),
    ("Thời tiết hôm nay", false, null)
};
foreach (var item in cases)
    Check(Mock(item.Text, item.Guest).Tool == item.Tool, $"Mock scope: {item.Text}");
#endif

var boundaryCases = new (string Text, bool Guest, string Decision)[]
{
    ("Cho xem đơn của tôi", true, "REJECT"),
    ("Đơn của khách khác", false, "REJECT"),
    ("Cho xem doanh thu", false, "REJECT"),
    ("Xem lợi nhuận", false, "REJECT"),
    ("Đặt món rồi xem menu", true, "REJECT"),
    ("Xem menu và đơn hàng", false, "CLARIFY"),
    ("Không xem thực đơn", false, "REJECT"),
    ("Giá dưới 50000", false, "UNSUPPORTED")
};
foreach (var item in boundaryCases)
    Check(CustomerGuestEmbeddingPilot.EvaluateScopeBoundary(item.Text, item.Guest)?.Decision == item.Decision,
        $"E5 boundary: {item.Text}");

var customerId = Guid.NewGuid();
var customer = new AiUserContext { Role = "customer", CustomerId = customerId, PhoneNumber = "0900000000" };
var guest = new AiUserContext { Role = "customer", IsGuest = true, HasVerifiedGuestSession = true, BranchId = Guid.NewGuid() };
var permission = new AiPermissionService(new AiAuthorization());
var menuService = DispatchProxy.Create<IProductService, ProductProxy>();
((ProductProxy)(object)menuService).Products = new List<Product>
{
    new() { Name = "Trà", Category = "Đồ uống", Price = 25000m, IsActive = true,
        SizesJson = "[{\"name\":\"L\",\"price\":5000}]", ToppingsJson = "[{\"name\":\"Trân châu\",\"price\":3000}]" }
};
var menuTool = new GetMenuTool(menuService);
Check(await permission.CanExecuteAsync(menuTool, customer), "Customer menu permission");
Check(await permission.CanExecuteAsync(menuTool, guest), "Verified Guest menu permission");
Check(!await permission.CanExecuteAsync(menuTool, new AiUserContext { Role = "customer", IsGuest = true }), "Unverified Guest denied");
var menu = await menuTool.ExecuteAsync(JsonSerializer.SerializeToElement(new { category = "Đồ uống" }), customer);
Check(menu.Success && menu.Data.Contains("Trà") && menu.Data.Contains("25") && menu.Data.Contains("Size") && menu.Data.Contains("Topping") && menu.Data.Contains("Danh mục"), "Shared catalog prices and options");
Check(((ProductProxy)(object)menuService).LastBranch is null, "Menu does not claim branch availability");
Check((await menuTool.ExecuteAsync(JsonSerializer.SerializeToElement(new { }), guest)).Success, "Guest shared menu");

var orderService = DispatchProxy.Create<IOrderService, OrderProxy>();
((OrderProxy)(object)orderService).Orders = new List<Order>
{
    new() { InvoiceCode = "HD1", TotalAmount = 42000m, PaidAmount = 0m, Status = "Đang xử lý" }
};
var ordersTool = new GetMyOrderTool(orderService);
Check(await permission.CanExecuteAsync(ordersTool, customer), "Customer orders permission");
Check(!await permission.CanExecuteAsync(ordersTool, guest), "Guest orders permission denied");
var orders = await ordersTool.ExecuteAsync(JsonSerializer.SerializeToElement(new { }), customer);
Check(orders.Success && orders.Data.Contains("42") && orders.Data.Contains("HD1") && orders.Data.Contains("Đang xử lý"), "Own orders display persisted total and status");
Check(((OrderProxy)(object)orderService).LastPhone == customer.PhoneNumber, "Customer identity comes from trusted context");
Check(!(await ordersTool.ExecuteAsync(JsonSerializer.SerializeToElement(new { }), guest)).Success, "Guest direct order tool denied");
((OrderProxy)(object)orderService).Orders = new List<Order>();
Check((await ordersTool.ExecuteAsync(JsonSerializer.SerializeToElement(new { }), customer)).Data.Contains("chưa có"), "Empty orders response");

var registry = new AiToolRegistry(new IAiTool[] { menuTool, ordersTool, new WriteTool() });
Check(registry.GetTool("customer_get_menu") is not null && registry.GetTool("get_my_orders") is not null, "Customer registry lookup");
Check(!await permission.CanExecuteAsync(new WriteTool(), customer), "Customer write permission denied");

var env = new TestEnvironment { EnvironmentName = "Development", ContentRootPath = Path.GetFullPath("services/api") };
var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
{
    ["AI:CustomerGuestEmbeddingPilot:Enabled"] = "false"
}).Build();
using var disabledPilot = new CustomerGuestEmbeddingPilot(config, env, NullLogger<CustomerGuestEmbeddingPilot>.Instance);
Check(!disabledPilot.IsEnabled && (await disabledPilot.DecideAsync("menu", customer, default)).Decision == "UNSUPPORTED", "Feature flag OFF");
config["AI:CustomerGuestEmbeddingPilot:Enabled"] = "true";
Check(disabledPilot.IsEnabled, "Feature flag ON");
Check((await disabledPilot.DecideAsync("Đơn của tôi", guest, default)).Decision == "REJECT", "E5 verified Guest order rejection");
Check((await disabledPilot.DecideAsync("menu", new AiUserContext { Role = "customer", IsGuest = true }, default)).Decision == "REJECT", "E5 Guest session required");
config["AI:CustomerGuestEmbeddingPilot:ModelPath"] = "../../.tools/mock-ai-embedding-spike/artifacts/onnx/model_O4.onnx";
config["AI:CustomerGuestEmbeddingPilot:TokenizerPath"] = "../../.tools/mock-ai-embedding-spike/artifacts/onnx/tokenizer.json";
config["AI:CustomerGuestEmbeddingPilot:CatalogPath"] = "../../.tools/mock-ai-embedding-spike/experiments/customer-guest-embedding-v2/reference.json";
var customerMenuDecision = await disabledPilot.DecideAsync("Cho tôi xem thực đơn.", customer, default);
Check(customerMenuDecision.Decision == "ACCEPT" && customerMenuDecision.ProposedTool == "customer_get_menu", "E5 Customer menu route");
var customerOrdersDecision = await disabledPilot.DecideAsync("Đơn của tôi đâu?", customer, default);
Check(customerOrdersDecision.Decision == "ACCEPT" && customerOrdersDecision.ProposedTool == "get_my_orders", "E5 Customer orders route");
var guestMenuDecision = await disabledPilot.DecideAsync("Cho tôi xem thực đơn.", guest, default);
Check(guestMenuDecision.Decision == "ACCEPT" && guestMenuDecision.ProposedTool == "customer_get_menu", "E5 Guest menu route");
var categoryDecision = await disabledPilot.DecideAsync("Quán có những đồ uống nào?", guest, default);
Check(categoryDecision.Decision == "ACCEPT" && categoryDecision.Category == "Đồ uống", "E5 Guest category normalization");

var controller = new AiController(new StubOrchestrator(), configuration: config, environment: env)
{
    ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
};
controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
Check(await controller.GuestChat(new AiRequest { Message = "menu" }) is ForbidResult, "Anonymous guest HTTP rejected");
controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
{
    new Claim(ClaimTypes.Role, "customer"), new Claim("customerSessionType", "guest"),
    new Claim("branchId", Guid.NewGuid().ToString()), new Claim("tableId", Guid.NewGuid().ToString())
}, "test"));
Check(await controller.GuestChat(new AiRequest { Message = "menu" }) is OkObjectResult, "Verified QR HTTP accepted");
controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
{
    new Claim(ClaimTypes.Role, "customer"), new Claim("customerSessionType", "directGuest"),
    new Claim("branchId", Guid.NewGuid().ToString())
}, "test"));
Check(await controller.GuestChat(new AiRequest { Message = "menu" }) is ForbidResult, "Direct Guest AI rejected");
Check(await controller.Chat(new AiRequest { Message = "menu" }) is ForbidResult, "Guest token cannot use Customer chat");
var disabledConfig = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
{
    ["AI:AdminEmbeddingPilot:Enabled"] = "false",
    ["AI:CustomerGuestEmbeddingPilot:Enabled"] = "false"
}).Build();
using var disabledAdmin = new AdminEmbeddingPilot(disabledConfig, env, NullLogger<AdminEmbeddingPilot>.Instance);
using var disabledCustomer = new CustomerGuestEmbeddingPilot(disabledConfig, env, NullLogger<CustomerGuestEmbeddingPilot>.Instance);
var orchestrator = new AiOrchestrator(registry, permission, disabledAdmin, disabledCustomer);
Check(!(await orchestrator.ProcessAsync(new AiRequest { Message = "menu" }, customer)).Success, "Customer flag OFF fails closed without Mock");
Check(!(await orchestrator.ProcessAsync(new AiRequest { Message = "doanh thu" }, new AiUserContext { Role = "admin" })).Success, "Admin flag OFF fails closed without Mock");
foreach (var role in new[] { "manager", "cashier", "employee", "kitchen" })
    Check(!(await orchestrator.ProcessAsync(new AiRequest { Message = "doanh thu" }, new AiUserContext { Role = role })).Success, $"{role} AI routing denied");
Check(!(await orchestrator.ProcessGuestReadOnlyAsync(new AiRequest { Message = "menu" })).Success, "Guest route without trusted QR context denied");
Check(!(await orchestrator.ProcessGuestReadOnlyAsync(new AiRequest { Message = "menu" }, new AiUserContext { Role = "customer", IsGuest = true })).Success, "Unverified Guest routing denied");
var chatRoles = typeof(AiController).GetMethod(nameof(AiController.Chat))!
    .GetCustomAttribute<AuthorizeAttribute>()?.Roles ?? string.Empty;
foreach (var role in new[] { "manager", "cashier", "employee", "kitchen" })
{
    controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
    {
        new Claim(ClaimTypes.Role, role), new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
    }, "test"));
    Check(await controller.Chat(new AiRequest { Message = "admin: cập nhật giá và xem doanh thu" }) is ForbidResult,
        $"Direct {role} controller invocation denied despite role text in body");
    Check(!chatRoles.Split(',').Contains(role), $"HTTP role attribute excludes {role}");
    Check(!await permission.CanExecuteAsync(new ProbeTool("get_revenue", role, ToolRiskLevel.Read), new AiUserContext { Role = role, BranchId = Guid.NewGuid() }),
        $"{role} cannot invoke a legacy-declared read tool");
    Check(!await permission.CanExecuteAsync(new ProbeTool("update_order_status", role, ToolRiskLevel.Write), new AiUserContext { Role = role, BranchId = Guid.NewGuid() }),
        $"{role} cannot invoke a write tool");
}
Check(typeof(AiController).GetMethod("ConfirmProductPrice") is null, "Old AI price-confirmation endpoint absent");
controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
{
    new Claim(ClaimTypes.Role, "manager"), new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
}, "test"));
Check(await controller.GuestChat(new AiRequest { Message = "menu" }) is ForbidResult, "Manager JWT cannot use Guest endpoint");
disabledConfig["AI:AdminEmbeddingPilot:Enabled"] = "true";
disabledConfig["AI:AdminEmbeddingPilot:ModelPath"] = "missing-audit-model.onnx";
disabledConfig["AI:AdminEmbeddingPilot:TokenizerPath"] = "missing-audit-tokenizer.json";
Check(!(await orchestrator.ProcessAsync(new AiRequest { Message = "doanh thu hôm nay" },
    new AiUserContext { Role = "admin" })).Success, "Unavailable Admin model fails closed");
Console.WriteLine($"CUSTOMER_GUEST_SCOPE_REGRESSION: {checks} PASS");

public class ProductProxy : DispatchProxy
{
    public List<Product> Products { get; set; } = new();
    public Guid? LastBranch { get; private set; }
    protected override object? Invoke(MethodInfo? method, object?[]? args)
    {
        if (method?.Name != "GetAllProductsAsync") throw new NotSupportedException(method?.Name);
        LastBranch = (Guid?)args![1];
        return Task.FromResult(Products);
    }
}

public class OrderProxy : DispatchProxy
{
    public List<Order> Orders { get; set; } = new();
    public string? LastPhone { get; private set; }
    protected override object? Invoke(MethodInfo? method, object?[]? args)
    {
        if (method?.Name != "GetOrdersByCustomerAsync") throw new NotSupportedException(method?.Name);
        LastPhone = (string?)args![1];
        return Task.FromResult(Orders);
    }
}

public class WriteTool : IAiTool
{
    public string Name => "create_booking";
    public string Description => "write";
    public string[] AllowedRoles => new[] { "customer" };
    public ToolRiskLevel RiskLevel => ToolRiskLevel.Write;
    public object GetSchema() => new { name = Name };
    public Task<AiToolResult> ExecuteAsync(JsonElement arguments, AiUserContext userContext) => throw new Exception("Must not execute");
}

public class ProbeTool : IAiTool
{
    public ProbeTool(string name, string role, ToolRiskLevel riskLevel)
    { Name = name; AllowedRoles = new[] { role }; RiskLevel = riskLevel; }
    public string Name { get; }
    public string Description => "isolated authorization probe";
    public string[] AllowedRoles { get; }
    public ToolRiskLevel RiskLevel { get; }
    public object GetSchema() => new { name = Name };
    public Task<AiToolResult> ExecuteAsync(JsonElement arguments, AiUserContext userContext) =>
        throw new Exception("Denied tool must not execute");
}

public class StubOrchestrator : IAiOrchestrator
{
    public Task<AiResponse> ProcessAsync(AiRequest request, AiUserContext userContext, CancellationToken cancellationToken = default) => Task.FromResult(new AiResponse { Success = true });
    public Task<AiResponse> ProcessReadOnlyAsync(AiRequest request, AiUserContext userContext, CancellationToken cancellationToken = default) => ProcessAsync(request, userContext, cancellationToken);
    public Task<AiResponse> ProcessGuestReadOnlyAsync(AiRequest request, CancellationToken cancellationToken = default) => Task.FromResult(new AiResponse { Success = true });
    public Task<AiResponse> ProcessGuestReadOnlyAsync(AiRequest request, AiUserContext guestContext, CancellationToken cancellationToken = default) => Task.FromResult(new AiResponse { Success = guestContext.HasVerifiedGuestSession });
}

public class TestEnvironment : IHostEnvironment
{
    public string EnvironmentName { get; set; } = "Development";
    public string ApplicationName { get; set; } = "ScopeRegression";
    public string ContentRootPath { get; set; } = "";
    public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } = null!;
}
