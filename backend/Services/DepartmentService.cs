using EmployeeLeaveManagement.Common;
using EmployeeLeaveManagement.Data;
using EmployeeLeaveManagement.DTOs.Department;
using EmployeeLeaveManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLeaveManagement.Services;

public interface IDepartmentService
{
    Task<List<DepartmentDto>> GetAllAsync();
    Task<DepartmentDto> GetByIdAsync(int id);
    Task<DepartmentDto> CreateAsync(CreateDepartmentDto dto);
    Task<DepartmentDto> UpdateAsync(int id, CreateDepartmentDto dto);
    Task DeleteAsync(int id);
}

public class DepartmentService : IDepartmentService
{
    private readonly ApplicationDbContext _db;

    public DepartmentService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<DepartmentDto>> GetAllAsync()
    {
        return await _db.Departments
            .Include(d => d.Employees)
            .AsNoTracking()
            .Select(d => MapToDto(d))
            .ToListAsync();
    }

    public async Task<DepartmentDto> GetByIdAsync(int id) => MapToDto(await FindOrThrowAsync(id));

    public async Task<DepartmentDto> CreateAsync(CreateDepartmentDto dto)
    {
        if (await _db.Departments.AnyAsync(d => d.Name == dto.Name))
        {
            throw new ConflictException($"A department named '{dto.Name}' already exists.");
        }

        var department = new Department { Name = dto.Name, Description = dto.Description };
        _db.Departments.Add(department);
        await _db.SaveChangesAsync();

        return MapToDto(department);
    }

    public async Task<DepartmentDto> UpdateAsync(int id, CreateDepartmentDto dto)
    {
        var department = await FindOrThrowAsync(id);
        department.Name = dto.Name;
        department.Description = dto.Description;
        await _db.SaveChangesAsync();
        return MapToDto(department);
    }

    public async Task DeleteAsync(int id)
    {
        var department = await FindOrThrowAsync(id);

        if (await _db.Employees.AnyAsync(e => e.DepartmentId == id))
        {
            throw new ConflictException("Cannot delete a department that still has employees assigned to it.");
        }

        _db.Departments.Remove(department);
        await _db.SaveChangesAsync();
    }

    private async Task<Department> FindOrThrowAsync(int id)
    {
        var department = await _db.Departments.Include(d => d.Employees).FirstOrDefaultAsync(d => d.Id == id);
        return department ?? throw new NotFoundException($"Department {id} was not found.");
    }

    private static DepartmentDto MapToDto(Department d) => new()
    {
        Id = d.Id,
        Name = d.Name,
        Description = d.Description,
        EmployeeCount = d.Employees.Count
    };
}
