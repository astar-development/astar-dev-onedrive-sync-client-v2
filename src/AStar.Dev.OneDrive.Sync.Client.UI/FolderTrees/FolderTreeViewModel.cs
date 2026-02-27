using System.Collections.ObjectModel;
using System.Windows.Input;
using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Contracts;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Repositories;
using AStar.Dev.OneDrive.Sync.Client.UI.Common;
using ReactiveUI;

namespace AStar.Dev.OneDrive.Sync.Client.UI.FolderTrees;

/// <summary>
/// ViewModel for managing a tree of folders, including loading and saving the tree state to a SQLite database.
/// </summary>
public class FolderTreeViewModel : ViewModelBase
{
    private const string RootNodeKey = "__root__";
    private readonly SqliteDatabaseMigrator _migrator;
    private readonly SqliteFolderTreeRepository _folderTreeRepository;

    /// <summary>
    /// The collection of root nodes in the folder tree. Each node can have child nodes, forming a hierarchical structure.
    /// </summary>
    public ObservableCollection<FolderNode> Nodes { get; } = [];

    /// <summary>
    /// A flattened view of all nodes in the tree, used for operations that require a linear collection of nodes (e.g., saving to the database).
    /// </summary>
    public ObservableCollection<FolderNode> FolderTree => Nodes;

    /// <summary>
    /// The currently selected node in the folder tree. This property can be used to track user interactions with the tree and perform actions based on the selected node.
    /// </summary>
    public FolderNode? SelectedNode
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>
    /// Command to toggle the selection state of a node. When executed, it will update the specified node's IsSelected property to the opposite of its current value.
    /// </summary>
    public ICommand ToggleNodeSelectionCommand { get; }

    /// <summary>
    /// Command to expand a node. When executed, it will set the specified node's IsExpanded property to true, causing its child nodes to become visible in the UI.
    /// </summary>
    public ICommand ExpandNodeCommand { get; }

    /// <summary>
    /// Command to collapse a node. When executed, it will set the specified node's IsExpanded property to false, causing its child nodes to be hidden in the UI.
    /// </summary>
    public ICommand CollapseNodeCommand { get; }

    /// <summary>
    /// Initializes a new instance of the FolderTreeViewModel class. This constructor sets up the necessary infrastructure for managing the folder tree, including initializing the database migrator and repository, and creating commands for node interactions.
    /// </summary>
    /// <param name="databasePath">The path to the SQLite database file. If null, a default path will be used.</param>
    public FolderTreeViewModel(string? databasePath = null)
    {
        _migrator = new SqliteDatabaseMigrator(databasePath);
        _folderTreeRepository = new SqliteFolderTreeRepository(databasePath);

        ToggleNodeSelectionCommand = CreateNodeCommand(node => node with { IsSelected = !node.IsSelected });
        ExpandNodeCommand = CreateNodeCommand(node => node with { IsExpanded = true });
        CollapseNodeCommand = CreateNodeCommand(node => node with { IsExpanded = false });
    }

    private RelayCommand CreateNodeCommand(Func<FolderNode, FolderNode> update)
        => new(parameter => UpdateNode(parameter as FolderNode, update));

    private void UpdateNode(FolderNode? node, Func<FolderNode, FolderNode> update)
    {
        if(node is null)
        {
            return;
        }

        _ = ReplaceNodeInCollection(Nodes, node, update(node));
    }

    private static bool ReplaceNodeInCollection(ObservableCollection<FolderNode> collection, FolderNode target, FolderNode replacement)
    {
        for(var index = 0; index < collection.Count; index++)
        {
            if(ReferenceEquals(collection[index], target))
            {
                collection[index] = replacement;
                return true;
            }

            if(ReplaceNodeInCollection(collection[index].Children, target, replacement))
            {
                return true;
            }
        }

        return false;
    }

    public Task<Result<bool, Exception>> SaveTreeAsync(CancellationToken cancellationToken = default)
        => Try.RunAsync(async () =>
        {
            await _migrator.EnsureMigratedAsync(cancellationToken);
            var state = FlattenTree(Nodes).ToList();
            await _folderTreeRepository.SaveAsync(state, cancellationToken);
            
            return true;
        });

    public Task<Result<bool, Exception>> LoadTreeAsync(CancellationToken cancellationToken = default)
        => Try.RunAsync(async () =>
        {
            await _migrator.EnsureMigratedAsync(cancellationToken);
            IReadOnlyList<FolderNodeState> state = await _folderTreeRepository.LoadAsync(cancellationToken);
            List<FolderNode> restored = BuildTree(state);

            Nodes.Clear();
            foreach(FolderNode node in restored)
            {
                Nodes.Add(node);
            }

            return true;
        });

    private static IEnumerable<FolderNodeState> FlattenTree(IReadOnlyList<FolderNode> nodes)
    {
        for(var index = 0; index < nodes.Count; index++)
        {
            foreach(FolderNodeState state in FlattenNode(nodes[index], null, index))
            {
                yield return state;
            }
        }
    }

    private static IEnumerable<FolderNodeState> FlattenNode(FolderNode node, string? parentId, int sortOrder)
    {
        yield return new FolderNodeState(
            node.Id,
            parentId,
            node.Name,
            node.IsSelected,
            node.IsExpanded,
            sortOrder);

        for(var index = 0; index < node.Children.Count; index++)
        {
            foreach(FolderNodeState state in FlattenNode(node.Children[index], node.Id, index))
            {
                yield return state;
            }
        }
    }

    private static List<FolderNode> BuildTree(IReadOnlyList<FolderNodeState> states)
    {
        var childrenLookup = states
            .GroupBy(x => x.ParentId ?? RootNodeKey)
            .ToDictionary(x => x.Key, x => x.OrderBy(y => y.SortOrder).ToList(), StringComparer.Ordinal);

        return BuildChildren(RootNodeKey, childrenLookup);
    }

    private static List<FolderNode> BuildChildren(
        string parentKey,
        IReadOnlyDictionary<string, List<FolderNodeState>> childrenLookup)
    {
        if(!childrenLookup.TryGetValue(parentKey, out List<FolderNodeState>? childrenStates))
        {
            return [];
        }

        var nodes = new List<FolderNode>();
        foreach(FolderNodeState state in childrenStates)
        {
            var node = new FolderNode(state.Id, state.Name, state.IsSelected, state.IsExpanded,
                [with(BuildChildren(state.Id, childrenLookup))]);
                
            nodes.Add(node);
        }

        return nodes;
    }
}
