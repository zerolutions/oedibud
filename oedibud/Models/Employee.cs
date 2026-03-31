using oedibud.Models;
using oedibud.Services;

public class Employee
{
    private static readonly int[] LevelThresholds = { 0, 12, 36, 72, 120, 180 }; // month thresholds for levels 1..6

    public int Id { get; set; }
    public string Name { get; set; } = "";
    public EmployeeGroup Group { get; set; } = EmployeeGroup.E13;
    public DateTime HireDate { get; set; }
    public int ExperienceMonth { get; set; }

    public int Level
    {
        get
        {
            var today = DateTime.Today;
            int monthsSinceHire = (today.Year - HireDate.Year) * 12 + (today.Month - HireDate.Month) + ExperienceMonth;

            // find highest level whose threshold is <= monthsSinceHire
            for (int lvl = LevelThresholds.Length; lvl >= 1; lvl--)
            {
                int threshold = LevelThresholds[Math.Max(0, lvl - 1)];
                if (monthsSinceHire >= threshold) return Math.Min(lvl, LevelThresholds.Length);
            }
            return 1;
        }
    }

    // navigation: payments assigned directly to this employee
    public List<PaymentAssignment> PaymentAssignments { get; set; } = new();

    public DateTime NextLevel
    {
        get
        {
            int current = Level;
            if (current >= LevelThresholds.Length) // already max level
                return DateTime.MaxValue;

            int nextThreshold = LevelThresholds[current]; // next level threshold in months
            // months from HireDate to reach next threshold, accounting for prior experience
            int monthsToNextFromHire = nextThreshold - ExperienceMonth;
            if (monthsToNextFromHire <= 0) // already reached by prior experience
                return HireDate;

            var olddate = HireDate.AddMonths(monthsToNextFromHire);

            return new DateTime(olddate.Year, olddate.Month, 1,  0, 0, 0, olddate.Kind); ;
        }
    }


}