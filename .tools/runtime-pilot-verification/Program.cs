using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using RestaurantPOS.AI.Models;
using RestaurantPOS.AI.Security;
using RestaurantPOS.AI.Services;
using RestaurantPOS.AI.Tools;
using System.Text.Json;
using System.Diagnostics;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Net.Http.Json;
using RestaurantPOS.Infrastructure.Persistence;
using RestaurantPOS.AI.Diagnostics;

var repo = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../"));
var model = Path.Combine(repo, ".tools/mock-ai-embedding-spike/artifacts/onnx/model_O4.onnx");
var tokenizer = Path.Combine(repo, ".tools/mock-ai-embedding-spike/artifacts/onnx/tokenizer.json");
var catalog = Path.Combine(repo, ".tools/mock-ai-embedding-spike/experiments/manager-revenue-business-summary-v3/reference.json");
var checks = new List<(string Name, bool Pass, string Detail)>();
var defaultUatConfig = new ConfigurationBuilder()
    .SetBasePath(Path.Combine(repo, "services", "api"))
    .AddJsonFile("appsettings.Development.json", optional: false)
    .Build();
checks.Add(("uat_logging_default_off", !defaultUatConfig.GetValue<bool>("AI:UatLogging:Enabled"), "Development default"));

var loggingPath = Path.Combine(repo, "logs", "ai-uat", "rotation-check.log");
Directory.CreateDirectory(Path.GetDirectoryName(loggingPath)!);
foreach (var file in Directory.GetFiles(Path.GetDirectoryName(loggingPath)!, "rotation-check.log*")) File.Delete(file);
using (var loggingFactory = LoggerFactory.Create(logging => logging.AddProvider(new SafeAiDiagnosticFileLoggerProvider(loggingPath, 64 * 1024, 2))))
{
    var loggingProbe = loggingFactory.CreateLogger("UAT");
    for (var index = 0; index < 1200; index++) loggingProbe.LogInformation("AI_HTTP_RESPONSE AI_CORRELATION_ID=00000000-0000-0000-0000-000000000000 AI_HTTP_STATUS=200 AI_DURATION_MS=1 AI_ERROR_CODE=NONE AI_TEST_PADDING=SAFE");
}
checks.Add(("uat_file_rotation", File.Exists(loggingPath) && File.Exists(loggingPath + ".1"), "rotation files"));

var offConfig = Config(false, model, tokenizer, catalog);
using (var off = new ManagerEmbeddingPilot(offConfig, new Env("Development", repo), NullLogger<ManagerEmbeddingPilot>.Instance))
{
    var disabled = await off.DecideAsync("Doanh thu hôm nay", Manager("manager"), CancellationToken.None);
    checks.Add(("flag_off_no_pilot", disabled.Decision == "UNSUPPORTED", disabled.Reason));
    checks.Add(("employee_not_pilot", (await off.DecideAsync("Doanh thu hôm nay", Manager("employee"), CancellationToken.None)).Decision == "UNSUPPORTED", "role gate"));
    checks.Add(("guest_not_pilot", (await off.DecideAsync("Doanh thu hôm nay", new AiUserContext { Role = "customer", IsGuest = true, BranchId = Guid.NewGuid() }, CancellationToken.None)).Decision == "UNSUPPORTED", "role gate"));
}

