using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RestaurantPOS.Infrastructure.Services;

namespace RestaurantPOS.Infrastructure.Configuration;

public static class BackgroundJobServiceCollectionExtensions
{
    public static IServiceCollection AddConfiguredBackgroundJobs(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Preserve the current behavior when the optional flag is absent.
        if (configuration.GetValue<bool?>("BackgroundJobs:EnableInsights") != false)
        {
            services.AddHostedService<InsightBackgroundService>();
        }

        return services;
    }
}
