using System.Globalization;
using System.Reflection;

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

            var parts = line.Split(';');

            var group = parts[0].Trim();

            var dict = new Dictionary<int, decimal>();

            for (int i = 1; i < parts.Length; i++)
            {
                if (decimal.TryParse(
                    parts[i],
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out var salary))
                {
                    dict[i] = salary;
                }
            }

            _data[group] = dict;
        }
    }

    public decimal GetSalary(string group, int level)
    {
        if (_data.TryGetValue(group, out var levels))
        {
            if (levels.TryGetValue(level, out var salary))
                return salary;
        }

        return 0;
    }
}
