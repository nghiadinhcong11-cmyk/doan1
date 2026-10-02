using System;
using System.Threading.Tasks;

namespace RestaurantPOS.Application.Services;

public interface IProactiveInsightService
{
    Task ProcessProactiveInsightsAsync();
}
