namespace EmployeeLeaveManagement.Models;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string Employee = "Employee";
}

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    // Kept as a plain string rather than an enum: it's what ends up in the JWT role
    // claim anyway, and it keeps AddPolicy/[Authorize(Roles=...)] wiring simple.
    // Must be one of the Roles.* constants above.
    public string Role { get; set; } = Roles.Employee;

    // Nullable: an Admin account doesn't strictly need a linked Employee profile.
    public int? EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
