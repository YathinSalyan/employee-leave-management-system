namespace EmployeeLeaveManagement.Models;

public class Employee
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public DateTime DateOfBirth { get; set; }
    public DateTime DateOfJoining { get; set; }
    public string Designation { get; set; } = string.Empty;
    public decimal Salary { get; set; }

    public int DepartmentId { get; set; }
    public Department? Department { get; set; }

    // Self-referencing FK: who this employee reports to.
    // Null for employees with no manager (e.g. top-level / Admin-managed directly).
    public int? ManagerId { get; set; }
    public Employee? Manager { get; set; }
    public ICollection<Employee> DirectReports { get; set; } = new List<Employee>();

    // Total leave days allotted per year. Used/remaining are computed at query time
    // from approved LeaveRequests rather than stored, to avoid drift.
    public int AnnualLeaveEntitlement { get; set; } = 20;

    public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
}
