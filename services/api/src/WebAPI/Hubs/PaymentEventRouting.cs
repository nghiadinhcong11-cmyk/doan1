using System;
using System.Collections.Generic;

namespace RestaurantPOS.WebAPI.Hubs;

public static class PaymentEventRouting
{
    public static IReadOnlyList<string> Groups(Guid branchId)
    {
        if (branchId == Guid.Empty) return Array.Empty<string>();

        var groups = new List<string> { "kitchen-admin" };
        foreach (var role in new[] { "manager", "employee", "cashier" })
        {
            groups.Add($"branch:{branchId:D}:role:{role}");
        }

        return groups;
    }
}
