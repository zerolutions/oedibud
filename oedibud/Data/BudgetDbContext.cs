using Microsoft.EntityFrameworkCore;
using oedibud.Models;

namespace oedibud.Data;

public class BudgetDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Employee> Employees => Set<Employee>();
}
