using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Infrastructure.Persistence;

namespace RestaurantPOS.Application.Services;

public class ProactiveInsightService : IProactiveInsightService
{
    private readonly ApplicationDbContext _context;
    private readonly IFinancialAnalysisService _financialService;
    private readonly IInsightExplanationService _explanationService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<ProactiveInsightService> _logger;

    private static readonly TimeZoneInfo VietnamZone = GetVietnamTimeZone();

    public ProactiveInsightService(
        ApplicationDbContext context,
        IFinancialAnalysisService financialService,
        IInsightExplanationService explanationService,
        INotificationService notificationService,
        ILogger<ProactiveInsightService> logger)
    {
        _context = context;
        _financialService = financialService;
        _explanationService = explanationService;
        _notificationService = notificationService;
        _logger = logger;
    }

    private static TimeZoneInfo GetVietnamTimeZone()
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); }
        catch { return TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh"); }
    }

    public async Task ProcessProactiveInsightsAsync()
    {
        _logger.LogInformation("Starting proactive insight detection job.");

        bool isPostgres = _context.Database.IsNpgsql();

        if (isPostgres)
        {
            // Explicitly open connection to maintain session for advisory lock
            await _context.Database.OpenConnectionAsync();

            // PostgreSQL Advisory Lock to ensure only one instance runs this at a time.
            // 2301 as a stable key for Task 23
            var isLocked = await _context.Database
                .SqlQueryRaw<bool>("SELECT pg_try_advisory_lock(2301)")
                .ToListAsync();

            if (!isLocked.FirstOrDefault())
            {
                _logger.LogWarning("Could not acquire advisory lock for proactive insights. Another instance might be running.");
                await _context.Database.CloseConnectionAsync();
                return;
            }
        }

        try
        {
            var branches = await _context.Branches.Where(b => b.IsActive).ToListAsync();

            // Analyze "Yesterday" vs "Day Before Yesterday"
            var nowVn = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, VietnamZone);
            var yesterdayVn = nowVn.Date.AddDays(-1);
            var dayBeforeVn = yesterdayVn.AddDays(-1);

            var startUtc = TimeZoneInfo.ConvertTimeToUtc(yesterdayVn, VietnamZone);
            var endUtc = TimeZoneInfo.ConvertTimeToUtc(yesterdayVn.AddDays(1), VietnamZone);
            var prevStartUtc = TimeZoneInfo.ConvertTimeToUtc(dayBeforeVn, VietnamZone);
            var prevEndUtc = TimeZoneInfo.ConvertTimeToUtc(dayBeforeVn.AddDays(1), VietnamZone);

            int createdCount = 0;
            int duplicateCount = 0;
            var newInsights = new List<BusinessInsight>();

            foreach (var branch in branches)
            {
                try
                {
                    var results = await DetectInsightsForBranchAsync(branch, startUtc, endUtc, prevStartUtc, prevEndUtc);
                    foreach (var insight in results)
                    {
                        var exists = await _context.BusinessInsights
                            .AnyAsync(i => i.DeduplicationKey == insight.DeduplicationKey);

                        if (!exists)
                        {
                            _context.BusinessInsights.Add(insight);
                            newInsights.Add(insight);
                            createdCount++;
                        }
                        else
                        {
                            duplicateCount++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error detecting insights for branch {BranchId} ({BranchName})", branch.Id, branch.Name);
                }
            }

            if (createdCount > 0)
            {
                await _context.SaveChangesAsync();

                // Generate AI explanations for new insights and create notifications
                foreach (var insight in newInsights)
                {
                    await _explanationService.ExplainInsightAsync(insight);

                    // Create notification for the insight
                    try
                    {
                        await _notificationService.CreateAsync(new Notification
                        {
                            Id = Guid.NewGuid(),
                            Type = "BusinessInsight",
                            Title = insight.Title,
                            Message = insight.Summary,
                            EntityType = "BusinessInsight",
                            EntityId = insight.Id,
                            Route = "/business-insights",
                            BranchId = insight.BranchId,
                            TargetRole = insight.BranchId.HasValue ? "manager" : "admin",
                            IsRead = false,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error creating notification for insight {InsightId}", insight.Id);
                    }
                }
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation("Proactive insight job completed. Created={Created}, Duplicates={Duplicates}", createdCount, duplicateCount);
        }
        finally
        {
            if (isPostgres)
            {
                await _context.Database.ExecuteSqlRawAsync("SELECT pg_advisory_unlock(2301)");
                await _context.Database.CloseConnectionAsync();
            }
        }
    }

    private async Task<List<BusinessInsight>> DetectInsightsForBranchAsync(Branch branch, DateTime startUtc, DateTime endUtc, DateTime prevStartUtc, DateTime prevEndUtc)
    {
        var insights = new List<BusinessInsight>();

        var analysis = await _financialService.GetFinancialAnalysisAsync(
            startUtc, endUtc, prevStartUtc, prevEndUtc, branch.Id, "custom_daily");

        foreach (var anomaly in analysis.Anomalies)
        {
            insights.Add(CreateInsight(branch.Id, anomaly.Type, anomaly.Severity,
                GetAnomalyTitle(anomaly.Type),
                anomaly.Description,
                analysis, // Store the whole analysis as evidence
                startUtc, endUtc, prevStartUtc, prevEndUtc));
        }

        return insights;
    }

    private string GetAnomalyTitle(string type) => type switch
    {
        "RevenueSignificantDrop" => "Doanh thu sụt giảm đáng kể",
        "RevenueSignificantIncrease" => "Doanh thu tăng trưởng mạnh",
        "ProfitMarginPressure" => "Áp lực lên lợi nhuận ước tính",
        "AovSignificantChange" => "Giá trị đơn hàng trung bình thay đổi bất thường",
        "ExpenseCategorySpike" => "Chi phí tăng đột biến",
        _ => "Cảnh báo kinh doanh"
    };

    private BusinessInsight CreateInsight(Guid? branchId, string type, string severity, string title, string summary, object evidence, DateTime start, DateTime end, DateTime? prevStart, DateTime? prevEnd)
    {
        string periodKey = start.ToString("yyyyMMdd");
        return new BusinessInsight
        {
            Id = Guid.NewGuid(),
            BranchId = branchId,
            Type = type,
            Severity = severity,
            Title = title,
            Summary = summary,
            EvidenceJson = JsonSerializer.Serialize(evidence),
            PeriodStart = start,
            PeriodEnd = end,
            ComparisonPeriodStart = prevStart,
            ComparisonPeriodEnd = prevEnd,
            Status = "Unread",
            DeduplicationKey = $"{branchId}:{type}:{periodKey}",
            CreatedAt = DateTime.UtcNow,
            DetectedAt = DateTime.UtcNow
        };
    }
}
