# Employee Leave & Management System — Backend

ASP.NET Core 8 Web API + EF Core + SQL Server, matching the project blueprint.

## Setup

1. Install the .NET 8 SDK if you don't have it: https://dotnet.microsoft.com/download
2. Update `appsettings.json`:
   - `ConnectionStrings:DefaultConnection` — point at your local SQL Server instance.
   - `Jwt:Key` — replace with a real random 32+ char secret before you rely on this for anything beyond local dev.
3. Restore and create the database:
   ```bash
   dotnet restore
   dotnet tool install --global dotnet-ef   # if you don't already have it
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```
4. Run it:
   ```bash
   dotnet run
   ```
   Swagger UI opens at `https://localhost:<port>/swagger` in Development.

On first run in Development, `DbSeeder` creates one account per role so you can log in immediately:

| Username  | Password      | Role     |
|-----------|---------------|----------|
| admin     | Admin@123     | Admin    |
| manager1  | Manager@123   | Manager  |
| yathin    | Employee@123  | Employee (reports to manager1) |

Delete or gate `DbSeeder` before this ever touches a non-local environment.

## Design decisions made while scaffolding

These were open questions in the original plan; here's what got picked and why:

- **`Employee.ManagerId`** — self-referencing FK, rather than inferring "manager" from department. Lets `GET /api/employees/me/team` return direct reports regardless of department, and keeps the door open for a manager whose team spans departments.
- **Leave balance** — `Employee.AnnualLeaveEntitlement` (int) is the only stored field. Used/remaining are computed on read as `Sum(DurationInDays)` over `Approved` requests, so balances can't drift out of sync with the actual request history.
- **`LeaveRequest.ApprovedByUserId`** — FK to `Users`, not a free-text name. `LeaveRequestDto.ApprovedByName` is derived from it.
- **Salary exposure** — `EmployeeDto` (used for team views and self-profile) has no `Salary` field. `AdminEmployeeDto` extends it with `Salary` and is only ever returned from Admin-only endpoints.
- **Manager's own leave** — a Manager has no manager to approve their request, so `LeaveService` lets `Admin` approve/reject *any* pending request, which covers this case without special-casing it.
- **Password hashing** — BCrypt.Net-Next (`BCrypt.HashPassword` / `BCrypt.Verify`), the standard lightweight choice outside full ASP.NET Identity.
- **CORS** — pre-wired for `http://localhost:4200` (default `ng serve` port) via `appsettings.json:Cors:AllowedOrigins`, so this isn't a debugging session on day 3.
- **Overlap rule** — `existing.StartDate <= new.EndDate && existing.EndDate >= new.StartDate`, scoped to the same employee, excluding already-rejected requests.

## What's implemented

- JWT auth (`POST /api/auth/login`), role claims, `[Authorize(Roles = ...)]` on every protected endpoint.
- Full CRUD for Employees and Departments (Admin-gated), plus self-service `GET/PUT /api/employees/me`.
- `GET /api/employees/me/team` for Managers.
- Leave apply / list (role-scoped) / get-by-id / approve / reject / cancel, with all six business rules from the blueprint enforced in `LeaveService`.
- Centralized exception handling (`ExceptionMiddleware` + `ApiException` hierarchy) mapping to proper status codes (400/401/403/404/409/500).
- Swagger with JWT bearer auth wired in, so you can authorize and hit every endpoint directly from the browser.

## What's not built (by design, per the blueprint's scope)

Dashboards are just aggregate queries over existing data (employee count, pending/approved/rejected counts) — deliberately left for the Angular side to call and compose, rather than baking bespoke dashboard endpoints into the API. Search/filter/pagination/sorting on the employee list are the "optional, after core works" items from the plan and aren't in yet.

## Next step

Angular frontend: `auth.service.ts` + `auth.guard.ts` + `auth.interceptor.ts` first (they gate everything else), then the employee/department/leave feature modules against these APIs.
