using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.Application.Models;

namespace AStar.Dev.OneDrive.Sync.Client.Application.Interfaces;

/// <summary>
/// Defines persistence operations for account/scoped delta checkpoints.
/// </summary>
public interface IDeltaCheckpointStore
{
    /// <summary>
    /// Loads a checkpoint for the provided account and scope.
    /// </summary>
    /// <param name="accountId">The account identifier.</param>
    /// <param name="scopeId">The sync scope identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The checkpoint when available.</returns>
    Task<Result<Option<SyncCheckpoint>, string>> LoadAsync(string accountId, string scopeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves a checkpoint value.
    /// </summary>
    /// <param name="checkpoint">The checkpoint to persist.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success or failure.</returns>
    Task<Result<Unit, string>> SaveAsync(SyncCheckpoint checkpoint, CancellationToken cancellationToken = default);
}