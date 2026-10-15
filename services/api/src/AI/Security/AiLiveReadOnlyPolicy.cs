using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RestaurantPOS.Application.Common.Security;

namespace RestaurantPOS.AI.Security;

/// <summary>
/// Server-side gate for the opt-in Development-only live AI E2E mode.
/// Request headers, query parameters and prompt content are intentionally not consulted.
/// </summary>
public sealed class AiLiveReadOnlyPolicy
{
    private const string EnabledKey = "AI:LiveReadOnlyE2E:Enabled";
    private const string AdminUserIdKey = "AI:LiveReadOnlyE2E:AdminUserId";
    private const string AllowedIdentitiesKey = "AI:LiveReadOnlyE2E:AllowedIdentities";
    private const string ModeKey = "AI:LiveReadOnlyE2E:Mode";
    private const string CustomerUserIdKey = "AI:LiveReadOnlyE2E:CustomerUserId";
    private const string ProviderModeKey = "AI:ProviderMode";
    private const string AutomaticMigrationKey = "DatabaseSafety:AllowAutomaticMigration";
    private const string BackgroundInsightsKey = "BackgroundJobs:EnableInsights";
    private const string DevelopmentProjectRefKey = "DEV_SUPABASE_PROJECT_REF";

    private readonly IConfiguration? _configuration;
    private readonly IHostEnvironment? _environment;

    public AiLiveReadOnlyPolicy()
    {
        // Permanently disabled compatibility instance for direct unit-test construction.
    }

    [ActivatorUtilitiesConstructor]
    public AiLiveReadOnlyPolicy(IConfiguration configuration, IHostEnvironment environment)
    {
        _configuration = configuration;
        _environment = environment;
    }

    public bool IsRequested =>
        TryGetEnabled(out var enabled) && enabled;

    public bool IsConfigurationValid =>
        IsRequested &&
        _environment?.IsDevelopment() == true &&
        IsMockProviderConfigured &&
        IsDatabaseSafeForE2E &&
        IsBackgroundJobsSafeForE2E &&
        IsApprovedDevelopmentTarget &&
        IsSupportedMode &&
        TryGetAllowedIdentities(out _);

    public string RuntimeMode =>
        string.Equals(_configuration?[ModeKey], "Customer", StringComparison.OrdinalIgnoreCase)
            ? "Customer"
            : "ManagerAB";

    public bool IsMisconfigured =>
        HasInvalidEnabledValue || (IsRequested && !IsConfigurationValid);

    public IReadOnlyList<AllowlistIdentityAttestation> GetAllowlistIdentitiesForAttestation() =>
        GetAllowedIdentities()
            .Select(identity => new AllowlistIdentityAttestation(identity.UserId, identity.Role, identity.BranchId))
            .ToArray();

