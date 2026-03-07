using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.Application.Interfaces;
using AStar.Dev.OneDrive.Sync.Client.Application.Models;
using AStar.Dev.OneDrive.Sync.Client.Domain.Entities;
using AStar.Dev.OneDrive.Sync.Client.Domain.Interfaces;

namespace AStar.Dev.OneDrive.Sync.Client.Application.Services;

/// <summary>
/// Orchestrates file synchronisation operations.
/// </summary>
public sealed class SyncService(ISyncFileRepository syncFileRepository) : ISyncService
{
    ///  <inheritdoc/>
    public Task<Result<IReadOnlyList<SyncFile>, string>> GetSyncFilesAsync(CancellationToken cancellationToken = default)
        => syncFileRepository.GetAllAsync(cancellationToken);

    ///  <inheritdoc/>
    public Task<Result<Unit, string>> PauseSyncAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<Result<Unit, string>>(Unit.Value);

    ///  <inheritdoc/>
    public Task<Result<Unit, string>> ResumeSyncAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<Result<Unit, string>>(Unit.Value);

    ///  <inheritdoc/>
    public Task<Result<Unit, string>> CancelSyncAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<Result<Unit, string>>(Unit.Value);

    ///  <inheritdoc/>
    public Task<Result<Unit, string>> RunDeltaSyncAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<Result<Unit, string>>(Unit.Value);

    ///  <inheritdoc/>
    public Task<Result<Unit, string>> EnqueueUploadAsync(SyncQueueItem queueItem, CancellationToken cancellationToken = default)
        => Task.FromResult<Result<Unit, string>>(Unit.Value);

    ///  <inheritdoc/>
    public Task<Result<Unit, string>> EnqueueDownloadAsync(SyncQueueItem queueItem, CancellationToken cancellationToken = default)
        => Task.FromResult<Result<Unit, string>>(Unit.Value);

    ///  <inheritdoc/>
    public Task<Result<IReadOnlyList<SyncQueueItem>, string>> GetFailedOperationsAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<Result<IReadOnlyList<SyncQueueItem>, string>>(Array.Empty<SyncQueueItem>());

    ///  <inheritdoc/>
    public Task<Result<Unit, string>> RetryFailedOperationsAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<Result<Unit, string>>(Unit.Value);
}
