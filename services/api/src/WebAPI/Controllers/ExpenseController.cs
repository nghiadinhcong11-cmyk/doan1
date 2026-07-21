using Microsoft.AspNetCore.Mvc;
using RestaurantPOS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RestaurantPOS.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExpenseController : ControllerBase
    {
        private static List<Expense> _expenses = new List<Expense>
        {
            new Expense { Id = Guid.NewGuid(), Description = "Thanh toán tiền điện tháng 6", Amount = 1250000, Category = "Điện nước", PaymentMethod = "Chuyển khoản", ExpenseDate = DateTime.Now.AddDays(-5) },
            new Expense { Id = Guid.NewGuid(), Description = "Nhập bia và nước ngọt", Amount = 3500000, Category = "Nhập hàng", PaymentMethod = "Tiền mặt", ExpenseDate = DateTime.Now.AddDays(-2) },
            new Expense { Id = Guid.NewGuid(), Description = "Trả lương nhân viên tháng 6", Amount = 15000000, Category = "Lương", PaymentMethod = "Chuyển khoản", ExpenseDate = DateTime.Now.AddDays(-1) }
        };

        [HttpGet]
        public IActionResult GetExpenses([FromQuery] string category, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            var query = _expenses.AsQueryable();

            if (!string.IsNullOrEmpty(category))
                query = query.Where(e => e.Category == category);

            if (fromDate.HasValue)
                query = query.Where(e => e.ExpenseDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(e => e.ExpenseDate <= toDate.Value);

            return Ok(query.OrderByDescending(e => e.ExpenseDate).ToList());
        }

        [HttpPost]
        public IActionResult CreateExpense(Expense expense)
        {
            expense.Id = Guid.NewGuid();
            expense.CreatedAt = DateTime.UtcNow;
            _expenses.Add(expense);
            return Ok(expense);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteExpense(Guid id)
        {
            var expense = _expenses.FirstOrDefault(e => e.Id == id);
            if (expense == null) return NotFound();
            _expenses.Remove(expense);
            return NoContent();
        }
    }
}
