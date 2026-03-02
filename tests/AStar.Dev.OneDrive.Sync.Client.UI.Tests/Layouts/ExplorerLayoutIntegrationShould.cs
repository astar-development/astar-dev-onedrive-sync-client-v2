using AStar.Dev.OneDrive.Sync.Client.UI.FolderTrees;
using AStar.Dev.OneDrive.Sync.Client.UI.Home;
using AStar.Dev.OneDrive.Sync.Client.UI.Layouts;

namespace AStar.Dev.OneDrive.Sync.Client.UI.Tests.Layouts;

public sealed class ExplorerLayoutIntegrationShould
{
    [Fact]
    public void RenderNodesFromViewModelWhenLayoutIsCreated()
    {
        var mainWindowVM = new MainWindowViewModel(initializeLayoutView: false);
        var root = new FolderNode(Guid.NewGuid().ToString("N"), "TestRoot", false, false, []);
        mainWindowVM.FolderTree.Nodes.Add(root);

        var explorerLayout = new ExplorerLayoutViewModel(mainWindowVM);

        explorerLayout.MainWindow.FolderTree.Nodes.ShouldContain(n => n.Name == "TestRoot");
    }

    [Fact]
    public void ReflectCheckboxStateWhenIsSelectedChanges()
    {
        var mainWindowVM = new MainWindowViewModel(initializeLayoutView: false);
        var node = new FolderNode(Guid.NewGuid().ToString("N"), "TestFolder", false, false, []);
        mainWindowVM.FolderTree.Nodes.Add(node);

        var explorerLayout = new ExplorerLayoutViewModel(mainWindowVM);
        FolderNode targetNode = explorerLayout.MainWindow.FolderTree.Nodes[0];
        explorerLayout.MainWindow.FolderTree.ToggleNodeSelectionCommand.Execute(targetNode);

        explorerLayout.MainWindow.FolderTree.Nodes[0].IsSelected.ShouldBeTrue();
    }

    [Fact]
    public void AllowExpandAndCollapseWhenCommandsExecute()
    {
        var mainWindowVM = new MainWindowViewModel(initializeLayoutView: false);
        var node = new FolderNode(Guid.NewGuid().ToString("N"), "TestFolder", false, false, []);
        mainWindowVM.FolderTree.Nodes.Add(node);

        var explorerLayout = new ExplorerLayoutViewModel(mainWindowVM);
        FolderNode targetNode = explorerLayout.MainWindow.FolderTree.Nodes[0];

        explorerLayout.MainWindow.FolderTree.ExpandNodeCommand.Execute(targetNode);
        explorerLayout.MainWindow.FolderTree.Nodes[0].IsExpanded.ShouldBeTrue();

        explorerLayout.MainWindow.FolderTree.CollapseNodeCommand.Execute(explorerLayout.MainWindow.FolderTree.Nodes[0]);
        explorerLayout.MainWindow.FolderTree.Nodes[0].IsExpanded.ShouldBeFalse();
    }
}
