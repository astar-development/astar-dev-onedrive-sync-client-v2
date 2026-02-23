using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows.Input;
using AStar.Dev.Functional.Extensions;
using AstarOneDrive.UI.Common;
using ReactiveUI;

namespace AstarOneDrive.UI.FolderTrees;

public class FolderTreeViewModel : ViewModelBase
{
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

    public FolderTreeViewModel()
    {
        ToggleNodeSelectionCommand = new RelayCommand(parameter =>
            UpdateNode(parameter as FolderNode, node => node with { IsSelected = !node.IsSelected }));
        ExpandNodeCommand = new RelayCommand(parameter =>
            UpdateNode(parameter as FolderNode, node => node with { IsExpanded = true }));
        CollapseNodeCommand = new RelayCommand(parameter =>
            UpdateNode(parameter as FolderNode, node => node with { IsExpanded = false }));
    }

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

    public Task<Result<bool, Exception>> SaveTreeAsync(CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(Nodes.ToList(), new JsonSerializerOptions { WriteIndented = true });
        return Try.RunAsync(async () =>
        {
            await File.WriteAllTextAsync(GetTreeFilePath(), json, cancellationToken);
            return true;
        });
    }

    public Task<Result<bool, Exception>> LoadTreeAsync(CancellationToken cancellationToken = default) =>
        Try.RunAsync(async () =>
        {
            var filePath = GetTreeFilePath();
            if (!File.Exists(filePath))
            {
                return true;
            }

            var json = await File.ReadAllTextAsync(filePath, cancellationToken);
            var restored = JsonSerializer.Deserialize<List<FolderNode>>(json) ?? [];

            Nodes.Clear();
            foreach (var node in restored)
            {
                Nodes.Add(node);
            }

            return true;
        });

    private static string GetTreeFilePath()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appFolder = Path.Combine(appDataPath, "AstarOneDrive");
        Directory.CreateDirectory(appFolder);
        return Path.Combine(appFolder, "folder-tree.json");
    }
}
