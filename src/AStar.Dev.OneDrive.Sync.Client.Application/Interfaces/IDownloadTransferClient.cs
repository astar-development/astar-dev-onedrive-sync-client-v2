using AStar.Dev.Functional.Extensions;

namespace AStar.Dev.OneDrive.Sync.Client.Application.Interfaces;

/// <summary>
/// Defines remote-to-temp file download operations.
/// </summary>
public interface IDownloadTransferClient
{
    /// <summary>
    /// Downloads a remote item to a temporary local file path.
    /// </summary>
    /// <param name="remotePath">The remote path.</param>
    /// <param name="tempPath">The temporary local file path.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success or failure.</returns>
    Task<Result<Unit, string>> DownloadAsync(string remotePath, string tempPath, CancellationToken cancellationToken = default);
}