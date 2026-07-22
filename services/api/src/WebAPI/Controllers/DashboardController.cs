using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantPOS.Infrastructure.Persistence;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace RestaurantPOS.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private const string DashboardCacheKey = "DashboardSummary";

        public DashboardController(ApplicationDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            // Kiểm tra cache trước để trả về ngay lập tức
            if (_cache.TryGetValue(DashboardCacheKey, out object? cachedData))
            {
                return Ok(cachedData);
            }

            try
            {
                var now = DateTime.UtcNow;
                var todayStart = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
                var todayEnd = todayStart.AddDays(1);

                // EF Core DbContext KHÔNG hỗ trợ chạy song song trên cùng 1 instance.
                // Chúng ta phải await tuần tự. Việc tối ưu tốc độ sẽ dựa vào IMemoryCache và AsNoTracking.

                // 1. Doanh thu hôm nay
                var todayRevenue = await _context.Orders.AsNoTracking()
                    .Where(o => o.CreatedAt >= todayStart && o.CreatedAt < todayEnd && o.Status == "Hoàn thành")
                    .SumAsync(o => (decimal?)o.PaidAmount) ?? 0m;

                // 2. Số lượng đơn hôm nay
                var totalOrders = await _context.Orders.AsNoTracking()
                    .Where(o => o.CreatedAt >= todayStart && o.CreatedAt < todayEnd)
                    .CountAsync();

                // 3. Lượng khách hàng
                var customerCount = await _context.Orders.AsNoTracking()
                    .Where(o => o.CreatedAt >= todayStart && o.CreatedAt < todayEnd && o.CustomerName != "Khách lẻ")
                    .Select(o => o.CustomerName)
                    .Distinct()
                    .CountAsync();

                // 4. Hoạt động gần đây
                var recentOrders = await _context.Orders.AsNoTracking()
                    .OrderByDescending(o => o.CreatedAt)
                    .Take(5)
                    .Select(o => new {
                        o.InvoiceCode,
                        o.CustomerName,
                        o.TotalAmount,
                        o.CreatedAt,
                        o.TableName
                    })
                    .ToListAsync();

                // 5. Top mặt hàng bán chạy
                var topProducts = await _context.OrderDetails.AsNoTracking()
                    .Where(d => _context.Orders.Any(o => o.Id == EF.Property<Guid>(d, "OrderId")
                                                        && o.CreatedAt >= todayStart
                                                        && o.CreatedAt < todayEnd
                                                        && o.Status == "Hoàn thành"))
                    .GroupBy(d => d.ProductName)
                    .Select(g => new
                    {
                        Name = g.Key,
                        Quantity = g.Sum(x => x.Quantity),
                        Revenue = g.Sum(x => x.Quantity * x.UnitPrice)
                    })
                    .OrderByDescending(x => x.Quantity)
                    .Take(5)
                    .ToListAsync();

                // 6. Dữ liệu biểu đồ
                var ordersToday = await _context.Orders.AsNoTracking()
                    .Where(o => o.CreatedAt >= todayStart && o.CreatedAt < todayEnd && o.Status == "Hoàn thành")
                    .Select(o => new { o.CreatedAt, o.PaidAmount })
                    .ToListAsync();

                var chartData = ordersToday
                    .GroupBy(o => o.CreatedAt.Hour)
                    .Select(g => new
                    {
                        Time = g.Key + ":00",
                        Amount = g.Sum(o => o.PaidAmount)
                    })
                    .OrderBy(g => g.Time)
                    .ToList();

                var result = new
                {
                    TodayRevenue = todayRevenue,
                    TotalOrders = totalOrders,
                    CustomerCount = customerCount,
                    TopProducts = topProducts,
                    ChartData = chartData,
                    RecentOrders = recentOrders,
                    EstimatedProfit = todayRevenue * 0.4m
                };

                // Lưu vào cache trong 2 phút để các lần load sau cực nhanh
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(2));
                _cache.Set(DashboardCacheKey, result, cacheOptions);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi Dashboard", detail = ex.Message });
            }
        }
    }
}
