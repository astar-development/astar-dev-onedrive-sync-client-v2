using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.Application.Models;

namespace AStar.Dev.OneDrive.Sync.Client.Application.Interfaces;

/// <summary>
/// Defines remote delta page retrieval operations.
/// </summary>
public interface IRemoteDeltaSource
{
    /// <summary>
    /// Retrieves a single remote delta page for the given account and scope.
    /// </summary>
    /// <param name="accountId">The account identifier.</param>
    /// <param name="scopeId">The sync scope identifier.</param>
    /// <param name="cursor">The current pagination or delta cursor.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The delta page result.</returns>
    Task<Result<RemoteDeltaPage, string>> GetDeltaPageAsync(string accountId, string scopeId, string? cursor, CancellationToken cancellationToken = default);
}