using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.Application.Models;

namespace AStar.Dev.OneDrive.Sync.Client.Application.Interfaces;

/// <summary>
/// Defines persistence operations for account-scoped local inventory snapshots.
/// </summary>
public interface ILocalInventoryStore
{
    /// <summary>
    /// Persists a full inventory snapshot for an account.
    /// </summary>
    /// <param name="accountId">The account identifier.</param>
    /// <param name="items">The snapshot items.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success or failure.</returns>
    Task<Result<Unit, string>> SaveAsync(string accountId, IReadOnlyList<LocalInventoryItem> items, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads the persisted inventory snapshot for an account.
    /// </summary>
    /// <param name="accountId">The account identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The persisted snapshot items.</returns>
    Task<Result<IReadOnlyList<LocalInventoryItem>, string>> LoadAsync(string accountId, CancellationToken cancellationToken = default);
}