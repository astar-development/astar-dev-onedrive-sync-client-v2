using AstarOneDrive.Domain.Entities;
using AstarOneDrive.Domain.Interfaces;

namespace AstarOneDrive.Infrastructure.Repositories;

/// <summary>
/// Placeholder repository that will integrate with the OneDrive API.
/// </summary>
public sealed class OneDriveSyncFileRepository : ISyncFileRepository
{
    public Task<IReadOnlyList<SyncFile>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // TODO: Implement OneDrive API integration
        IReadOnlyList<SyncFile> files = [];
        return Task.FromResult(files);
    }
}
