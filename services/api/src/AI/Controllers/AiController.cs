using Microsoft.AspNetCore.Mvc;
using RestaurantPOS.AI.Models;
using RestaurantPOS.AI.Services;
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
        private readonly IProductPriceConfirmationStore? _confirmationStore;
        private readonly IProductService? _productService;
        private readonly ILogger<AiController>? _logger;
        private readonly AiLiveReadOnlyPolicy _liveReadOnlyPolicy;
        private readonly IConfiguration? _configuration;
        private readonly IHostEnvironment? _environment;

        public AiController(IAiOrchestrator orchestrator)
            : this(orchestrator, null, null)
        {
        }

        [ActivatorUtilitiesConstructor]
        public AiController(
            IAiOrchestrator orchestrator,
            IProductPriceConfirmationStore? confirmationStore,
            IProductService? productService,
            ILogger<AiController>? logger = null,
            AiLiveReadOnlyPolicy? liveReadOnlyPolicy = null,
            IConfiguration? configuration = null,
            IHostEnvironment? environment = null)
        {
            _orchestrator = orchestrator;
            _confirmationStore = confirmationStore;
            _productService = productService;
            _logger = logger;
            _liveReadOnlyPolicy = liveReadOnlyPolicy ?? new AiLiveReadOnlyPolicy();
            _configuration = configuration;
            _environment = environment;
        }

        /// <summary>
        /// Gửi tin nhắn đàm thoại với Trợ lý AI (tự động phân quyền và kích hoạt Tool phù hợp)
        /// </summary>
        /// <param name="request">Nội dung tin nhắn và lịch sử hội thoại</param>
        [EnableRateLimiting("ai-limiter")]
        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] AiRequest request)
        {
            try
            {
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

                    var branchIdClaim = User.FindFirst("branchId")?.Value;
                    if (Guid.TryParse(branchIdClaim, out var parsedBranchId))
                    {
                        branchId = parsedBranchId;
                    }
                }
                else
                {
                    // 2. Không có JWT token: Người dùng chưa đăng nhập
                    // Bảo mật: Ngăn chặn giả mạo vai trò quản trị viên hoặc nhân viên qua query param
                    userRole = "customer";
                    userName = "Khách hàng";
                }

                if (string.Equals(userRole, "customer", StringComparison.OrdinalIgnoreCase)
                    && string.Equals(User.FindFirst("customerSessionType")?.Value, "guest", StringComparison.OrdinalIgnoreCase))
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

                if (_liveReadOnlyPolicy.IsMisconfigured)
                    return StatusCode(503, new { error = "AI read-only test mode configuration is invalid." });

                if (_liveReadOnlyPolicy.IsRequested && !_liveReadOnlyPolicy.IsEnabledFor(User))
                    return Forbid();

                var result = _liveReadOnlyPolicy.IsEnabledFor(User)
                    ? await _orchestrator.ProcessReadOnlyAsync(request, userContext, HttpContext.RequestAborted)
                    : await _orchestrator.ProcessAsync(request, userContext, HttpContext.RequestAborted);
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
        /// Development-only public Mock AI route for non-personal restaurant questions.
        /// It never trusts client identity and exposes only the public menu tool.
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

            // Personal orders must never be downgraded to an anonymous lookup.
            if (MockAiIntentRecognizer.IsPersonalOrderRequest(request.Message))
            {
                return Ok(new AiResponse
                {
                    Success = true,
                    Message = "Vui lòng đăng nhập để xem đơn hàng của bạn."
                });
            }

            if (_environment?.IsDevelopment() != true ||
                !string.Equals(_configuration?["AI:ProviderMode"], "Mock", StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(404, new { error = "Guest Mock AI is not available." });
            }

            try
            {
                var result = await _orchestrator.ProcessGuestReadOnlyAsync(request, HttpContext.RequestAborted);
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
        public IActionResult GetLiveReadOnlyStatus()
        {
            if (_liveReadOnlyPolicy.IsMisconfigured)
                return StatusCode(503, new { enabled = false });

            return Ok(new { enabled = _liveReadOnlyPolicy.IsEnabledFor(User) });
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

            var providerMode = string.Equals(
                _configuration["AI:ProviderMode"],
                "Mock",
                StringComparison.OrdinalIgnoreCase)
                ? "Mock"
                : "NON_MOCK";

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
                readOnlyEnabled = _liveReadOnlyPolicy.IsEnabledFor(User),
                automaticMigrationEnabled = migrationEnabled,
                backgroundInsightsEnabled,
                databaseTargetGuard
            });
        }

        [HttpPost("product-price-confirmation")]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<IActionResult> ConfirmProductPrice([FromBody] ProductPriceConfirmationRequest request)
        {
            if (_confirmationStore == null || _productService == null)
                return StatusCode(500, new { message = "Product price confirmation is not configured." });

            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (!string.Equals(role, RoleNames.Admin, StringComparison.OrdinalIgnoreCase))
                return Forbid();

            if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId) || userId == Guid.Empty)
                return Forbid();

            if (request == null || string.IsNullOrWhiteSpace(request.Token))
                return BadRequest(new { message = "Confirmation token is required." });

            if (!_confirmationStore.TryConsume(request.Token, userId, out var pending, out var error))
                return BadRequest(new { message = error });

            var result = await _productService.UpdatePriceByIdAsync(
                pending.ProductId,
                pending.ExpectedCurrentPrice,
                pending.ProposedNewPrice);

            if (!result.Success)
                return Conflict(new { message = result.Message });

            return Ok(new { message = result.Message });
        }
    }
}
