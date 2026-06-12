using oedibud.Models;
using System.Globalization;
using System.Text;

namespace oedibud.Services;

public class CsvExportService
{
    private const char Delimiter = ';';

    private static string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        if (value.Contains(Delimiter) || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }

    public string ExportEmployees(IEnumerable<Employee> employees)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Id;Name;HireDate");
        foreach (var e in employees)
            sb.AppendLine($"{e.Id};{Escape(e.Name)};{e.HireDate:yyyy-MM-dd}");
        return sb.ToString();
    }

    public string ExportProjects(IEnumerable<Project> projects)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Id;Title;Start;End");
        foreach (var p in projects)
            sb.AppendLine($"{p.Id};{Escape(p.Title)};{p.Start:yyyy-MM-dd};{p.End:yyyy-MM-dd}");
        return sb.ToString();
    }

    public string ExportPayments(IEnumerable<Payment> payments)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Id;Title;Start;End;ProjectId;Amount;AmountIsContractsBound;DetecatedTo");
        foreach (var p in payments)
            sb.AppendLine(
                $"{p.Id};{Escape(p.Title)};{p.Start:yyyy-MM-dd};{p.End:yyyy-MM-dd}" +
                $";{p.ProjectId};{p.Amount.ToString(CultureInfo.InvariantCulture)}" +
                $";{p.AmountIsContractsBound};{p.DetecatedTo?.ToString() ?? ""}");
        return sb.ToString();
    }

    public string ExportContracts(IEnumerable<Contract> contracts)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Id;EmployeeId;Start;End;Fte;Group;ExperienceMonth;EmployerBruttoAddition;AnualPaymentAddition;Level");
        foreach (var c in contracts)
            sb.AppendLine(
                $"{c.Id};{c.EmployeeId};{c.Start:yyyy-MM-dd};{c.End:yyyy-MM-dd}" +
                $";{c.Fte.ToString(CultureInfo.InvariantCulture)};{c.Group}" +
                $";{c.ExperienceMonth};{c.EmployerBruttoAddition.ToString(CultureInfo.InvariantCulture)}" +
                $";{c.AnualPaymentAddition.ToString(CultureInfo.InvariantCulture)};{c.Level}");
        return sb.ToString();
    }
}
