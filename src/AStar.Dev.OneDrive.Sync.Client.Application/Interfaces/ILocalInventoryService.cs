using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.Application.Models;

namespace AStar.Dev.OneDrive.Sync.Client.Application.Interfaces;

/// <summary>
/// Defines local inventory scan entry points used by startup and manual workflows.
/// </summary>
public interface ILocalInventoryService
{
    /// <summary>
    /// Runs a startup inventory scan and persists the resulting snapshot.
    /// </summary>
    /// <param name="accountId">The account identifier.</param>
    /// <param name="rootPath">The local root path to scan.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The persisted local inventory snapshot.</returns>
    Task<Result<IReadOnlyList<LocalInventoryItem>, string>> RunStartupScanAsync(string accountId, string rootPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs a manual inventory scan and persists the resulting snapshot.
    /// </summary>
    /// <param name="accountId">The account identifier.</param>
    /// <param name="rootPath">The local root path to scan.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The persisted local inventory snapshot.</returns>
    Task<Result<IReadOnlyList<LocalInventoryItem>, string>> RunManualScanAsync(string accountId, string rootPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads the persisted local inventory snapshot for an account.
    /// </summary>
    /// <param name="accountId">The account identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The persisted local inventory snapshot.</returns>
    Task<Result<IReadOnlyList<LocalInventoryItem>, string>> LoadSnapshotAsync(string accountId, CancellationToken cancellationToken = default);
}