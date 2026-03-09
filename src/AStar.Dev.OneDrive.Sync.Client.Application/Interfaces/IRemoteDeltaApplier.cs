using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.Application.Models;

namespace AStar.Dev.OneDrive.Sync.Client.Application.Interfaces;

/// <summary>
/// Defines application of pulled remote delta changes.
/// </summary>
public interface IRemoteDeltaApplier
{
    /// <summary>
    /// Applies a delta page of remote changes.
    /// </summary>
    /// <param name="accountId">The account identifier.</param>
    /// <param name="scopeId">The sync scope identifier.</param>
    /// <param name="changes">The changes to apply.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success or failure.</returns>
    Task<Result<Unit, string>> ApplyAsync(string accountId, string scopeId, IReadOnlyList<RemoteDeltaItem> changes, CancellationToken cancellationToken = default);
}