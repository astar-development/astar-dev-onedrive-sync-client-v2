using AstarOneDrive.Domain.Entities;

namespace AstarOneDrive.Application.Interfaces;

/// <summary>
/// Defines the contract for the file synchronisation service.
/// </summary>
public interface ISyncService
{
    Task<IReadOnlyList<SyncFile>> GetSyncFilesAsync(CancellationToken cancellationToken = default);
}
