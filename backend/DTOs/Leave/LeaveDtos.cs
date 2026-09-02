using System.ComponentModel.DataAnnotations;
using EmployeeLeaveManagement.Models;

namespace EmployeeLeaveManagement.DTOs.Leave;

public class LeaveRequestDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public string LeaveType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int DurationInDays { get; set; }
    public string? Reason { get; set; }
    public LeaveStatus Status { get; set; }
    public DateTime AppliedDate { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTime? ApprovedDate { get; set; }
}

public class CreateLeaveRequestDto
{
    [Required] public string LeaveType { get; set; } = string.Empty;
    [Required] public DateTime StartDate { get; set; }
    [Required] public DateTime EndDate { get; set; }
    public string? Reason { get; set; }
}
