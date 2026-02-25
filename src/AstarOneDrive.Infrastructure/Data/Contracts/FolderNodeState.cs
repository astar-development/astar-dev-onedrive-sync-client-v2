namespace AstarOneDrive.Infrastructure.Data.Contracts;

public sealed record FolderNodeState(
    string Id,
    string? ParentId,
    string Name,
    bool IsSelected,
    bool IsExpanded,
    int SortOrder);
