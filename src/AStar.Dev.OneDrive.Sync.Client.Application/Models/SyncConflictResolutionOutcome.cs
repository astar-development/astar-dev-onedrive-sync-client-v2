namespace AStar.Dev.OneDrive.Sync.Client.Application.Models;

/// <summary>
/// Represents the deterministic result of conflict policy evaluation.
/// </summary>
public enum SyncConflictResolutionOutcome
{
    ProceedWithRemote = 0,
    ProceedWithLocal = 1,
    RenameAndProceed = 2,
    QueueForManualResolution = 3
}
