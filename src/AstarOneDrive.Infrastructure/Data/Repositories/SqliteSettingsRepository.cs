using AstarOneDrive.Infrastructure.Data.Contracts;
using AstarOneDrive.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AstarOneDrive.Infrastructure.Data.Repositories;

public sealed class SqliteSettingsRepository(string? databasePath = null)
{
    public async Task SaveAsync(SettingsState state, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        await using var context = AstarOneDriveDbContextFactory.Create(databasePath);
        await UpsertSettingAsync(context, "SelectedTheme", state.SelectedTheme, now, cancellationToken);
        await UpsertSettingAsync(context, "SelectedLanguage", state.SelectedLanguage, now, cancellationToken);
        await UpsertSettingAsync(context, "SelectedLayout", state.SelectedLayout, now, cancellationToken);
        await UpsertSettingAsync(context, "UserName", state.UserName, now, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<SettingsState?> LoadAsync(CancellationToken cancellationToken = default)
    {
        await using var context = AstarOneDriveDbContextFactory.Create(databasePath);
        var entries = await context.Settings.AsNoTracking().ToListAsync(cancellationToken);
        if (entries.Count == 0)
        {
            return null;
        }

        var values = entries.ToDictionary(x => x.Key, x => x.Value, StringComparer.Ordinal);
        return new SettingsState(
            values.GetValueOrDefault("SelectedTheme", "Light"),
            values.GetValueOrDefault("SelectedLanguage", "en-GB"),
            values.GetValueOrDefault("SelectedLayout", "Explorer"),
            values.GetValueOrDefault("UserName", "User"));
    }

    private static async Task UpsertSettingAsync(
        AstarOneDriveDbContext context,
        string key,
        string value,
        DateTime updatedUtc,
        CancellationToken cancellationToken)
    {
        var setting = await context.Settings.SingleOrDefaultAsync(x => x.Key == key, cancellationToken);
        if (setting is null)
        {
            context.Settings.Add(new SettingEntity
            {
                Id = Guid.NewGuid().ToString(),
                Key = key,
                Value = value,
                UpdatedUtc = updatedUtc
            });
            return;
        }

        setting.Value = value;
        setting.UpdatedUtc = updatedUtc;
    }
}
