using System.Threading.Tasks;

namespace RestaurantPOS.Application.Services
{
    public interface IDashboardService
    {
        Task<object> GetSummaryAsync(string? branchId);
        Task<decimal> GetRevenueByPeriodAsync(string period, Guid? branchId = null);
        void InvalidateSummaryCache();
    }
}
