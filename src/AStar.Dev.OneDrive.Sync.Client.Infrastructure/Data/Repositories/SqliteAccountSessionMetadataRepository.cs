using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Contracts;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Repositories;

/// <summary>
/// Persists account session metadata in SQLite settings entries.
/// </summary>
public sealed class SqliteAccountSessionMetadataRepository(string? databasePath = null)
{
    private readonly SqliteDatabaseMigrator _migrator = new(databasePath);

    /// <summary>
    /// Saves account session metadata.
    /// </summary>
    /// <param name="state">The session metadata state.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task SaveAsync(AccountSessionMetadataState state, CancellationToken cancellationToken = default)
    {
        await _migrator.EnsureMigratedAsync(cancellationToken);
        await using AstarOneDriveDbContextModel context = AstarOneDriveDbContextFactory.Create(databasePath);
        DateTime now = DateTime.UtcNow;

        await UpsertSettingAsync(context, Key(state.AccountId, "Email"), state.Email, now, cancellationToken);
        await UpsertSettingAsync(context, Key(state.AccountId, "QuotaBytes"), state.QuotaBytes.ToString(), now, cancellationToken);
        await UpsertSettingAsync(context, Key(state.AccountId, "UsedBytes"), state.UsedBytes.ToString(), now, cancellationToken);
        await UpsertSettingAsync(context, Key(state.AccountId, "AccessTokenExpiresUtc"), state.AccessTokenExpiresUtc.ToString("O"), now, cancellationToken);
        await UpsertSettingAsync(context, Key(state.AccountId, "LastAuthenticatedUtc"), state.LastAuthenticatedUtc.ToString("O"), now, cancellationToken);
        await UpsertSettingAsync(context, Key(state.AccountId, "LastTokenRefreshUtc"), state.LastTokenRefreshUtc?.ToString("O") ?? string.Empty, now, cancellationToken);
        await UpsertSettingAsync(context, Key(state.AccountId, "RequiresReauthentication"), state.RequiresReauthentication.ToString(), now, cancellationToken);

        _ = await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Loads account session metadata.
    /// </summary>
    /// <param name="accountId">The account identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The metadata state when found.</returns>
    public async Task<AccountSessionMetadataState?> LoadAsync(string accountId, CancellationToken cancellationToken = default)
    {
        await _migrator.EnsureMigratedAsync(cancellationToken);
        await using AstarOneDriveDbContextModel context = AstarOneDriveDbContextFactory.Create(databasePath);

        var prefix = KeyPrefix(accountId);
        List<SettingEntity> entries = await context.Settings
            .AsNoTracking()
            .Where(x => x.Key.StartsWith(prefix))
            .ToListAsync(cancellationToken);

        if(entries.Count == 0)
        {
            return null;
        }

        var values = entries.ToDictionary(x => x.Key, x => x.Value, StringComparer.Ordinal);
        if(!values.TryGetValue(Key(accountId, "Email"), out var email)
           || !values.TryGetValue(Key(accountId, "QuotaBytes"), out var quota)
           || !values.TryGetValue(Key(accountId, "UsedBytes"), out var used)
           || !values.TryGetValue(Key(accountId, "AccessTokenExpiresUtc"), out var expires)
           || !values.TryGetValue(Key(accountId, "LastAuthenticatedUtc"), out var lastAuth)
           || !values.TryGetValue(Key(accountId, "RequiresReauthentication"), out var requiresReauth))
        {
            return null;
        }

        DateTime? lastRefreshUtc = values.TryGetValue(Key(accountId, "LastTokenRefreshUtc"), out var refresh) && DateTime.TryParse(refresh, out DateTime parsedRefresh)
            ? parsedRefresh
            : null;

        return new AccountSessionMetadataState(
            accountId,
            email,
            long.Parse(quota),
            long.Parse(used),
            DateTime.Parse(expires),
            DateTime.Parse(lastAuth),
            lastRefreshUtc,
            bool.Parse(requiresReauth));
    }

    /// <summary>
    /// Removes account session metadata.
    /// </summary>
    /// <param name="accountId">The account identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task RemoveAsync(string accountId, CancellationToken cancellationToken = default)
    {
        await _migrator.EnsureMigratedAsync(cancellationToken);
        await using AstarOneDriveDbContextModel context = AstarOneDriveDbContextFactory.Create(databasePath);
        var prefix = KeyPrefix(accountId);
        List<SettingEntity> entries = await context.Settings
            .Where(x => x.Key.StartsWith(prefix))
            .ToListAsync(cancellationToken);
        if(entries.Count == 0)
        {
            return;
        }

        context.Settings.RemoveRange(entries);
        _ = await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task UpsertSettingAsync(AstarOneDriveDbContextModel context, string key, string value, DateTime updatedUtc, CancellationToken cancellationToken)
    {
        SettingEntity? existing = await context.Settings.SingleOrDefaultAsync(x => x.Key == key, cancellationToken);
        if(existing is null)
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

        existing.Value = value;
        existing.UpdatedUtc = updatedUtc;
    }

    private static string KeyPrefix(string accountId)
        => $"AccountSession:{accountId}:";

    private static string Key(string accountId, string name)
        => $"{KeyPrefix(accountId)}{name}";
}