using AStar.Dev.Functional.Extensions;

namespace AStar.Dev.OneDrive.Sync.Client.Application.Interfaces;

/// <summary>
/// Defines filesystem operations used by the upload pipeline.
/// </summary>
public interface IUploadFileSystem
{
    /// <summary>
    /// Validates a local upload source path.
    /// </summary>
    Task<Result<Unit, string>> ValidateUploadPathAsync(string localPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the local file size in bytes.
    /// </summary>
    Task<Result<long, string>> GetFileSizeBytesAsync(string localPath, CancellationToken cancellationToken = default);
}