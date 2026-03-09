namespace AStar.Dev.OneDrive.Sync.Client.Application.Models;

/// <summary>
/// Represents the operation intent for queued sync work items.
/// </summary>
public enum SyncOperationType
{
    Create,
    Update,
    Delete
}