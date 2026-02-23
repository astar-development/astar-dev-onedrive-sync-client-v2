using System.Collections.ObjectModel;

namespace AstarOneDrive.UI.FolderTrees;

public record FolderNode(
    string Id,
    string Name,
    bool IsSelected,
    bool IsExpanded,
    ObservableCollection<FolderNode> Children);