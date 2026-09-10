using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using RestaurantPOS.Application.Services;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Infrastructure.Persistence;
using Xunit;

namespace RestaurantPOS.Tests;

public class SystemSettingFallbackTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly SqliteConnection _connection;
    private readonly SystemSettingService _service;

    public SystemSettingFallbackTests()
    {
        (_context, _connection) = TestDbContextFactory.Create();
        _service = new SystemSettingService(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task GetSettings_FallbackToGlobal_WhenBranchSettingMissing()
    {
        // Arrange
        var branchId = Guid.NewGuid();
        _context.SystemSettings.Add(new SystemSetting { Id = Guid.NewGuid(), Key = "vatPercent", Value = "10", BranchId = null });
        await _context.SaveChangesAsync();

        // Act
        var settings = await _service.GetSettingsAsync(branchId);

        // Assert
        Assert.True(settings.ContainsKey("vatPercent"));
        Assert.Equal("10", settings["vatPercent"]);
    }

    [Fact]
    public async Task GetSettings_OverrideGlobal_WhenBranchSettingExists()
    {
        // Arrange
        var branchId = Guid.NewGuid();
        _context.SystemSettings.Add(new SystemSetting { Id = Guid.NewGuid(), Key = "vatPercent", Value = "10", BranchId = null });
        _context.SystemSettings.Add(new SystemSetting { Id = Guid.NewGuid(), Key = "vatPercent", Value = "8", BranchId = branchId });
        await _context.SaveChangesAsync();

        // Act
        var settings = await _service.GetSettingsAsync(branchId);

        // Assert
        Assert.True(settings.ContainsKey("vatPercent"));
        Assert.Equal("8", settings["vatPercent"]);
    }

    [Fact]
    public async Task GetSettings_Isolation_BranchACannotReadBranchB()
    {
        // Arrange
        var branchA = Guid.NewGuid();
        var branchB = Guid.NewGuid();
        _context.SystemSettings.Add(new SystemSetting { Id = Guid.NewGuid(), Key = "vatPercent", Value = "8", BranchId = branchA });
        _context.SystemSettings.Add(new SystemSetting { Id = Guid.NewGuid(), Key = "vatPercent", Value = "12", BranchId = branchB });
        await _context.SaveChangesAsync();

        // Act
        var settingsA = await _service.GetSettingsAsync(branchA);

        // Assert
        Assert.Equal("8", settingsA["vatPercent"]);
        Assert.False(settingsA.ContainsValue("12"));
    }

    [Fact]
    public async Task GetSettings_MixedFallbackAndSpecific()
    {
        // Arrange
        var branchId = Guid.NewGuid();
        _context.SystemSettings.Add(new SystemSetting { Id = Guid.NewGuid(), Key = "vatPercent", Value = "10", BranchId = null });
        _context.SystemSettings.Add(new SystemSetting { Id = Guid.NewGuid(), Key = "serviceFee", Value = "5", BranchId = null });
        _context.SystemSettings.Add(new SystemSetting { Id = Guid.NewGuid(), Key = "serviceFee", Value = "2", BranchId = branchId });
        await _context.SaveChangesAsync();

        // Act
        var settings = await _service.GetSettingsAsync(branchId);

        // Assert
        Assert.Equal("10", settings["vatPercent"]);
        Assert.Equal("2", settings["serviceFee"]);
    }

    [Fact]
    public async Task GetSettingValue_FallbackWorks()
    {
        // Arrange
        var branchId = Guid.NewGuid();
        _context.SystemSettings.Add(new SystemSetting { Id = Guid.NewGuid(), Key = "vatPercent", Value = "10", BranchId = null });
        await _context.SaveChangesAsync();

        // Act
        var value = await _service.GetSettingValueAsync(branchId, "vatPercent");

        // Assert
        Assert.Equal("10", value);
    }

    [Fact]
    public async Task GetSettingValue_OverrideWorks()
    {
        // Arrange
        var branchId = Guid.NewGuid();
        _context.SystemSettings.Add(new SystemSetting { Id = Guid.NewGuid(), Key = "vatPercent", Value = "10", BranchId = null });
        _context.SystemSettings.Add(new SystemSetting { Id = Guid.NewGuid(), Key = "vatPercent", Value = "8", BranchId = branchId });
        await _context.SaveChangesAsync();

        // Act
        var value = await _service.GetSettingValueAsync(branchId, "vatPercent");

        // Assert
        Assert.Equal("8", value);
    }
}