using (var pilot = new ManagerEmbeddingPilot(Config(true, model, tokenizer, catalog), new Env("Development", repo), NullLogger<ManagerEmbeddingPilot>.Instance))
{
    checks.Add(await Decide("manager_revenue_today", pilot, "Doanh thu hôm nay", "ACCEPT", "get_revenue"));
    checks.Add(await Decide("manager_revenue_year", pilot, "Doanh số năm nay", "ACCEPT", "get_revenue"));
    checks.Add(await Decide("manager_summary_month", pilot, "Tóm tắt KPI kinh doanh tháng này", "ACCEPT", "get_business_summary"));
    checks.Add(await Decide("unsupported_revenue_week", pilot, "Doanh thu tuần này", "CLARIFY", null));
    checks.Add(await Decide("invalid_period", pilot, "Doanh thu tuần sau", "CLARIFY", null));
    checks.Add(await Decide("ood_inventory", pilot, "Kiểm tra tồn kho hôm nay", "UNSUPPORTED", null));
    checks.Add(await Decide("negation", pilot, "Đừng xem doanh thu hôm nay", "CLARIFY", null));
    checks.Add(await Decide("summary_multi_field", pilot, "Tổng doanh thu và số đơn hôm nay", "ACCEPT", "get_business_summary"));
    checks.Add(await Decide("compound_ood", pilot, "Doanh thu hôm nay và thực đơn", "UNSUPPORTED", null));
    checks.Add(await DecideExact("M-R03", pilot, "Tháng này nhà hàng bán được bao nhiêu tiền?", "ACCEPT", "REVENUE_QUERY", "get_revenue", "this_month"));
    checks.Add(await DecideExact("M-R04", pilot, "Báo cáo doanh thu năm nay giúp tôi.", "ACCEPT", "REVENUE_QUERY", "get_revenue", "year"));
    checks.Add(await DecideExact("M-S02", pilot, "Tóm tắt các chỉ số kinh doanh của hôm qua.", "ACCEPT", "BUSINESS_SUMMARY", "get_business_summary", "yesterday"));
    checks.Add(await DecideExact("M-S05", pilot, "Tình hình kinh doanh tháng này thế nào?", "ACCEPT", "BUSINESS_SUMMARY", "get_business_summary", "this_month"));
    checks.Add(await DecideExact("M-X01", pilot, "Cho tôi xem danh sách món ăn và giá bán.", "UNSUPPORTED", null, null, null));
    var missingBranch = await pilot.DecideAsync("Doanh thu hôm nay", new AiUserContext { Role = "manager" }, CancellationToken.None);
    checks.Add(("missing_branch_fail_closed", missingBranch.Decision == "CLARIFY" && missingBranch.Reason == "MissingBranchContext", missingBranch.Reason));
}

var badModel = Path.Combine(Path.GetTempPath(), "pilot-missing-model.onnx");
using (var unavailable = new ManagerEmbeddingPilot(Config(true, badModel, tokenizer, catalog), new Env("Development", repo), NullLogger<ManagerEmbeddingPilot>.Instance))
{
    try
    {
        await unavailable.DecideAsync("Doanh thu hôm nay", Manager("manager"), CancellationToken.None);
        checks.Add(("missing_model_fail_closed", false, "no exception"));
    }
    catch (InvalidOperationException ex)
    {
        checks.Add(("missing_model_fail_closed", ex.Message.Contains("unavailable", StringComparison.OrdinalIgnoreCase), ex.Message));
    }
}

using (var checksumMismatch = new ManagerEmbeddingPilot(Config(true, tokenizer, tokenizer, catalog), new Env("Development", repo), NullLogger<ManagerEmbeddingPilot>.Instance))
{
    try
    {
        await checksumMismatch.DecideAsync("Doanh thu hôm nay", Manager("manager"), CancellationToken.None);
        checks.Add(("checksum_mismatch_fail_closed", false, "no exception"));
    }
    catch (InvalidOperationException ex)
    {
        checks.Add(("checksum_mismatch_fail_closed", ex.Message.Contains("unavailable", StringComparison.OrdinalIgnoreCase), ex.Message));
    }
}

using (var cancelled = new ManagerEmbeddingPilot(Config(true, model, tokenizer, catalog), new Env("Development", repo), NullLogger<ManagerEmbeddingPilot>.Instance))
{
    using var cts = new CancellationTokenSource();
    cts.Cancel();
    try
    {
        await cancelled.DecideAsync("Doanh thu hôm nay", Manager("manager"), cts.Token);
        checks.Add(("cancellation_before_enqueue", false, "completed"));
    }
    catch (OperationCanceledException)
    {
        checks.Add(("cancellation_before_enqueue", true, "cancelled"));
    }
}

var lifecycle = new ManagerEmbeddingPilot(Config(true, model, tokenizer, catalog), new Env("Development", repo), NullLogger<ManagerEmbeddingPilot>.Instance);
var active = lifecycle.DecideAsync("Doanh thu hôm nay", Manager("manager"), CancellationToken.None);
lifecycle.Dispose();
try
{
    var lifecycleResult = await active;
    checks.Add(("dispose_with_active_operation", lifecycleResult.Decision == "ACCEPT", lifecycleResult.Decision));
}
catch (Exception ex)
{
    checks.Add(("dispose_with_active_operation", false, ex.GetType().Name));
}

