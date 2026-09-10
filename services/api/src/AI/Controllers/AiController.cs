using Microsoft.AspNetCore.Mvc;
using RestaurantPOS.AI.Models;
using RestaurantPOS.AI.Services;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;

namespace RestaurantPOS.AI.Controllers
{
    /// <summary>
    /// API Trợ lý ảo AI Assistant (Google Gemini + AI Tools)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [EnableRateLimiting("ai-limiter")]
    public class AiController : ControllerBase
    {
        private readonly IAiOrchestrator _orchestrator;

        public AiController(IAiOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        /// <summary>
        /// Gửi tin nhắn đàm thoại với Trợ lý AI (tự động phân quyền và kích hoạt Tool phù hợp)
        /// </summary>
        /// <param name="request">Nội dung tin nhắn và lịch sử hội thoại</param>
        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] AiRequest request)
        {
            try
            {
                string userRole = "customer";
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

                var userContext = new AiUserContext
                {
                    UserId = userId,
                    Role = userRole,
                    UserName = userName,
                    BranchId = branchId
                };

                var result = await _orchestrator.ProcessAsync(request, userContext);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new AiResponse { Success = false, Error = ex.Message });
            }
        }
    }
}
