namespace oedibud.Models;

public class PayScaleEntry
{
    public int Id { get; set; }

    public int Year { get; set; }

    public string PayGroup { get; set; } = "";

    public int Step { get; set; }

    public decimal MonthlySalary { get; set; }
}
