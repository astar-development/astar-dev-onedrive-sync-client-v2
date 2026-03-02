namespace AStar.Dev.OneDrive.Sync.Client.UI.SyncStatus;

/// <summary>
/// Represents a single synchronization activity log entry.
/// </summary>
/// <param name="Timestamp">The timestamp when the activity occurred.</param>
/// <param name="Level">The severity level of the activity.</param>
/// <param name="Message">The activity message.</param>
public record SyncActivityEntry(DateTime Timestamp, string Level, string Message);
