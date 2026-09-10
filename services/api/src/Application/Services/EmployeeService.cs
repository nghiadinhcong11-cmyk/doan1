using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Infrastructure.Persistence;

namespace RestaurantPOS.Application.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly ApplicationDbContext _context;

        public EmployeeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<object>> GetActiveStaffAsync(Guid? branchId)
        {
            var nowUtc = DateTime.UtcNow;
            var todayUtc = nowUtc.Date;
            var tomorrowUtc = todayUtc.AddDays(1);

            var query = from a in _context.Attendances
                        join e in _context.Employees on a.EmployeeId equals e.Id
                        where a.CheckInTime >= todayUtc && a.CheckInTime < tomorrowUtc
                        select new
                        {
                            e.FullName,
                            e.Position,
                            e.BranchName,
                            e.BranchId,
                            a.CheckInTime,
                            a.CheckOutTime
                        };

            if (branchId.HasValue)
            {
                query = query.Where(x => x.BranchId == branchId.Value);
            }

            var list = await query.ToListAsync();
            return list.Select(x => (object)new
            {
                name = x.FullName,
                position = x.Position,
                branch = x.BranchName,
                checkIn = x.CheckInTime.ToString("HH:mm"),
                status = x.CheckOutTime == null ? "Đang làm việc" : "Đã về"
            }).ToList();
        }

        public async Task<List<WorkSchedule>> GetEmployeeShiftsAsync(string employeeName, Guid? branchId)
        {
            var today = DateTime.UtcNow.Date;
            var endOfWeek = today.AddDays(7);

            var query = _context.WorkSchedules
                .Where(s => s.EmployeeName.Contains(employeeName) && s.Date >= today && s.Date <= endOfWeek);

            if (branchId.HasValue)
            {
                query = query.Where(s => s.BranchId == branchId.Value);
            }

            return await query.OrderBy(s => s.Date).ToListAsync();
        }

        public async Task<List<Shift>> GetShiftsByEmployeeIdAsync(Guid employeeId, DateTime date)
        {
            return await _context.Shifts
                .Where(s => s.EmployeeId == employeeId && s.StartTime.Date == date.Date)
                .OrderBy(s => s.StartTime)
                .ToListAsync();
        }
    }
}
