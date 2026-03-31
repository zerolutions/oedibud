using Microsoft.EntityFrameworkCore;
using oedibud.Models;

namespace oedibud.Data;

public class BudgetDbContext : DbContext
{
    public BudgetDbContext(DbContextOptions<BudgetDbContext> options)
        : base(options)
    {
    }

    public DbSet<Employee> Employees { get; set; } = null!;
    // inside your existing BudgetDbContext class:
    public DbSet<Project> Projects { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Contract> Contracts { get; set; }
}
