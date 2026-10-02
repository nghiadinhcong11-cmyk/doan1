using System.Threading.Tasks;
using RestaurantPOS.Domain.Entities;

namespace RestaurantPOS.Application.Services;

public interface IInsightExplanationService
{
    Task ExplainInsightAsync(BusinessInsight insight);
}
