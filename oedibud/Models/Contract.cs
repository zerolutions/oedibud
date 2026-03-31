using System;

namespace oedibud.Models;

public class Contract
{
    public int Id { get; set; }

    // FK to Employee
    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public float Fte { get; set; }

    //public decimal GetSalleryAtSpecificMonth(int year, int month)
    //{
    //    var date = new DateTime(year, month, 1);
    //    var levelAtDate = CalculateLevelAtDate(date);
    //    var sal = new TvLSalaryService();
    //    Console.WriteLine(levelAtDate);
    //    return sal.GetSalary(Group, levelAtDate);
    //}
    //private int CalculateLevelAtDate(DateTime date)
    //{
    //    int monthsSinceHire = (date.Year - HireDate.Year) * 12 + (date.Month - HireDate.Month) + ExperienceMonth;
    //    for (int lvl = LevelThresholds.Length; lvl >= 1; lvl--)
    //    {
    //        int threshold = LevelThresholds[Math.Max(0, lvl - 1)];
    //        if (monthsSinceHire >= threshold) return Math.Min(lvl, LevelThresholds.Length);
    //    }
    //    return 1;
    //}
}

