using Microsoft.EntityFrameworkCore;
using oedibud.Data;
using oedibud.Models;

namespace oedibud.Services;

public class UserSettingsService(IDbContextFactory<BudgetDbContext> factory)
{
    private const int SettingsId = 1;

    public async Task<UserSettings> LoadAsync()
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.UserSettings.FindAsync(SettingsId)
               ?? new UserSettings { Id = SettingsId };
    }

    public async Task SaveAsync(UserSettings settings)
    {
        await using var db = await factory.CreateDbContextAsync();
        var existing = await db.UserSettings.FindAsync(settings.Id);
        if (existing is null)
            db.UserSettings.Add(settings);
        else
        {
            existing.StartMonth = settings.StartMonth;
            existing.QuarterView = settings.QuarterView;
            existing.ExpandedEmployeeIds = settings.ExpandedEmployeeIds;
            existing.ExpandedProjectIds = settings.ExpandedProjectIds;
        }
        await db.SaveChangesAsync();
    }

    public static HashSet<int> DecodeIds(string raw) =>
        string.IsNullOrWhiteSpace(raw)
            ? []
            : raw.Split(',', StringSplitOptions.RemoveEmptyEntries)
                 .Select(int.Parse)
                 .ToHashSet();

    public static string EncodeIds(HashSet<int> ids) =>
        string.Join(',', ids);
}
