using EmployeeLeaveManagement.Common;
using EmployeeLeaveManagement.DTOs.Leave;
using EmployeeLeaveManagement.Models;
using EmployeeLeaveManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeLeaveManagement.Controllers;

[ApiController]
[Route("api/leaves")]
[Authorize]
public class LeaveController : ControllerBase
{
    private readonly ILeaveService _leaveService;

    public LeaveController(ILeaveService leaveService)
    {
        _leaveService = leaveService;
    }

    // Admin: all requests. Manager: their team's requests. Employee: their own.
    [HttpGet]
    public async Task<ActionResult<List<LeaveRequestDto>>> GetAll()
    {
        var userId = User.GetUserId();
        var role = User.GetRole();
        var employeeId = User.GetEmployeeId();
        return Ok(await _leaveService.GetForCurrentUserAsync(userId, role, employeeId));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<LeaveRequestDto>> GetById(int id)
    {
        var userId = User.GetUserId();
        var role = User.GetRole();
        var employeeId = User.GetEmployeeId();
        return Ok(await _leaveService.GetByIdAsync(id, userId, role, employeeId));
    }

    [HttpPost]
    [Authorize(Roles = Roles.Employee + "," + Roles.Manager)]
    public async Task<ActionResult<LeaveRequestDto>> Apply(CreateLeaveRequestDto dto)
    {
        var employeeId = User.GetEmployeeId() ?? throw new NotFoundException("This account has no linked employee profile.");
        var created = await _leaveService.CreateAsync(employeeId, dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}/approve")]
    [Authorize(Roles = Roles.Manager + "," + Roles.Admin)]
    public async Task<ActionResult<LeaveRequestDto>> Approve(int id)
    {
        var userId = User.GetUserId();
        var role = User.GetRole();
        var employeeId = User.GetEmployeeId();
        return Ok(await _leaveService.ApproveAsync(id, userId, role, employeeId));
    }

    [HttpPut("{id:int}/reject")]
    [Authorize(Roles = Roles.Manager + "," + Roles.Admin)]
    public async Task<ActionResult<LeaveRequestDto>> Reject(int id)
    {
        var userId = User.GetUserId();
        var role = User.GetRole();
        var employeeId = User.GetEmployeeId();
        return Ok(await _leaveService.RejectAsync(id, userId, role, employeeId));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Employee + "," + Roles.Manager)]
    public async Task<IActionResult> Cancel(int id)
    {
        var employeeId = User.GetEmployeeId() ?? throw new NotFoundException("This account has no linked employee profile.");
        await _leaveService.CancelAsync(id, employeeId);
        return NoContent();
    }
}
