using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.Application.Interfaces;
using AStar.Dev.OneDrive.Sync.Client.Application.Models;

namespace AStar.Dev.OneDrive.Sync.Client.Application.Services;

/// <summary>
/// Pulls and applies remote delta pages while managing cursor checkpoints.
/// </summary>
public sealed class DeltaSyncService(
    IRemoteDeltaSource source,
    IDeltaCheckpointStore checkpoints,
    IRemoteDeltaApplier applier) : IDeltaSyncService
{
    /// <inheritdoc />
    public async Task<Result<DeltaPullSummary, string>> PullAsync(string accountId, string scopeId, CancellationToken cancellationToken = default)
    {
        Result<Option<SyncCheckpoint>, string> loadResult = await checkpoints.LoadAsync(accountId, scopeId, cancellationToken);
        if(loadResult is Result<Option<SyncCheckpoint>, string>.Error loadError)
        {
            return loadError.Reason;
        }

        Option<SyncCheckpoint> checkpointOption = ((Result<Option<SyncCheckpoint>, string>.Ok)loadResult).Value;
        var persistedToken = checkpointOption is Option<SyncCheckpoint>.Some some ? some.Value.DeltaToken : null;
        var cursor = persistedToken;
        var finalToken = persistedToken;
        var pages = 0;
        var changes = 0;

        while(true)
        {
            Result<RemoteDeltaPage, string> pageResult = await source.GetDeltaPageAsync(accountId, scopeId, cursor, cancellationToken);
            if(pageResult is Result<RemoteDeltaPage, string>.Error pageError)
            {
                return pageError.Reason;
            }

            RemoteDeltaPage page = ((Result<RemoteDeltaPage, string>.Ok)pageResult).Value;
            pages++;
            if(page.Changes.Count > 0)
            {
                Result<Unit, string> applyResult = await applier.ApplyAsync(accountId, scopeId, page.Changes, cancellationToken);
                if(applyResult is Result<Unit, string>.Error applyError)
                {
                    return applyError.Reason;
                }

                changes += page.Changes.Count;
            }

            if(!string.IsNullOrWhiteSpace(page.NextPageToken))
            {
                cursor = page.NextPageToken;
                continue;
            }

            finalToken = string.IsNullOrWhiteSpace(page.DeltaToken) ? finalToken : page.DeltaToken;
            break;
        }

        if(!string.IsNullOrWhiteSpace(finalToken) && !string.Equals(finalToken, persistedToken, StringComparison.Ordinal))
        {
            var checkpoint = new SyncCheckpoint(accountId, scopeId, finalToken, DateTime.UtcNow);
            Result<Unit, string> saveResult = await checkpoints.SaveAsync(checkpoint, cancellationToken);
            if(saveResult is Result<Unit, string>.Error saveError)
            {
                return saveError.Reason;
            }
        }

        Result<DeltaPullSummary, string> result = new DeltaPullSummary(pages, changes, finalToken ?? string.Empty);
        return result;
    }
}