    public static string? ComputeAllowlistHmac(
        IEnumerable<AllowlistIdentityAttestation> identities,
        string? key)
    {
        if (string.IsNullOrWhiteSpace(key)) return null;
        var canonical = string.Join("|", identities
            .OrderBy(identity => identity.UserId.ToString("N"), StringComparer.Ordinal)
            .ThenBy(identity => identity.Role.ToLowerInvariant(), StringComparer.Ordinal)
            .ThenBy(identity => identity.BranchId?.ToString("N") ?? string.Empty, StringComparer.Ordinal)
            .Select(identity =>
                $"{identity.UserId:N};{identity.Role.ToLowerInvariant()};{identity.BranchId?.ToString("N") ?? ""}"));
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
        return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(canonical))).ToLowerInvariant();
    }

    public bool IsEnabledFor(ClaimsPrincipal user)
    {
        if (!IsConfigurationValid)
        {
            return false;
        }

        var role = user.FindFirst(ClaimTypes.Role)?.Value?.Trim().ToLowerInvariant();
        if (!Guid.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId) ||
            userId == Guid.Empty)
        {
            return false;
        }

        if (RuntimeMode == "Customer")
        {
            return role == RoleNames.Customer &&
                   GetAllowedIdentities().Count == 1 &&
                   GetAllowedIdentities()[0].UserId == userId;
        }

        if (!RoleNames.IsStaffRole(role))
            return false;

        var branchClaim = user.FindFirst("branchId")?.Value;
        Guid? branchId = Guid.TryParse(branchClaim, out var parsedBranchId) && parsedBranchId != Guid.Empty
            ? parsedBranchId
            : null;

        foreach (var identity in GetAllowedIdentities())
        {
            if (identity.UserId != userId ||
                !string.Equals(identity.Role, role, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (RoleNames.IsBranchScopedStaffRole(role))
                return identity.BranchId.HasValue && branchId == identity.BranchId;

            return true;
        }

        return false;
    }

    private bool TryGetAllowedIdentities(out IReadOnlyList<AllowedIdentity> identities)
    {
        identities = GetAllowedIdentities();
        return identities.Count > 0;
    }

    private IReadOnlyList<AllowedIdentity> GetAllowedIdentities()
    {
        var identities = new List<AllowedIdentity>();

        if (RuntimeMode == "Customer")
        {
            return Guid.TryParse(_configuration?[CustomerUserIdKey], out var customerId) && customerId != Guid.Empty
                ? new[] { new AllowedIdentity(customerId, RoleNames.Customer, null) }
                : Array.Empty<AllowedIdentity>();
        }

        var section = _configuration?.GetSection(AllowedIdentitiesKey);
        var children = section?.GetChildren() ?? Array.Empty<IConfigurationSection>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var child in children)
        {
            if (!Guid.TryParse(child["UserId"], out var userId) || userId == Guid.Empty)
                return Array.Empty<AllowedIdentity>();

            var role = child["Role"]?.Trim().ToLowerInvariant();
            if (!RoleNames.IsStaffRole(role))
                return Array.Empty<AllowedIdentity>();

            Guid? branchId = null;
            var branchValue = child["BranchId"];
            if (!string.IsNullOrWhiteSpace(branchValue))
            {
                if (!Guid.TryParse(branchValue, out var parsedBranchId) || parsedBranchId == Guid.Empty)
                    return Array.Empty<AllowedIdentity>();
                branchId = parsedBranchId;
            }

            if (RoleNames.IsBranchScopedStaffRole(role) && !branchId.HasValue)
                return Array.Empty<AllowedIdentity>();

            var identityKey = $"{userId:N}|{role}|{branchId:N}";
            if (!seen.Add(identityKey))
                return Array.Empty<AllowedIdentity>();

            identities.Add(new AllowedIdentity(userId, role!, branchId));
        }

        // Keep the existing single-admin configuration working while allowing
        // the explicitly configured multi-role list to extend it.
        if (Guid.TryParse(_configuration?[AdminUserIdKey], out var adminId) && adminId != Guid.Empty)
        {
            var identityKey = $"{adminId:N}|{RoleNames.Admin}|";
            if (seen.Add(identityKey))
                identities.Add(new AllowedIdentity(adminId, RoleNames.Admin, null));
        }

        return identities;
    }

    private bool IsMockProviderConfigured =>
        string.Equals(_configuration?[ProviderModeKey], "Mock", StringComparison.OrdinalIgnoreCase);

    private bool IsDatabaseSafeForE2E =>
        TryParseBoolean(_configuration?[AutomaticMigrationKey], out var enabled) && !enabled;

    private bool IsBackgroundJobsSafeForE2E =>
        TryParseBoolean(_configuration?[BackgroundInsightsKey], out var enabled) && !enabled;

    private bool IsApprovedDevelopmentTarget =>
        string.Equals(
            _configuration?[DevelopmentProjectRefKey],
            QrTokenBackfillTargetGuard.AuthorizedDevProjectRef,
            StringComparison.Ordinal);

    private bool IsSupportedMode =>
        string.IsNullOrWhiteSpace(_configuration?[ModeKey]) ||
        string.Equals(_configuration?[ModeKey], "ManagerAB", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(_configuration?[ModeKey], "Customer", StringComparison.OrdinalIgnoreCase);

    private static bool TryParseBoolean(string? value, out bool result) =>
        bool.TryParse(value, out result);

    private bool HasInvalidEnabledValue =>
        _configuration?[EnabledKey] is string rawValue &&
        !bool.TryParse(rawValue, out _);

    private bool TryGetEnabled(out bool enabled)
    {
        enabled = false;
        var rawValue = _configuration?[EnabledKey];
        return rawValue == null || bool.TryParse(rawValue, out enabled);
    }

    private sealed record AllowedIdentity(Guid UserId, string Role, Guid? BranchId);
}

public sealed record AllowlistIdentityAttestation(Guid UserId, string Role, Guid? BranchId);
