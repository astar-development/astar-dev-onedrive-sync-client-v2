namespace AStar.Dev.OneDrive.Sync.Client.Application.Models;

/// <summary>
/// Defines the policy used to resolve a classified synchronization conflict.
/// </summary>
public enum SyncConflictResolutionPolicy
{
    RemoteWins = 0,
    LocalWins = 1,
    RenameCopy = 2,
    Manual = 3
}
