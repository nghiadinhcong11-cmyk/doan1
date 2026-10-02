using Microsoft.EntityFrameworkCore;
using RestaurantPOS.Infrastructure.Persistence;
using RestaurantPOS.AI.Models;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantPOS.AI.Services
{
    public class AiContextService
    {
        private readonly ApplicationDbContext _context;

        public AiContextService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string> GetContextAsync(AiUserContext userContext)
        {
            var vnZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var nowVn = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnZone);

            return userContext.Role.ToLower() switch
            {
                "admin" => await GetAdminContext(nowVn),
                "manager" => await GetEmployeeContext(userContext, nowVn),
                "employee" => await GetEmployeeContext(userContext, nowVn),
                "cashier" => await GetEmployeeContext(userContext, nowVn),
                "kitchen" => await GetEmployeeContext(userContext, nowVn),
                "customer" => await GetCustomerContext(userContext, nowVn),
                _ => "Thông tin hệ thống hiện không khả dụng."
            };
        }

        private async Task<string> GetAdminContext(DateTime nowVn)
        {
            var todayStart = TimeZoneInfo.ConvertTimeToUtc(nowVn.Date, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));

            var query = _context.Orders
                .Where(o => o.CreatedAt >= todayStart && o.Status == "Hoàn thành");

            decimal revenue = _context.Database.ProviderName?.Contains("Sqlite", StringComparison.OrdinalIgnoreCase) == true
                ? (await query.Select(o => o.PaidAmount).ToListAsync()).Sum()
                : await query.SumAsync(o => (decimal?)o.PaidAmount) ?? 0m;

            var activeTables = await _context.Tables.CountAsync(t => t.Status == "Có khách");
            var branchCount = await _context.Branches.CountAsync();

            return $@"
- Hệ thống: DOAN POS (Quản trị Hệ thống)
- Chi nhánh: {branchCount} chi nhánh đang hoạt động.
- Hôm nay ({nowVn:dd/MM/yyyy}): Doanh thu toàn chuỗi đạt {revenue:N0} VNĐ.
- Vận hành: Tổng số {activeTables} bàn đang có khách.";
        }

        private async Task<string> GetEmployeeContext(AiUserContext userContext, DateTime nowVn)
        {
            var branchName = "Toàn hệ thống";
            if (userContext.BranchId.HasValue)
            {
                var branch = await _context.Branches.FindAsync(userContext.BranchId.Value);
                branchName = branch?.Name ?? "Chi nhánh ẩn";
            }

            var tableStats = await _context.Tables
                .Where(t => t.BranchId == userContext.BranchId)
                .GroupBy(t => t.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var tablesText = string.Join(", ", tableStats.Select(s => $"{s.Status}: {s.Count}"));

            return $@"
- Nhân viên: {userContext.UserName}
- Chi nhánh: {branchName}
- Tình trạng bàn tại chi nhánh: {tablesText}
- Thời gian: {nowVn:HH:mm} ngày {nowVn:dd/MM/yyyy}";
        }

        private async Task<string> GetCustomerContext(AiUserContext userContext, DateTime nowVn)
        {
            var categories = await _context.Products
                .Where(p => p.IsActive)
                .Select(p => p.Category)
                .Distinct()
                .ToListAsync();

            var branches = await _context.Branches
                .Select(b => new { b.Id, b.Name })
                .ToListAsync();

            var branchInfo = string.Join(", ", branches.Select(b => $"{b.Name} (ID: {b.Id})"));

            var orderNote = "";
            // ... (giữ nguyên phần orderNote)

            return $@"
- Chào mừng bạn đến với nhà hàng DOAN POS.
- Thực đơn hôm nay có các nhóm: {string.Join(", ", categories)}.
- Danh sách chi nhánh: {branchInfo}.
{orderNote}
- Thời gian hiện tại: {nowVn:dd/MM/yyyy HH:mm}.";
        }

        // Giữ lại phương thức cũ để tránh lỗi compile nếu Orchestrator chưa cập nhật, nhưng đánh dấu lỗi thời
        [Obsolete("Sử dụng GetContextAsync(AiUserContext) để đảm bảo bảo mật.")]
        public async Task<string> GetCurrentContextAsync()
        {
            return await GetAdminContext(DateTime.Now);
        }
    }
}
