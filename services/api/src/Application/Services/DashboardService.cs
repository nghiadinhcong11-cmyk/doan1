using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using RestaurantPOS.Infrastructure.Persistence;

namespace RestaurantPOS.Application.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private const string DashboardCacheKey = "DashboardSummary";

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
            _cache.Remove(DashboardCacheKey);
            // Branch-specific keys are short-lived; compacting also removes stale branch summaries.
            if (_cache is MemoryCache memoryCache)
            {
                memoryCache.Compact(1.0);
            }
        }

        public async Task<object> GetSummaryAsync(string? branchId)
        {
            string cacheKey = string.IsNullOrEmpty(branchId) ? DashboardCacheKey : $"{DashboardCacheKey}_{branchId}";

            if (_cache.TryGetValue(cacheKey, out object? cachedData) && cachedData != null)
            {
                return cachedData;
            }

            var nowUtc = DateTime.UtcNow;
            var nowVn = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, VietnamZone);

            var todayStartVn = new DateTime(nowVn.Year, nowVn.Month, nowVn.Day, 0, 0, 0);
            var todayEndVn = todayStartVn.AddDays(1);

            var startUtc = TimeZoneInfo.ConvertTimeToUtc(todayStartVn, VietnamZone);
            var endUtc = TimeZoneInfo.ConvertTimeToUtc(todayEndVn, VietnamZone);

            var yesterdayStartUtc = startUtc.AddDays(-1);
            var yesterdayEndUtc = startUtc;

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
            var ordersToday = await ordersQuery
                .Where(o => o.CreatedAt >= startUtc && o.CreatedAt < endUtc && o.Status == "Hoàn thành" && o.PaidAmount > 0)
                .Select(o => new { o.Id, o.CreatedAt, o.PaidAmount, o.BranchId })
                .ToListAsync();

            var todayRevenue = ordersToday.Sum(o => (double)o.PaidAmount);

            // Chi phí đã ghi nhận theo chi nhánh. Đây là cost recording, không phải recipe/stock consumption.
            var expensesQuery = _context.Expenses.AsNoTracking()
                .Where(e => e.ExpenseDate >= startUtc && e.ExpenseDate < endUtc)
                .Where(e => !scopedBranchId.HasValue || e.BranchId == scopedBranchId.Value);
            // SQLite (used by the unit-test provider) cannot translate SUM(decimal).
            // Keep the production path database-side for PostgreSQL while retaining test-provider compatibility.
            var totalExpenses = _context.Database.ProviderName?.Contains("Sqlite", StringComparison.OrdinalIgnoreCase) == true
                ? (await expensesQuery.Select(e => e.Amount).ToListAsync()).Sum()
                : await expensesQuery.Select(e => (decimal?)e.Amount).SumAsync() ?? 0m;

            // Giá vốn được tính từ CostPrice của Product/Topping trong các món đã hoàn thành.
            var completedOrderIds = ordersToday.Select(o => o.Id).ToList();
            var costDetails = await _context.OrderDetails.AsNoTracking()
                .Where(d => completedOrderIds.Contains(EF.Property<Guid>(d, "OrderId")))
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
            var costOfGoodsSold = costDetails.Sum(d =>
                d.Quantity * (d.ProductId.HasValue && productCosts.TryGetValue(d.ProductId.Value, out var productCost) ? productCost : 0m)
                + d.Quantity * (d.ToppingId.HasValue && toppingCosts.TryGetValue(d.ToppingId.Value, out var toppingCost) ? toppingCost : 0m));
            var netProfit = (decimal)todayRevenue - totalExpenses - costOfGoodsSold;

            var yearStartVn = new DateTime(nowVn.Year, 1, 1);
            var yearStartUtc = TimeZoneInfo.ConvertTimeToUtc(yearStartVn, VietnamZone);
            var revenueHistoryStartUtc = yesterdayStartUtc < yearStartUtc ? yesterdayStartUtc : yearStartUtc;
            var revenueHistory = await ordersQuery
                .Where(o => o.CreatedAt >= revenueHistoryStartUtc && o.CreatedAt < endUtc && o.Status == "Hoàn thành")
                .Select(o => new { o.CreatedAt, o.PaidAmount })
                .ToListAsync();
            var yesterdayRevenue = revenueHistory
                .Where(o => o.CreatedAt >= yesterdayStartUtc && o.CreatedAt < yesterdayEndUtc)
                .Sum(o => (double)o.PaidAmount);

            double revenueTrend = yesterdayRevenue > 0 ? ((todayRevenue - yesterdayRevenue) / yesterdayRevenue) * 100 : 100;

            // 2. Biểu đồ giờ
            var hourlyDict = ordersToday
                .Select(o => new { Hour = ToVietnamTime(o.CreatedAt).Hour, o.PaidAmount })
                .GroupBy(o => o.Hour)
                .ToDictionary(g => g.Key, g => g.Sum(o => (double)o.PaidAmount));

            var chartData = Enumerable.Range(0, 24).Select(h => new
            {
                time = h.ToString("D2") + ":00",
                amount = hourlyDict.ContainsKey(h) ? hourlyDict[h] : 0.0
            }).ToList();

            // 3. Biểu đồ tuần (7 ngày gần nhất), derived from the single history query above.
            var last7DaysStartVn = nowVn.Date.AddDays(-6);
            var last7DaysStartUtc = TimeZoneInfo.ConvertTimeToUtc(last7DaysStartVn, VietnamZone);

            var weekOrders = revenueHistory.Where(o => o.CreatedAt >= last7DaysStartUtc);

            var weeklyRevenue = Enumerable.Range(0, 7).Select(i => {
                var date = last7DaysStartVn.AddDays(i);
                var dayAmount = weekOrders
                    .Where(o => ToVietnamTime(o.CreatedAt).Date == date.Date)
                    .Sum(o => (double)o.PaidAmount);

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

            // 4. Biểu đồ tháng (Trong năm), derived from the same history query.
            var yearOrders = revenueHistory.Where(o => o.CreatedAt >= yearStartUtc).ToList();

            var monthlyRevenue = Enumerable.Range(1, 12).Select(m => {
                var monthAmount = yearOrders
                    .Where(o => ToVietnamTime(o.CreatedAt).Month == m)
                    .Sum(o => (double)o.PaidAmount);
                return new { month = $"Tháng {m:D2}", amount = monthAmount };
            }).ToList();

            int firstMonthWithData = yearOrders.Any() ? yearOrders.Min(o => ToVietnamTime(o.CreatedAt).Month) : nowVn.Month;
            monthlyRevenue = monthlyRevenue
                .Where(m => int.Parse(m.month.Replace("Tháng ", "")) >= firstMonthWithData && int.Parse(m.month.Replace("Tháng ", "")) <= nowVn.Month)
                .ToList();

            // 5. Thống kê doanh thu theo chi nhánh (Hôm nay), derived from ordersToday.
            var branchesQuery = _context.Branches.AsNoTracking().Where(b => b.IsActive);
            if (scopedBranchId.HasValue)
            {
                branchesQuery = branchesQuery.Where(b => b.Id == scopedBranchId.Value);
            }

            var branchesForDashboard = await branchesQuery
                .Select(b => new { b.Id, b.Name })
                .ToListAsync();
            var revenueByBranchId = ordersToday
                .Where(o => o.BranchId.HasValue)
                .GroupBy(o => o.BranchId!.Value)
                .ToDictionary(g => g.Key, g => g.Sum(o => (double)o.PaidAmount));
            var branchRevenue = branchesForDashboard
                .Select(b => new
                {
                    name = b.Name,
                    value = revenueByBranchId.TryGetValue(b.Id, out var revenue) ? revenue : 0d
                })
                .ToList();

            var orderStatuses = await ordersQuery
                .Where(o => o.CreatedAt >= startUtc && o.CreatedAt < endUtc)
                .GroupBy(o => o.Status)
                .Select(g => new { name = g.Key, value = g.Count() })
                .OrderByDescending(x => x.value)
                .ToListAsync();

            var paymentMethods = await ordersQuery
                .Where(o => o.CreatedAt >= startUtc && o.CreatedAt < endUtc
                    && o.Status == "Hoàn thành"
                    && o.PaidAmount > 0
                    && o.PaymentMethod != null
                    && o.PaymentMethod != "")
                .GroupBy(o => o.PaymentMethod!)
                .Select(g => new { name = g.Key, value = g.Sum(o => (double)o.PaidAmount) })
                .OrderByDescending(x => x.value)
                .ToListAsync();

            // 6. Top mặt hàng & Hoạt động. Aggregate top products in PostgreSQL.
            var topProducts = await _context.OrderDetails.AsNoTracking()
                .Where(d => _context.Orders.Any(o => o.Id == EF.Property<Guid>(d, "OrderId") &&
                    (!scopedBranchId.HasValue || o.BranchId == scopedBranchId.Value) &&
                    o.CreatedAt >= startUtc && o.CreatedAt < endUtc && o.Status == "Hoàn thành"))
                .GroupBy(d => d.ProductName)
                .Select(g => new { name = g.Key, quantity = g.Sum(x => x.Quantity), revenue = g.Sum(x => (double)x.Quantity * (double)x.UnitPrice) })
                .OrderByDescending(x => x.quantity).Take(5).ToListAsync();

            var recentOrders = await ordersQuery
                .OrderByDescending(o => o.CreatedAt).Take(5)
                .Select(o => new
                {
                    invoiceCode = o.InvoiceCode,
                    customerName = o.CustomerName,
                    totalAmount = (double)o.TotalAmount,
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
            var result = new
            {
                todayRevenue = todayRevenue,
                revenueTrend = Math.Round(revenueTrend, 1),
                totalOrders = ordersToday.Count,
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
                estimatedProfit = (double)netProfit,
                employeeStats = new {
                    total = await _context.Employees
                        .Where(e => string.IsNullOrEmpty(branchId) || e.BranchId == Guid.Parse(branchId))
                        .CountAsync(),
                    active = await _context.Employees
                        .Where(e => string.IsNullOrEmpty(branchId) || e.BranchId == Guid.Parse(branchId))
                        .CountAsync(e => e.IsActive),
                    onDuty = await _context.Attendances.AsNoTracking()
                        .Where(a => a.CheckInTime >= startUtc && a.CheckInTime < endUtc && a.CheckOutTime == null)
                        .Where(a => string.IsNullOrEmpty(branchId) || _context.Employees.Any(e => e.Id == a.EmployeeId && e.BranchId == Guid.Parse(branchId)))
                        .Select(a => a.EmployeeId).Distinct().CountAsync()
                }
            };

            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(2));
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

            decimal totalExpenses = _context.Database.ProviderName?.Contains("Sqlite", StringComparison.OrdinalIgnoreCase) == true
                ? (await expensesQuery.Select(e => e.Amount).ToListAsync()).Sum()
                : await expensesQuery.Select(e => (decimal?)e.Amount).SumAsync() ?? 0m;

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
