namespace AStar.Dev.OneDrive.Sync.Client.Domain.Entities;

/// <summary>
/// Represents a file that is tracked for synchronization with OneDrive.
/// </summary>
public sealed class SyncFile(string name, string localPath, string remotePath)
{
    public string Name { get; } = name;

    public string LocalPath { get; } = localPath;

    public string RemotePath { get; } = remotePath;
}
