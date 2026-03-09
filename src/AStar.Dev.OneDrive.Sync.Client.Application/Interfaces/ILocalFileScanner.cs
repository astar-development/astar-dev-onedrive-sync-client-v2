using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.Application.Models;

namespace AStar.Dev.OneDrive.Sync.Client.Application.Interfaces;

/// <summary>
/// Defines local filesystem scanning operations for inventory generation.
/// </summary>
public interface ILocalFileScanner
{
    /// <summary>
    /// Scans a local root path and returns deterministic file inventory items.
    /// </summary>
    /// <param name="rootPath">The local root path to scan.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The local file inventory snapshot.</returns>
    Task<Result<IReadOnlyList<LocalInventoryItem>, string>> ScanAsync(string rootPath, CancellationToken cancellationToken = default);
}