using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantPOS.WebAPI.Controllers
{
    /// <summary>Cung cấp các endpoint quản lý chi nhánh.</summary>
    [ApiController]
    [Route("api/[controller]")]
    public class BranchController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BranchController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>Lấy danh sách chi nhánh.</summary>
        [HttpGet]
        public async Task<IActionResult> GetBranches()
        {
            try
            {
                return Ok(await _context.Branches.OrderByDescending(b => b.IsMain).ThenBy(b => b.Name).ToListAsync());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
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
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
