using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RestaurantPOS.Application.Services;

namespace RestaurantPOS.Infrastructure.Services;

public class InsightBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<InsightBackgroundService> _logger;
    private static readonly TimeZoneInfo VietnamZone = GetVietnamTimeZone();

    public InsightBackgroundService(IServiceProvider serviceProvider, ILogger<InsightBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    private static TimeZoneInfo GetVietnamTimeZone()
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); }
        catch { return TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh"); }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Insight Background Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var nowVn = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, VietnamZone);

                // Target time: 00:05 AM next day (or today if it hasn't passed yet)
                var nextRunVn = nowVn.Date.AddDays(1).AddMinutes(5);
                if (nowVn.Hour == 0 && nowVn.Minute < 5)
                {
                    nextRunVn = nowVn.Date.AddMinutes(5);
                }

                var delay = nextRunVn - nowVn;
                _logger.LogInformation("Next proactive insight run at {NextRun} (In {Delay})", nextRunVn, delay);

                await Task.Delay(delay, stoppingToken);

                using (var scope = _serviceProvider.CreateScope())
                {
                    var insightService = scope.ServiceProvider.GetRequiredService<IProactiveInsightService>();
                    await insightService.ProcessProactiveInsightsAsync();
                }
            }
            catch (OperationCanceledException)
            {
                // Graceful shutdown
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in Insight Background Service.");
                // Wait a bit before retrying to avoid tight loop on persistent errors
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        _logger.LogInformation("Insight Background Service is stopping.");
    }
}
