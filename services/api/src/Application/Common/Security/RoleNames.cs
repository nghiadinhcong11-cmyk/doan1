using System;
using System.Collections.Generic;

namespace RestaurantPOS.Application.Common.Security;

/// <summary>
/// Canonical role names and scope rules used by authentication and authorization.
/// </summary>
public static class RoleNames
{
    public const string Admin = "admin";
    public const string Manager = "manager";
    public const string Employee = "employee";
    public const string Cashier = "cashier";
    public const string Kitchen = "kitchen";
    public const string Customer = "customer";

    private static readonly HashSet<string> StaffRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        Admin,
        Manager,
        Employee,
        Cashier,
        Kitchen
    };

    private static readonly HashSet<string> KnownRoles = new(StaffRoles, StringComparer.OrdinalIgnoreCase)
    {
        Customer
    };

    public static bool IsStaffRole(string? role) =>
        !string.IsNullOrWhiteSpace(role) && StaffRoles.Contains(role.Trim());

    public static bool IsBranchScopedStaffRole(string? role) =>
        string.Equals(role?.Trim(), Manager, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(role?.Trim(), Employee, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(role?.Trim(), Cashier, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(role?.Trim(), Kitchen, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Validates the branch scope carried by a staff JWT against the employee's
    /// current branch assignment. Global administrators do not require a branch
    /// claim; every other staff role must have a non-empty matching branch.
    /// </summary>
    public static bool IsValidBranchClaim(string? role, Guid? currentEmployeeBranchId, string? jwtBranchId)
    {
        if (!IsKnownRole(role) || !IsStaffRole(role))
            return false;

        if (!IsBranchScopedStaffRole(role))
            return true;

        if (!Guid.TryParse(jwtBranchId, out var tokenBranchId) || tokenBranchId == Guid.Empty)
            return false;

        return currentEmployeeBranchId.HasValue &&
               currentEmployeeBranchId.Value != Guid.Empty &&
               currentEmployeeBranchId.Value == tokenBranchId;
    }

    public static bool IsValidStaffTokenState(
        string? tokenRole,
        string? currentEmployeeRole,
        bool currentEmployeeIsActive,
        Guid? currentEmployeeBranchId,
        string? jwtBranchId)
    {
        if (!currentEmployeeIsActive || !IsKnownRole(tokenRole) || !IsStaffRole(tokenRole))
            return false;

        if (!TryNormalizeStaffRole(currentEmployeeRole, out var normalizedEmployeeRole) ||
            !string.Equals(normalizedEmployeeRole, tokenRole?.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return IsValidBranchClaim(tokenRole, currentEmployeeBranchId, jwtBranchId);
    }

    public static bool IsKnownRole(string? role) =>
        !string.IsNullOrWhiteSpace(role) && KnownRoles.Contains(role.Trim());

    public static bool TryNormalizeStaffRole(string? role, out string normalizedRole)
    {
        normalizedRole = role?.Trim().ToLowerInvariant() ?? string.Empty;
        return IsStaffRole(normalizedRole);
    }
}
