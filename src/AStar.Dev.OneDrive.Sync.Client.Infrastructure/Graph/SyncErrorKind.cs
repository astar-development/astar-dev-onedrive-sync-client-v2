namespace AStar.Dev.OneDrive.Sync.Client.Infrastructure.Graph;

/// <summary>
/// Defines typed error categories for sync and remote API operations.
/// </summary>
public enum SyncErrorKind
{
    Authentication,
    Network,
    Throttled,
    Conflict,
    NotFound,
    Api
}