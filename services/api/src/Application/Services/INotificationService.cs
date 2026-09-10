using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestaurantPOS.Domain.Entities;

namespace RestaurantPOS.Application.Services;

public interface INotificationService
{
    Task<IReadOnlyList<Notification>> GetForUserAsync(string role, Guid? userId, Guid? branchId, bool unreadOnly = false);
    Task<Notification> CreateAsync(Notification notification);
    Task<bool> MarkReadAsync(Guid id, string role, Guid? userId, Guid? branchId);
    Task<int> MarkAllReadAsync(string role, Guid? userId, Guid? branchId);
}