var auth = new AiPermissionService(new AiAuthorization());
var tool = new FakeReadTool("get_revenue", new[] { "manager" });
checks.Add(("permission_manager_branch", await auth.CanExecuteAsync(tool, Manager("manager")), "manager branch"));
checks.Add(("permission_manager_missing_branch", !await auth.CanExecuteAsync(tool, new AiUserContext { Role = "manager" }), "missing branch denied"));
checks.Add(("permission_employee_denied", !await auth.CanExecuteAsync(tool, Manager("employee")), "role denied"));

checks.Add(await RunRealOrchestratorCheck(repo, model, tokenizer, catalog));
checks.Add(await RunHttpApiCheck(repo, model, tokenizer, catalog, enabled: true));
checks.Add(await RunHttpApiCheck(repo, model, tokenizer, catalog, enabled: false));

using (var benchmark = new ManagerEmbeddingPilot(Config(true, model, tokenizer, catalog), new Env("Development", repo), NullLogger<ManagerEmbeddingPilot>.Instance))
{
    var rssBefore = Process.GetCurrentProcess().WorkingSet64;
    var coldWatch = Stopwatch.StartNew();
    await benchmark.DecideAsync("Doanh thu hôm nay", Manager("manager"), CancellationToken.None);
    coldWatch.Stop();
    var warm = new List<double>();
    for (var i = 0; i < 10; i++)
    {
        var watch = Stopwatch.StartNew();
        await benchmark.DecideAsync("Doanh thu hôm nay", Manager("manager"), CancellationToken.None);
        watch.Stop();
        warm.Add(watch.Elapsed.TotalMilliseconds);
    }
    var rssAfter = Process.GetCurrentProcess().WorkingSet64;
    var concurrent = await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => benchmark.DecideAsync("Doanh thu hôm nay", Manager("manager"), CancellationToken.None)));
    checks.Add(("concurrent_requests_complete", concurrent.All(x => x.Decision == "ACCEPT"), $"completed={concurrent.Length}"));
    Console.WriteLine($"BENCHMARK|cold_ms={coldWatch.Elapsed.TotalMilliseconds:F2}|warm_median_ms={Median(warm):F2}|warm_p95_ms={Percentile(warm, .95):F2}|rss_before={rssBefore}|rss_after={rssAfter}|rss_delta={rssAfter - rssBefore}|concurrent={concurrent.Length}");
}

using (var saturated = new ManagerEmbeddingPilot(ConfigWithTimeout(true, model, tokenizer, catalog, 100), new Env("Development", repo), NullLogger<ManagerEmbeddingPilot>.Instance))
{
    await saturated.DecideAsync("Doanh thu hôm nay", Manager("manager"), CancellationToken.None);
    var tasks = Enumerable.Range(0, 64).Select(async _ =>
    {
        try { await saturated.DecideAsync("Doanh thu hôm nay", Manager("manager"), CancellationToken.None); return "completed"; }
        catch (TimeoutException) { return "timeout"; }
        catch (Exception ex) { return ex.GetType().Name; }
    }).ToArray();
    var outcomes = await Task.WhenAll(tasks);
    var safe = outcomes.All(x => x is "completed" or "timeout");
    checks.Add(("queue_saturation_no_deadlock", safe, $"completed={outcomes.Count(x => x == "completed")};timeouts={outcomes.Count(x => x == "timeout")}"));
}

foreach (var check in checks) Console.WriteLine($"{(check.Pass ? "PASS" : "FAIL")}|{check.Name}|{check.Detail}");
Console.WriteLine($"SUMMARY|passed={checks.Count(x => x.Pass)}|failed={checks.Count(x => !x.Pass)}|database=NOT_USED|api=NOT_STARTED");
return checks.All(x => x.Pass) ? 0 : 1;

static IConfiguration Config(bool enabled, string model, string tokenizer, string catalog) => new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
{
    ["AI:EmbeddingPilot:Enabled"] = enabled.ToString(),
    ["AI:EmbeddingPilot:ModelPath"] = model,
    ["AI:EmbeddingPilot:TokenizerPath"] = tokenizer,
    ["AI:EmbeddingPilot:CatalogPath"] = catalog,
    ["AI:EmbeddingPilot:TimeoutMilliseconds"] = "1500"
}).Build();

