using EmployeeLeaveManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLeaveManagement.Data;

// Development-only convenience seeding: gives you one working account per role
// immediately after your first migration, so you can test login without
// hand-inserting rows. Not intended for production use.
public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db)
    {
        await db.Database.MigrateAsync();

        if (await db.Users.AnyAsync()) return; // already seeded

        var department = new Department { Name = "Engineering", Description = "Product engineering" };
        db.Departments.Add(department);
        await db.SaveChangesAsync();

        var manager = new Employee
        {
            FirstName = "Meera",
            LastName = "Manager",
            Email = "manager1@example.com",
            DateOfBirth = new DateTime(1988, 4, 12),
            DateOfJoining = new DateTime(2019, 6, 1),
            Designation = "Engineering Manager",
            Salary = 0, // set a real figure post-seed if it matters for your demo
            DepartmentId = department.Id,
            AnnualLeaveEntitlement = 24
        };
        db.Employees.Add(manager);
        await db.SaveChangesAsync();

        var employee = new Employee
        {
            FirstName = "Yathin",
            LastName = "Employee",
            Email = "employee1@example.com",
            DateOfBirth = new DateTime(1996, 9, 3),
            DateOfJoining = new DateTime(2022, 1, 10),
            Designation = "Software Engineer",
            Salary = 0,
            DepartmentId = department.Id,
            ManagerId = manager.Id,
            AnnualLeaveEntitlement = 20
        };
        db.Employees.Add(employee);
        await db.SaveChangesAsync();

        db.Users.AddRange(
            new User
            {
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Role = Roles.Admin
            },
            new User
            {
                Username = "manager1",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Manager@123"),
                Role = Roles.Manager,
                EmployeeId = manager.Id
            },
            new User
            {
                Username = "yathin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Employee@123"),
                Role = Roles.Employee,
                EmployeeId = employee.Id
            }
        );

        await db.SaveChangesAsync();
    }
}
