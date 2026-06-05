using System;
using System.ComponentModel.DataAnnotations.Schema;
using oedibud.Models;
using oedibud.Services;

namespace oedibud.Models;

public class Contract
{
    public int Id { get; set; }

    private static readonly int[] LevelThresholds = { 0, 12, 36, 72, 120, 180 }; // month thresholds for levels 1..6
    [NotMapped]
    private EmployeeGroup group = EmployeeGroup.E13;

    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public decimal Fte { get; set; }
    public EmployeeGroup Group { get => group; set {
        group = value; 
        AnualPaymentAddition = GetJahressonderzahlungFactor(); 
        }
    }
    public int ExperienceMonth { get; set; }
    public decimal EmployerBruttoAddition { get; set; }
    public decimal AnualPaymentAddition { get; set; }
    [NotMapped]
    public decimal FtePercent
    {
        get => decimal.Floor(Fte * 10000) / 100; // round down to 2 decimals
        set => Fte = value / 100;
    }
    [NotMapped]
    public decimal EmployerBruttoAdditionPercent
    {
        get => decimal.Floor(EmployerBruttoAddition * 10000) / 100; // round down to 2 decimals
        set => EmployerBruttoAddition = value / 100;
    }
    [NotMapped]
    public decimal AnualPaymentAdditionPercent
    {
        get => decimal.Floor(AnualPaymentAddition * 10000) / 100; // round down to 2 decimals
        set => AnualPaymentAddition = value / 100;
    }

    // Navigation: payments assigned to this contract (many-to-many via ContractPayment)
    public List<ContractPayment> ContractPayments { get; set; } = new();

    public int Level {get; set;}

    public decimal GetJahressonderzahlungFactor()
    {
        var groupStr = Group.ToString();
        // stark vereinfacht – ggf. anpassen!
        if (groupStr.StartsWith("E14") || groupStr.StartsWith("E15"))
            return 0.3253m;

        if (groupStr.StartsWith("E12") || groupStr.StartsWith("E13"))
            return 0.4647m;

        if (groupStr.StartsWith("E9") || groupStr.StartsWith("E10") || groupStr.StartsWith("E11"))
            return 0.7435m;

        if (groupStr.StartsWith("E5") || groupStr.StartsWith("E6") || groupStr.StartsWith("E7") || groupStr.StartsWith("E8"))
            return 0.8814m;

        return 0.8743m; // E1–E4
    }

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

