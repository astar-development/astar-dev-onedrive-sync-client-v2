using AStar.Dev.Functional.Extensions;

namespace AStar.Dev.OneDrive.Sync.Client.Infrastructure.Graph;

/// <summary>
/// Defines resilient access to the OneDrive Graph API.
/// </summary>
public interface IOneDriveGraphClient
{
    /// <summary>
    /// Sends a Graph request with retry/error mapping behavior.
    /// </summary>
    Task<Result<OneDriveGraphResponse, SyncError>> SendAsync(OneDriveGraphRequest request, CancellationToken cancellationToken = default);
}