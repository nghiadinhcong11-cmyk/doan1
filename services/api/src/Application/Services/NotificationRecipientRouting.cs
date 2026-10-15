using System;
using System.Collections.Generic;
using RestaurantPOS.Domain.Entities;

namespace RestaurantPOS.Application.Services;

/// <summary>
/// Resolves notification recipients to authenticated SignalR groups.
/// An empty result is an invalid/ambiguous scope and must never become a broadcast.
/// </summary>
public static class NotificationRecipientRouting
{
    public static IReadOnlyList<string> Resolve(Notification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);

        if (notification.TargetUserId.HasValue)
        {
            // A user target is exact. Mixing it with a role/branch target would require
            // an intersection query that SignalR group broadcasts cannot enforce safely.
            if (notification.TargetUserId.Value == Guid.Empty ||
                !string.IsNullOrWhiteSpace(notification.TargetRole) ||
                notification.BranchId.HasValue)
            {
                return Array.Empty<string>();
            }

            return new[] { UserGroup(notification.TargetUserId.Value) };
        }

        if (!string.IsNullOrWhiteSpace(notification.TargetRole) && notification.BranchId.HasValue)
        {
            var groups = new List<string>
            {
                BranchRoleGroup(notification.BranchId.Value, notification.TargetRole)
            };

            // Preserve the existing BusinessInsight admin visibility rule.
            if (string.Equals(notification.Type, "BusinessInsight", StringComparison.Ordinal))
            {
                groups.Add("role:admin");
            }

            return groups;
        }

        if (!string.IsNullOrWhiteSpace(notification.TargetRole))
        {
            return new[] { $"role:{notification.TargetRole}" };
        }

        if (notification.BranchId.HasValue)
        {
            return new[] { BranchGroup(notification.BranchId.Value) };
        }

        // Notifications without a recipient scope are not safe to deliver.
        return Array.Empty<string>();
    }

    public static string UserGroup(Guid userId) => $"user:{userId:D}";

    public static string BranchGroup(Guid branchId) => $"branch:{branchId:D}";

    public static string BranchRoleGroup(Guid branchId, string role) => $"branch:{branchId:D}:role:{role}";
}
