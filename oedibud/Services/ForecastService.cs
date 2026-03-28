using oedibud.Data;
using oedibud.Models;
using Microsoft.EntityFrameworkCore;

namespace oedibud.Services;

public class ForecastService
{
    private readonly BudgetDbContext _db;
    private readonly TvLSalaryService _salary;

    public ForecastService(BudgetDbContext db, TvLSalaryService salary)
    {
        _db = db;
        _salary = salary;
    }

    public async Task<List<MonthlyCost>> GetForecast(int months = 24)
    {
        var employees = await _db.Employees.ToListAsync();

        var result = new List<MonthlyCost>();

        var current = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

        for (int i = 0; i < months; i++)
        {
            var month = current.AddMonths(i);

            decimal total = 0;

            foreach (var emp in employees)
            {
                var level = CalculateLevel(emp, month);

                var salary = _salary.GetSalary(emp.Group, level);

                total += salary;
            }

            result.Add(new MonthlyCost
            {
                Month = month,
                Cost = total
            });
        }

        return result;
    }

    private int CalculateLevel(Employee emp, DateTime date)
    {
        var years = (date - emp.HireDate).TotalDays / 365;

        if (years < 1) return 1;
        if (years < 3) return 2;
        if (years < 6) return 3;
        if (years < 10) return 4;
        if (years < 15) return 5;

        return 6;
    }
}
