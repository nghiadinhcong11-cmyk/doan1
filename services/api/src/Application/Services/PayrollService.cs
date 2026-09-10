using Microsoft.EntityFrameworkCore;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Infrastructure.Persistence;

namespace RestaurantPOS.Application.Services
{
    public class PayrollService : IPayrollService
    {
        private readonly ApplicationDbContext _db;
        public PayrollService(ApplicationDbContext db) => _db = db;

        public async Task<Payroll> CalculateAsync(Guid employeeId, Guid branchId, int month, int year, decimal? overtimeHours = null)
        {
            if (month is < 1 or > 12 || year < 2000 || year > 2100) throw new ArgumentException("Tháng/năm không hợp lệ.");
            var employee = await _db.Employees.FindAsync(employeeId) ?? throw new KeyNotFoundException("Không tìm thấy nhân viên.");
            var payroll = await _db.Payrolls.Include(p => p.Adjustments).FirstOrDefaultAsync(p => p.EmployeeId == employeeId && p.BranchId == branchId && p.Month == month && p.Year == year);
            if (payroll?.Status is "Locked" or "Paid") throw new InvalidOperationException("Bảng lương đã chốt, không thể tính lại.");

            var target = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
            var profile = await _db.EmployeeSalaryProfiles.AsNoTracking()
                .Where(s => s.EmployeeId == employeeId && s.BranchId == branchId && s.EffectiveFrom < target.AddMonths(1) && (!s.EffectiveTo.HasValue || s.EffectiveTo >= target))
                .OrderByDescending(s => s.EffectiveFrom).FirstOrDefaultAsync();
            var settings = await GetSettingsAsync(branchId);
            var employeeType = profile?.EmployeeType ?? employee.EmployeeType ?? "FullTime";
            var monthlySalary = profile?.MonthlySalary ?? employee.BasicSalary;
            var hourlyRate = profile?.HourlyRate ?? settings.DefaultPartTimeHourlyRate;
            if (employeeType == "FullTime" && monthlySalary <= 0) throw new InvalidOperationException("Nhân viên chưa có lương tháng.");
            if (employeeType == "PartTime" && hourlyRate <= 0) throw new InvalidOperationException("Nhân viên chưa có đơn giá theo giờ.");
            if (employeeType == "FullTime" && hourlyRate <= 0) hourlyRate = monthlySalary / Math.Max(1m, settings.StandardWorkDays * settings.StandardWorkHoursPerDay);

            var from = target; var to = target.AddMonths(1);
            var attendance = await _db.Attendances.AsNoTracking().Where(a => a.EmployeeId == employeeId && a.BranchId == branchId && a.CheckInTime >= from && a.CheckInTime < to && a.CheckOutTime.HasValue && a.CheckOutTime >= a.CheckInTime).ToListAsync();
            var schedules = await _db.WorkSchedules.AsNoTracking().Where(s => s.EmployeeId == employeeId && s.BranchId == branchId && s.Date >= from && s.Date < to).ToListAsync();
            var actualDays = attendance.Select(a => a.CheckInTime.Date).Distinct().Count();
            var totalHours = attendance.Sum(a => (decimal)(a.CheckOutTime!.Value - a.CheckInTime).TotalHours);
            var regularHours = employeeType == "PartTime" ? totalHours : Math.Min(totalHours, settings.StandardWorkDays * settings.StandardWorkHoursPerDay);
            var calculatedOvertime = attendance.Sum(a =>
            {
                var schedule = schedules.FirstOrDefault(s => s.Date.Date == a.CheckInTime.Date);
                if (schedule != null && TimeSpan.TryParse(schedule.EndTime, out var end))
                {
                    var scheduledEnd = DateTime.SpecifyKind(a.CheckInTime.Date.Add(end), DateTimeKind.Utc);
                    return Math.Max(0m, (decimal)(a.CheckOutTime!.Value - scheduledEnd).TotalHours);
                }
                return Math.Max(0m, (decimal)(a.CheckOutTime!.Value - a.CheckInTime).TotalHours - settings.StandardWorkHoursPerDay);
            });
            var otHours = Math.Max(0m, overtimeHours ?? calculatedOvertime);
            var otRate = profile?.OvertimeRate > 0 ? profile.OvertimeRate : settings.OvertimeMultiplier;
            var regularPay = employeeType == "PartTime" ? regularHours * hourlyRate : monthlySalary;
            var missing = Math.Max(0m, settings.StandardWorkDays - actualDays);
            var missingDeduction = employeeType == "FullTime" ? missing * settings.FullTimeDeductionPerMissingDay : 0m;
            if (payroll == null) { payroll = new Payroll { Id = Guid.NewGuid(), EmployeeId = employeeId, BranchId = branchId, Month = month, Year = year, CreatedAt = DateTime.UtcNow }; _db.Payrolls.Add(payroll); }
            payroll.EmployeeType = employeeType; payroll.StandardWorkDays = settings.StandardWorkDays; payroll.ActualWorkDays = actualDays; payroll.TotalHours = Math.Round(totalHours, 2); payroll.RegularHours = Math.Round(regularHours, 2); payroll.OvertimeHours = Math.Round(otHours, 2);
            payroll.BaseSalary = monthlySalary; payroll.HourlyRate = hourlyRate; payroll.RegularPay = decimal.Round(regularPay, 0); payroll.OvertimePay = decimal.Round(settings.OvertimeEnabled ? otHours * hourlyRate * otRate : 0m, 0); payroll.Allowance = profile?.Allowance ?? 0m;
            RecalculateTotals(payroll, missingDeduction, payroll.Allowance); payroll.Status = "Calculated"; payroll.CalculatedAt = DateTime.UtcNow; payroll.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(); return payroll;
        }