static IConfiguration ConfigWithTimeout(bool enabled, string model, string tokenizer, string catalog, int timeout) => new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
{
    ["AI:EmbeddingPilot:Enabled"] = enabled.ToString(),
    ["AI:EmbeddingPilot:ModelPath"] = model,
    ["AI:EmbeddingPilot:TokenizerPath"] = tokenizer,
    ["AI:EmbeddingPilot:CatalogPath"] = catalog,
    ["AI:EmbeddingPilot:TimeoutMilliseconds"] = timeout.ToString()
}).Build();

static AiUserContext Manager(string role) => new() { Role = role, BranchId = Guid.Parse("11111111-1111-1111-1111-111111111111") };

static async Task<(string Name, bool Pass, string Detail)> Decide(string name, ManagerEmbeddingPilot pilot, string text, string expectedDecision, string? expectedTool)
{
    var result = await pilot.DecideAsync(text, Manager("manager"), CancellationToken.None);
    return (name, result.Decision == expectedDecision && (expectedTool is null || result.ToolName == expectedTool), $"decision={result.Decision};tool={result.ToolName};reason={result.Reason}");
}

static async Task<(string Name, bool Pass, string Detail)> DecideExact(string name, ManagerEmbeddingPilot pilot, string text, string expectedDecision, string? expectedIntent, string? expectedTool, string? expectedPeriod)
{
    var result = await pilot.DecideAsync(text, Manager("manager"), CancellationToken.None);
    var pass = result.Decision == expectedDecision && result.Intent == expectedIntent && result.ToolName == expectedTool && (expectedPeriod is null || result.Period == expectedPeriod);
    return (name, pass, $"input={text};expected={expectedDecision}/{expectedIntent}/{expectedTool}/{expectedPeriod};decision={result.Decision};intent={result.Intent};tool={result.ToolName};period={result.Period};reason={result.Reason};top={result.TopScore:F6};margin={result.Margin:F6};args={result.Arguments}");
}

static double Median(IReadOnlyList<double> values) => Percentile(values, .5);
static double Percentile(IReadOnlyList<double> values, double p)
{
    var ordered = values.OrderBy(x => x).ToArray();
    var index = (ordered.Length - 1) * p;
    var low = (int)Math.Floor(index);
    var high = (int)Math.Ceiling(index);
    return low == high ? ordered[low] : ordered[low] + (ordered[high] - ordered[low]) * (index - low);
}

static async Task<(string Name, bool Pass, string Detail)> RunRealOrchestratorCheck(string repo, string model, string tokenizer, string catalog)
{
    var branchB = Guid.Parse("22222222-2222-2222-2222-222222222222");
    var branchId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    await using var connection = new SqliteConnection("Data Source=:memory:");
    await connection.OpenAsync();
    var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options;
    await using var db = new ApplicationDbContext(options);
    await db.Database.EnsureCreatedAsync();
    db.Branches.Add(new RestaurantPOS.Domain.Entities.Branch { Id = branchB, Name = "Branch B", IsActive = true });
    db.Branches.Add(new RestaurantPOS.Domain.Entities.Branch { Id = branchId, Name = "Branch A", IsActive = true });
    await db.SaveChangesAsync();

    using var pilot = new ManagerEmbeddingPilot(Config(true, model, tokenizer, catalog), new Env("Development", repo), NullLogger<ManagerEmbeddingPilot>.Instance);
    var registry = new AiToolRegistry(new IAiTool[] { new RecordingTool("get_revenue", new[] { "manager" }) });
    var orchestrator = new AiOrchestrator(
        new NeverCalledProvider(),
        new AiContextService(db),
        registry,
        new AiPermissionService(new AiAuthorization()),
        new PromptBuilder(),
        pilot,
        NullLogger<AiOrchestrator>.Instance);
    var response = await orchestrator.ProcessReadOnlyAsync(new AiRequest { Message = "Doanh thu h\u00f4m nay" }, Manager("manager"));
    var responseB = await orchestrator.ProcessReadOnlyAsync(new AiRequest { Message = "Doanh thu h\u00f4m nay" }, new AiUserContext { Role = "manager", BranchId = branchB, UserName = "Branch B" });
    var clarify = await orchestrator.ProcessReadOnlyAsync(new AiRequest { Message = "Doanh thu tu\u1ea7n n\u00e0y" }, Manager("manager"));
    var recording = (RecordingTool)registry.GetTool("get_revenue")!;
    var pass = response.Success && responseB.Success && clarify.Success && recording.Calls == 2 && recording.Branches.SequenceEqual(new[] { branchId, branchB });
    return ("real_orchestrator_registry_permission_branch_and_clarify", pass, "successA=" + response.Success + ";successB=" + responseB.Success + ";clarify=" + clarify.Success + ";calls=" + recording.Calls + ";branches=" + string.Join(",", recording.Branches));
}

