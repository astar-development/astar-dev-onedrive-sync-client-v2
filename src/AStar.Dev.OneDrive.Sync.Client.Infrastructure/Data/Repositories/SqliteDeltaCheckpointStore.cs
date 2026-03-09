using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.Application.Interfaces;
using AStar.Dev.OneDrive.Sync.Client.Application.Models;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Repositories;

/// <summary>
/// Persists delta checkpoints in SQLite settings records.
/// </summary>
public sealed class SqliteDeltaCheckpointStore(string? databasePath = null) : IDeltaCheckpointStore
{
    /// <inheritdoc />
    public Task<Result<Option<SyncCheckpoint>, string>> LoadAsync(string accountId, string scopeId, CancellationToken cancellationToken = default)
        => Try.RunAsync(async () =>
        {
            await using AstarOneDriveDbContextModel context = AstarOneDriveDbContextFactory.Create(databasePath);
            var key = BuildKey(accountId, scopeId);
            SettingEntity? setting = await context.Settings.AsNoTracking().SingleOrDefaultAsync(x => x.Key == key, cancellationToken);
            return setting is null
                ? Option.None<SyncCheckpoint>()
                : new Option<SyncCheckpoint>.Some(new SyncCheckpoint(accountId, scopeId, setting.Value, setting.UpdatedUtc));
        }).MapFailureAsync(error => error.Message);

    /// <inheritdoc />
    public Task<Result<Unit, string>> SaveAsync(SyncCheckpoint checkpoint, CancellationToken cancellationToken = default)
        => Try.RunAsync(async () =>
        {
            await using AstarOneDriveDbContextModel context = AstarOneDriveDbContextFactory.Create(databasePath);
            var key = BuildKey(checkpoint.AccountId, checkpoint.ScopeId);
            SettingEntity? setting = await context.Settings.SingleOrDefaultAsync(x => x.Key == key, cancellationToken);
            if(setting is null)
            {
                _ = context.Settings.Add(new SettingEntity
                {
                    Id = Guid.NewGuid().ToString("N"),
                    Key = key,
                    Value = checkpoint.DeltaToken,
                    UpdatedUtc = checkpoint.UpdatedUtc
                });
            }
            else
            {
                setting.Value = checkpoint.DeltaToken;
                setting.UpdatedUtc = checkpoint.UpdatedUtc;
            }

            _ = await context.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }).MapFailureAsync(error => error.Message);

    private static string BuildKey(string accountId, string scopeId)
        => $"DeltaCheckpoint:{accountId}:{scopeId}";
}