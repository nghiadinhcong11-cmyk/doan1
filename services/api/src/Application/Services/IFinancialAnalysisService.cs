using System;
using System.Threading.Tasks;
using RestaurantPOS.Application.DTOs.Financial;

namespace RestaurantPOS.Application.Services
{
    public interface IFinancialAnalysisService
    {
        Task<FinancialAnalysisDto> GetFinancialAnalysisAsync(DateTime startUtc, DateTime endUtc, DateTime prevStartUtc, DateTime prevEndUtc, Guid? branchId, string periodName);
    }
}
