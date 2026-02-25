using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.Domain.Entities;

namespace AStar.Dev.OneDrive.Sync.Client.Application.Interfaces;

/// <summary>
/// Defines the contract for the file synchronisation service.
/// </summary>
public interface ISyncService
{
    Task<Result<IReadOnlyList<SyncFile>, string>> GetSyncFilesAsync(CancellationToken cancellationToken = default);
}
