using AStar.Dev.Functional.Extensions;
using AstarOneDrive.Domain.Entities;

namespace AstarOneDrive.Application.Interfaces;

/// <summary>
/// Defines the contract for the file synchronisation service.
/// </summary>
public interface ISyncService
{
    Task<Result<IReadOnlyList<SyncFile>, string>> GetSyncFilesAsync(CancellationToken cancellationToken = default);
}
