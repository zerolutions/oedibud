using Microsoft.EntityFrameworkCore;
using oedibud.Data;
using oedibud.Models;
using System.Globalization;
using System.Text;

namespace oedibud.Services;

public record ImportResult(int Added, int Updated, int Skipped, List<string> Errors);

public class CsvImportService
{
    private readonly IDbContextFactory<BudgetDbContext> _dbFactory;

    public CsvImportService(IDbContextFactory<BudgetDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    // Auto-detect delimiter: pick ';' if it appears more often than ','
    private static char DetectDelimiter(string firstLine)
    {
        int commas = firstLine.Count(c => c == ',');
        int semicolons = firstLine.Count(c => c == ';');
        return semicolons >= commas ? ';' : ',';
    }

    private static (string[] headers, List<string[]> rows) ParseCsv(string csvContent)
    {
        var lines = csvContent.ReplaceLineEndings("\n")
                              .Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length == 0) return ([], []);

        char delimiter = DetectDelimiter(lines[0]);
        var headers = ParseCsvLine(lines[0], delimiter);
        var rows = lines.Skip(1)
                        .Select(l => ParseCsvLine(l, delimiter))
                        .Where(r => r.Any(f => !string.IsNullOrWhiteSpace(f)))
                        .ToList();
        return (headers, rows);
    }

