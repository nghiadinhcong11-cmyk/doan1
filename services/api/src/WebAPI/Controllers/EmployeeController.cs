using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantPOS.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EmployeeController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetEmployees([FromQuery] string? search, [FromQuery] bool? isActive, [FromQuery] string? department, [FromQuery] string? position)
        {
            var query = _context.Employees.AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(e => e.FullName.Contains(search) || e.EmployeeCode.Contains(search) || (e.PhoneNumber != null && e.PhoneNumber.Contains(search)));

            if (isActive.HasValue)
                query = query.Where(e => e.IsActive == isActive.Value);

            if (!string.IsNullOrEmpty(department))
                query = query.Where(e => e.Department == department);

            if (!string.IsNullOrEmpty(position))
                query = query.Where(e => e.Position == position);

            return Ok(await query.OrderByDescending(e => e.CreatedAt).ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> CreateEmployee(Employee employee)
        {
            try
            {
                employee.Id = Guid.NewGuid();
                employee.CreatedAt = DateTime.UtcNow;

                // Ép kiểu sang UTC cho PostgreSQL
                employee.StartDate = DateTime.SpecifyKind(employee.StartDate, DateTimeKind.Utc);
                if (employee.BirthDate.HasValue)
                {
                    employee.BirthDate = DateTime.SpecifyKind(employee.BirthDate.Value, DateTimeKind.Utc);
                }

                if (string.IsNullOrEmpty(employee.EmployeeCode))
                {
                    var count = await _context.Employees.CountAsync();
                    employee.EmployeeCode = $"NV{count + 1:D5}";
                }

                _context.Employees.Add(employee);
                await _context.SaveChangesAsync();
                return Ok(employee);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, inner = ex.InnerException?.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployee(Guid id, [FromBody] Employee employee)
        {
            if (id != employee.Id) return BadRequest(new { message = "ID không khớp" });

            // Ép kiểu sang UTC cho PostgreSQL
            employee.StartDate = DateTime.SpecifyKind(employee.StartDate, DateTimeKind.Utc);
            if (employee.BirthDate.HasValue)
            {
                employee.BirthDate = DateTime.SpecifyKind(employee.BirthDate.Value, DateTimeKind.Utc);
            }
            employee.CreatedAt = DateTime.SpecifyKind(employee.CreatedAt, DateTimeKind.Utc);

            _context.Entry(employee).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(employee);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, inner = ex.InnerException?.Message });
            }
        }

        [HttpPatch("{id}/toggle-status")]
        public async Task<IActionResult> ToggleStatus(Guid id)
        {
            var emp = await _context.Employees.FindAsync(id);
            if (emp == null) return NotFound();

            emp.IsActive = !emp.IsActive;
            await _context.SaveChangesAsync();
            return Ok(new { isActive = emp.IsActive });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(Guid id)
        {
            var emp = await _context.Employees.FindAsync(id);
            if (emp == null) return NotFound();

            _context.Employees.Remove(emp);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
