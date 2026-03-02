using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Contracts;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Repositories;

/// <summary>
/// SQLite repository for persisting and retrieving application settings.
/// </summary>
public sealed class SqliteSettingsRepository(string? databasePath = null)
{
    /// <summary>
    /// Saves the application settings to the database.
    /// </summary>
    /// <param name="state">The settings state to save.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task SaveAsync(SettingsState state, CancellationToken cancellationToken = default)
    {
        DateTime now = DateTime.UtcNow;

        await using AstarOneDriveDbContextModel context = AstarOneDriveDbContextFactory.Create(databasePath);
        await UpsertSettingAsync(context, "SelectedTheme", state.SelectedTheme, now, cancellationToken);
        await UpsertSettingAsync(context, "SelectedLanguage", state.SelectedLanguage, now, cancellationToken);
        await UpsertSettingAsync(context, "SelectedLayout", state.SelectedLayout, now, cancellationToken);
        await UpsertSettingAsync(context, "UserName", state.UserName, now, cancellationToken);
        _ = await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Loads the application settings from the database asynchronously.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The settings state, or null if no settings exist.</returns>
    public async Task<SettingsState?> LoadAsync(CancellationToken cancellationToken = default)
    {
        await using AstarOneDriveDbContextModel context = AstarOneDriveDbContextFactory.Create(databasePath);
        List<SettingEntity> entries = await context.Settings.AsNoTracking().ToListAsync(cancellationToken);
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

    /// <summary>
    /// Loads the application settings from the database synchronously.
    /// </summary>
    /// <returns>The settings state, or null if no settings exist.</returns>
    public SettingsState? Load()
    {
        using AstarOneDriveDbContextModel context = AstarOneDriveDbContextFactory.Create(databasePath);
        var entries = context.Settings.AsNoTracking().ToList();
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

    private static async Task UpsertSettingAsync(AstarOneDriveDbContextModel context, string key, string value, DateTime updatedUtc, CancellationToken cancellationToken)
    {
        SettingEntity? setting = await context.Settings.SingleOrDefaultAsync(x => x.Key == key, cancellationToken);
        if (setting is null)
        {
            _ = context.Settings.Add(new SettingEntity
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
