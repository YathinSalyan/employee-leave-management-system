using System.ComponentModel.DataAnnotations;
using EmployeeLeaveManagement.Models;

namespace EmployeeLeaveManagement.DTOs.Employee;

// Default shape returned for team views / self profile. No Salary field —
// Managers viewing their team, or employees viewing colleagues, shouldn't see it.
public class EmployeeDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public DateTime DateOfBirth { get; set; }
    public DateTime DateOfJoining { get; set; }
    public string Designation { get; set; } = string.Empty;

    public int DepartmentId { get; set; }
    public string? DepartmentName { get; set; }

    public int? ManagerId { get; set; }
    public string? ManagerName { get; set; }

    public int AnnualLeaveEntitlement { get; set; }
    public int UsedLeaveDays { get; set; }
    public int RemainingLeaveDays { get; set; }
}

// Admin-only view: everything in EmployeeDto plus Salary.
public class AdminEmployeeDto : EmployeeDto
{
    public decimal Salary { get; set; }
}

public class CreateEmployeeDto
{
    [Required] public string FirstName { get; set; } = string.Empty;
    [Required] public string LastName { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    [Required] public DateTime DateOfBirth { get; set; }
    [Required] public DateTime DateOfJoining { get; set; }
    [Required] public string Designation { get; set; } = string.Empty;
    [Range(0, double.MaxValue)] public decimal Salary { get; set; }
    public int DepartmentId { get; set; }
    public int? ManagerId { get; set; }
    public int AnnualLeaveEntitlement { get; set; } = 20;

    // Also provisions the linked login account.
    [Required] public string Username { get; set; } = string.Empty;
    [Required, MinLength(6)] public string Password { get; set; } = string.Empty;
    [Required] public string Role { get; set; } = Roles.Employee;
}

public class UpdateEmployeeDto
{
    [Required] public string FirstName { get; set; } = string.Empty;
    [Required] public string LastName { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    [Required] public DateTime DateOfBirth { get; set; }
    [Required] public DateTime DateOfJoining { get; set; }
    [Required] public string Designation { get; set; } = string.Empty;
    [Range(0, double.MaxValue)] public decimal Salary { get; set; }
    public int DepartmentId { get; set; }
    public int? ManagerId { get; set; }
    public int AnnualLeaveEntitlement { get; set; }
}

// Narrow DTO for the self-service "update my profile" endpoint — an employee
// can update their own contact info, not their salary/department/manager.
public class UpdateOwnProfileDto
{
    public string? Phone { get; set; }
    [EmailAddress] public string? Email { get; set; }
}
