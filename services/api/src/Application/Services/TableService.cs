using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
            var query = _context.Tables.AsNoTracking().Where(t => t.Name != "Mang về");

            if (branchId.HasValue)
            {
                query = query.Where(t => t.BranchId == branchId.Value);
            }

            if (!string.IsNullOrEmpty(search))
                query = query.Where(t => t.Name.Contains(search) || (t.Description != null && t.Description.Contains(search)));

            if (!string.IsNullOrEmpty(area))
                query = query.Where(t => t.AreaName == area);

            if (isActive.HasValue)
                query = query.Where(t => t.IsActive == isActive.Value);

            return await query.OrderBy(t => t.Name).ToListAsync();
        }

        public async Task<RestaurantTable?> GetByIdAsync(Guid id, Guid? authorizedBranchId)
        {
            var table = await _context.Tables.FindAsync(id);
            if (table == null) return null;

            if (authorizedBranchId.HasValue && table.BranchId != authorizedBranchId.Value)
                return null;

            return table;
        }

        public async Task<RestaurantTable> CreateTableAsync(RestaurantTable table, Guid? authorizedBranchId)
        {
            if (authorizedBranchId.HasValue)
            {
                table.BranchId = authorizedBranchId.Value;
                var branch = await _context.Branches.FindAsync(authorizedBranchId.Value);
                if (branch != null) table.BranchName = branch.Name;
            }

            table.Id = Guid.NewGuid();
            table.CreatedAt = DateTime.UtcNow;
            if (string.IsNullOrEmpty(table.Status)) table.Status = "Trống";

            _context.Tables.Add(table);
            await _context.SaveChangesAsync();
            return table;
        }

        public async Task<RestaurantTable?> UpdateTableAsync(Guid id, RestaurantTable tableUpdate, Guid? authorizedBranchId)
        {
            var table = await _context.Tables.FindAsync(id);
            if (table == null) return null;

            if (authorizedBranchId.HasValue && table.BranchId != authorizedBranchId.Value)
                return null;

            table.Name = tableUpdate.Name;
            table.AreaName = tableUpdate.AreaName;
            table.SeatCount = tableUpdate.SeatCount;
            table.IsActive = tableUpdate.IsActive;
            table.Description = tableUpdate.Description;
            table.Status = tableUpdate.Status;

            // Branch change is only for admins
            if (!authorizedBranchId.HasValue)
            {
                table.BranchId = tableUpdate.BranchId;
                table.BranchName = tableUpdate.BranchName;
            }

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
