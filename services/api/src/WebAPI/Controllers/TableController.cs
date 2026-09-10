using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Application.Services;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RestaurantPOS.WebAPI.Controllers
{
    /// <summary>Cung cấp các endpoint quản lý bàn phục vụ.</summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TableController : ControllerBase
    {
        private readonly ITableService _tableService;

        public TableController(ITableService tableService)
        {
            _tableService = tableService;
        }

        /// <summary>Lấy danh sách bàn theo các bộ lọc.</summary>
        [HttpGet]
        public async Task<IActionResult> GetTables([FromQuery] string? search, [FromQuery] string? area, [FromQuery] bool? isActive, [FromQuery] Guid? branchId)
        {
            // Nếu là staff, bắt buộc lọc theo chi nhánh của họ nếu họ không phải admin
            if (User.Identity != null && User.Identity.IsAuthenticated && !IsCustomer())
            {
                if (!TryResolveBranch(branchId, out var effectiveBranchId))
                    return Forbid();

                return Ok(await _tableService.GetTablesAsync(effectiveBranchId, search, area, isActive));
            }

            // Đối với khách vãng lai hoặc customer, cho phép xem bàn của một chi nhánh cụ thể
            // (Không cho phép xem toàn bộ bàn của tất cả chi nhánh nếu không truyền branchId)
            if (!branchId.HasValue)
            {
                // Nếu là Admin thì có thể xem tất cả, nhưng thường guest flow cần branchId
                if (!IsAdmin())
                    return BadRequest("Vui lòng cung cấp mã chi nhánh (branchId).");
            }

            return Ok(await _tableService.GetTablesAsync(branchId, search, area, isActive));
        }

        /// <summary>Lấy thông tin chi tiết một bàn.</summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTable(Guid id)
        {
            var branchId = GetUserBranchId();
            var table = await _tableService.GetByIdAsync(id, IsAdmin() ? null : branchId);

            if (table == null) return NotFound();
            return Ok(table);
        }

        /// <summary>Tạo bàn mới.</summary>
        [HttpPost]
        [Authorize(Roles = "admin,manager")]
        public async Task<IActionResult> CreateTable(RestaurantTable table)
        {
            try
            {
                var branchId = GetUserBranchId();

                // Manager/Employee không được phép tạo bàn cho chi nhánh khác
                if (!IsAdmin())
                {
                    if (branchId == null) return Forbid();
                    if (table.BranchId.HasValue && table.BranchId != branchId)
                        return Forbid();
                }

                var result = await _tableService.CreateTableAsync(table, IsAdmin() ? null : branchId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new {
                    message = "Lỗi khi lưu bàn vào database",
                    error = ex.Message
                });
            }
        }

        /// <summary>Cập nhật thông tin bàn.</summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "admin,manager")]
        public async Task<IActionResult> UpdateTable(Guid id, RestaurantTable tableUpdate)
        {
            var branchId = GetUserBranchId();

            // Không cho phép Manager đổi chi nhánh của bàn sang chi nhánh khác
            if (!IsAdmin())
            {
                if (branchId == null) return Forbid();
                if (tableUpdate.BranchId.HasValue && tableUpdate.BranchId != branchId)
                    return Forbid();
            }

            var result = await _tableService.UpdateTableAsync(id, tableUpdate, IsAdmin() ? null : branchId);
            if (result == null) return NotFound();

            return Ok(result);
        }

        /// <summary>Cập nhật trạng thái sử dụng của bàn.</summary>
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "admin,manager,employee,cashier")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] string status)
        {
            var branchId = GetUserBranchId();
            if (!IsAdmin() && branchId == null) return Forbid();

            var success = await _tableService.UpdateStatusAsync(id, status, IsAdmin() ? null : branchId);

            if (!success) return NotFound();
            return Ok(new { id, status });
        }

        /// <summary>Xóa bàn theo mã định danh.</summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin,manager")]
        public async Task<IActionResult> DeleteTable(Guid id)
        {
            var branchId = GetUserBranchId();
            if (!IsAdmin() && branchId == null) return Forbid();

            var success = await _tableService.DeleteTableAsync(id, IsAdmin() ? null : branchId);

            if (!success) return NotFound();
            return NoContent();
        }

        private bool TryResolveBranch(Guid? requestedBranchId, out Guid? effectiveBranchId)
        {
            effectiveBranchId = requestedBranchId;
            if (IsAdmin()) return true;

            var userBranchId = GetUserBranchId();
            if (userBranchId == null) return false;

            if (requestedBranchId.HasValue && requestedBranchId.Value != userBranchId.Value)
                return false;

            effectiveBranchId = userBranchId;
            return true;
        }

        private Guid? GetUserBranchId()
        {
            var claimValue = User.FindFirst("branchId")?.Value;
            return Guid.TryParse(claimValue, out var userBranchId) ? userBranchId : null;
        }

        private bool IsAdmin() => User.IsInRole("admin");
        private bool IsCustomer() => User.IsInRole("customer");
    }
}
