namespace AstarOneDrive.Domain.Entities;

/// <summary>
/// Represents a file that is tracked for synchronization with OneDrive.
/// </summary>
public sealed class SyncFile
{
    public SyncFile(string name, string localPath, string remotePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(localPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(remotePath);

        Name = name;
        LocalPath = localPath;
        RemotePath = remotePath;
    }

    public string Name { get; }

    public string LocalPath { get; }

    public string RemotePath { get; }
}
