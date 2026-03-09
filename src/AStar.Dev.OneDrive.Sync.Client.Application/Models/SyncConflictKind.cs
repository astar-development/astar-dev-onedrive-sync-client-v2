namespace AStar.Dev.OneDrive.Sync.Client.Application.Models;

/// <summary>
/// Identifies the detected synchronization conflict category.
/// </summary>
public enum SyncConflictKind
{
    None = 0,
    EtagMismatch = 1,
    TimestampDrift = 2,
    RenameDelete = 3
}
