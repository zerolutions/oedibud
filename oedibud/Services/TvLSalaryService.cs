using oedibud.Models;
using System.Globalization;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace oedibud.Services;

public class TvLSalaryService
{
    private readonly Dictionary<string, Dictionary<int, decimal>> _data = new();

    public TvLSalaryService()
    {
        LoadLatestCsv();
    }

    private void LoadLatestCsv()
    {
        var assembly = Assembly.GetExecutingAssembly();

        var resourceNames = assembly.GetManifestResourceNames()
            .Where(n => n.Contains("Entgelt_"))
            .OrderByDescending(n => n)
            .ToList();

        var latest = resourceNames.First();

        using var stream = assembly.GetManifestResourceStream(latest);
        using var reader = new StreamReader(stream);

        // Header überspringen
        reader.ReadLine();

        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            if (string.IsNullOrWhiteSpace(line)) continue;

            var parts = line.Split(';');
            if (parts.Length < 2) continue;

            var group = parts[0].Trim();

            var dict = new Dictionary<int, decimal>();

            for (int i = 1; i < parts.Length; i++)
            {
                var token = parts[i].Trim();
                if (string.IsNullOrEmpty(token)) continue;

                // try several parse strategies to tolerate comma as decimal separator
                if (!decimal.TryParse(token, NumberStyles.Any, CultureInfo.InvariantCulture, out var salary))
                {
                    if (!decimal.TryParse(token, NumberStyles.Any, CultureInfo.GetCultureInfo("de-DE"), out salary))
                    {
                        // fallback: replace comma with dot
                        var alt = token.Replace(',', '.');
                        decimal.TryParse(alt, NumberStyles.Any, CultureInfo.InvariantCulture, out salary);
                    }
                }

                if (salary != 0)
                {
                    dict[i] = salary;
                }
            }

            // store with normalized key (remove spaces, to upper) to make lookup robust
            _data[NormalizeKey(group)] = dict;
        }
    }

    private static string NormalizeKey(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return string.Empty;
        // remove spaces and control chars, normalize to uppercase
        var chars = s.Where(c => !char.IsWhiteSpace(c)).ToArray();
        return new string(chars).ToUpperInvariant();
    }

    public decimal GetSalary(EmployeeGroup group, int level)
    {
        // Resolve the DisplayAttribute (if present) from the enum member:
        var member = typeof(EmployeeGroup).GetMember(group.ToString()).FirstOrDefault();
        var displayName = member?.GetCustomAttribute<DisplayAttribute>()?.Name ?? group.ToString();

        // try normalized lookup first
        var key = NormalizeKey(displayName);
        if (_data.TryGetValue(key, out var levels))
        {
            if (levels.TryGetValue(level, out var salary))
                return salary;
        }

        // fallback: try original displayName as-is
        if (_data.TryGetValue(displayName, out levels))
        {
            if (levels.TryGetValue(level, out var salary))
                return salary;
        }

        return 0;
    }
}
