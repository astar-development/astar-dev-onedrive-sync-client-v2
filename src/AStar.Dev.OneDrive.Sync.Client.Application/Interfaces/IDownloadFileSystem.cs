using AStar.Dev.Functional.Extensions;

namespace AStar.Dev.OneDrive.Sync.Client.Application.Interfaces;

/// <summary>
/// Defines local filesystem operations used by the download pipeline.
/// </summary>
public interface IDownloadFileSystem
{
    /// <summary>
    /// Validates destination path and storage constraints before download.
    /// </summary>
    /// <param name="localPath">The destination local path.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success or failure.</returns>
    Task<Result<Unit, string>> ValidatePathAndDiskAsync(string localPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Builds a temporary file path used for staged download writes.
    /// </summary>
    /// <param name="localPath">The destination local path.</param>
    /// <returns>The temp file path.</returns>
    string GetTempPath(string localPath);

    /// <summary>
    /// Atomically finalizes a staged temp file into the destination path.
    /// </summary>
    /// <param name="tempPath">The temp file path.</param>
    /// <param name="localPath">The destination local path.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success or failure.</returns>
    Task<Result<Unit, string>> FinalizeAtomicAsync(string tempPath, string localPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cleans up a temporary file path after failure or cancellation.
    /// </summary>
    /// <param name="tempPath">The temp file path.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success or failure.</returns>
    Task<Result<Unit, string>> CleanupTempAsync(string tempPath, CancellationToken cancellationToken = default);
}