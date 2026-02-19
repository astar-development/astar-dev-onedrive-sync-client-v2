using AstarOneDrive.Domain.Entities;

namespace AstarOneDrive.Domain.Interfaces;

/// <summary>
/// Defines the contract for accessing sync file data.
/// </summary>
public interface ISyncFileRepository
{
    Task<IReadOnlyList<SyncFile>> GetAllAsync(CancellationToken cancellationToken = default);
}
