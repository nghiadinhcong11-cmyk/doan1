using System;

namespace RestaurantPOS.Application.DTOs.Expenses;

public class ExpenseCreateDto
{
    public Guid BranchId { get; set; }
    public string? Category { get; set; }
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public DateTime ExpenseDate { get; set; }
    public string? PaymentMethod { get; set; }
    public string? Note { get; set; }
}

public sealed class ExpenseUpdateDto : ExpenseCreateDto { }
