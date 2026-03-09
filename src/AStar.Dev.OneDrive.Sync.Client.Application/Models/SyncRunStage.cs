namespace AStar.Dev.OneDrive.Sync.Client.Application.Models;

/// <summary>
/// Represents orchestration pipeline stages for a sync run.
/// </summary>
public enum SyncRunStage
{
    Scan = 0,
    Delta = 1,
    Upload = 2,
    Download = 3,
    Completed = 4
}