    private static string[] ParseCsvLine(string line, char delimiter)
    {
        var fields = new List<string>();
        bool inQuotes = false;
        var current = new StringBuilder();

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == delimiter && !inQuotes)
            {
                fields.Add(current.ToString().Trim());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }
        fields.Add(current.ToString().Trim());
        return [.. fields];
    }

    private static string? GetField(string[] headers, string[] row, string columnName)
    {
        var idx = Array.FindIndex(headers, h => h.Trim().Equals(columnName, StringComparison.OrdinalIgnoreCase));
        if (idx < 0 || idx >= row.Length) return null;
        var val = row[idx];
        return string.IsNullOrWhiteSpace(val) ? null : val;
    }

    // -------------------------------------------------------------------------
    // Employees
    // -------------------------------------------------------------------------
    public async Task<ImportResult> ImportEmployeesAsync(string csvContent)
    {
        var (headers, rows) = ParseCsv(csvContent);
        int added = 0, updated = 0, skipped = 0;
        var errors = new List<string>();

        await using var db = await _dbFactory.CreateDbContextAsync();

        for (int i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            try
            {
                var name = GetField(headers, row, "Name") ?? "";
                var hireDateStr = GetField(headers, row, "HireDate");
                var hireDate = TryParseDate(hireDateStr) ?? DateTime.Today;

                var idStr = GetField(headers, row, "Id");
                if (int.TryParse(idStr, out int id) && id > 0)
                {
                    var existing = await db.Employees.FindAsync(id);
                    if (existing != null)
                    {
                        existing.Name = name;
                        existing.HireDate = hireDate;
                        updated++;
                    }
                    else
                    {
                        db.Employees.Add(new Employee { Id = id, Name = name, HireDate = hireDate });
                        added++;
                    }
                }
                else
                {
                    db.Employees.Add(new Employee { Name = name, HireDate = hireDate });
                    added++;
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Zeile {i + 2}: {ex.Message}");
                skipped++;
            }
        }

        await db.SaveChangesAsync();
        return new ImportResult(added, updated, skipped, errors);
    }

    // -------------------------------------------------------------------------
    // Projects
    // -------------------------------------------------------------------------
    public async Task<ImportResult> ImportProjectsAsync(string csvContent)
    {
        var (headers, rows) = ParseCsv(csvContent);
        int added = 0, updated = 0, skipped = 0;
        var errors = new List<string>();

        await using var db = await _dbFactory.CreateDbContextAsync();

        for (int i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            try
            {
                var title = GetField(headers, row, "Title") ?? "";
                var start = TryParseDate(GetField(headers, row, "Start")) ?? DateTime.Today;
                var end = TryParseDate(GetField(headers, row, "End")) ?? DateTime.Today.AddYears(1);

                var idStr = GetField(headers, row, "Id");
                if (int.TryParse(idStr, out int id) && id > 0)
                {
                    var existing = await db.Projects.FindAsync(id);
                    if (existing != null)
                    {
                        existing.Title = title;
                        existing.Start = start;
                        existing.End = end;
                        updated++;
                    }
                    else
                    {
                        db.Projects.Add(new Project { Id = id, Title = title, Start = start, End = end });
                        added++;
                    }
                }
                else
                {
                    db.Projects.Add(new Project { Title = title, Start = start, End = end });
                    added++;
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Zeile {i + 2}: {ex.Message}");
                skipped++;
            }
        }

        await db.SaveChangesAsync();
        return new ImportResult(added, updated, skipped, errors);
    }

    // -------------------------------------------------------------------------
    // Payments
    // -------------------------------------------------------------------------
    public async Task<ImportResult> ImportPaymentsAsync(string csvContent)
    {
        var (headers, rows) = ParseCsv(csvContent);
        int added = 0, updated = 0, skipped = 0;
        var errors = new List<string>();

        await using var db = await _dbFactory.CreateDbContextAsync();

        for (int i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            try
            {
                var title = GetField(headers, row, "Title") ?? "";
                var start = TryParseDate(GetField(headers, row, "Start")) ?? DateTime.Today;
                var end = TryParseDate(GetField(headers, row, "End")) ?? DateTime.Today;
                var projectId = int.TryParse(GetField(headers, row, "ProjectId"), out int pid) ? pid : 0;
                var amount = TryParseDecimal(GetField(headers, row, "Amount")) ?? 0m;
                var contractsBound = TryParseBool(GetField(headers, row, "AmountIsContractsBound")) ?? false;
                EmployeeGroup? dedicatedTo = null;
                var dedicatedStr = GetField(headers, row, "DetecatedTo");
                if (dedicatedStr != null && Enum.TryParse<EmployeeGroup>(dedicatedStr, true, out var eg))
                    dedicatedTo = eg;

                var idStr = GetField(headers, row, "Id");
                if (int.TryParse(idStr, out int id) && id > 0)
                {
                    var existing = await db.Payments.FindAsync(id);
                    if (existing != null)
                    {
                        existing.Title = title;
                        existing.Start = start;
                        existing.End = end;
                        existing.ProjectId = projectId;
                        existing.Amount = amount;
                        existing.AmountIsContractsBound = contractsBound;
                        existing.DetecatedTo = dedicatedTo;
                        updated++;
                    }
                    else
                    {
                        db.Payments.Add(new Payment
                        {
                            Id = id, Title = title, Start = start, End = end,
                            ProjectId = projectId, Amount = amount, AmountIsContractsBound = contractsBound, DetecatedTo = dedicatedTo
                        });
                        added++;
                    }
                }
                else
                {
                    db.Payments.Add(new Payment
                    {
                        Title = title, Start = start, End = end,
                        ProjectId = projectId, Amount = amount, AmountIsContractsBound = contractsBound, DetecatedTo = dedicatedTo
                    });
                    added++;
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Zeile {i + 2}: {ex.Message}");
                skipped++;
            }
        }

        await db.SaveChangesAsync();
        return new ImportResult(added, updated, skipped, errors);
    }

    // -------------------------------------------------------------------------
    // Contracts
    // -------------------------------------------------------------------------
    public async Task<ImportResult> ImportContractsAsync(string csvContent)
    {
        var (headers, rows) = ParseCsv(csvContent);
        int added = 0, updated = 0, skipped = 0;
        var errors = new List<string>();

        await using var db = await _dbFactory.CreateDbContextAsync();

        for (int i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            try
            {
                var employeeId = int.TryParse(GetField(headers, row, "EmployeeId"), out int eid) ? eid : 0;
                var start = TryParseDate(GetField(headers, row, "Start")) ?? DateTime.Today;
                var end = TryParseDate(GetField(headers, row, "End")) ?? DateTime.Today.AddYears(1);
                var fte = TryParseDecimal(GetField(headers, row, "Fte")) ?? 1m;
                var group = Enum.TryParse<EmployeeGroup>(GetField(headers, row, "Group"), true, out var g) ? g : EmployeeGroup.E13;
                var expMonth = int.TryParse(GetField(headers, row, "ExperienceMonth"), out int em) ? em : 0;
                var employerBrutto = TryParseDecimal(GetField(headers, row, "EmployerBruttoAddition")) ?? 0m;
                var anualPayment = TryParseDecimal(GetField(headers, row, "AnualPaymentAddition")) ?? 0m;
                var level = int.TryParse(GetField(headers, row, "Level"), out int lv) ? lv : 1;

                var idStr = GetField(headers, row, "Id");
                if (int.TryParse(idStr, out int id) && id > 0)
                {
                    var existing = await db.Contracts.FindAsync(id);
                    if (existing != null)
                    {
                        existing.EmployeeId = employeeId;
                        existing.Start = start;
                        existing.End = end;
                        existing.Fte = fte;
                        existing.Group = group;
                        existing.ExperienceMonth = expMonth;
                        existing.EmployerBruttoAddition = employerBrutto;
                        existing.AnualPaymentAddition = anualPayment;
                        existing.Level = level;
                        updated++;
                    }
                    else
                    {
                        db.Contracts.Add(new Contract
                        {
                            Id = id, EmployeeId = employeeId, Start = start, End = end,
                            Fte = fte, Group = group, ExperienceMonth = expMonth,
                            EmployerBruttoAddition = employerBrutto, AnualPaymentAddition = anualPayment, Level = level
                        });
                        added++;
                    }
                }
                else
                {
                    db.Contracts.Add(new Contract
                    {
                        EmployeeId = employeeId, Start = start, End = end,
                        Fte = fte, Group = group, ExperienceMonth = expMonth,
                        EmployerBruttoAddition = employerBrutto, AnualPaymentAddition = anualPayment, Level = level
                    });
                    added++;
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Zeile {i + 2}: {ex.Message}");
                skipped++;
            }
        }

        await db.SaveChangesAsync();
        return new ImportResult(added, updated, skipped, errors);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------
    private static DateTime? TryParseDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
            return d;
        if (DateTime.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.None, out var d2))
            return d2;
        return null;
    }

    private static decimal? TryParseDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        // Support both '.' and ',' as decimal separator
        var normalized = value.Replace(',', '.');
        if (decimal.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
            return d;
        return null;
    }

    private static bool? TryParseBool(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        if (bool.TryParse(value, out var b))
            return b;
        return null;
    }
}
