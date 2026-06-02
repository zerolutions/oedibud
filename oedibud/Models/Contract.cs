using System;
using System.ComponentModel.DataAnnotations.Schema;
using oedibud.Models;
using oedibud.Services;

namespace oedibud.Models;

public class Contract
{
    public int Id { get; set; }

    private static readonly int[] LevelThresholds = { 0, 12, 36, 72, 120, 180 }; // month thresholds for levels 1..6
    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public decimal Fte { get; set; }
    public EmployeeGroup Group { get; set; } = EmployeeGroup.E13;
    public int ExperienceMonth { get; set; }
    public decimal EmployerBruttoAddition { get; set; }
    public decimal AnualPaymentAddition { get; set; }
    [NotMapped]
    public decimal FtePercent
    {
        get => Fte * 100;
        set => Fte = value / 100;
    }

    // Navigation: payments assigned to this contract (many-to-many via ContractPayment)
    public List<ContractPayment> ContractPayments { get; set; } = new();

    public int Level {get; set;}
    // public int Level
    // {
    //     get
    //     {
    //         var today = DateTime.Today;
    //         int monthsSinceHire = (today.Year - HireDate.Year) * 12 + (today.Month - HireDate.Month) + ExperienceMonth;

    //         // find highest level whose threshold is <= monthsSinceHire
    //         for (int lvl = LevelThresholds.Length; lvl >= 1; lvl--)
    //         {
    //             int threshold = LevelThresholds[Math.Max(0, lvl - 1)];
    //             if (monthsSinceHire >= threshold) return Math.Min(lvl, LevelThresholds.Length);
    //         }
    //         return 1;
    //     }
    // }

    // navigation: (PaymentAssignment removed - direct employee assignments are not supported)

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
                return Start; // next level is effective immediately at contract start

            var olddate = Start.AddMonths(monthsToNextFromHire);

            return new DateTime(olddate.Year, olddate.Month, 1,  0, 0, 0, olddate.Kind); ;
        }
    }

    public int GetLevelAt(DateTime date)
    {
        int monthsSinceHire = (date.Year - this.Start.Year) * 12 + (date.Month - this.Start.Month) + this.ExperienceMonth;

        for (int lvl = LevelThresholds.Length; lvl >= 1; lvl--)
        {
            int threshold = LevelThresholds[Math.Max(0, lvl - 1)];
            if (monthsSinceHire >= threshold) return Math.Min(lvl, LevelThresholds.Length);
        }
        return 1;
    }

}

