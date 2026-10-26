using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using RestaurantPOS.Application.Services;

namespace RestaurantPOS.WebAPI.Controllers
{
    /// <summary>
    /// API Thống kê báo cáo, doanh thu và số liệu tổng hợp cho Quản trị viên
    /// </summary>
    /// <summary>Cung cấp số liệu tổng quan cho màn hình dashboard.</summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "admin,manager")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly ILogger<DashboardController>? _logger;

        public DashboardController(IDashboardService dashboardService, ILogger<DashboardController>? logger = null)
        {
            _dashboardService = dashboardService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy số liệu tổng hợp dashboard: Doanh thu theo giờ/tuần/tháng, cơ cấu chi nhánh, top món bán chạy, nhân sự
        /// </summary>
        /// <param name="branchId">Tùy chọn lọc theo chi nhánh cụ thể (hoặc null nếu xem toàn chuỗi)</param>
        /// <param name="period">Khoảng thời gian: today, week hoặc month (12 tháng gần nhất).</param>
        /// <param name="forceRefresh">Xóa cache tổng hợp trước khi tải lại khi người dùng yêu cầu làm mới thủ công.</param>
        [HttpGet("summary")]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> GetSummary([FromQuery] string? branchId = null, [FromQuery] string period = "today", [FromQuery] bool forceRefresh = false)
        {
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            if (role != "admin" && role != "manager") return Forbid();

            if (role == "admin" && !string.IsNullOrWhiteSpace(branchId) && !Guid.TryParse(branchId, out _))
                return BadRequest(new { message = "branchId không hợp lệ" });

            if (role == "manager")
            {
                var ownBranch = User.FindFirst("branchId")?.Value;
                if (!Guid.TryParse(ownBranch, out var managerBranchId)) return Forbid();

                // Manager chỉ được xem dashboard của chính mình
                branchId = managerBranchId.ToString();
            }

            try
            {
                if (forceRefresh)
                    _dashboardService.InvalidateSummaryCache();

                var summary = await _dashboardService.GetSummaryAsync(branchId, period);
                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Dashboard summary failed for an authorized request.");
                return StatusCode(500, new { message = "Không thể tải dữ liệu dashboard." });
            }
        }
    }
}
