namespace oedibud.Models;

public class Employee
{
    private readonly int level;

    public int Id { get; set; }

    public string Name { get; set; } = "";

    public EmployeeGroup Group { get; set; } = EmployeeGroup.E13;

    public int Level
    {
        get
        {
            if (ExperienceMonth < 12) return 1;
            if (ExperienceMonth < 36) return 2;
            if (ExperienceMonth < 72) return 3;
            if (ExperienceMonth < 120) return 4;
            if (ExperienceMonth < 180) return 5;
            return 6;

        }
    }

    public DateTime HireDate { get; set; }

    public int ExperienceMonth { get; set; }
}
