using System.Security.Claims;

namespace EmployeeLeaveManagement.Common;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return value is null ? throw new UnauthorizedAccessException() : int.Parse(value);
    }

    public static string GetRole(this ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimTypes.Role) ?? throw new UnauthorizedAccessException();
    }

    // The Employee record linked to the logged-in user. Null for accounts
    // (typically Admin) that have no linked Employee profile.
    public static int? GetEmployeeId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue("employeeId");
        return value is null ? null : int.Parse(value);
    }
}
