using System.Collections.Specialized;
using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.UI.FolderTrees;
using AStar.Dev.Utilities;
using Shouldly;

namespace AStar.Dev.OneDrive.Sync.Client.UI.Tests.ViewModels.FolderTrees;

public sealed class FolderTreeViewModelTests
{
    [Fact]
    public void Constructor_InitializesWithEmptyNodesCollection()
    {
        var viewModel = new FolderTreeViewModel();

        viewModel.Nodes.ShouldBeEmpty();
    }

    [Fact]
    public void Nodes_CollectionChanged_FiresOnAddAndRemove()
    {
        var viewModel = new FolderTreeViewModel();
        var changeCount = 0;
        viewModel.Nodes.CollectionChanged += (_, _) => changeCount++;
        FolderNode node = CreateNode("root");

        viewModel.Nodes.Add(node);
        _ = viewModel.Nodes.Remove(node);

        changeCount.ShouldBe(2);
    }

    [Fact]
    public void SelectedNode_Set_RaisesPropertyChanged()
    {
        var viewModel = new FolderTreeViewModel();
        var raised = false;
        viewModel.PropertyChanged += (_, args) => raised |= args.PropertyName == nameof(FolderTreeViewModel.SelectedNode);

        viewModel.SelectedNode = CreateNode("selected");

        raised.ShouldBeTrue();
    }

    [Fact]
    public void ToggleNodeSelectionCommand_TogglesNodeSelection()
    {
        var viewModel = new FolderTreeViewModel();
        FolderNode node = CreateNode("root");
        viewModel.Nodes.Add(node);

        viewModel.ToggleNodeSelectionCommand.Execute(node);

        viewModel.Nodes[0].IsSelected.ShouldBeTrue();
    }

    [Fact]
    public void ExpandAndCollapseCommands_UpdateNodeExpandedState()
    {
        var viewModel = new FolderTreeViewModel();
        FolderNode node = CreateNode("root");
        viewModel.Nodes.Add(node);

        viewModel.ExpandNodeCommand.Execute(node);
        viewModel.Nodes[0].IsExpanded.ShouldBeTrue();
        viewModel.CollapseNodeCommand.Execute(viewModel.Nodes[0]);
        viewModel.Nodes[0].IsExpanded.ShouldBeFalse();
    }

    [Fact]
    public async Task SaveAndLoadTreeAsync_PersistsAndRestoresHierarchy()
    {
        var databasePath = CreateDatabasePath();
        var viewModel = new FolderTreeViewModel(databasePath);
        FolderNode child = CreateNode("child");
        FolderNode root = CreateNode("root") with { Children = [child], IsSelected = true, IsExpanded = true };
        viewModel.Nodes.Add(root);

        Result<bool, Exception> saveResult = await viewModel.SaveTreeAsync(TestContext.Current.CancellationToken);
        Pattern.IsSuccess(saveResult).ShouldBeTrue();

        var loadedViewModel = new FolderTreeViewModel(databasePath);
        Result<bool, Exception> loadResult = await loadedViewModel.LoadTreeAsync(TestContext.Current.CancellationToken);
        Pattern.IsSuccess(loadResult).ShouldBeTrue();
        loadedViewModel.Nodes.Count.ShouldBe(1);
        loadedViewModel.Nodes[0].Children.Count.ShouldBe(1);
        loadedViewModel.Nodes[0].IsSelected.ShouldBeTrue();
    }

    private static FolderNode CreateNode(string name)
        => new(Guid.NewGuid().ToString("N"), name, false, false, []);

    private static string CreateDatabasePath()
        => Path.GetTempPath().CombinePath($"astar-ui-folders-tests-{Guid.NewGuid():N}", "astar-onedrive.db");
}
