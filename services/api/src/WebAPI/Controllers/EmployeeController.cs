using Microsoft.AspNetCore.Mvc;
using RestaurantPOS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RestaurantPOS.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : ControllerBase
    {
        private static List<Employee> _employees = new List<Employee>
        {
            new Employee {
                Id = Guid.NewGuid(),
                EmployeeCode = "NV00001",
                FullName = "Trần Văn Hoàng",
                PhoneNumber = "0987654321",
                Position = "Thu ngân",
                Department = "Bán hàng",
                IsActive = true,
                StartDate = DateTime.Now.AddMonths(-6)
            },
            new Employee {
                Id = Guid.NewGuid(),
                EmployeeCode = "NV00002",
                FullName = "Nguyễn Thị Mai",
                PhoneNumber = "0912345678",
                Position = "Phục vụ",
                Department = "Bán hàng",
                IsActive = true,
                StartDate = DateTime.Now.AddMonths(-2)
            }
        };

        [HttpGet]
        public IActionResult GetEmployees([FromQuery] string search, [FromQuery] bool? isActive)
        {
            var query = _employees.AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(e => e.FullName.Contains(search, StringComparison.OrdinalIgnoreCase) || e.EmployeeCode.Contains(search));

            if (isActive.HasValue)
                query = query.Where(e => e.IsActive == isActive.Value);

            return Ok(query.ToList());
        }

        [HttpPost]
        public IActionResult CreateEmployee(Employee employee)
        {
            employee.Id = Guid.NewGuid();
            _employees.Add(employee);
            return Ok(employee);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateEmployee(Guid id, Employee update)
        {
            var emp = _employees.FirstOrDefault(e => e.Id == id);
            if (emp == null) return NotFound();

            emp.FullName = update.FullName;
            emp.PhoneNumber = update.PhoneNumber;
            emp.Position = update.Position;
            emp.IsActive = update.IsActive;

            return Ok(emp);
        }
    }
}
