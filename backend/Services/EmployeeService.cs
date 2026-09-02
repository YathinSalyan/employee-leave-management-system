using EmployeeLeaveManagement.Common;
using EmployeeLeaveManagement.Data;
using EmployeeLeaveManagement.DTOs.Employee;
using EmployeeLeaveManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLeaveManagement.Services;

public interface IEmployeeService
{
    Task<List<EmployeeDto>> GetAllAsync();
    Task<EmployeeDto> GetByIdAsync(int id);
    Task<AdminEmployeeDto> GetByIdForAdminAsync(int id);
    Task<List<EmployeeDto>> GetTeamAsync(int managerEmployeeId);
    Task<AdminEmployeeDto> CreateAsync(CreateEmployeeDto dto);
    Task<EmployeeDto> UpdateAsync(int id, UpdateEmployeeDto dto);
    Task<EmployeeDto> UpdateOwnProfileAsync(int employeeId, UpdateOwnProfileDto dto);
    Task DeleteAsync(int id);
}

public class EmployeeService : IEmployeeService
{
    private readonly ApplicationDbContext _db;

    public EmployeeService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<EmployeeDto>> GetAllAsync()
    {
        var employees = await _db.Employees
            .Include(e => e.Department)
            .Include(e => e.Manager)
            .Include(e => e.LeaveRequests)
            .AsNoTracking()
            .ToListAsync();

        return employees.Select(MapToDto).ToList();
    }

    public async Task<EmployeeDto> GetByIdAsync(int id) => MapToDto(await FindOrThrowAsync(id));

    public async Task<AdminEmployeeDto> GetByIdForAdminAsync(int id) => MapToAdminDto(await FindOrThrowAsync(id));

    public async Task<List<EmployeeDto>> GetTeamAsync(int managerEmployeeId)
    {
        var team = await _db.Employees
            .Include(e => e.Department)
            .Include(e => e.Manager)
            .Include(e => e.LeaveRequests)
            .Where(e => e.ManagerId == managerEmployeeId)
            .AsNoTracking()
            .ToListAsync();

        return team.Select(MapToDto).ToList();
    }

    public async Task<AdminEmployeeDto> CreateAsync(CreateEmployeeDto dto)
    {
        if (await _db.Employees.AnyAsync(e => e.Email == dto.Email))
        {
            throw new ConflictException($"An employee with email '{dto.Email}' already exists.");
        }

        if (await _db.Users.AnyAsync(u => u.Username == dto.Username))
        {
            throw new ConflictException($"Username '{dto.Username}' is already taken.");
        }

        if (!await _db.Departments.AnyAsync(d => d.Id == dto.DepartmentId))
        {
            throw new NotFoundException($"Department {dto.DepartmentId} was not found.");
        }

        if (dto.ManagerId.HasValue && !await _db.Employees.AnyAsync(e => e.Id == dto.ManagerId))
        {
            throw new NotFoundException($"Manager (employee {dto.ManagerId}) was not found.");
        }

        var employee = new Employee
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            DateOfBirth = dto.DateOfBirth,
            DateOfJoining = dto.DateOfJoining,
            Designation = dto.Designation,
            Salary = dto.Salary,
            DepartmentId = dto.DepartmentId,
            ManagerId = dto.ManagerId,
            AnnualLeaveEntitlement = dto.AnnualLeaveEntitlement
        };

        _db.Employees.Add(employee);
        await _db.SaveChangesAsync();

        var user = new User
        {
            Username = dto.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = dto.Role,
            EmployeeId = employee.Id
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return await GetByIdForAdminAsync(employee.Id);
    }

    public async Task<EmployeeDto> UpdateAsync(int id, UpdateEmployeeDto dto)
    {
        var employee = await FindOrThrowAsync(id);

        if (dto.ManagerId == id)
        {
            throw new BadRequestException("An employee cannot be their own manager.");
        }

        employee.FirstName = dto.FirstName;
        employee.LastName = dto.LastName;
        employee.Email = dto.Email;
        employee.Phone = dto.Phone;
        employee.DateOfBirth = dto.DateOfBirth;
        employee.DateOfJoining = dto.DateOfJoining;
        employee.Designation = dto.Designation;
        employee.Salary = dto.Salary;
        employee.DepartmentId = dto.DepartmentId;
        employee.ManagerId = dto.ManagerId;
        employee.AnnualLeaveEntitlement = dto.AnnualLeaveEntitlement;

        await _db.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task<EmployeeDto> UpdateOwnProfileAsync(int employeeId, UpdateOwnProfileDto dto)
    {
        var employee = await FindOrThrowAsync(employeeId);

        if (!string.IsNullOrWhiteSpace(dto.Phone)) employee.Phone = dto.Phone;
        if (!string.IsNullOrWhiteSpace(dto.Email)) employee.Email = dto.Email;

        await _db.SaveChangesAsync();
        return await GetByIdAsync(employeeId);
    }

    public async Task DeleteAsync(int id)
    {
        var employee = await FindOrThrowAsync(id);

        if (await _db.Employees.AnyAsync(e => e.ManagerId == id))
        {
            throw new ConflictException("Cannot delete an employee who still manages a team. Reassign their reports first.");
        }

        _db.Employees.Remove(employee);
        await _db.SaveChangesAsync();
    }

    private async Task<Employee> FindOrThrowAsync(int id)
    {
        var employee = await _db.Employees
            .Include(e => e.Department)
            .Include(e => e.Manager)
            .Include(e => e.LeaveRequests)
            .FirstOrDefaultAsync(e => e.Id == id);

        return employee ?? throw new NotFoundException($"Employee {id} was not found.");
    }

    private static int UsedLeaveDays(Employee e) =>
        e.LeaveRequests.Where(l => l.Status == LeaveStatus.Approved).Sum(l => l.DurationInDays);

    private static EmployeeDto MapToDto(Employee e)
    {
        var used = UsedLeaveDays(e);
        return new EmployeeDto
        {
            Id = e.Id,
            FirstName = e.FirstName,
            LastName = e.LastName,
            Email = e.Email,
            Phone = e.Phone,
            DateOfBirth = e.DateOfBirth,
            DateOfJoining = e.DateOfJoining,
            Designation = e.Designation,
            DepartmentId = e.DepartmentId,
            DepartmentName = e.Department?.Name,
            ManagerId = e.ManagerId,
            ManagerName = e.Manager is null ? null : $"{e.Manager.FirstName} {e.Manager.LastName}",
            AnnualLeaveEntitlement = e.AnnualLeaveEntitlement,
            UsedLeaveDays = used,
            RemainingLeaveDays = e.AnnualLeaveEntitlement - used
        };
    }

    private static AdminEmployeeDto MapToAdminDto(Employee e)
    {
        var dto = MapToDto(e);
        return new AdminEmployeeDto
        {
            Id = dto.Id,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            DateOfBirth = dto.DateOfBirth,
            DateOfJoining = dto.DateOfJoining,
            Designation = dto.Designation,
            DepartmentId = dto.DepartmentId,
            DepartmentName = dto.DepartmentName,
            ManagerId = dto.ManagerId,
            ManagerName = dto.ManagerName,
            AnnualLeaveEntitlement = dto.AnnualLeaveEntitlement,
            UsedLeaveDays = dto.UsedLeaveDays,
            RemainingLeaveDays = dto.RemainingLeaveDays,
            Salary = e.Salary
        };
    }
}
