using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestaurantPOS.Domain.Entities;

namespace RestaurantPOS.Application.Services
{
    public interface IEmployeeService
    {
        Task<List<object>> GetActiveStaffAsync(Guid? branchId);
        Task<List<WorkSchedule>> GetEmployeeShiftsAsync(string employeeName, Guid? branchId);
        Task<List<Shift>> GetShiftsByEmployeeIdAsync(Guid employeeId, DateTime date);
    }
}
