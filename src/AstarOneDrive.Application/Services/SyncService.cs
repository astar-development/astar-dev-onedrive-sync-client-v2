using AstarOneDrive.Application.Interfaces;
using AstarOneDrive.Domain.Entities;
using AstarOneDrive.Domain.Interfaces;

namespace AstarOneDrive.Application.Services;

/// <summary>
/// Orchestrates file synchronisation operations.
/// </summary>
public sealed class SyncService : ISyncService
{
    private readonly ISyncFileRepository _syncFileRepository;

    public SyncService(ISyncFileRepository syncFileRepository)
    {
        ArgumentNullException.ThrowIfNull(syncFileRepository);
        _syncFileRepository = syncFileRepository;
    }

    public Task<IReadOnlyList<SyncFile>> GetSyncFilesAsync(CancellationToken cancellationToken = default)
        => _syncFileRepository.GetAllAsync(cancellationToken);
}
