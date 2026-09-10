using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestaurantPOS.Domain.Entities;

namespace RestaurantPOS.Application.Services;

public interface ISystemSettingService
{
    Task<Dictionary<string, string>> GetSettingsAsync(Guid? branchId);
    Task UpdateSettingsAsync(Guid? branchId, Dictionary<string, string> settings);
    Task<string?> GetSettingValueAsync(Guid? branchId, string key, string defaultValue = "");
}
