using System.Collections.ObjectModel;
using System.Windows.Input;
using AStar.Dev.Functional.Extensions;
using AstarOneDrive.Infrastructure.Data;
using AstarOneDrive.Infrastructure.Data.Contracts;
using AstarOneDrive.Infrastructure.Data.Repositories;
using AstarOneDrive.UI.Common;
using ReactiveUI;

namespace AstarOneDrive.UI.FolderTrees;

public class FolderTreeViewModel : ViewModelBase
{
    private const string RootNodeKey = "__root__";
    private readonly SqliteDatabaseMigrator _migrator;
    private readonly SqliteFolderTreeRepository _folderTreeRepository;

    public ObservableCollection<FolderNode> Nodes { get; } = [];
    public ObservableCollection<FolderNode> FolderTree => Nodes;

    public FolderNode? SelectedNode
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public ICommand ToggleNodeSelectionCommand { get; }
    public ICommand ExpandNodeCommand { get; }
    public ICommand CollapseNodeCommand { get; }

    public FolderTreeViewModel(string? databasePath = null)
    {
        _migrator = new SqliteDatabaseMigrator(databasePath);
        _folderTreeRepository = new SqliteFolderTreeRepository(databasePath);

        ToggleNodeSelectionCommand = CreateNodeCommand(node => node with { IsSelected = !node.IsSelected });
        ExpandNodeCommand = CreateNodeCommand(node => node with { IsExpanded = true });
        CollapseNodeCommand = CreateNodeCommand(node => node with { IsExpanded = false });
    }

    private ICommand CreateNodeCommand(Func<FolderNode, FolderNode> update) =>
        new RelayCommand(parameter => UpdateNode(parameter as FolderNode, update));

    private void UpdateNode(FolderNode? node, Func<FolderNode, FolderNode> update)
    {
        if (node is null)
        {
            return;
        }

        _ = ReplaceNodeInCollection(Nodes, node, update(node));
    }

    private static bool ReplaceNodeInCollection(
        ObservableCollection<FolderNode> collection,
        FolderNode target,
        FolderNode replacement)
    {
        for (var index = 0; index < collection.Count; index++)
        {
            if (ReferenceEquals(collection[index], target))
            {
                collection[index] = replacement;
                return true;
            }

            if (ReplaceNodeInCollection(collection[index].Children, target, replacement))
            {
                return true;
            }
        }

        return false;
    }

    public Task<Result<bool, Exception>> SaveTreeAsync(CancellationToken cancellationToken = default) =>
        Try.RunAsync(async () =>
        {
            await _migrator.EnsureMigratedAsync(cancellationToken);
            var state = FlattenTree(Nodes).ToList();
            await _folderTreeRepository.SaveAsync(state, cancellationToken);
            return true;
        });

    public Task<Result<bool, Exception>> LoadTreeAsync(CancellationToken cancellationToken = default) =>
        Try.RunAsync(async () =>
        {
            await _migrator.EnsureMigratedAsync(cancellationToken);
            var state = await _folderTreeRepository.LoadAsync(cancellationToken);
            var restored = BuildTree(state);

            Nodes.Clear();
            foreach (var node in restored)
            {
                Nodes.Add(node);
            }

            return true;
        });

    private static IEnumerable<FolderNodeState> FlattenTree(IReadOnlyList<FolderNode> nodes)
    {
        for (var index = 0; index < nodes.Count; index++)
        {
            foreach (var state in FlattenNode(nodes[index], null, index))
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

        for (var index = 0; index < node.Children.Count; index++)
        {
            foreach (var state in FlattenNode(node.Children[index], node.Id, index))
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
        if (!childrenLookup.TryGetValue(parentKey, out var childrenStates))
        {
            return [];
        }

        var nodes = new List<FolderNode>();
        foreach (var state in childrenStates)
        {
            var node = new FolderNode(
                state.Id,
                state.Name,
                state.IsSelected,
                state.IsExpanded,
                new ObservableCollection<FolderNode>(BuildChildren(state.Id, childrenLookup)));
            nodes.Add(node);
        }

        return nodes;
    }
}
