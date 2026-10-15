using System;
using System.Text.Json;

namespace RestaurantPOS.Application.DTOs;

/// <summary>
/// Allow-listed employee update payload. BranchId uses JsonElement so an omitted
/// property can be distinguished from an explicit null transfer request.
/// </summary>
public sealed class EmployeeUpdateRequest
{
    public Guid? Id { get; set; }
    public string? EmployeeCode { get; set; }
    public string? EmployeeType { get; set; }
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Position { get; set; }
    public string? Department { get; set; }
    public decimal? BasicSalary { get; set; }
    public JsonElement BranchId { get; set; }
    public string? CitizenId { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? Gender { get; set; }
    public string? Address { get; set; }
    public DateTime? StartDate { get; set; }
    public bool? IsActive { get; set; }
    public string? Role { get; set; }
    public string? Note { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
}
