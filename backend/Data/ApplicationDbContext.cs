using EmployeeLeaveManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace EmployeeLeaveManagement.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();

    // PostgreSQL's timestamptz columns reject any DateTime that isn't explicitly
    // tagged Kind=Utc — SQL Server never cared about Kind at all, so this wasn't
    // an issue before. Rather than fixing every individual place a date gets
    // created (DbSeeder, incoming DTOs, etc. — easy to miss one), this applies
    // a single conversion to every DateTime/DateTime? property in the whole
    // model, automatically, including any added in the future.
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
        configurationBuilder.Properties<DateTime?>().HaveConversion<UtcNullableDateTimeConverter>();
    }

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

// Normalizes any DateTime to Kind=Utc on write (so Postgres accepts it) and
// tags it Utc on read (Npgsql already returns UTC from timestamptz columns —
// this just makes that explicit rather than assumed). The actual date/time
// value is never altered, only the Kind tag — since every DateTime this app
// stores is either DateTime.UtcNow already, or a calendar date (birthdate,
// leave dates) where time-of-day and timezone are never meaningfully used.
public class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
{
    public UtcDateTimeConverter() : base(
        v => v.Kind == DateTimeKind.Utc ? v : DateTime.SpecifyKind(v, DateTimeKind.Utc),
        v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
    {
    }
}

public class UtcNullableDateTimeConverter : ValueConverter<DateTime?, DateTime?>
{
    public UtcNullableDateTimeConverter() : base(
        v => v.HasValue && v.Value.Kind != DateTimeKind.Utc ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v,
        v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v)
    {
    }
}
