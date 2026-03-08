using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.UI.FolderTrees;
using AStar.Dev.Utilities;

namespace AStar.Dev.OneDrive.Sync.Client.UI.Tests.FolderTrees;

public sealed class FolderTreeViewModelShould
{
    [Fact]
    public void InitializeWithEmptyNodesCollection()
    {
        var viewModel = new FolderTreeViewModel();

        viewModel.Nodes.ShouldBeEmpty();
    }

    [Fact]
    public void FireOnAddAndRemoveWhenNodesAreUpdated()
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
    public void RaisePropertyChangedWhenSettingSelectedNodeIsUpdated()
    {
        var viewModel = new FolderTreeViewModel();
        var raised = false;
        viewModel.PropertyChanged += (_, args) => raised |= args.PropertyName == nameof(FolderTreeViewModel.SelectedNode);

        viewModel.SelectedNode = CreateNode("selected");

        raised.ShouldBeTrue();
    }

    [Fact]
    public void ToggleNodeSelectionWhenToggled()
    {
        var viewModel = new FolderTreeViewModel();
        FolderNode node = CreateNode("root");
        viewModel.Nodes.Add(node);

        viewModel.ToggleNodeSelectionCommand.Execute(node);

        viewModel.Nodes[0].IsSelected.ShouldBeTrue();
    }

    [Fact]
    public void UpdateNodeExpandedStateWhenExpandOrCollapseCommandsAreExecuted()
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
    public async Task PersistAndRestoreHierarchyWhenSaveAndLoadTreeAsyncIsCalled()
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

    [Fact]
    public async Task PersistAndRestoreSelectionStatePerAccount()
    {
        var databasePath = CreateDatabasePath();
        var viewModel = new FolderTreeViewModel(databasePath);
        var accountA = "acct-a";
        var accountB = "acct-b";
        viewModel.Nodes.Add(CreateNode("a-root") with { IsSelected = true, IsExpanded = true });

        Result<bool, Exception> saveA = await viewModel.SaveTreeAsync(accountA, TestContext.Current.CancellationToken);
        Pattern.IsSuccess(saveA).ShouldBeTrue();

        viewModel.Nodes.Clear();
        viewModel.Nodes.Add(CreateNode("b-root") with { IsSelected = false, IsExpanded = false });
        Result<bool, Exception> saveB = await viewModel.SaveTreeAsync(accountB, TestContext.Current.CancellationToken);
        Pattern.IsSuccess(saveB).ShouldBeTrue();

        var loadedViewModel = new FolderTreeViewModel(databasePath);
        Result<bool, Exception> loadA = await loadedViewModel.LoadTreeAsync(accountA, TestContext.Current.CancellationToken);
        Pattern.IsSuccess(loadA).ShouldBeTrue();
        loadedViewModel.Nodes.Count.ShouldBe(1);
        loadedViewModel.Nodes[0].Name.ShouldBe("a-root");
        loadedViewModel.Nodes[0].IsSelected.ShouldBeTrue();
        loadedViewModel.Nodes[0].IsExpanded.ShouldBeTrue();

        Result<bool, Exception> loadB = await loadedViewModel.LoadTreeAsync(accountB, TestContext.Current.CancellationToken);
        Pattern.IsSuccess(loadB).ShouldBeTrue();
        loadedViewModel.Nodes.Count.ShouldBe(1);
        loadedViewModel.Nodes[0].Name.ShouldBe("b-root");
        loadedViewModel.Nodes[0].IsSelected.ShouldBeFalse();
        loadedViewModel.Nodes[0].IsExpanded.ShouldBeFalse();
    }

    [Fact]
    public async Task SwitchAccountReloadsTreeAndClearsStaleSelection()
    {
        var databasePath = CreateDatabasePath();
        var viewModel = new FolderTreeViewModel(databasePath);
        var accountA = "acct-a";
        var accountB = "acct-b";
        FolderNode selectedNode = CreateNode("a-root") with { IsSelected = true };
        viewModel.Nodes.Add(selectedNode);
        viewModel.SelectedNode = selectedNode;
        _ = await viewModel.SaveTreeAsync(accountA, TestContext.Current.CancellationToken);

        viewModel.Nodes.Clear();
        viewModel.Nodes.Add(CreateNode("b-root"));
        _ = await viewModel.SaveTreeAsync(accountB, TestContext.Current.CancellationToken);

        Result<bool, Exception> switchToA = await viewModel.SwitchAccountAsync(accountA, TestContext.Current.CancellationToken);
        Pattern.IsSuccess(switchToA).ShouldBeTrue();
        viewModel.ActiveAccountId.ShouldBe(accountA);
        viewModel.Nodes[0].Name.ShouldBe("a-root");

        Result<bool, Exception> switchToB = await viewModel.SwitchAccountAsync(accountB, TestContext.Current.CancellationToken);
        Pattern.IsSuccess(switchToB).ShouldBeTrue();
        viewModel.ActiveAccountId.ShouldBe(accountB);
        viewModel.Nodes[0].Name.ShouldBe("b-root");
        viewModel.SelectedNode.ShouldBeNull();
    }

