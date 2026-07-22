using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantPOS.Infrastructure.Persistence;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantPOS.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            try
            {
                var now = DateTime.UtcNow;
                var todayStart = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
                var todayEnd = todayStart.AddDays(1);

                // 1. Doanh thu hôm nay
                var todayRevenue = await _context.Orders
                    .Where(o => o.CreatedAt >= todayStart && o.CreatedAt < todayEnd && o.Status == "Hoàn thành")
                    .SumAsync(o => o.PaidAmount);

                // 2. Số lượng đơn hôm nay
                var totalOrders = await _context.Orders
                    .Where(o => o.CreatedAt >= todayStart && o.CreatedAt < todayEnd)
                    .CountAsync();

                // 3. Top mặt hàng bán chạy hôm nay
                // Sử dụng cách truy vấn đơn giản hơn, không dùng Include trên ProductId (vì nó là Guid, không phải navigation property)
                var topProducts = await _context.OrderDetails
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

                // 4. Lượng khách hàng (Số hóa đơn có tên khách khác "Khách lẻ")
                var customerCount = await _context.Orders
                    .Where(o => o.CreatedAt >= todayStart && o.CreatedAt < todayEnd && o.CustomerName != "Khách lẻ")
                    .Select(o => o.CustomerName)
                    .Distinct()
                    .CountAsync();

                // 5. Doanh thu theo giờ (Cho biểu đồ)
                var ordersToday = await _context.Orders
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

                // 6. Hoạt động gần đây (5 hóa đơn mới nhất)
                var recentOrders = await _context.Orders
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

                return Ok(new
                {
                    TodayRevenue = todayRevenue,
                    TotalOrders = totalOrders,
                    CustomerCount = customerCount,
                    TopProducts = topProducts,
                    ChartData = chartData,
                    RecentOrders = recentOrders,
                    EstimatedProfit = todayRevenue * 0.4m // Giả định lợi nhuận 40%
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi Dashboard", detail = ex.Message });
            }
        }
    }
}
