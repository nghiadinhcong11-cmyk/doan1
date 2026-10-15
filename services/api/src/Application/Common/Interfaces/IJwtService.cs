using System;
using System.Security.Claims;

namespace RestaurantPOS.Application.Common.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(Guid userId, string username, string fullName, string role, Guid? branchId = null, string? branchName = null, string? position = null, string? customerSessionType = null, Guid? tableId = null, string? customerAuthMethod = null);
        string GenerateDirectGuestToken(Guid branchId, Guid? tableId, TimeSpan lifetime);
        ClaimsPrincipal? ValidateToken(string token);
    }
}
