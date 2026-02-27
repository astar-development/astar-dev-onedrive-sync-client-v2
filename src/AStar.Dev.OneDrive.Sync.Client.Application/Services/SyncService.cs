using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.Application.Interfaces;
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
}
