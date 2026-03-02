namespace AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Entities;

/// <summary>
/// Database entity representing a file or folder tracked for synchronization with OneDrive.
/// </summary>
public sealed class SyncFileEntity
{
    /// <summary>
    /// Gets or sets the unique identifier of the sync file.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the identifier of the account that owns this file.
    /// </summary>
    public string AccountId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the identifier of the parent folder, or null if this is a root item.
    /// </summary>
    public string? ParentId { get; set; }

    /// <summary>
    /// Gets or sets the name of the file or folder.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the local file system path.
    /// </summary>
    public string LocalPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the remote OneDrive path.
    /// </summary>
    public string RemotePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of item (e.g., "File" or "Folder").
    /// </summary>
    public string ItemType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether this item is selected for synchronization.
    /// </summary>
    public bool IsSelected { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this folder is currently expanded in the UI.
    /// </summary>
    public bool IsExpanded { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp of the last successful synchronization, or null if never synced.
    /// </summary>
    public DateTime? LastSyncUtc { get; set; }

    /// <summary>
    /// Gets or sets the OneDrive CTag (change tag) for change detection.
    /// </summary>
    public string? CTag { get; set; }

    /// <summary>
    /// Gets or sets the OneDrive ETag (entity tag) for versioning.
    /// </summary>
    public string? ETag { get; set; }

    /// <summary>
    /// Gets or sets the size of the file in bytes, or null for folders.
    /// </summary>
    public long? SizeBytes { get; set; }

    /// <summary>
    /// Gets or sets the sort order for display purposes.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when this record was created.
    /// </summary>
    public DateTime CreatedUtc { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when this record was last updated.
    /// </summary>
    public DateTime UpdatedUtc { get; set; }

    /// <summary>
    /// Gets or sets the navigation property to the owning account.
    /// </summary>
    public AccountEntity? Account { get; set; }

    /// <summary>
    /// Gets or sets the navigation property to the parent folder.
    /// </summary>
    public SyncFileEntity? Parent { get; set; }

    /// <summary>
    /// Gets or sets the collection of child items contained within this folder.
    /// </summary>
    public ICollection<SyncFileEntity> Children { get; set; } = [];
}
