using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RestaurantPOS.Application.Services;

namespace RestaurantPOS.WebAPI.Controllers
{
    /// <summary>Cung cấp các endpoint quản lý chi nhánh.</summary>
    [ApiController]
    [Route("api/[controller]")]
    public class BranchController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IDashboardService? _dashboardService;

        public BranchController(ApplicationDbContext context, IDashboardService? dashboardService = null)
        {
            _context = context;
            _dashboardService = dashboardService;
        }

        /// <summary>Lấy danh sách chi nhánh.</summary>
        [HttpGet]
        public async Task<IActionResult> GetBranches()
        {
            try
            {
                var branches = await _context.Branches
                    .Where(b => b.IsActive)
                    .OrderByDescending(b => b.IsMain)
                    .ThenBy(b => b.Name)
                    .Select(b => new BranchPublicDto
                    {
                        Id = b.Id,
                        Name = b.Name,
                        Address = b.Address,
                        PhoneNumber = b.PhoneNumber,
                        ImageUrl = b.ImageUrl
                    })
                    .ToListAsync();
                return Ok(branches);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>Lấy danh sách đầy đủ cho màn hình quản trị chi nhánh.</summary>
        [HttpGet("management")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetManagementBranches()
        {
            var branches = await _context.Branches
                .OrderByDescending(b => b.IsMain)
                .ThenBy(b => b.Name)
                .Select(b => new BranchManagementDto
                {
                    Id = b.Id,
                    Name = b.Name,
                    Address = b.Address,
                    PhoneNumber = b.PhoneNumber,
                    IsMain = b.IsMain,
                    IsActive = b.IsActive,
                    BankName = b.BankName,
                    AccountNumber = b.AccountNumber,
                    AccountHolder = b.AccountHolder,
                    ImageUrl = b.ImageUrl,
                    TaxCode = b.TaxCode,
                    RepresentativeName = b.RepresentativeName,
                    RepresentativeEmail = b.RepresentativeEmail,
                    Industry = b.Industry,
                    BusinessType = b.BusinessType,
                    CreatedAt = b.CreatedAt
                })
                .ToListAsync();

            return Ok(branches);
        }

        /// <summary>Lấy thông tin VietQR của một chi nhánh cho POS đã xác thực.</summary>
        [HttpGet("{id:guid}/payment-info")]
        [Authorize(Roles = "admin,manager,employee,cashier")]
        public async Task<IActionResult> GetPaymentInfo(Guid id)
        {
            if (!HasBranchAccess(id)) return Forbid();

            var paymentInfo = await _context.Branches
                .Where(b => b.Id == id && b.IsActive)
                .Select(b => new BranchPaymentInfoDto
                {
                    Id = b.Id,
                    BankName = b.BankName,
                    AccountNumber = b.AccountNumber,
                    AccountHolder = b.AccountHolder
                })
                .FirstOrDefaultAsync();

            return paymentInfo == null ? NotFound() : Ok(paymentInfo);
        }

        /// <summary>Tạo chi nhánh mới.</summary>
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> CreateBranch([FromBody] Branch branch)
        {
            try
            {
                branch.Id = Guid.NewGuid();
                branch.CreatedAt = DateTime.UtcNow;

                // Nếu đây là trụ sở chính, bỏ chính của các chi nhánh khác
                if (branch.IsMain)
                {
                    var mainBranches = await _context.Branches.Where(b => b.IsMain).ToListAsync();
                    foreach (var b in mainBranches)
                    {
                        b.IsMain = false;
                        _context.Entry(b).State = EntityState.Modified;
                    }
                }

                _context.Branches.Add(branch);
                await _context.SaveChangesAsync();
                _dashboardService?.InvalidateSummaryCache();
                return Ok(branch);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi tạo chi nhánh", detail = ex.Message, inner = ex.InnerException?.Message });
            }
        }

        /// <summary>Cập nhật thông tin chi nhánh.</summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateBranch(Guid id, [FromBody] Branch branch)
        {
            if (id != branch.Id) return BadRequest(new { message = "ID không khớp" });

            try
            {
                var existingBranch = await _context.Branches.FindAsync(id);
                if (existingBranch == null) return NotFound();

                // Cập nhật các trường thông tin
                existingBranch.Name = branch.Name;
                existingBranch.Address = branch.Address;
                existingBranch.PhoneNumber = branch.PhoneNumber;
                existingBranch.IsMain = branch.IsMain;
                existingBranch.IsActive = branch.IsActive;
                existingBranch.BankName = branch.BankName;
                existingBranch.AccountNumber = branch.AccountNumber;
                existingBranch.AccountHolder = branch.AccountHolder;
                existingBranch.ImageUrl = branch.ImageUrl;
                existingBranch.TaxCode = branch.TaxCode;
                existingBranch.RepresentativeName = branch.RepresentativeName;
                existingBranch.RepresentativeEmail = branch.RepresentativeEmail;
                existingBranch.Industry = branch.Industry;
                existingBranch.BusinessType = branch.BusinessType;

                if (existingBranch.IsMain)
                {
                    var otherMains = await _context.Branches.Where(b => b.IsMain && b.Id != id).ToListAsync();
                    foreach (var b in otherMains)
                    {
                        b.IsMain = false;
                        _context.Entry(b).State = EntityState.Modified;
                    }
                }

                await _context.SaveChangesAsync();
                _dashboardService?.InvalidateSummaryCache();
                return Ok(existingBranch);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật chi nhánh", detail = ex.Message });
            }
        }

        /// <summary>Bật hoặc tắt trạng thái hoạt động của chi nhánh.</summary>
        [HttpPatch("{id}/toggle-status")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> ToggleStatus(Guid id)
        {
            try
            {
                var branch = await _context.Branches.FindAsync(id);
                if (branch == null) return NotFound();

                branch.IsActive = !branch.IsActive;
                await _context.SaveChangesAsync();
                _dashboardService?.InvalidateSummaryCache();
                return Ok(new { isActive = branch.IsActive });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>Xóa chi nhánh theo mã định danh.</summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteBranch(Guid id)
        {
            try
            {
                var branch = await _context.Branches.FindAsync(id);
                if (branch == null) return NotFound();
                if (branch.IsMain) return BadRequest(new { message = "Không thể xóa trụ sở chính" });

                _context.Branches.Remove(branch);
                await _context.SaveChangesAsync();
                _dashboardService?.InvalidateSummaryCache();
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        private bool HasBranchAccess(Guid branchId)
        {
            if (User.IsInRole("admin")) return true;
            return Guid.TryParse(User.FindFirst("branchId")?.Value, out var userBranchId)
                && userBranchId == branchId;
        }
    }

    public sealed class BranchPublicDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string? Address { get; init; }
        public string? PhoneNumber { get; init; }
        public string? ImageUrl { get; init; }
    }

    public sealed class BranchManagementDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string? Address { get; init; }
        public string? PhoneNumber { get; init; }
        public bool IsMain { get; init; }
        public bool IsActive { get; init; }
        public string? BankName { get; init; }
        public string? AccountNumber { get; init; }
        public string? AccountHolder { get; init; }
        public string? ImageUrl { get; init; }
        public string? TaxCode { get; init; }
        public string? RepresentativeName { get; init; }
        public string? RepresentativeEmail { get; init; }
        public string? Industry { get; init; }
        public string? BusinessType { get; init; }
        public DateTime CreatedAt { get; init; }
    }

    public sealed class BranchPaymentInfoDto
    {
        public Guid Id { get; init; }
        public string? BankName { get; init; }
        public string? AccountNumber { get; init; }
        public string? AccountHolder { get; init; }
    }
}
