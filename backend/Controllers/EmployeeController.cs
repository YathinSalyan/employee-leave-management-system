using EmployeeLeaveManagement.Common;
using EmployeeLeaveManagement.DTOs.Employee;
using EmployeeLeaveManagement.Models;
using EmployeeLeaveManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeLeaveManagement.Controllers;

[ApiController]
[Route("api/employees")]
[Authorize]
public class EmployeeController : ControllerBase
{
    private readonly IEmployeeService _employeeService;
    private readonly ILeaveService _leaveService;

    public EmployeeController(IEmployeeService employeeService, ILeaveService leaveService)
    {
        _employeeService = employeeService;
        _leaveService = leaveService;
    }

    [HttpGet]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<List<EmployeeDto>>> GetAll() => Ok(await _employeeService.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EmployeeDto>> GetById(int id)
    {
        // Admin can view anyone. Managers can view their own team. Employees can view themselves.
        var role = User.GetRole();
        var employeeId = User.GetEmployeeId();

        if (role == Roles.Admin)
        {
            return Ok(await _employeeService.GetByIdForAdminAsync(id));
        }

        if (employeeId == id)
        {
            return Ok(await _employeeService.GetByIdAsync(id));
        }

        if (role == Roles.Manager)
        {
            var team = await _employeeService.GetTeamAsync(employeeId ?? -1);
            var match = team.FirstOrDefault(e => e.Id == id);
            if (match is not null) return Ok(match);
        }

        throw new ForbiddenException("You are not permitted to view this employee.");
    }

    [HttpGet("me")]
    public async Task<ActionResult<EmployeeDto>> GetOwnProfile()
    {
        var employeeId = User.GetEmployeeId() ?? throw new NotFoundException("This account has no linked employee profile.");
        return Ok(await _employeeService.GetByIdAsync(employeeId));
    }

    [HttpPut("me")]
    public async Task<ActionResult<EmployeeDto>> UpdateOwnProfile(UpdateOwnProfileDto dto)
    {
        var employeeId = User.GetEmployeeId() ?? throw new NotFoundException("This account has no linked employee profile.");
        return Ok(await _employeeService.UpdateOwnProfileAsync(employeeId, dto));
    }

    [HttpGet("me/team")]
    [Authorize(Roles = Roles.Manager)]
    public async Task<ActionResult<List<EmployeeDto>>> GetOwnTeam()
    {
        var employeeId = User.GetEmployeeId() ?? throw new NotFoundException("This account has no linked employee profile.");
        return Ok(await _employeeService.GetTeamAsync(employeeId));
    }

    [HttpGet("{id:int}/leaves")]
    public async Task<ActionResult> GetEmployeeLeaves(int id)
    {
        var userId = User.GetUserId();
        var role = User.GetRole();
        var employeeId = User.GetEmployeeId();
        return Ok(await _leaveService.GetByEmployeeAsync(id, userId, role, employeeId));
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<AdminEmployeeDto>> Create(CreateEmployeeDto dto)
    {
        var created = await _employeeService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<EmployeeDto>> Update(int id, UpdateEmployeeDto dto) =>
        Ok(await _employeeService.UpdateAsync(id, dto));

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        await _employeeService.DeleteAsync(id);
        return NoContent();
    }
}
