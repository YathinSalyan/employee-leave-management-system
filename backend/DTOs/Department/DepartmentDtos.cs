using System.ComponentModel.DataAnnotations;

namespace EmployeeLeaveManagement.DTOs.Department;

public class DepartmentDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int EmployeeCount { get; set; }
}

public class CreateDepartmentDto
{
    [Required] public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
