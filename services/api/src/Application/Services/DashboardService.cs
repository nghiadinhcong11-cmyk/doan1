using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using RestaurantPOS.Infrastructure.Persistence;

namespace RestaurantPOS.Application.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private const string DashboardCacheKey = "DashboardSummary";
        private static readonly object CacheInvalidationLock = new();
        private static CancellationTokenSource DashboardCacheInvalidation = new();

        private static readonly TimeZoneInfo VietnamZone = GetVietnamTimeZone();

        private static TimeZoneInfo GetVietnamTimeZone()
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); }
            catch { return TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh"); }
        }

        private static DateTime ToVietnamTime(DateTime utcTime)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utcTime, DateTimeKind.Utc), VietnamZone);
        }

        public DashboardService(ApplicationDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        /// <summary>Xóa cache dashboard sau khi dữ liệu doanh thu hoặc chi phí thay đổi.</summary>
        public void InvalidateSummaryCache()
        {
            lock (CacheInvalidationLock)
            {
                var previous = DashboardCacheInvalidation;
                DashboardCacheInvalidation = new CancellationTokenSource();
                previous.Cancel();
                previous.Dispose();
            }
        }

        private static CancellationToken GetDashboardCacheInvalidationToken()
        {
            lock (CacheInvalidationLock)
            {
                return DashboardCacheInvalidation.Token;
            }
        }

        public async Task<object> GetSummaryAsync(string? branchId, string period = "today")
        {
            if (!string.IsNullOrWhiteSpace(branchId) && !Guid.TryParse(branchId, out _))
                throw new ArgumentException("branchId không hợp lệ", nameof(branchId));

            period = period.Trim().ToLowerInvariant();
            if (period is not ("today" or "week" or "month"))
                throw new ArgumentException("period không hợp lệ", nameof(period));

            string cacheKey = string.IsNullOrEmpty(branchId)
                ? $"{DashboardCacheKey}_{period}"
                : $"{DashboardCacheKey}_{branchId}_{period}";

            if (_cache.TryGetValue(cacheKey, out object? cachedData) && cachedData != null)
            {
                return cachedData;
            }

            // Keep the invalidation generation used by this calculation. If a
            // mutation commits while the summary is being built, the old
            // snapshot must not be stored under the new generation.
            var calculationInvalidationToken = GetDashboardCacheInvalidationToken();

            var nowUtc = DateTime.UtcNow;
            var nowVn = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, VietnamZone);

            var todayStartVn = new DateTime(nowVn.Year, nowVn.Month, nowVn.Day, 0, 0, 0);
            var periodStartVn = period switch
            {
                "week" => todayStartVn.AddDays(-6),
                "month" => todayStartVn.AddMonths(-11).AddDays(1 - todayStartVn.AddMonths(-11).Day),
                _ => todayStartVn
            };
            var periodEndVn = todayStartVn.AddDays(1);
            var previousPeriodStartVn = period switch
            {
                "week" => periodStartVn.AddDays(-7),
                "month" => periodStartVn.AddMonths(-12),
                _ => periodStartVn.AddDays(-1)
            };
            var previousPeriodEndVn = periodStartVn;

            var startUtc = TimeZoneInfo.ConvertTimeToUtc(periodStartVn, VietnamZone);
            var endUtc = TimeZoneInfo.ConvertTimeToUtc(periodEndVn, VietnamZone);

            var previousPeriodStartUtc = TimeZoneInfo.ConvertTimeToUtc(previousPeriodStartVn, VietnamZone);
            var previousPeriodEndUtc = TimeZoneInfo.ConvertTimeToUtc(previousPeriodEndVn, VietnamZone);

            Guid? scopedBranchId = null;
            IQueryable<RestaurantPOS.Domain.Entities.Order> ordersQuery = _context.Orders.AsNoTracking();
            if (!string.IsNullOrEmpty(branchId) && Guid.TryParse(branchId, out Guid bId))
            {
                scopedBranchId = bId;
                ordersQuery = ordersQuery.Where(o => o.BranchId == bId);
            }

            // 1. Lấy toàn bộ đơn hàng trong ngày hôm nay (Theo giờ VN)
            // Revenue is defined as the total PaidAmount of orders with status "Hoàn thành"
            // where PaidAmount > 0.
            var ordersInPeriod = await ordersQuery
                .Where(o => o.CreatedAt >= startUtc && o.CreatedAt < endUtc && o.Status == "Hoàn thành" && o.PaidAmount > 0)
                .Select(o => new { o.Id, o.CreatedAt, o.PaidAmount, o.BranchId, o.PaymentMethod })
                .ToListAsync();

            var periodRevenue = ordersInPeriod.Sum(o => o.PaidAmount);

            // Chi phí đã ghi nhận theo chi nhánh. Đây là cost recording, không phải recipe/stock consumption.
            var expensesQuery = _context.Expenses.AsNoTracking()
                .Where(e => e.ExpenseDate >= startUtc && e.ExpenseDate < endUtc)
                .Where(e => !scopedBranchId.HasValue || e.BranchId == scopedBranchId.Value);
            Dictionary<Guid, decimal> expensesByBranchId = await expensesQuery
                .GroupBy(e => e.BranchId)
                .Select(g => new { BranchId = g.Key, Amount = g.Sum(e => e.Amount) })
                .ToDictionaryAsync(x => x.BranchId, x => x.Amount);
            var totalExpenses = expensesByBranchId.Values.Sum();

            // Giá vốn được tính từ CostPrice của Product/Topping trong các món đã hoàn thành.
            var completedOrderIds = ordersInPeriod.Select(o => o.Id).ToList();
            var costDetails = await _context.OrderDetails.AsNoTracking()
                .Where(d => completedOrderIds.Contains(EF.Property<Guid>(d, "OrderId")))
                .Select(d => new
                {
                    BranchId = _context.Orders
                        .Where(o => o.Id == EF.Property<Guid>(d, "OrderId"))
                        .Select(o => o.BranchId)
                        .FirstOrDefault(),
                    d.Quantity,
                    d.ProductId,
                    d.ToppingId
                })
                .ToListAsync();
            var productIds = costDetails.Where(d => d.ProductId.HasValue).Select(d => d.ProductId!.Value).Distinct().ToList();
            var toppingIds = costDetails.Where(d => d.ToppingId.HasValue).Select(d => d.ToppingId!.Value).Distinct().ToList();
            var productCosts = await _context.Products.AsNoTracking()
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p.CostPrice);
            var toppingCosts = await _context.Toppings.AsNoTracking()
                .Where(t => toppingIds.Contains(t.Id))
                .ToDictionaryAsync(t => t.Id, t => t.CostPrice);
            var costOfGoodsSold = costDetails.Sum(d =>
                d.Quantity * (d.ProductId.HasValue && productCosts.TryGetValue(d.ProductId.Value, out var productCost) ? productCost : 0m)
                + d.Quantity * (d.ToppingId.HasValue && toppingCosts.TryGetValue(d.ToppingId.Value, out var toppingCost) ? toppingCost : 0m));
            var netProfit = periodRevenue - totalExpenses - costOfGoodsSold;

            var revenueHistory = await ordersQuery
                .Where(o => o.CreatedAt >= previousPeriodStartUtc && o.CreatedAt < endUtc && o.Status == "Hoàn thành" && o.PaidAmount > 0)
                .Select(o => new { o.CreatedAt, o.PaidAmount })
                .ToListAsync();
            var previousPeriodRevenue = revenueHistory
                .Where(o => o.CreatedAt >= previousPeriodStartUtc && o.CreatedAt < previousPeriodEndUtc)
                .Sum(o => o.PaidAmount);

            var revenueTrendState = previousPeriodRevenue > 0
                ? "percentage"
                : periodRevenue > 0 ? "from_zero" : "no_change";
            decimal revenueTrend = previousPeriodRevenue > 0
                ? ((periodRevenue - previousPeriodRevenue) / previousPeriodRevenue) * 100
                : 0m;

            // 2. Biểu đồ giờ
            var hourlyDict = ordersInPeriod
                .Select(o => new { Hour = ToVietnamTime(o.CreatedAt).Hour, o.PaidAmount })
                .GroupBy(o => o.Hour)
                .ToDictionary(g => g.Key, g => g.Sum(o => o.PaidAmount));

            var chartData = Enumerable.Range(0, 24).Select(h => new
            {
                time = h.ToString("D2") + ":00",
                amount = hourlyDict.ContainsKey(h) ? hourlyDict[h] : 0m
            }).ToList();

            // 3. Biểu đồ tuần (7 ngày gần nhất), derived from the single history query above.
            var last7DaysStartVn = nowVn.Date.AddDays(-6);
            var last7DaysStartUtc = TimeZoneInfo.ConvertTimeToUtc(last7DaysStartVn, VietnamZone);

            var weekOrders = revenueHistory.Where(o => o.CreatedAt >= startUtc);

            var weeklyRevenue = Enumerable.Range(0, 7).Select(i => {
                var date = last7DaysStartVn.AddDays(i);
                var dayAmount = weekOrders
                    .Where(o => ToVietnamTime(o.CreatedAt).Date == date.Date)
                    .Sum(o => o.PaidAmount);

                string dayLabel = date.DayOfWeek switch {
                    DayOfWeek.Sunday => "CN",
                    DayOfWeek.Monday => "T2",
                    DayOfWeek.Tuesday => "T3",
                    DayOfWeek.Wednesday => "T4",
                    DayOfWeek.Thursday => "T5",
                    DayOfWeek.Friday => "T6",
                    DayOfWeek.Saturday => "T7",
                    _ => ""
                };

                return new { date = $"{dayLabel} {date:dd/MM}", amount = dayAmount };
            }).ToList();

            // 4. Biểu đồ 12 tháng gần nhất.
            var monthlyRevenue = Enumerable.Range(0, 12).Select(i => {
                var month = new DateTime(nowVn.Year, nowVn.Month, 1).AddMonths(-11 + i);
                var monthAmount = revenueHistory
                    .Where(o => ToVietnamTime(o.CreatedAt).Year == month.Year && ToVietnamTime(o.CreatedAt).Month == month.Month)
                    .Sum(o => o.PaidAmount);
                return new { month = $"{month:MM/yyyy}", amount = monthAmount };
            }).ToList();

            // 5. Thống kê doanh thu theo chi nhánh trong cùng khoảng thời gian.
            var branchesQuery = _context.Branches.AsNoTracking().Where(b => b.IsActive);
            if (scopedBranchId.HasValue)
            {
                branchesQuery = branchesQuery.Where(b => b.Id == scopedBranchId.Value);
            }

            var branchesForDashboard = await branchesQuery
                .Select(b => new { b.Id, b.Name })
                .ToListAsync();
            var revenueByBranchId = ordersInPeriod
                .GroupBy(o => o.BranchId)
                .ToDictionary(g => g.Key, g => new
                {
                    Revenue = g.Sum(o => o.PaidAmount),
                    OrderCount = g.Count()
                });
            var costByBranchId = costDetails
                .GroupBy(d => d.BranchId)
                .ToDictionary(g => g.Key, g => g.Sum(d =>
                    d.Quantity * (d.ProductId.HasValue && productCosts.TryGetValue(d.ProductId.Value, out var productCost) ? productCost : 0m)
                    + d.Quantity * (d.ToppingId.HasValue && toppingCosts.TryGetValue(d.ToppingId.Value, out var toppingCost) ? toppingCost : 0m)));
            var branchRevenue = branchesForDashboard
                .Select(b =>
                {
                    revenueByBranchId.TryGetValue(b.Id, out var orderStats);
                    var revenue = orderStats?.Revenue ?? 0m;
                    var expenses = expensesByBranchId.TryGetValue(b.Id, out var branchExpenses) ? branchExpenses : 0m;
                    var cost = costByBranchId.TryGetValue(b.Id, out var branchCost) ? branchCost : 0m;
                    return new BranchDashboardMetricDto
                    {
                        Name = b.Name,
                        Revenue = revenue,
                        OrderCount = orderStats?.OrderCount ?? 0,
                        Expenses = expenses,
                        CostOfGoodsSold = cost,
                        EstimatedProfit = revenue - expenses - cost
                    };
                })
                .ToList();

            var orderStatuses = await ordersQuery
                .Where(o => o.CreatedAt >= startUtc && o.CreatedAt < endUtc)
                .GroupBy(o => o.Status)
                .Select(g => new { name = g.Key, value = g.Count() })
                .OrderByDescending(x => x.value)
                .ToListAsync();

            var paymentMethods = ordersInPeriod
                .Where(o => !string.IsNullOrWhiteSpace(o.PaymentMethod))
                .GroupBy(o => o.PaymentMethod!)
                .Select(g => new { name = g.Key, value = g.Sum(o => o.PaidAmount) })
                .OrderByDescending(x => x.value)
                .ToList();

            // 6. Top mặt hàng & Hoạt động. Keep monetary aggregation in decimal
            // and materialize the filtered rows before grouping for chart data.
            var topProductRows = await _context.OrderDetails.AsNoTracking()
                .Where(d => _context.Orders.Any(o => o.Id == EF.Property<Guid>(d, "OrderId") &&
                    (!scopedBranchId.HasValue || o.BranchId == scopedBranchId.Value) &&
                    o.CreatedAt >= startUtc && o.CreatedAt < endUtc && o.Status == "Hoàn thành" && o.PaidAmount > 0))
                .Select(d => new { d.ProductName, d.Quantity, d.UnitPrice })
                .ToListAsync();
            var topProducts = topProductRows
                .GroupBy(d => d.ProductName)
                .Select(g => new { name = g.Key, quantity = g.Sum(x => x.Quantity), revenue = g.Sum(x => x.Quantity * x.UnitPrice) })
                .OrderByDescending(x => x.quantity).Take(5).ToList();

            var recentOrders = await ordersQuery
                .Where(o => o.CreatedAt >= startUtc && o.CreatedAt < endUtc)
                .OrderByDescending(o => o.CreatedAt).Take(5)
                .Select(o => new
                {
                    invoiceCode = o.InvoiceCode,
                    customerName = o.CustomerName,
                    totalAmount = o.TotalAmount,
                    createdAt = o.CreatedAt,
                    tableName = o.TableName,
                    status = o.Status,
                    branchName = _context.Branches
                        .Where(b => b.Id == o.BranchId)
                        .Select(b => b.Name)
                        .FirstOrDefault()
                })
                .ToListAsync();

            // 7. Stats & Employee
            var employeeScope = _context.Employees.AsNoTracking()
                .Where(e => !scopedBranchId.HasValue || e.BranchId == scopedBranchId.Value);
            var employeeCounts = await employeeScope
                .GroupBy(_ => 1)
                .Select(g => new { total = g.Count(), active = g.Count(e => e.IsActive) })
                .FirstOrDefaultAsync();
            var onDutyQuery = _context.Attendances.AsNoTracking()
                .Where(a => a.CheckInTime >= startUtc && a.CheckInTime < endUtc && a.CheckOutTime == null)
                .Where(a => !scopedBranchId.HasValue || _context.Employees.Any(e => e.Id == a.EmployeeId && e.BranchId == scopedBranchId.Value));
            var onDuty = await onDutyQuery.Select(a => a.EmployeeId).Distinct().CountAsync();

            var result = new
            {
                period = period,
                periodStart = startUtc,
                periodEnd = endUtc,
                todayRevenue = periodRevenue,
                averageOrderValue = ordersInPeriod.Count > 0 ? Math.Round(periodRevenue / ordersInPeriod.Count, 0) : 0m,
                profitMargin = periodRevenue > 0 ? Math.Round(netProfit / periodRevenue * 100, 2) : 0m,
                revenueTrend = Math.Round(revenueTrend, 1),
                revenueTrendState = revenueTrendState,
                generatedAt = nowUtc,
                totalOrders = ordersInPeriod.Count,
                customerCount = await ordersQuery.Where(o => o.CreatedAt >= startUtc && o.CreatedAt < endUtc && o.CustomerName != "Khách lẻ").Select(o => o.CustomerName).Distinct().CountAsync(),
                topProducts = topProducts,
                chartData = chartData,
                weeklyRevenue = weeklyRevenue,
                monthlyRevenue = monthlyRevenue,
                branchRevenue = branchRevenue,
                orderStatuses = orderStatuses,
                paymentMethods = paymentMethods,
                recentOrders = recentOrders,
                totalExpenses = totalExpenses,
                costOfGoodsSold = costOfGoodsSold,
                netProfit = netProfit,
                // Giữ field cũ để frontend hiện tại không bị breaking change.
                estimatedProfit = netProfit,
                employeeStats = new {
                    total = employeeCounts?.total ?? 0,
                    active = employeeCounts?.active ?? 0,
                    onDuty
                }
            };

            lock (CacheInvalidationLock)
            {
                if (calculationInvalidationToken != DashboardCacheInvalidation.Token)
                    return result;

                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
                };
                cacheOptions.AddExpirationToken(new CancellationChangeToken(calculationInvalidationToken));
                _cache.Set(cacheKey, result, cacheOptions);
            }

            return result;
        }

        public async Task<decimal> GetRevenueByPeriodAsync(string period, Guid? branchId = null)
        {
            var nowVn = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, VietnamZone);
            DateTime start, end;

            if (period == "month")
            {
                start = new DateTime(nowVn.Year, nowVn.Month, 1);
                end = start.AddMonths(1);
            }
            else if (period == "year")
            {
                start = new DateTime(nowVn.Year, 1, 1);
                end = start.AddYears(1);
            }
            else // default today
            {
                start = new DateTime(nowVn.Year, nowVn.Month, nowVn.Day);
                end = start.AddDays(1);
            }

            var startUtc = TimeZoneInfo.ConvertTimeToUtc(start, VietnamZone);
            var endUtc = TimeZoneInfo.ConvertTimeToUtc(end, VietnamZone);

            var report = await GetRevenueReportAsync(startUtc, endUtc, branchId);
            return report.Revenue;
        }

        public async Task<List<TopProductDto>> GetBestSellersAsync(DateTime startUtc, DateTime endUtc, Guid? branchId, int limit)
        {
            var query = _context.OrderDetails.AsNoTracking()
                .Where(d => _context.Orders.Any(o => o.Id == EF.Property<Guid>(d, "OrderId") &&
                    (!branchId.HasValue || o.BranchId == branchId.Value) &&
                    o.CreatedAt >= startUtc && o.CreatedAt < endUtc && o.Status == "Hoàn thành"));

            var topProducts = await query
                .GroupBy(d => d.ProductName)
                .Select(g => new TopProductDto
                {
                    ProductName = g.Key,
                    TotalQuantity = g.Sum(x => x.Quantity),
                    TotalRevenue = g.Sum(x => x.Quantity * x.UnitPrice)
                })
                .OrderByDescending(x => x.TotalQuantity)
                .Take(limit)
                .ToListAsync();

            return topProducts;
        }

        public async Task<(decimal Revenue, int OrderCount)> GetRevenueReportAsync(DateTime startUtc, DateTime endUtc, Guid? branchId)
        {
            var query = _context.Orders.AsNoTracking()
                .Where(o => o.CreatedAt >= startUtc && o.CreatedAt < endUtc && o.Status == "Hoàn thành" && o.PaidAmount > 0);

            if (branchId.HasValue)
            {
                query = query.Where(o => o.BranchId == branchId.Value);
            }

            var data = await query
                .Select(o => o.PaidAmount)
                .ToListAsync();

            return (data.Sum(), data.Count);
        }

        public async Task<BusinessSummaryDto> GetBusinessSummaryAsync(DateTime startUtc, DateTime endUtc, Guid? branchId)
        {
            var ordersQuery = _context.Orders.AsNoTracking()
                .Where(o => o.CreatedAt >= startUtc && o.CreatedAt < endUtc && o.Status == "Hoàn thành" && o.PaidAmount > 0);

            if (branchId.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.BranchId == branchId.Value);
            }

            var orders = await ordersQuery
                .Select(o => new { o.Id, o.PaidAmount })
                .ToListAsync();

            decimal revenue = orders.Sum(o => o.PaidAmount);
            int count = orders.Count;
            decimal aov = count > 0 ? revenue / count : 0;

            var expensesQuery = _context.Expenses.AsNoTracking()
                .Where(e => e.ExpenseDate >= startUtc && e.ExpenseDate < endUtc);
            if (branchId.HasValue)
            {
                expensesQuery = expensesQuery.Where(e => e.BranchId == branchId.Value);
            }

            decimal totalExpenses = await expensesQuery.Select(e => (decimal?)e.Amount).SumAsync() ?? 0m;

            var expenseItems = await expensesQuery.Select(e => new { e.Category, e.Amount }).ToListAsync();
            var expenseBreakdown = expenseItems
                .GroupBy(e => e.Category)
                .Select(g => new ExpenseCategoryDto
                {
                    Category = g.Key,
                    Amount = g.Sum(x => x.Amount),
                    Percentage = totalExpenses > 0 ? Math.Round(g.Sum(x => x.Amount) / totalExpenses * 100, 2) : 0
                })
                .OrderByDescending(x => x.Amount)
                .ToList();

            // COGS Calculation (Estimated)
            var orderIds = orders.Select(o => o.Id).ToList();
            var costDetails = await _context.OrderDetails.AsNoTracking()
                .Where(d => orderIds.Contains(EF.Property<Guid>(d, "OrderId")))
                .Select(d => new { d.Quantity, d.ProductId, d.ToppingId })
                .ToListAsync();

            var productIds = costDetails.Where(d => d.ProductId.HasValue).Select(d => d.ProductId!.Value).Distinct().ToList();
            var toppingIds = costDetails.Where(d => d.ToppingId.HasValue).Select(d => d.ToppingId!.Value).Distinct().ToList();

            var productCosts = await _context.Products.AsNoTracking()
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p.CostPrice);
            var toppingCosts = await _context.Toppings.AsNoTracking()
                .Where(t => toppingIds.Contains(t.Id))
                .ToDictionaryAsync(t => t.Id, t => t.CostPrice);

            decimal cogs = costDetails.Sum(d =>
                d.Quantity * (d.ProductId.HasValue && productCosts.TryGetValue(d.ProductId.Value, out var pc) ? pc : 0m) +
                d.Quantity * (d.ToppingId.HasValue && toppingCosts.TryGetValue(d.ToppingId.Value, out var tc) ? tc : 0m));

            var topProducts = await GetBestSellersAsync(startUtc, endUtc, branchId, 5);

            return new BusinessSummaryDto
            {
                Revenue = revenue,
                OrderCount = count,
                AverageOrderValue = Math.Round(aov, 0),
                TotalExpenses = totalExpenses,
                CostOfGoodsSold = cogs,
                NetProfit = revenue - totalExpenses - cogs,
                ProfitMargin = revenue > 0 ? Math.Round((revenue - totalExpenses - cogs) / revenue * 100, 2) : 0,
                ExpenseBreakdown = expenseBreakdown,
                TopProducts = topProducts
            };
        }
    }
}
