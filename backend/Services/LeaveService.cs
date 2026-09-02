using EmployeeLeaveManagement.Common;
using EmployeeLeaveManagement.Data;
using EmployeeLeaveManagement.DTOs.Leave;
using EmployeeLeaveManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLeaveManagement.Services;

public interface ILeaveService
{
    // Scoped by the caller's role: Admin sees all, Manager sees their team's, Employee sees only their own.
    Task<List<LeaveRequestDto>> GetForCurrentUserAsync(int userId, string role, int? employeeId);
    Task<LeaveRequestDto> GetByIdAsync(int id, int userId, string role, int? employeeId);
    Task<List<LeaveRequestDto>> GetByEmployeeAsync(int employeeId, int userId, string role, int? requestingEmployeeId);
    Task<LeaveRequestDto> CreateAsync(int employeeId, CreateLeaveRequestDto dto);
    Task<LeaveRequestDto> ApproveAsync(int leaveId, int approverUserId, string approverRole, int? approverEmployeeId);
    Task<LeaveRequestDto> RejectAsync(int leaveId, int approverUserId, string approverRole, int? approverEmployeeId);
    Task CancelAsync(int leaveId, int employeeId);
}

public class LeaveService : ILeaveService
{
    private readonly ApplicationDbContext _db;
    private readonly IEmailService _emailService;

    public LeaveService(ApplicationDbContext db, IEmailService emailService)
    {
        _db = db;
        _emailService = emailService;
    }

    public async Task<List<LeaveRequestDto>> GetForCurrentUserAsync(int userId, string role, int? employeeId)
    {
        var query = _db.LeaveRequests
            .Include(l => l.Employee)
            .Include(l => l.ApprovedByUser)
            .AsNoTracking()
            .AsQueryable();

        query = role switch
        {
            // Rule: employee can only view their own leave requests.
            Roles.Employee when employeeId.HasValue => query.Where(l => l.EmployeeId == employeeId),

            // Manager sees requests from employees who report to them.
            Roles.Manager when employeeId.HasValue => query.Where(l => l.Employee!.ManagerId == employeeId),

            Roles.Admin => query,

            _ => query.Where(l => false) // no linked employee profile and not Admin -> nothing to show
        };

        var results = await query.OrderByDescending(l => l.AppliedDate).ToListAsync();
        return results.Select(MapToDto).ToList();
    }

    public async Task<LeaveRequestDto> GetByIdAsync(int id, int userId, string role, int? employeeId)
    {
        var leave = await FindOrThrowAsync(id);
        EnsureCanView(leave, role, employeeId);
        return MapToDto(leave);
    }

    public async Task<List<LeaveRequestDto>> GetByEmployeeAsync(int employeeId, int userId, string role, int? requestingEmployeeId)
    {
        // Admin can view anyone's history. Managers can view their own team's.
        // Employees can only view their own.
        var isSelf = requestingEmployeeId == employeeId;
        var isManagerOfEmployee = role == Roles.Manager &&
            await _db.Employees.AnyAsync(e => e.Id == employeeId && e.ManagerId == requestingEmployeeId);

        if (role != Roles.Admin && !isSelf && !isManagerOfEmployee)
        {
            throw new ForbiddenException("You are not permitted to view this employee's leave history.");
        }

        var results = await _db.LeaveRequests
            .Include(l => l.Employee)
            .Include(l => l.ApprovedByUser)
            .Where(l => l.EmployeeId == employeeId)
            .OrderByDescending(l => l.AppliedDate)
            .AsNoTracking()
            .ToListAsync();

        return results.Select(MapToDto).ToList();
    }

    public async Task<LeaveRequestDto> CreateAsync(int employeeId, CreateLeaveRequestDto dto)
    {
        // Rule: end date cannot be before start date.
        if (dto.EndDate.Date < dto.StartDate.Date)
        {
            throw new BadRequestException("End date cannot be before the start date.");
        }

        // Rule: leave cannot be submitted for invalid (past) dates.
        if (dto.StartDate.Date < DateTime.UtcNow.Date)
        {
            throw new BadRequestException("Leave cannot be submitted for a start date in the past.");
        }

        // Rule: employee cannot submit overlapping leave requests (ignoring already-rejected ones).
        var overlaps = await _db.LeaveRequests.AnyAsync(l =>
            l.EmployeeId == employeeId &&
            l.Status != LeaveStatus.Rejected &&
            l.StartDate.Date <= dto.EndDate.Date &&
            l.EndDate.Date >= dto.StartDate.Date);

        if (overlaps)
        {
            throw new ConflictException("This leave request overlaps with an existing pending or approved request.");
        }

        var leave = new LeaveRequest
        {
            EmployeeId = employeeId,
            LeaveType = dto.LeaveType,
            StartDate = dto.StartDate.Date,
            EndDate = dto.EndDate.Date,
            Reason = dto.Reason,
            Status = LeaveStatus.Pending,
            AppliedDate = DateTime.UtcNow
        };

        _db.LeaveRequests.Add(leave);
        await _db.SaveChangesAsync();

        var savedLeave = await FindOrThrowAsync(leave.Id);
        await _emailService.SendLeaveSubmittedEmailAsync(savedLeave);

        return MapToDto(savedLeave);
    }

