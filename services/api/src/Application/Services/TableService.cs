using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RestaurantPOS.Application.Common.Security;
using RestaurantPOS.Application.DTOs;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Infrastructure.Persistence;

namespace RestaurantPOS.Application.Services
{
    public class TableService : ITableService
    {
        private readonly ApplicationDbContext _context;

        public TableService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<RestaurantTable>> GetTablesAsync(Guid? branchId, string? search = null, string? area = null, bool? isActive = null)
        {
            var query = _context.Tables.AsNoTracking().Include(t => t.Area).Where(t => t.Name != "Mang về");

            if (branchId.HasValue)
            {
                query = query.Where(t => t.BranchId == branchId.Value);
            }

            if (!string.IsNullOrEmpty(search))
                query = query.Where(t => t.Name.Contains(search) || (t.Description != null && t.Description.Contains(search)));

            if (!string.IsNullOrEmpty(area))
                query = query.Where(t => t.Area != null && t.Area.Name == area);

            if (isActive.HasValue)
                query = query.Where(t => t.IsActive == isActive.Value);

            return await query.OrderBy(t => t.Name).ToListAsync();
        }

        public async Task<RestaurantTable?> GetByIdAsync(Guid id, Guid? authorizedBranchId)
        {
            var table = await _context.Tables.Include(t => t.Area).FirstOrDefaultAsync(t => t.Id == id);
            if (table == null) return null;

            if (authorizedBranchId.HasValue && table.BranchId != authorizedBranchId.Value)
                return null;

            return table;
        }

        public async Task<RestaurantTable> CreateTableAsync(TableRequestDto request, Guid? authorizedBranchId)
        {
            var branchId = authorizedBranchId ?? request.BranchId;
            if (branchId == Guid.Empty)
                throw new InvalidOperationException("Bàn mới phải thuộc một chi nhánh.");

            var branch = await _context.Branches.FirstOrDefaultAsync(candidate => candidate.Id == branchId)
                ?? throw new InvalidOperationException("Chi nhánh không tồn tại.");
            var area = await GetAreaForBranchAsync(request.AreaId, branchId);

            if (await _context.Tables.AnyAsync(candidate => candidate.BranchId == branchId && candidate.Name == request.Name))
                throw new InvalidOperationException("A table with this name already exists in the branch.");

            var table = new RestaurantTable
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                AreaId = area.Id,
                Area = area,
                SeatCount = request.SeatCount,
                Description = request.Description,
                IsActive = request.IsActive,
                Status = string.IsNullOrEmpty(request.Status) ? "Trống" : request.Status,
                QrCodeUrl = request.QrCodeUrl,
                QrToken = QrTokenGenerator.Generate(),
                BranchId = branchId,
                BranchName = branch.Name,
                CreatedAt = DateTime.UtcNow
            };

            _context.Tables.Add(table);
            await _context.SaveChangesAsync();
            return table;
        }

        public async Task<RestaurantTable?> UpdateTableAsync(Guid id, TableRequestDto request, Guid? authorizedBranchId)
        {
            var table = await _context.Tables.Include(t => t.Area).FirstOrDefaultAsync(t => t.Id == id);
            if (table == null) return null;

            if (authorizedBranchId.HasValue && table.BranchId != authorizedBranchId.Value)
                return null;

            var targetBranchId = authorizedBranchId ?? request.BranchId;
            if (targetBranchId == Guid.Empty)
                throw new InvalidOperationException("Bàn phải thuộc một chi nhánh.");

            var branch = await _context.Branches.FirstOrDefaultAsync(candidate => candidate.Id == targetBranchId)
                ?? throw new InvalidOperationException("Chi nhánh không tồn tại.");
            var area = await GetAreaForBranchAsync(request.AreaId, targetBranchId);

            if (await _context.Tables.AnyAsync(candidate => candidate.Id != id && candidate.BranchId == targetBranchId && candidate.Name == request.Name))
                throw new InvalidOperationException("A table with this name already exists in the branch.");

            table.Name = request.Name;
            table.AreaId = area.Id;
            table.Area = area;
            table.SeatCount = request.SeatCount;
            table.IsActive = request.IsActive;
            table.Description = request.Description;
            table.Status = request.Status ?? table.Status;
            table.QrCodeUrl = request.QrCodeUrl;
            table.BranchId = targetBranchId;
            table.BranchName = branch.Name;

            await _context.SaveChangesAsync();
            return table;
        }

        public async Task<bool> UpdateStatusAsync(Guid id, string status, Guid? authorizedBranchId)
        {
            var table = await _context.Tables.FindAsync(id);
            if (table == null) return false;

            if (authorizedBranchId.HasValue && table.BranchId != authorizedBranchId.Value)
                return false;

            table.Status = status;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteTableAsync(Guid id, Guid? authorizedBranchId)
        {
            var table = await _context.Tables.FindAsync(id);
            if (table == null) return false;

            if (authorizedBranchId.HasValue && table.BranchId != authorizedBranchId.Value)
                return false;

            _context.Tables.Remove(table);
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<Area> GetAreaForBranchAsync(Guid areaId, Guid branchId)
        {
            if (areaId == Guid.Empty)
                throw new InvalidOperationException("Bàn mới phải thuộc một khu vực.");

            var area = await _context.Areas.FirstOrDefaultAsync(a => a.Id == areaId);
            if (area == null)
                throw new InvalidOperationException("Khu vực không tồn tại.");
            if (area.BranchId != branchId)
                throw new InvalidOperationException("Khu vực không thuộc chi nhánh của bàn.");
            return area;
        }

        public async Task<object> GetTableSummaryAsync(Guid? branchId)
        {
            var query = _context.Tables.AsNoTracking().Where(t => t.IsActive && t.Name != "Mang về");
            if (branchId.HasValue)
            {
                query = query.Where(t => t.BranchId == branchId.Value);
            }

            var total = await query.CountAsync();
            var occupied = await query.CountAsync(t => t.Status == "Có khách" || t.Status == "Occupied");
            var available = total - occupied;

            return new
            {
                total,
                occupied,
                available
            };
        }

        public async Task<Dictionary<string, int>> GetTableStatusCountsAsync(Guid? branchId)
        {
            var query = _context.Tables.AsNoTracking().Where(t => t.IsActive && t.Name != "Mang về");
            if (branchId.HasValue)
            {
                query = query.Where(t => t.BranchId == branchId.Value);
            }

            var stats = await query
                .GroupBy(t => t.Status)
                .Select(g => new { Status = g.Key ?? "Trống", Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count);

            return stats;
        }
    }
}
