using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Application.DTOs.Notifications;

namespace RestaurantPOS.Application.Services;

public interface INotificationService
{
    Task<NotificationPageDto> GetForUserAsync(string role, Guid userId, Guid? branchId, string? readState, string? type, DateTime? from, DateTime? to, int page, int pageSize);
    Task<int> GetUnreadCountAsync(string role, Guid userId, Guid? branchId);
    Task<Notification> CreateAsync(Notification notification);
    Task<bool> MarkReadAsync(Guid id, string role, Guid userId, Guid? branchId);
    Task<int> MarkEntityReadAsync(Guid entityId, string? entityType, string role, Guid userId, Guid? branchId);
    Task<int> MarkAllReadAsync(string role, Guid userId, Guid? branchId, DateTime cutoffUtc);
}
