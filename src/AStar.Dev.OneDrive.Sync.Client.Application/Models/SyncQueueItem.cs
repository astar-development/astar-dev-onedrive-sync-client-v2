namespace AStar.Dev.OneDrive.Sync.Client.Application.Models;

/// <summary>
/// Represents a queued sync operation item.
/// </summary>
/// <param name="Id">The unique queue item identifier.</param>
/// <param name="LocalPath">The local file system path.</param>
/// <param name="RemotePath">The remote OneDrive path.</param>
/// <param name="OperationType">The operation intent.</param>
/// <param name="CorrelationId">The operation correlation identifier.</param>
/// <param name="ConflictContext">Optional metadata used for conflict classification.</param>
/// <param name="ConflictResolutionPolicy">Optional policy override used for conflict resolution.</param>
public sealed record SyncQueueItem(
        string Id,
        string LocalPath,
        string RemotePath,
        SyncOperationType OperationType = SyncOperationType.Update,
        string? CorrelationId = null,
        SyncConflictContext? ConflictContext = null,
        SyncConflictResolutionPolicy? ConflictResolutionPolicy = null);
