using System;
using System.Threading.Tasks;

namespace RestaurantPOS.Application.Common.Interfaces
{
    public interface IKitchenNotifier
    {
        Task NotifyNewOrderRequestAsync(object payload);
        Task NotifyRequestStatusUpdatedAsync(Guid requestId, string status);
        Task NotifyRequestStatusUpdatedAsync(Guid requestId, string status, Guid? branchId);
    }
}
