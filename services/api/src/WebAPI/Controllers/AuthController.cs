using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantPOS.Infrastructure.Persistence;
using System;
using System.Threading.Tasks;

namespace RestaurantPOS.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (request.Mode == "admin")
            {
                if (request.Username == "admin" && request.Password == "123456")
                {
                    return Ok(new { role = "admin", fullName = "Quản trị viên" });
                }
                return BadRequest(new { message = "Tài khoản quản trị không đúng" });
            }

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Username == request.Username && e.Password == request.Password && e.IsActive);

            if (employee == null)
            {
                return BadRequest(new { message = "Tên đăng nhập hoặc mật khẩu không chính xác" });
            }

            return Ok(new {
                role = "cashier",
                fullName = employee.FullName,
                branchName = employee.BranchName,
                employeeId = employee.Id
            });
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Mode { get; set; } // admin or cashier
    }
}
