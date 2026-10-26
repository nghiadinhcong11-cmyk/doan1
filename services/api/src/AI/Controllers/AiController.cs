using Microsoft.AspNetCore.Mvc;
using RestaurantPOS.AI.Models;
using RestaurantPOS.AI.Services;
using RestaurantPOS.AI.Diagnostics;
using RestaurantPOS.AI.Security;
using RestaurantPOS.Application.Common.Security;
using RestaurantPOS.Application.Services;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace RestaurantPOS.AI.Controllers
{
    /// <summary>
    /// API Trợ lý ảo AI Assistant (Mock AI + AI Tools)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AiController : ControllerBase
    {
        private readonly IAiOrchestrator _orchestrator;
        private readonly ILogger<AiController>? _logger;
        private readonly IConfiguration? _configuration;
        private readonly IHostEnvironment? _environment;
        private readonly AiCorrelationContext? _correlationContext;

        public AiController(IAiOrchestrator orchestrator)
            : this(orchestrator, null)
        {
        }

        [ActivatorUtilitiesConstructor]
        public AiController(
            IAiOrchestrator orchestrator,
            ILogger<AiController>? logger = null,
            IConfiguration? configuration = null,
            IHostEnvironment? environment = null,
            AiCorrelationContext? correlationContext = null)
        {
            _orchestrator = orchestrator;
            _logger = logger;
            _configuration = configuration;
            _environment = environment;
            _correlationContext = correlationContext;
        }

        /// <summary>
        /// Gửi tin nhắn đàm thoại với Trợ lý AI (tự động phân quyền và kích hoạt Tool phù hợp)
        /// </summary>
        /// <param name="request">Nội dung tin nhắn và lịch sử hội thoại</param>
        [EnableRateLimiting("ai-limiter")]
        [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Customer)]
        [RequestSizeLimit(16 * 1024)]
        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] AiRequest request)
        {
            var correlationId = _correlationContext?.Begin() ?? Guid.NewGuid();
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            HttpContext.Response.OnCompleted(() =>
            {
                _logger?.LogInformation(
                    "AI_HTTP_RESPONSE AI_CORRELATION_ID={CorrelationId} AI_HTTP_STATUS={HttpStatus} AI_DURATION_MS={DurationMilliseconds} AI_ERROR_CODE={ErrorCode}",
                    correlationId,
                    HttpContext.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds,
                    HttpContext.Response.StatusCode >= 400 ? "HTTP_ERROR" : "NONE");
                return Task.CompletedTask;
            });
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Message) || request.Message.Length > 1000)
                    return BadRequest(new { error = "Nội dung câu hỏi không hợp lệ." });

                if (request.History is not null &&
                    (request.History.Count > 6 || request.History.Any(item => item.Content?.Length > 1000)))
                    return BadRequest(new { error = "Lịch sử hội thoại không hợp lệ." });

                string userRole = "";
                Guid userId = Guid.Empty;
                string userName = "Khách vãng lai";
                Guid? branchId = null;

                // 1. Nếu có JWT Bearer token hợp lệ, đọc thông tin từ ClaimsPrincipal
                if (User.Identity?.IsAuthenticated == true)
                {
                    var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
                    if (!string.IsNullOrEmpty(roleClaim))
                    {
                        userRole = roleClaim.ToLower();
                    }

                    var nameIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (Guid.TryParse(nameIdClaim, out var parsedId))
                    {
                        userId = parsedId;
                    }

                    userName = User.FindFirst("fullName")?.Value
                               ?? User.FindFirst(ClaimTypes.Name)?.Value
                               ?? "User";

                    // Keep the signed branch claim available for an explicit
                    // "current branch" query. Tools default to chain-wide scope;
                    // the user message never supplies an arbitrary branch id.
                    var branchIdClaim = User.FindFirst("branchId")?.Value;
                    if (Guid.TryParse(branchIdClaim, out var parsedBranchId))
                    {
                        branchId = parsedBranchId;
                    }

                    // Admin's branch selector is sent as an explicit request
                    // scope. It is only accepted for Admin; managers keep the
                    // signed JWT branch scope and cannot override it.
                    if (string.Equals(userRole, RoleNames.Admin, StringComparison.OrdinalIgnoreCase) &&
                        Guid.TryParse(Request.Headers["X-Branch-Id"].ToString(), out var selectedBranchId))
                    {
                        branchId = selectedBranchId;
                    }
                }
                else
                {
                    // 2. Không có JWT token: Người dùng chưa đăng nhập
                    // Bảo mật: Ngăn chặn giả mạo vai trò quản trị viên hoặc nhân viên qua query param
                    userRole = "customer";
                    userName = "Khách hàng";
                }

                if (User.Identity?.IsAuthenticated != true || userId == Guid.Empty ||
                    (userRole != RoleNames.Admin && userRole != RoleNames.Customer))
                    return Forbid();

                if (string.Equals(userRole, "customer", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(User.FindFirst("customerSessionType")?.Value, "registered", StringComparison.OrdinalIgnoreCase))
                {
                    return Forbid();
                }

                var userContext = new AiUserContext
                {
                    UserId = userId,
                    Role = userRole,
                    UserName = userName,
                    BranchId = branchId,
                    CustomerId = string.Equals(userRole, "customer", StringComparison.OrdinalIgnoreCase) ? userId : null,
                    PhoneNumber = string.Equals(userRole, "customer", StringComparison.OrdinalIgnoreCase) ? User.Identity?.Name : null
                };


                _logger?.LogInformation(
                    "AI_HTTP_REQUEST AI_CORRELATION_ID={CorrelationId} AI_ROUTE={Route} AI_ROLE={Role} AI_ADMIN_EMBEDDING_FLAG={FeatureFlag} AI_ENVIRONMENT={Environment}",
                    correlationId,
                    HttpContext.Request.Path.Value ?? "/api/Ai/chat",
                    string.IsNullOrWhiteSpace(userRole) ? "UNKNOWN" : userRole,
                    _configuration?.GetValue<bool>("AI:AdminEmbeddingPilot:Enabled") == true ? "ON" : "OFF",
                    _environment?.EnvironmentName ?? "UNKNOWN");

                var result = await _orchestrator.ProcessReadOnlyAsync(request, userContext, HttpContext.RequestAborted);
                return Ok(result);
            }
            catch (Exception)
            {
                _logger?.LogWarning(
                    "AI_FAILURE_CODE={FailureCode} AI_PHASE={Phase} AI_SUBPHASE={Subphase} AI_CORRELATION_ID={CorrelationId} TOOL_EXECUTED={ToolExecuted} AI_DURATION_MS={DurationMilliseconds} AI_TOOL_NAME={ToolName}",
                    AiFailureCodes.OrchestrationFailed,
                    "controller",
                    "UNKNOWN",
                    Guid.NewGuid(),
                    "NO",
                    "UNKNOWN",
                    "UNKNOWN");
                return StatusCode(500, new AiResponse { Success = false, Error = "AI request could not be processed." });
            }
        }

        /// <summary>
        /// Development-only E5 menu route for a verified QR guest session.
        /// </summary>
        [AllowAnonymous]
        [EnableRateLimiting("ai-limiter")]
        [RequestSizeLimit(16 * 1024)]
        [HttpPost("guest-chat")]
        public async Task<IActionResult> GuestChat([FromBody] AiRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Message) || request.Message.Length > 1000)
                return BadRequest(new { error = "Nội dung câu hỏi không hợp lệ." });

            if (request.History is not null &&
                (request.History.Count > 6 || request.History.Any(item => item.Content?.Length > 1000)))
                return BadRequest(new { error = "Lịch sử hội thoại không hợp lệ." });

            if (User.Identity?.IsAuthenticated == true && User.IsInRole(RoleNames.Kitchen))
                return Forbid();

            // AI guest access requires a server-issued QR session even in Mock mode.
            if (!GuestSessionContext.TryCreate(User, out var guestSession) ||
                guestSession?.SessionType != GuestSessionContext.QrSessionType)
                return Forbid();

            var customerGuestEmbeddingEnabled = _configuration?.GetValue<bool>("AI:CustomerGuestEmbeddingPilot:Enabled") == true;
            if (_environment?.IsDevelopment() != true || !customerGuestEmbeddingEnabled)
            {
                return StatusCode(404, new { error = "Guest AI is not available." });
            }

            try
            {
                var hasRegisteredCustomerId = Guid.TryParse(
                    User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    out var registeredCustomerId);
                var isRegisteredQrCustomer = string.Equals(
                    User.FindFirst("customerAuthMethod")?.Value,
                    "registeredQr",
                    StringComparison.OrdinalIgnoreCase) && hasRegisteredCustomerId;
                var guestContext = new AiUserContext
                {
                    UserId = isRegisteredQrCustomer ? registeredCustomerId : Guid.Empty,
                    Role = RoleNames.Customer,
                    IsGuest = !isRegisteredQrCustomer,
                    CustomerId = isRegisteredQrCustomer ? registeredCustomerId : null,
                    PhoneNumber = isRegisteredQrCustomer ? User.Identity?.Name : null,
                    UserName = isRegisteredQrCustomer
                        ? User.FindFirst("fullName")?.Value ?? User.Identity?.Name
                        : "Khách vãng lai",
                    BranchId = guestSession?.BranchId,
                    HasVerifiedGuestSession = guestSession is not null
                };

                var result = await _orchestrator.ProcessGuestReadOnlyAsync(
                    request,
                    guestContext,
                    HttpContext.RequestAborted);
                return Ok(result);
            }
            catch (Exception)
            {
                _logger?.LogWarning(
                    "AI_FAILURE_CODE={FailureCode} AI_PHASE=guest_controller AI_SUBPHASE=UNKNOWN AI_CORRELATION_ID={CorrelationId} TOOL_EXECUTED=NO",
                    AiFailureCodes.OrchestrationFailed,
                    Guid.NewGuid());
                return StatusCode(500, new AiResponse { Success = false, Error = "AI request could not be processed." });
            }
        }

        [HttpGet("live-read-only-status")]
        [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Customer)]
        public IActionResult GetLiveReadOnlyStatus()
        {
            return Ok(new { enabled = true });
        }

        [HttpPost("product-price-confirmation")]
        [Authorize(Roles = RoleNames.Admin)]
        [RequestSizeLimit(4 * 1024)]
        public async Task<IActionResult> ConfirmProductPrice([FromBody] ProductPriceConfirmationRequest request)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.Token))
                return BadRequest(new { message = "Confirmation token is required." });

            var userId = Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var parsedId)
                ? parsedId
                : Guid.Empty;
            var context = new AiUserContext
            {
                UserId = userId,
                Role = RoleNames.Admin,
                UserName = User.FindFirst("fullName")?.Value ?? User.FindFirst(ClaimTypes.Name)?.Value ?? "Admin"
            };

            var result = await _orchestrator.ConfirmProductPriceAsync(
                request.Token, context, HttpContext.RequestAborted);
            return result.Success ? Ok(new { message = result.Message }) : BadRequest(new { message = result.Error });
        }

        [HttpPost("product-name-confirmation")]
        [Authorize(Roles = RoleNames.Admin)]
        [RequestSizeLimit(4 * 1024)]
        public async Task<IActionResult> ConfirmProductName([FromBody] ProductNameConfirmationRequest request)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.Token))
                return BadRequest(new { message = "Confirmation token is required." });
            var userId = Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var parsedId) ? parsedId : Guid.Empty;
            var context = new AiUserContext { UserId = userId, Role = RoleNames.Admin, UserName = User.FindFirst("fullName")?.Value ?? User.FindFirst(ClaimTypes.Name)?.Value ?? "Admin" };
            var result = await _orchestrator.ConfirmProductNameAsync(request.Token, context, HttpContext.RequestAborted);
            return result.Success ? Ok(new { message = result.Message }) : BadRequest(new { message = result.Error });
        }

        [HttpPost("catalog-creation-confirmation")]
        [Authorize(Roles = RoleNames.Admin)]
        [RequestSizeLimit(4 * 1024)]
        public async Task<IActionResult> ConfirmCatalogCreation([FromBody] CatalogCreationConfirmationRequest request)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.Type))
                return BadRequest(new { message = "Confirmation token and type are required." });
            var userId = Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var parsedId) ? parsedId : Guid.Empty;
            var context = new AiUserContext { UserId = userId, Role = RoleNames.Admin, UserName = User.FindFirst("fullName")?.Value ?? User.FindFirst(ClaimTypes.Name)?.Value ?? "Admin" };
            var result = await _orchestrator.ConfirmCatalogCreationAsync(request.Token, request.Type, context, HttpContext.RequestAborted);
            return result.Success ? Ok(new { message = result.Message }) : BadRequest(new { message = result.Error });
        }

        /// <summary>
        /// Returns non-sensitive Development runtime gates for local E2E preflight.
        /// It intentionally does not expose connection strings, project identifiers or secrets.
        /// </summary>
        [HttpGet("runtime-status")]
        [Authorize(Roles = RoleNames.Admin)]
        public IActionResult GetRuntimeStatus()
        {
            if (_environment?.IsDevelopment() != true || _configuration == null)
                return NotFound();

            const string providerMode = "E5_ONLY";

            var migrationEnabled = _configuration.GetValue<bool>("DatabaseSafety:AllowAutomaticMigration");
            var backgroundInsightsEnabled = _configuration.GetValue<bool>("BackgroundJobs:EnableInsights");
            var databaseTargetGuard = string.Equals(
                _configuration["DEV_SUPABASE_PROJECT_REF"],
                QrTokenBackfillTargetGuard.AuthorizedDevProjectRef,
                StringComparison.Ordinal);

            return Ok(new
            {
                environment = "Development",
                providerMode,
                readOnlyEnabled = true,
                automaticMigrationEnabled = migrationEnabled,
                backgroundInsightsEnabled,
                databaseTargetGuard
            });
        }

    }
}
