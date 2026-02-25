using System.Collections.ObjectModel;

namespace AStar.Dev.OneDrive.Sync.Client.UI.FolderTrees;

public record FolderNode(
    string Id,
    string Name,
    bool IsSelected,
    bool IsExpanded,
    ObservableCollection<FolderNode> Children);
