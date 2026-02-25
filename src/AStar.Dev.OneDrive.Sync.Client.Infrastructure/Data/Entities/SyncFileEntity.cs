namespace AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Entities;

public sealed class SyncFileEntity
{
    public string Id { get; set; } = string.Empty;

    public string AccountId { get; set; } = string.Empty;

    public string? ParentId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string LocalPath { get; set; } = string.Empty;

    public string RemotePath { get; set; } = string.Empty;

    public string ItemType { get; set; } = string.Empty;

    public bool IsSelected { get; set; }

    public bool IsExpanded { get; set; }

    public DateTime? LastSyncUtc { get; set; }

    public string? CTag { get; set; }

    public string? ETag { get; set; }

    public long? SizeBytes { get; set; }

    public int SortOrder { get; set; }

    public DateTime CreatedUtc { get; set; }

    public DateTime UpdatedUtc { get; set; }

    public AccountEntity? Account { get; set; }

    public SyncFileEntity? Parent { get; set; }

    public ICollection<SyncFileEntity> Children { get; set; } = [];
}
