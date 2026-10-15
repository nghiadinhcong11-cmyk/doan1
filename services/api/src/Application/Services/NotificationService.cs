using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Application.DTOs.Notifications;
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

    public async Task<NotificationPageDto> GetForUserAsync(string role, Guid userId, Guid? branchId, string? readState, string? type, DateTime? from, DateTime? to, int page, int pageSize)
    {
        page = Math.Clamp(page, 1, 100000);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var query = _context.Notifications.Where(n =>
            (n.TargetRole == null || n.TargetRole == role) &&
            (n.TargetUserId == null || n.TargetUserId == userId) &&
            (n.BranchId == null || (branchId.HasValue && n.BranchId == branchId.Value)));
        if (!string.IsNullOrWhiteSpace(type)) query = query.Where(n => n.Type == type);
        if (from.HasValue) query = query.Where(n => n.CreatedAt >= from.Value);
        if (to.HasValue) query = query.Where(n => n.CreatedAt < to.Value.Date.AddDays(1));

        var withReadState = query.Select(n => new
        {
            Notification = n,
            IsRead = n.UseLegacyReadState
                ? n.IsRead
                : _context.NotificationReads.Any(read => read.NotificationId == n.Id && read.UserId == userId && read.ReadAt != default)
        });
        if (string.Equals(readState, "unread", StringComparison.OrdinalIgnoreCase)) withReadState = withReadState.Where(item => !item.IsRead);
        if (string.Equals(readState, "read", StringComparison.OrdinalIgnoreCase)) withReadState = withReadState.Where(item => item.IsRead);

        var total = await withReadState.CountAsync();
        var rows = await withReadState.AsNoTracking()
            .OrderByDescending(item => item.Notification.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        var branchIds = rows.Where(row => row.Notification.BranchId.HasValue).Select(row => row.Notification.BranchId!.Value).Distinct().ToList();
        var branchNames = await _context.Branches.AsNoTracking().Where(branch => branchIds.Contains(branch.Id)).ToDictionaryAsync(branch => branch.Id, branch => branch.Name);
        return new NotificationPageDto
        {
            Items = rows.Select(row => Map(row.Notification, row.IsRead, branchNames)).ToList(),
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<int> GetUnreadCountAsync(string role, Guid userId, Guid? branchId)
    {
        var page = await GetForUserAsync(role, userId, branchId, "unread", null, null, null, 1, 1);
        return page.Total;
    }

    public async Task<Notification> CreateAsync(Notification notification)
    {
        var groups = NotificationRecipientRouting.Resolve(notification);
        if (groups.Count == 0)
        {
            throw new InvalidOperationException("Notification recipient scope is missing or ambiguous.");
        }

        notification.Id = notification.Id == Guid.Empty ? Guid.NewGuid() : notification.Id;
        notification.CreatedAt = notification.CreatedAt == default ? DateTime.UtcNow : notification.CreatedAt;
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        await _hub.Clients.Groups(groups).SendAsync("NotificationCreated", notification);
        return notification;
    }

    public async Task<bool> MarkReadAsync(Guid id, string role, Guid userId, Guid? branchId)
    {
        var notification = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == id &&
            (n.TargetRole == null || n.TargetRole == role) &&
            (n.TargetUserId == null || n.TargetUserId == userId) &&
            (n.BranchId == null || (branchId.HasValue && n.BranchId == branchId.Value)));
        if (notification == null) return false;
        if (notification.UseLegacyReadState)
        {
            notification.IsRead = true;
        }
        else if (!await _context.NotificationReads.AnyAsync(read => read.NotificationId == id && read.UserId == userId))
        {
            _context.NotificationReads.Add(new NotificationRead { NotificationId = id, UserId = userId, ReadAt = DateTime.UtcNow });
        }
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> MarkAllReadAsync(string role, Guid userId, Guid? branchId, DateTime cutoffUtc)
    {
        var notifications = await _context.Notifications.Where(n => n.CreatedAt <= cutoffUtc &&
            (n.TargetRole == null || n.TargetRole == role) &&
            (n.TargetUserId == null || n.TargetUserId == userId) &&
            (n.BranchId == null || (branchId.HasValue && n.BranchId == branchId.Value))).ToListAsync();
        var legacy = notifications.Where(notification => notification.UseLegacyReadState && !notification.IsRead).ToList();
        foreach (var notification in legacy) notification.IsRead = true;
        var ids = notifications.Where(notification => !notification.UseLegacyReadState).Select(notification => notification.Id).ToList();
        if (ids.Count > 0)
        {
            var existing = await _context.NotificationReads.Where(read => read.UserId == userId && ids.Contains(read.NotificationId)).Select(read => read.NotificationId).ToListAsync();
            var newReadCount = ids.Except(existing).Count();
            var now = DateTime.UtcNow;
            foreach (var id in ids.Except(existing)) _context.NotificationReads.Add(new NotificationRead { NotificationId = id, UserId = userId, ReadAt = now });
            await _context.SaveChangesAsync();
            return legacy.Count + newReadCount;
        }
        await _context.SaveChangesAsync();
        return legacy.Count;
    }

    private static NotificationDto Map(Notification notification, bool isRead, IReadOnlyDictionary<Guid, string> branchNames) => new()
    {
        Id = notification.Id,
        Type = notification.Type,
        Title = notification.Title,
        Message = notification.Message,
        EntityType = notification.EntityType,
        EntityId = notification.EntityId,
        Route = notification.Route,
        BranchId = notification.BranchId,
        BranchName = notification.BranchId.HasValue && branchNames.TryGetValue(notification.BranchId.Value, out var name) ? name : null,
        CreatedAt = notification.CreatedAt,
        IsRead = isRead
    };
}