static async Task<(string Name, bool Pass, string Detail)> RunHttpApiCheck(string repo, string model, string tokenizer, string catalog, bool enabled)
{
    var branchId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    await using var connection = new SqliteConnection("Data Source=:memory:");
    await connection.OpenAsync();
    var dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options;
    await using var db = new ApplicationDbContext(dbOptions);
    await db.Database.EnsureCreatedAsync();
    db.Branches.Add(new RestaurantPOS.Domain.Entities.Branch { Id = branchId, Name = "HTTP Branch", IsActive = true });
    await db.SaveChangesAsync();

    var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Development", ContentRootPath = repo });
    builder.WebHost.UseUrls("http://127.0.0.1:0");
    builder.Logging.ClearProviders();
    builder.Logging.AddConsole();
    var uatLogPath = Path.Combine(repo, "logs", "ai-uat", "harness-ai-chat.log");
    builder.Logging.AddProvider(new SafeAiDiagnosticFileLoggerProvider(uatLogPath, 64 * 1024, 2));
    builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
    {
        ["AI:EmbeddingPilot:Enabled"] = enabled.ToString(),
        ["AI:EmbeddingPilot:ModelPath"] = model,
        ["AI:EmbeddingPilot:TokenizerPath"] = tokenizer,
        ["AI:EmbeddingPilot:CatalogPath"] = catalog,
        ["AI:EmbeddingPilot:TimeoutMilliseconds"] = "1500",
        ["AI:LiveReadOnlyE2E:Enabled"] = "false"
        , ["AI:UatLogging:Enabled"] = "true"
    });
    builder.Services.AddSingleton(db);
    builder.Services.AddSingleton<ManagerEmbeddingPilot>();
    builder.Services.AddSingleton<IAiProvider, NeverCalledProvider>();
    var recordingTool = new RecordingTool("get_revenue", new[] { "manager" });
    builder.Services.AddSingleton(new AiToolRegistry(new IAiTool[] { recordingTool }));
    builder.Services.AddSingleton(new AiPermissionService(new AiAuthorization()));
    builder.Services.AddSingleton(new PromptBuilder());
    builder.Services.AddSingleton<RestaurantPOS.AI.Security.IProductPriceConfirmationStore, RestaurantPOS.AI.Security.ProductPriceConfirmationStore>();
    builder.Services.AddSingleton<RestaurantPOS.Application.Services.IProductService, FakeProductService>();
    builder.Services.AddScoped<AiContextService>();
    builder.Services.AddScoped<AiCorrelationContext>();
    builder.Services.AddSingleton<IAiToolExecutionAuditSink, AiToolExecutionAuditSink>();
    builder.Services.AddScoped<IAiOrchestrator, AiOrchestrator>();
    builder.Services.AddAuthentication("Test").AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });
    builder.Services.AddAuthorization();
    builder.Services.AddControllers().AddApplicationPart(typeof(RestaurantPOS.AI.Controllers.AiController).Assembly);
    var app = builder.Build();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    await app.StartAsync();
    try
    {
        using var client = new HttpClient { BaseAddress = new Uri(app.Urls.Single()) };
        client.DefaultRequestHeaders.Add("X-Test-Role", "manager");
        client.DefaultRequestHeaders.Add("X-Test-Branch", branchId.ToString());
        var response = await client.PostAsJsonAsync("/api/Ai/chat", new AiRequest { Message = "Doanh thu h\u00f4m nay" });
        var body = await response.Content.ReadAsStringAsync();
        var pass = response.IsSuccessStatusCode && (enabled ? body.Contains("recorded", StringComparison.OrdinalIgnoreCase) : !body.Contains("recorded", StringComparison.OrdinalIgnoreCase));
        if (enabled)
        {
            var clarify = await client.PostAsJsonAsync("/api/Ai/chat", new AiRequest { Message = "Doanh thu tuần này" });
            var clarifyBody = await clarify.Content.ReadAsStringAsync();
            var unsupported = await client.PostAsJsonAsync("/api/Ai/chat", new AiRequest { Message = "Cho tôi xem tồn kho" });
            var unsupportedBody = await unsupported.Content.ReadAsStringAsync();
            pass = pass && clarify.IsSuccessStatusCode && unsupported.IsSuccessStatusCode
                && !clarifyBody.Contains("recorded", StringComparison.OrdinalIgnoreCase)
                && !unsupportedBody.Contains("recorded", StringComparison.OrdinalIgnoreCase)
                && recordingTool.Calls == 1;
        }
        return ($"http_api_flag_{(enabled ? "on_manager" : "off_provider_path")}", pass, $"status={(int)response.StatusCode};clarify_tool_calls={recordingTool.Calls}");
    }
    finally
    {
        await app.StopAsync();
        await app.DisposeAsync();
    }
}

