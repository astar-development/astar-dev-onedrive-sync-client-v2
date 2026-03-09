namespace AStar.Dev.OneDrive.Sync.Client.Application.Models;

/// <summary>
/// Represents a single remote delta change entry.
/// </summary>
/// <param name="Id">The provider item identifier.</param>
/// <param name="Path">The remote path.</param>
/// <param name="ChangeKind">The type of change.</param>
public sealed record RemoteDeltaItem(string Id, string Path, RemoteDeltaChangeKind ChangeKind);