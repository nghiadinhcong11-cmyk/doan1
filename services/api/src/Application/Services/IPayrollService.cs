using RestaurantPOS.Domain.Entities;

namespace RestaurantPOS.Application.Services
{
    public interface IPayrollService
    {
        Task<Payroll> CalculateAsync(Guid employeeId, Guid branchId, int month, int year, decimal? overtimeHours = null);
        Task<Payroll?> GetAsync(Guid id);
        Task<(IReadOnlyList<Payroll> Items, int Total)> ListAsync(int month, int year, Guid? branchId, string? status, Guid? employeeId, int page, int pageSize);
        Task<EmployeeSalaryProfile?> GetSalaryAsync(Guid employeeId, Guid branchId);
        Task<EmployeeSalaryProfile> SaveSalaryAsync(Guid employeeId, Guid branchId, EmployeeSalaryProfile input);
        Task<PayrollSettings> GetSettingsAsync(Guid? branchId);
        Task<PayrollSettings> SaveSettingsAsync(Guid? branchId, PayrollSettings input);
        Task<PayrollAdjustment> AddAdjustmentAsync(Guid payrollId, string type, decimal amount, string reason, Guid? createdBy);
        Task LockAsync(Guid id);
        Task UnlockAsync(Guid id);
    }
}
