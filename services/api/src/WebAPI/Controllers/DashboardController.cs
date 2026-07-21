using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantPOS.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        // Giả sử bạn đã có DbContext được Inject vào đây
        // private readonly ApplicationDbContext _context;
        // public DashboardController(ApplicationDbContext context) => _context = context;

        [HttpGet("summary")]
        public IActionResult GetSummary()
        {
            // Logic tính toán thực tế sẽ truy vấn từ DB:
            // var today = DateTime.Today;
            // var revenue = _context.Orders.Where(o => o.CreatedAt.Date == today).Sum(o => o.TotalAmount);
            // var expense = _context.Expenses.Where(e => e.ExpenseDate.Date == today).Sum(e => e.Amount);

            var summary = new
            {
                TodayRevenue = 5420000, // Tổng tiền bán được trong ngày
                TotalOrders = 42,        // Tổng số hóa đơn
                TodayExpenses = 1200000, // Tổng các khoản chi trong ngày
                EstimatedProfit = 4220000, // Lợi nhuận = Doanh thu - Chi phí

                // Dữ liệu cho biểu đồ (Revenue Chart)
                ChartData = new[]
                {
                    new { Time = "08:00", Amount = 450000 },
                    new { Time = "10:00", Amount = 1200000 },
                    new { Time = "12:00", Amount = 2100000 },
                    new { Time = "14:00", Amount = 800000 },
                    new { Time = "16:00", Amount = 1500000 },
                    new { Time = "18:00", Amount = 3200000 }
                },

                // Danh sách chi tiêu gần nhất
                RecentExpenses = new[]
                {
                    new { Id = 1, Description = "Tiền điện tháng 7", Amount = 1500000, Category = "Tiện ích" },
                    new { Id = 2, Description = "Nhập bia Sài Gòn", Amount = 2400000, Category = "Hàng hóa" },
                    new { Id = 3, Description = "Trả lương nhân viên A", Amount = 500000, Category = "Lương" }
                }
            };

            return Ok(summary);
        }
    }
}
