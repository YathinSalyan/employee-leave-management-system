namespace EmployeeLeaveManagement.Models;

public enum LeaveStatus
{
    Pending,
    Approved,
    Rejected
}

public class LeaveRequest
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public string LeaveType { get; set; } = string.Empty; // e.g. "Casual", "Sick", "Vacation"
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Reason { get; set; }

    public LeaveStatus Status { get; set; } = LeaveStatus.Pending;
    public DateTime AppliedDate { get; set; } = DateTime.UtcNow;

    // FK to Users, not a free-text name — lets the dashboard show who approved it
    // and lets the "only the manager can approve their own team" rule be enforced
    // by comparing this user's linked Employee to the request's Employee.ManagerId.
    public int? ApprovedByUserId { get; set; }
    public User? ApprovedByUser { get; set; }
    public DateTime? ApprovedDate { get; set; }

    public int DurationInDays => (EndDate.Date - StartDate.Date).Days + 1;
}
