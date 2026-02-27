using System.Collections.ObjectModel;

namespace AStar.Dev.OneDrive.Sync.Client.UI.FolderTrees;

/// <summary>
/// Represents a node in the folder tree structure, containing information about the folder such as its ID, name, selection state, expansion state, and any child nodes. This record is used to build the hierarchical representation of folders in the UI, allowing for features like selection and expansion of folders to view their contents.
/// </summary>
/// <param name="Id">The unique identifier of the folder.</param>
/// <param name="Name">The name of the folder.</param>
/// <param name="IsSelected">Indicates whether the folder is currently selected.</param>
/// <param name="IsExpanded">Indicates whether the folder is currently expanded to show its children.</param>
/// <param name="Children">The collection of child nodes under this folder.</param>
public record FolderNode(string Id, string Name, bool IsSelected, bool IsExpanded, ObservableCollection<FolderNode> Children);
