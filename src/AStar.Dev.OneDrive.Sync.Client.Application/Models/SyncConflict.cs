namespace AStar.Dev.OneDrive.Sync.Client.Application.Models;

/// <summary>
/// Represents a sync conflict requiring user resolution.
/// </summary>
/// <param name="QueueItemId">The related queue item identifier.</param>
/// <param name="ConflictType">The conflict category.</param>
/// <param name="Reason">The user-visible conflict reason.</param>
public sealed record SyncConflict(string QueueItemId, string ConflictType, string Reason);
