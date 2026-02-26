using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.Domain.Entities;

namespace AStar.Dev.OneDrive.Sync.Client.Domain.Interfaces;

/// <summary>
/// Defines the contract for accessing sync file data.
/// </summary>
public interface ISyncFileRepository
{
    Task<Result<IReadOnlyList<SyncFile>, string>> GetAllAsync(CancellationToken cancellationToken = default);
}
