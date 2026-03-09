using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.Application.Models;
using AStar.Dev.OneDrive.Sync.Client.Domain.Entities;

namespace AStar.Dev.OneDrive.Sync.Client.Application.Interfaces;

/// <summary>
/// Defines the contract for the file synchronisation service.
/// </summary>
public interface ISyncService
{
    Task<Result<IReadOnlyList<SyncFile>, string>> GetSyncFilesAsync(CancellationToken cancellationToken = default);
    Task<Result<Unit, string>> PauseSyncAsync(CancellationToken cancellationToken = default);
    Task<Result<Unit, string>> ResumeSyncAsync(CancellationToken cancellationToken = default);
    Task<Result<Unit, string>> CancelSyncAsync(CancellationToken cancellationToken = default);
    Task<Result<Unit, string>> RunDeltaSyncAsync(CancellationToken cancellationToken = default);
    Task<Result<Unit, string>> EnqueueUploadAsync(SyncQueueItem queueItem, CancellationToken cancellationToken = default);
    Task<Result<Unit, string>> EnqueueDownloadAsync(SyncQueueItem queueItem, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<SyncQueueItem>, string>> GetFailedOperationsAsync(CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<SyncConflict>, string>> GetConflictsAsync(CancellationToken cancellationToken = default);
    Task<Result<Unit, string>> RetryFailedOperationsAsync(CancellationToken cancellationToken = default);
}
