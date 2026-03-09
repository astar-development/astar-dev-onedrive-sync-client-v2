namespace AStar.Dev.OneDrive.Sync.Client.Application.Models;

/// <summary>
/// Provides metadata used to classify synchronization conflicts.
/// </summary>
/// <param name="LocalETag">The local-side known eTag value.</param>
/// <param name="RemoteETag">The remote-side known eTag value.</param>
/// <param name="LocalTimestampUtc">The local-side modification timestamp.</param>
/// <param name="RemoteTimestampUtc">The remote-side modification timestamp.</param>
/// <param name="LocalRenamed">Indicates whether the local item was renamed.</param>
/// <param name="RemoteRenamed">Indicates whether the remote item was renamed.</param>
/// <param name="LocalDeleted">Indicates whether the local item was deleted.</param>
/// <param name="RemoteDeleted">Indicates whether the remote item was deleted.</param>
public sealed record SyncConflictContext(
    string? LocalETag,
    string? RemoteETag,
    DateTime? LocalTimestampUtc,
    DateTime? RemoteTimestampUtc,
    bool LocalRenamed,
    bool RemoteRenamed,
    bool LocalDeleted,
    bool RemoteDeleted);