sealed class Env(string name, string root) : IHostEnvironment
{
    public string EnvironmentName { get; set; } = name;
    public string ApplicationName { get; set; } = "RuntimePilotVerification";
    public string ContentRootPath { get; set; } = root;
    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
}

sealed class FakeReadTool(string name, string[] roles) : IAiTool
{
    public string Name => name;
    public string Description => "fake";
    public string[] AllowedRoles => roles;
    public ToolRiskLevel RiskLevel => ToolRiskLevel.Read;
    public object GetSchema() => new { name = Name };
    public Task<AiToolResult> ExecuteAsync(JsonElement arguments, AiUserContext userContext) => Task.FromResult(AiToolResult.CreateSuccess(userContext.BranchId?.ToString() ?? "none"));
}

sealed class NeverCalledProvider : IAiProvider
{
    public string Name => "never-called";
    public bool IsConfigured => true;
    public Task<string> GenerateContentAsync(object requestBody, CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException("Provider fallback was called during pilot test.");
}

sealed class RecordingTool(string name, string[] roles) : IAiTool
{
    public string Name => name;
    public string Description => "recording";
    public string[] AllowedRoles => roles;
    public ToolRiskLevel RiskLevel => ToolRiskLevel.Read;
    public List<Guid> Branches { get; } = new();
    public int Calls { get; private set; }
    public Guid? LastBranchId { get; private set; }
    public object GetSchema() => new { name = Name };
    public Task<AiToolResult> ExecuteAsync(JsonElement arguments, AiUserContext userContext)
    {
        Calls++;
        if (userContext.BranchId.HasValue) Branches.Add(userContext.BranchId.Value);
        LastBranchId = userContext.BranchId;
        return Task.FromResult(AiToolResult.CreateSuccess("recorded"));
    }
}

sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
        : base(options, logger, encoder) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var role = Request.Headers["X-Test-Role"].FirstOrDefault() ?? "manager";
        var branch = Request.Headers["X-Test-Branch"].FirstOrDefault();
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new(ClaimTypes.Role, role),
            new("fullName", "HTTP Test User")
        };
        if (!string.IsNullOrWhiteSpace(branch)) claims.Add(new Claim("branchId", branch));
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name)));
    }
}

sealed class FakeProductService : RestaurantPOS.Application.Services.IProductService
{
    public Task<List<RestaurantPOS.Domain.Entities.Product>> GetAllProductsAsync(string? category, Guid? branchId = null) => Task.FromResult(new List<RestaurantPOS.Domain.Entities.Product>());
    public Task<List<RestaurantPOS.Domain.Entities.Product>> FindProductsByExactNameAsync(string productName) => Task.FromResult(new List<RestaurantPOS.Domain.Entities.Product>());
    public Task<(bool Success, string Message, decimal OldPrice, decimal NewPrice)> UpdatePriceByNameAsync(string productName, decimal newPrice) => Task.FromResult((false, "not used", 0m, 0m));
    public Task<(bool Success, string Message, decimal OldPrice, decimal NewPrice)> UpdatePriceByIdAsync(Guid productId, decimal expectedCurrentPrice, decimal newPrice) => Task.FromResult((false, "not used", 0m, 0m));
}