    [Fact]
    public async Task PersistDistinctAccountDataWhenTwoAccountsShareTheSameNodeIds()
    {
        var databasePath = CreateDatabasePath();
        var viewModel = new FolderTreeViewModel(databasePath);
        var accountA = "acct:a";
        var accountB = "acct:b";
        var sharedNodeId = "shared-node";

        viewModel.Nodes.Add(new FolderNode(sharedNodeId, "a-root", true, true, []));
        Result<bool, Exception> saveA = await viewModel.SaveTreeAsync(accountA, TestContext.Current.CancellationToken);
        Pattern.IsSuccess(saveA).ShouldBeTrue();

        viewModel.Nodes.Clear();
        viewModel.Nodes.Add(new FolderNode(sharedNodeId, "b-root", false, false, []));
        Result<bool, Exception> saveB = await viewModel.SaveTreeAsync(accountB, TestContext.Current.CancellationToken);
        Pattern.IsSuccess(saveB).ShouldBeTrue();

        Result<bool, Exception> loadA = await viewModel.LoadTreeAsync(accountA, TestContext.Current.CancellationToken);
        Pattern.IsSuccess(loadA).ShouldBeTrue();
        viewModel.Nodes.Count.ShouldBe(1);
        viewModel.Nodes[0].Id.ShouldBe(sharedNodeId);
        viewModel.Nodes[0].Name.ShouldBe("a-root");
        viewModel.Nodes[0].IsSelected.ShouldBeTrue();

        Result<bool, Exception> loadB = await viewModel.LoadTreeAsync(accountB, TestContext.Current.CancellationToken);
        Pattern.IsSuccess(loadB).ShouldBeTrue();
        viewModel.Nodes.Count.ShouldBe(1);
        viewModel.Nodes[0].Id.ShouldBe(sharedNodeId);
        viewModel.Nodes[0].Name.ShouldBe("b-root");
        viewModel.Nodes[0].IsSelected.ShouldBeFalse();
    }

    [Fact]
    public async Task RestoreHierarchyPerAccountWhenLoadingDifferentAccounts()
    {
        var databasePath = CreateDatabasePath();
        var viewModel = new FolderTreeViewModel(databasePath);
        var accountA = "acct:a";
        var accountB = "acct:b";

        FolderNode accountAChild = new("child", "a-child", false, true, []);
        FolderNode accountARoot = new("root", "a-root", true, true, [accountAChild]);
        viewModel.Nodes.Add(accountARoot);
        Result<bool, Exception> saveA = await viewModel.SaveTreeAsync(accountA, TestContext.Current.CancellationToken);
        Pattern.IsSuccess(saveA).ShouldBeTrue();

        viewModel.Nodes.Clear();
        FolderNode accountBGrandchild = new("grandchild", "b-grandchild", true, false, []);
        FolderNode accountBChild = new("child", "b-child", false, true, [accountBGrandchild]);
        FolderNode accountBRoot = new("root", "b-root", false, false, [accountBChild]);
        viewModel.Nodes.Add(accountBRoot);
        Result<bool, Exception> saveB = await viewModel.SaveTreeAsync(accountB, TestContext.Current.CancellationToken);
        Pattern.IsSuccess(saveB).ShouldBeTrue();

        Result<bool, Exception> loadA = await viewModel.LoadTreeAsync(accountA, TestContext.Current.CancellationToken);
        Pattern.IsSuccess(loadA).ShouldBeTrue();
        viewModel.Nodes.Count.ShouldBe(1);
        viewModel.Nodes[0].Name.ShouldBe("a-root");
        viewModel.Nodes[0].Children.Count.ShouldBe(1);
        viewModel.Nodes[0].Children[0].Name.ShouldBe("a-child");

        Result<bool, Exception> loadB = await viewModel.LoadTreeAsync(accountB, TestContext.Current.CancellationToken);
        Pattern.IsSuccess(loadB).ShouldBeTrue();
        viewModel.Nodes.Count.ShouldBe(1);
        viewModel.Nodes[0].Name.ShouldBe("b-root");
        viewModel.Nodes[0].Children.Count.ShouldBe(1);
        viewModel.Nodes[0].Children[0].Name.ShouldBe("b-child");
        viewModel.Nodes[0].Children[0].Children.Count.ShouldBe(1);
        viewModel.Nodes[0].Children[0].Children[0].Name.ShouldBe("b-grandchild");
    }

    private static FolderNode CreateNode(string name)
        => new(Guid.NewGuid().ToString("N"), name, false, false, []);

    private static string CreateDatabasePath()
        => Path.GetTempPath().CombinePath($"astar-ui-folders-tests-{Guid.NewGuid():N}", "astar-onedrive.db");
}