    public async Task<LeaveRequestDto> ApproveAsync(int leaveId, int approverUserId, string approverRole, int? approverEmployeeId) =>
        await DecideAsync(leaveId, approverUserId, approverRole, approverEmployeeId, LeaveStatus.Approved);

    public async Task<LeaveRequestDto> RejectAsync(int leaveId, int approverUserId, string approverRole, int? approverEmployeeId) =>
        await DecideAsync(leaveId, approverUserId, approverRole, approverEmployeeId, LeaveStatus.Rejected);

    private async Task<LeaveRequestDto> DecideAsync(int leaveId, int approverUserId, string approverRole, int? approverEmployeeId, LeaveStatus decision)
    {
        var leave = await FindOrThrowAsync(leaveId);

        // Rule: once approved/rejected, it can't be modified again.
        if (leave.Status != LeaveStatus.Pending)
        {
            throw new ConflictException($"This request has already been {leave.Status}. It cannot be changed.");
        }

        // Rule: only the manager can approve/reject their own team's leave (Admin can act on anyone's,
        // which also covers Managers' own leave requests since those have no manager to approve them).
        if (approverRole == Roles.Manager)
        {
            if (leave.Employee!.ManagerId != approverEmployeeId)
            {
                throw new ForbiddenException("You can only approve or reject leave requests from your own team.");
            }
        }
        else if (approverRole != Roles.Admin)
        {
            throw new ForbiddenException("Only a manager or admin can approve or reject leave requests.");
        }

        leave.Status = decision;
        leave.ApprovedByUserId = approverUserId;
        leave.ApprovedDate = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        var approverUsername = await _db.Users
            .Where(u => u.Id == approverUserId)
            .Select(u => u.Username)
            .FirstOrDefaultAsync() ?? "the approver";

        await _emailService.SendLeaveDecisionEmailAsync(leave, approverUsername);

        return MapToDto(await FindOrThrowAsync(leaveId));
    }

    public async Task CancelAsync(int leaveId, int employeeId)
    {
        var leave = await FindOrThrowAsync(leaveId);

        if (leave.EmployeeId != employeeId)
        {
            throw new ForbiddenException("You can only cancel your own leave requests.");
        }

        // Rule: once approved/rejected, the employee shouldn't be able to modify that request.
        if (leave.Status != LeaveStatus.Pending)
        {
            throw new ConflictException("Only pending requests can be cancelled.");
        }

        _db.LeaveRequests.Remove(leave);
        await _db.SaveChangesAsync();
    }

    private void EnsureCanView(LeaveRequest leave, string role, int? employeeId)
    {
        if (role == Roles.Admin) return;
        if (role == Roles.Employee && leave.EmployeeId == employeeId) return;
        if (role == Roles.Manager && leave.Employee?.ManagerId == employeeId) return;

        throw new ForbiddenException("You are not permitted to view this leave request.");
    }

    private async Task<LeaveRequest> FindOrThrowAsync(int id)
    {
        var leave = await _db.LeaveRequests
            .Include(l => l.Employee)
                .ThenInclude(e => e!.Manager)
            .Include(l => l.ApprovedByUser)
            .FirstOrDefaultAsync(l => l.Id == id);

        return leave ?? throw new NotFoundException($"Leave request {id} was not found.");
    }

    private static LeaveRequestDto MapToDto(LeaveRequest l) => new()
    {
        Id = l.Id,
        EmployeeId = l.EmployeeId,
        EmployeeName = l.Employee is null ? null : $"{l.Employee.FirstName} {l.Employee.LastName}",
        LeaveType = l.LeaveType,
        StartDate = l.StartDate,
        EndDate = l.EndDate,
        DurationInDays = l.DurationInDays,
        Reason = l.Reason,
        Status = l.Status,
        AppliedDate = l.AppliedDate,
        ApprovedByName = l.ApprovedByUser?.Username,
        ApprovedDate = l.ApprovedDate
    };
}
