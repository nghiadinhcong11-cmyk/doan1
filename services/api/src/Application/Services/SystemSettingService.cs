using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Infrastructure.Persistence;

namespace RestaurantPOS.Application.Services;

public class SystemSettingService : ISystemSettingService
{
    private readonly ApplicationDbContext _context;

    public SystemSettingService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Dictionary<string, string>> GetSettingsAsync(Guid? branchId)
    {
        // Lấy tất cả settings có thể áp dụng: Global (BranchId == null) hoặc Branch cụ thể
        var allSettings = await _context.SystemSettings
            .Where(x => x.BranchId == null || x.BranchId == branchId)
            .ToListAsync();

        // Ưu tiên Branch-specific setting (có BranchId) hơn Global setting (BranchId == null)
        return allSettings
            .GroupBy(x => x.Key)
            .Select(g => g.OrderByDescending(x => x.BranchId.HasValue).First())
            .ToDictionary(x => x.Key, x => x.Value);
    }

    public async Task UpdateSettingsAsync(Guid? branchId, Dictionary<string, string> settings)
    {
        var existingSettings = await _context.SystemSettings
            .Where(x => x.BranchId == branchId)
            .ToListAsync();

        foreach (var kvp in settings)
        {
            var setting = existingSettings.FirstOrDefault(x => x.Key == kvp.Key);
            if (setting == null)
            {
                setting = new SystemSetting
                {
                    Id = Guid.NewGuid(),
                    BranchId = branchId,
                    Key = kvp.Key,
                    Value = kvp.Value,
                    Type = InferType(kvp.Value),
                    UpdatedAt = DateTime.UtcNow
                };
                _context.SystemSettings.Add(setting);
            }
            else
            {
                setting.Value = kvp.Value;
                setting.Type = InferType(kvp.Value);
                setting.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task<string?> GetSettingValueAsync(Guid? branchId, string key, string defaultValue = "")
    {
        var settings = await _context.SystemSettings
            .Where(x => (x.BranchId == null || x.BranchId == branchId) && x.Key == key)
            .ToListAsync();

        var setting = settings
            .OrderByDescending(x => x.BranchId.HasValue)
            .FirstOrDefault();

        return setting?.Value ?? defaultValue;
    }

    private string InferType(string value)
    {
        if (bool.TryParse(value, out _)) return "boolean";
        if (decimal.TryParse(value, out _)) return "number";
        return "string";
    }
}
