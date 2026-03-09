using AStar.Dev.Functional.Extensions;

namespace AStar.Dev.OneDrive.Sync.Client.Application.Interfaces;

/// <summary>
/// Defines upload transfer operations for OneDrive synchronization.
/// </summary>
public interface IUploadTransferClient
{
    /// <summary>
    /// Uploads a local file using a single request.
    /// </summary>
    Task<Result<Unit, string>> UploadAsync(string localPath, string remotePath, string correlationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Uploads a local file using chunked transfer.
    /// </summary>
    Task<Result<Unit, string>> UploadChunkedAsync(string localPath, string remotePath, string correlationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a remote item.
    /// </summary>
    Task<Result<Unit, string>> DeleteAsync(string remotePath, string correlationId, CancellationToken cancellationToken = default);
}