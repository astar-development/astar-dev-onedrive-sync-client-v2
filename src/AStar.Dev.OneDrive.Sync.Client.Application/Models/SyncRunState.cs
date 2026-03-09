namespace AStar.Dev.OneDrive.Sync.Client.Application.Models;

/// <summary>
/// Represents persisted sync orchestrator state used for crash-safe resume.
/// </summary>
/// <param name="AccountId">The account identifier.</param>
/// <param name="ScopeId">The sync scope identifier.</param>
/// <param name="RootPath">The local root path used by the run.</param>
/// <param name="UseStartupScan">Indicates whether startup scan semantics are active.</param>
/// <param name="Stage">The current orchestration stage.</param>
/// <param name="PendingUploads">Queued uploads pending processing.</param>
/// <param name="PendingDownloads">Queued downloads pending processing.</param>
/// <param name="UpdatedUtc">The state update timestamp.</param>
public sealed record SyncRunState(
    string AccountId,
    string ScopeId,
    string RootPath,
    bool UseStartupScan,
    SyncRunStage Stage,
    IReadOnlyList<SyncQueueItem> PendingUploads,
    IReadOnlyList<SyncQueueItem> PendingDownloads,
    DateTime UpdatedUtc);
