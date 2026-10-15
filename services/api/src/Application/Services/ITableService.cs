using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestaurantPOS.Application.DTOs;
using RestaurantPOS.Domain.Entities;

namespace RestaurantPOS.Application.Services
{
    public interface ITableService
    {
        Task<List<RestaurantTable>> GetTablesAsync(Guid? branchId, string? search = null, string? area = null, bool? isActive = null);
        Task<RestaurantTable?> GetByIdAsync(Guid id, Guid? authorizedBranchId);
        Task<RestaurantTable> CreateTableAsync(TableRequestDto request, Guid? authorizedBranchId);
        Task<RestaurantTable?> UpdateTableAsync(Guid id, TableRequestDto request, Guid? authorizedBranchId);
        Task<bool> UpdateStatusAsync(Guid id, string status, Guid? authorizedBranchId);
        Task<bool> DeleteTableAsync(Guid id, Guid? authorizedBranchId);
        Task<object> GetTableSummaryAsync(Guid? branchId);
        Task<Dictionary<string, int>> GetTableStatusCountsAsync(Guid? branchId);
    }
}
