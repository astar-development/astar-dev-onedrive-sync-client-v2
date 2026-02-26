using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.Domain.Entities;
using AStar.Dev.OneDrive.Sync.Client.Domain.Interfaces;

namespace AStar.Dev.OneDrive.Sync.Client.Infrastructure.Repositories;

/// <summary>
/// Placeholder repository that will integrate with the OneDrive API.
/// </summary>
public sealed class OneDriveSyncFileRepository : ISyncFileRepository
{
    public Task<Result<IReadOnlyList<SyncFile>, string>> GetAllAsync(CancellationToken cancellationToken = default)
        // TODO: Implement OneDrive API integration
        => Task.FromResult<Result<IReadOnlyList<SyncFile>, string>>(new List<SyncFile>());
}
