using AStar.Dev.Functional.Extensions;
using AstarOneDrive.Domain.Entities;

namespace AstarOneDrive.Domain.Interfaces;

/// <summary>
/// Defines the contract for accessing sync file data.
/// </summary>
public interface ISyncFileRepository
{
    Task<Result<IReadOnlyList<SyncFile>, string>> GetAllAsync(CancellationToken cancellationToken = default);
}
