using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
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

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        /// <summary>
        /// Lấy số liệu tổng hợp dashboard: Doanh thu theo giờ/tuần/tháng, cơ cấu chi nhánh, top món bán chạy, nhân sự
        /// </summary>
        /// <param name="branchId">Tùy chọn lọc theo chi nhánh cụ thể (hoặc null nếu xem toàn chuỗi)</param>
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary([FromQuery] string? branchId = null)
        {
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            if (role != "admin" && role != "manager") return Forbid();

            if (role == "manager")
            {
                var ownBranch = User.FindFirst("branchId")?.Value;
                if (!Guid.TryParse(ownBranch, out var managerBranchId)) return Forbid();

                // Manager chỉ được xem dashboard của chính mình
                branchId = managerBranchId.ToString();
            }

            try
            {
                var summary = await _dashboardService.GetSummaryAsync(branchId);
                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
