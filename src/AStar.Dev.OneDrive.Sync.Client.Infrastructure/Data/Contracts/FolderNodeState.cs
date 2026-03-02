namespace AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Contracts;

/// <summary>
/// Represents the state of a folder node in the tree structure for persistence.
/// </summary>
/// <param name="Id">The unique identifier of the folder.</param>
/// <param name="ParentId">The identifier of the parent folder, or null if this is a root folder.</param>
/// <param name="Name">The name of the folder.</param>
/// <param name="IsSelected">Indicates whether the folder is selected for synchronization.</param>
/// <param name="IsExpanded">Indicates whether the folder is expanded in the UI.</param>
/// <param name="SortOrder">The sort order for display purposes.</param>
public sealed record FolderNodeState(string Id, string? ParentId, string Name, bool IsSelected, bool IsExpanded, int SortOrder);
