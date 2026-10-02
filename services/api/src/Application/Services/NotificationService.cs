using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Infrastructure.Persistence;
using RestaurantPOS.WebAPI.Hubs;

namespace RestaurantPOS.Application.Services;

public sealed class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<KitchenHub> _hub;

    public NotificationService(ApplicationDbContext context, IHubContext<KitchenHub> hub)
    {
        _context = context;
        _hub = hub;
    }

    public async Task<IReadOnlyList<Notification>> GetForUserAsync(string role, Guid? userId, Guid? branchId, bool unreadOnly = false)
    {
        var query = _context.Notifications.Where(n =>
            (n.TargetRole == null || n.TargetRole == role) &&
            (n.TargetUserId == null || n.TargetUserId == userId) &&
            (n.BranchId == null || n.BranchId == branchId));
        if (unreadOnly) query = query.Where(n => !n.IsRead);
        return await query.OrderByDescending(n => n.CreatedAt).Take(100).ToListAsync();
    }

    public async Task<Notification> CreateAsync(Notification notification)
    {
        notification.Id = notification.Id == Guid.Empty ? Guid.NewGuid() : notification.Id;
        notification.CreatedAt = notification.CreatedAt == default ? DateTime.UtcNow : notification.CreatedAt;
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        var groups = new List<string>();
        if (!string.IsNullOrWhiteSpace(notification.TargetRole) && notification.BranchId.HasValue)
        {
            groups.Add($"branch:{notification.BranchId.Value}:role:{notification.TargetRole}");
            // Admins should also receive business insights
            if (notification.Type == "BusinessInsight")
            {
                groups.Add("role:admin");
            }
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(notification.TargetRole)) groups.Add($"role:{notification.TargetRole}");
            if (notification.BranchId.HasValue) groups.Add($"branch:{notification.BranchId.Value}");
        }

        if (groups.Count == 0) await _hub.Clients.All.SendAsync("NotificationCreated", notification);
        else await _hub.Clients.Groups(groups).SendAsync("NotificationCreated", notification);
        return notification;
    }

    public async Task<bool> MarkReadAsync(Guid id, string role, Guid? userId, Guid? branchId)
    {
        var notification = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == id &&
            (n.TargetRole == null || n.TargetRole == role) &&
            (n.TargetUserId == null || n.TargetUserId == userId) &&
            (n.BranchId == null || n.BranchId == branchId));
        if (notification == null) return false;
        notification.IsRead = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> MarkAllReadAsync(string role, Guid? userId, Guid? branchId)
    {
        var notifications = await _context.Notifications.Where(n => !n.IsRead &&
            (n.TargetRole == null || n.TargetRole == role) &&
            (n.TargetUserId == null || n.TargetUserId == userId) &&
            (n.BranchId == null || n.BranchId == branchId)).ToListAsync();
        foreach (var notification in notifications) notification.IsRead = true;
        await _context.SaveChangesAsync();
        return notifications.Count;
    }
}