        private static void RecalculateTotals(Payroll p, decimal missingDeduction = 0m, decimal? baseAllowance = null)
        {
            p.Bonus = p.Adjustments.Where(a => a.Type == "Bonus").Sum(a => a.Amount);
            p.Deduction = missingDeduction + p.Adjustments.Where(a => a.Type == "Deduction").Sum(a => a.Amount);
            var allowanceBase = baseAllowance ?? (p.Allowance - p.Adjustments.Where(a => a.Type == "Allowance").Sum(a => a.Amount));
            p.Allowance = allowanceBase + p.Adjustments.Where(a => a.Type == "Allowance").Sum(a => a.Amount);
            p.GrossSalary = p.RegularPay + p.OvertimePay + p.Allowance + p.Bonus;
            p.NetSalary = p.GrossSalary - p.Deduction;
        }

        public Task<Payroll?> GetAsync(Guid id) => _db.Payrolls.AsNoTracking().Include(p => p.Adjustments).FirstOrDefaultAsync(p => p.Id == id);
        public async Task<(IReadOnlyList<Payroll> Items, int Total)> ListAsync(int month, int year, Guid? branchId, string? status, Guid? employeeId, int page, int pageSize)
        {
            var q = _db.Payrolls.AsNoTracking().Where(p => p.Month == month && p.Year == year);
            if (branchId.HasValue) q = q.Where(p => p.BranchId == branchId); if (!string.IsNullOrWhiteSpace(status)) q = q.Where(p => p.Status == status); if (employeeId.HasValue) q = q.Where(p => p.EmployeeId == employeeId);
            var total = await q.CountAsync(); var items = await q.OrderBy(p => p.EmployeeId).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(); return (items, total);
        }
        public async Task<EmployeeSalaryProfile?> GetSalaryAsync(Guid employeeId, Guid branchId) => await _db.EmployeeSalaryProfiles.AsNoTracking().Where(s => s.EmployeeId == employeeId && s.BranchId == branchId).OrderByDescending(s => s.EffectiveFrom).FirstOrDefaultAsync();
        public async Task<EmployeeSalaryProfile> SaveSalaryAsync(Guid employeeId, Guid branchId, EmployeeSalaryProfile input)
        {
            if (input.EmployeeType is not ("FullTime" or "PartTime")) throw new ArgumentException("Loại nhân viên không hợp lệ.");
            if (input.EmployeeType == "FullTime" && input.MonthlySalary <= 0) throw new ArgumentException("Lương tháng phải lớn hơn 0.");
            if (input.EmployeeType == "PartTime" && input.HourlyRate <= 0) throw new ArgumentException("Đơn giá theo giờ phải lớn hơn 0.");
            var existing = await _db.EmployeeSalaryProfiles.FirstOrDefaultAsync(s => s.EmployeeId == employeeId && s.BranchId == branchId && s.EffectiveFrom == input.EffectiveFrom);
            if (existing == null) { input.Id = Guid.NewGuid(); input.EmployeeId = employeeId; input.BranchId = branchId; input.UpdatedAt = DateTime.UtcNow; _db.EmployeeSalaryProfiles.Add(input); existing = input; } else { input.Id = existing.Id; input.EmployeeId = employeeId; input.BranchId = branchId; input.UpdatedAt = DateTime.UtcNow; _db.Entry(existing).CurrentValues.SetValues(input); }
            var employee = await _db.Employees.FindAsync(employeeId); if (employee != null) { employee.EmployeeType = input.EmployeeType; if (input.EmployeeType == "FullTime") employee.BasicSalary = input.MonthlySalary; }
            await _db.SaveChangesAsync(); return existing;
        }
        public async Task<PayrollSettings> GetSettingsAsync(Guid? branchId) => await _db.PayrollSettings.FirstOrDefaultAsync(s => s.BranchId == branchId) ?? new PayrollSettings { Id = Guid.NewGuid(), BranchId = branchId };
        public async Task<PayrollSettings> SaveSettingsAsync(Guid? branchId, PayrollSettings input)
        {
            if (input.StandardWorkDays <= 0 || input.StandardWorkHoursPerDay <= 0 || input.OvertimeMultiplier < 0) throw new ArgumentException("Cấu hình ngày/giờ công không hợp lệ.");
            var existing = await _db.PayrollSettings.FirstOrDefaultAsync(s => s.BranchId == branchId); if (existing == null) { input.Id = Guid.NewGuid(); input.BranchId = branchId; _db.PayrollSettings.Add(input); existing = input; } else { input.Id = existing.Id; input.BranchId = branchId; input.UpdatedAt = DateTime.UtcNow; _db.Entry(existing).CurrentValues.SetValues(input); } await _db.SaveChangesAsync(); return existing;
        }
        public async Task<PayrollAdjustment> AddAdjustmentAsync(Guid payrollId, string type, decimal amount, string reason, Guid? createdBy)
        {
            if (type is not ("Bonus" or "Deduction" or "Allowance") || amount <= 0 || string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("Khoản điều chỉnh không hợp lệ.");
            var p = await _db.Payrolls.Include(x => x.Adjustments).FirstOrDefaultAsync(x => x.Id == payrollId) ?? throw new KeyNotFoundException("Không tìm thấy bảng lương."); if (p.Status is "Locked" or "Paid") throw new InvalidOperationException("Bảng lương đã chốt.");
            var a = new PayrollAdjustment { Id = Guid.NewGuid(), PayrollId = payrollId, Type = type, Amount = amount, Reason = reason.Trim(), CreatedBy = createdBy }; p.Adjustments.Add(a); _db.PayrollAdjustments.Add(a); RecalculateTotals(p); p.UpdatedAt = DateTime.UtcNow; await _db.SaveChangesAsync(); return a;
        }
        public async Task LockAsync(Guid id) { var p = await _db.Payrolls.FindAsync(id) ?? throw new KeyNotFoundException("Không tìm thấy bảng lương."); if (p.Status == "Paid") throw new InvalidOperationException("Bảng lương đã thanh toán."); p.Status = "Locked"; p.LockedAt = DateTime.UtcNow; await _db.SaveChangesAsync(); }
        public async Task UnlockAsync(Guid id) { var p = await _db.Payrolls.FindAsync(id) ?? throw new KeyNotFoundException("Không tìm thấy bảng lương."); if (p.Status == "Paid") throw new InvalidOperationException("Bảng lương đã thanh toán."); p.Status = "Calculated"; p.LockedAt = null; await _db.SaveChangesAsync(); }
    }
}
