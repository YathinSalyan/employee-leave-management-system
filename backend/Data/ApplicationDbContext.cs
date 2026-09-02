using EmployeeLeaveManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLeaveManagement.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- Users ---
        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.Username).IsUnique();
            e.Property(u => u.Role).HasMaxLength(20);

            e.HasOne(u => u.Employee)
                .WithMany()
                .HasForeignKey(u => u.EmployeeId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // --- Departments -> Employees (one-to-many) ---
        modelBuilder.Entity<Department>(e =>
        {
            e.HasIndex(d => d.Name).IsUnique();

            e.HasMany(d => d.Employees)
                .WithOne(emp => emp.Department)
                .HasForeignKey(emp => emp.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict); // don't let a department delete wipe out employees
        });

        // --- Employees ---
        modelBuilder.Entity<Employee>(e =>
        {
            e.HasIndex(emp => emp.Email).IsUnique();
            e.Property(emp => emp.Salary).HasColumnType("decimal(12,2)");

            // Self-referencing Manager relationship. Restrict delete so removing a
            // manager doesn't cascade-delete (or silently orphan) their whole team.
            e.HasOne(emp => emp.Manager)
                .WithMany(emp => emp.DirectReports)
                .HasForeignKey(emp => emp.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // --- Employee -> LeaveRequests (one-to-many) ---
        modelBuilder.Entity<LeaveRequest>(e =>
        {
            e.HasOne(l => l.Employee)
                .WithMany(emp => emp.LeaveRequests)
                .HasForeignKey(l => l.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade); // deleting an employee record clears their leave history

            e.HasOne(l => l.ApprovedByUser)
                .WithMany()
                .HasForeignKey(l => l.ApprovedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            e.Property(l => l.LeaveType).HasMaxLength(50);
            e.Property(l => l.Status).HasConversion<string>().HasMaxLength(20);
        });
    }
}
