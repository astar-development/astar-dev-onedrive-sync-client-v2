namespace AStar.Dev.OneDrive.Sync.Client.Application.Models;

/// <summary>
/// Represents a queued sync operation item.
/// </summary>
/// <param name="Id">The unique queue item identifier.</param>
/// <param name="LocalPath">The local file system path.</param>
/// <param name="RemotePath">The remote OneDrive path.</param>
public sealed record SyncQueueItem(string Id, string LocalPath, string RemotePath);
