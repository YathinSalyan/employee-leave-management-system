using EmployeeLeaveManagement.DTOs.Department;
using EmployeeLeaveManagement.Models;
using EmployeeLeaveManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeLeaveManagement.Controllers;

[ApiController]
[Route("api/departments")]
[Authorize]
public class DepartmentController : ControllerBase
{
    private readonly IDepartmentService _departmentService;

    public DepartmentController(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    [HttpGet]
    public async Task<ActionResult<List<DepartmentDto>>> GetAll() => Ok(await _departmentService.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DepartmentDto>> GetById(int id) => Ok(await _departmentService.GetByIdAsync(id));

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<DepartmentDto>> Create(CreateDepartmentDto dto)
    {
        var created = await _departmentService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<DepartmentDto>> Update(int id, CreateDepartmentDto dto) =>
        Ok(await _departmentService.UpdateAsync(id, dto));

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        await _departmentService.DeleteAsync(id);
        return NoContent();
    }
}
