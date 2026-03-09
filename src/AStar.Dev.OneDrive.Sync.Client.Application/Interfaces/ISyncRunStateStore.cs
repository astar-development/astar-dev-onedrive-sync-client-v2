using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.Application.Models;

namespace AStar.Dev.OneDrive.Sync.Client.Application.Interfaces;

/// <summary>
/// Defines persistence operations for sync orchestration run state.
/// </summary>
public interface ISyncRunStateStore
{
    /// <summary>
    /// Loads persisted run state for an account and scope.
    /// </summary>
    Task<Result<Option<SyncRunState>, string>> LoadAsync(string accountId, string scopeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves run state for crash-safe resume.
    /// </summary>
    Task<Result<Unit, string>> SaveAsync(SyncRunState state, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears persisted run state for an account and scope.
    /// </summary>
    Task<Result<Unit, string>> ClearAsync(string accountId, string scopeId, CancellationToken cancellationToken = default);
}
