using AStar.Dev.OneDrive.Sync.Client.UI.AccountManagement;
using AStar.Dev.OneDrive.Sync.Client.UI.Common;
using AStar.Dev.OneDrive.Sync.Client.UI.FolderTrees;
using AStar.Dev.OneDrive.Sync.Client.UI.Home;
using AStar.Dev.OneDrive.Sync.Client.UI.Logs;
using AStar.Dev.OneDrive.Sync.Client.UI.Settings;
using AStar.Dev.OneDrive.Sync.Client.UI.SyncStatus;
using AStar.Dev.OneDrive.Sync.Client.UI.Tests.ThemeManager;

namespace AStar.Dev.OneDrive.Sync.Client.UI.Tests.Home;

[Collection(ThemeManagerTestCollection.Name)]
public sealed class MainWindowViewModelShould
{
    [Fact]
    public void HaveDefaultCurrentLayoutSetAsExplorer()
    {
        var vm = new MainWindowViewModel();
        vm.CurrentLayout.ShouldBe(LayoutType.Explorer);
    }

    [Fact]
    public void SetLayoutToExplorerWhenSwitchToExplorerCommandIsExecuted()
    {
        var vm = new MainWindowViewModel
        {
            CurrentLayout = LayoutType.Dashboard
        };

        vm.SwitchToExplorerCommand.Execute(null);

        vm.CurrentLayout.ShouldBe(LayoutType.Explorer);
    }

    [Fact]
    public void SetLayoutToDashboardWhenSwitchToDashboardCommandIsExecuted()
    {
        var vm = new MainWindowViewModel
        {
            CurrentLayout = LayoutType.Explorer
        };

        vm.SwitchToDashboardCommand.Execute(null);

        vm.CurrentLayout.ShouldBe(LayoutType.Dashboard);
    }

    [Fact]
    public void SetLayoutToTerminalWhenSwitchToTerminalCommandIsExecuted()
    {
        var vm = new MainWindowViewModel
        {
            CurrentLayout = LayoutType.Explorer
        };

        vm.SwitchToTerminalCommand.Execute(null);

        vm.CurrentLayout.ShouldBe(LayoutType.Terminal);
    }

    [Fact]
    public void ResetTerminalTabToStatusWhenSwitchToTerminalCommandIsExecuted()
    {
        var vm = new MainWindowViewModel
        {
            TerminalSelectedTabIndex = 2
        };

        vm.SwitchToTerminalCommand.Execute(null);

        vm.TerminalSelectedTabIndex.ShouldBe(0);
    }

    [Fact]
    public void SetTerminalLayoutWithSettingsTabWhenOpenUserSettingsCommandIsExecuted()
    {
        var vm = new MainWindowViewModel();

        vm.OpenUserSettingsCommand.Execute(null);

        vm.CurrentLayout.ShouldBe(LayoutType.Terminal);
        vm.TerminalSelectedTabIndex.ShouldBe(2);
    }

    [Fact]
    public void MaintainTheSameViewModelAcrossLayoutSwaps()
    {
        var vm = new MainWindowViewModel();

        AccountListViewModel accountsInstance = vm.Accounts;
        FolderTreeViewModel folderTreeInstance = vm.FolderTree;
        SyncStatusViewModel syncInstance = vm.Sync;
        LogsViewModel logsInstance = vm.Logs;
        SettingsViewModel settingsInstance = vm.Settings;

        vm.SwitchToDashboardCommand.Execute(null);
        vm.Accounts.ShouldBeSameAs(accountsInstance);
        vm.FolderTree.ShouldBeSameAs(folderTreeInstance);
        vm.Sync.ShouldBeSameAs(syncInstance);
        vm.Logs.ShouldBeSameAs(logsInstance);
        vm.Settings.ShouldBeSameAs(settingsInstance);

        vm.SwitchToTerminalCommand.Execute(null);
        vm.Accounts.ShouldBeSameAs(accountsInstance);
        vm.FolderTree.ShouldBeSameAs(folderTreeInstance);
        vm.Sync.ShouldBeSameAs(syncInstance);
        vm.Logs.ShouldBeSameAs(logsInstance);
        vm.Settings.ShouldBeSameAs(settingsInstance);
    }

    [Fact]
    public void UpdateSettingsViewModelWhenTheLayoutChanges()
    {
        var vm = new MainWindowViewModel();

        vm.SwitchToDashboardCommand.Execute(null);
        vm.Settings.SelectedLayout.ShouldBe("Dashboard");
        vm.SwitchToTerminalCommand.Execute(null);
        vm.Settings.SelectedLayout.ShouldBe("Terminal");
        vm.SwitchToExplorerCommand.Execute(null);
        vm.Settings.SelectedLayout.ShouldBe("Explorer");
    }

    [Fact]
    public async Task UpdateMainWindowLayoutWhenSettingsViewModelLayoutChanges()
    {
        var vm = new MainWindowViewModel();

        _ = await vm.Settings.LoadSettingsAsync();

        vm.Settings.SelectedLayout = "Explorer";
        vm.CurrentLayout.ShouldBe(LayoutType.Explorer);
        vm.Settings.SelectedLayout = "Dashboard";
        vm.CurrentLayout.ShouldBe(LayoutType.Dashboard);
        vm.Settings.SelectedLayout = "Terminal";
        vm.CurrentLayout.ShouldBe(LayoutType.Terminal);
    }

    [Fact]
    public void SetTerminalLayoutToSettingsTabWhenOpenUserSettingsCommandIsExecuted()
    {
        var vm = new MainWindowViewModel();

        vm.OpenUserSettingsCommand.Execute(null);

        vm.CurrentLayout.ShouldBe(LayoutType.Terminal);
        vm.TerminalSelectedTabIndex.ShouldBe(2);
    }

    [Fact]
    public void SetTerminalLayoutToSettingsTabWhenOpenAppSettingsCommandIsExecuted()
    {
        var vm = new MainWindowViewModel();

        vm.OpenAppSettingsCommand.Execute(null);

        vm.CurrentLayout.ShouldBe(LayoutType.Terminal);
        vm.TerminalSelectedTabIndex.ShouldBe(2);
    }

    [Fact]
    public void SetTheTerminalSelectedTabIndexToZeroWhenWindowIsInitialized()
    {
        var vm = new MainWindowViewModel();

        vm.TerminalSelectedTabIndex.ShouldBe(0);
    }

    [Fact]
    public void RaisePropertyChangedWhenTerminalSelectedTabIndexChanges()
    {
        var vm = new MainWindowViewModel();
        var propertyChanged = false;
        vm.PropertyChanged += (_, e) =>
        {
            if(e.PropertyName == nameof(MainWindowViewModel.TerminalSelectedTabIndex))
                propertyChanged = true;
        };

        vm.TerminalSelectedTabIndex = 1;

        propertyChanged.ShouldBeTrue();
    }

    [Fact]
    public void ClearFolderTreeWhenSelectedAccountIsCleared()
    {
        var vm = new MainWindowViewModel();
        var account = new AccountInfo("acct-1", "user@example.com", 1000, 10);
        FolderNode node = new(Guid.NewGuid().ToString("N"), "root", false, false, []);

        vm.Accounts.SelectedAccount = account;
        vm.FolderTree.Nodes.Add(node);
        vm.FolderTree.SelectedNode = node;

        vm.Accounts.SelectedAccount = null;

        vm.FolderTree.Nodes.ShouldBeEmpty();
        vm.FolderTree.SelectedNode.ShouldBeNull();
        vm.FolderTree.ActiveAccountId.ShouldBeNull();
    }

    [Fact]
    public async Task ReloadFolderTreeWhenSelectedAccountChanges()
    {
        var vm = new MainWindowViewModel();
        var accountA = new AccountInfo("acct:a", "a@example.com", 1000, 10);
        var accountB = new AccountInfo("acct:b", "b@example.com", 2000, 20);

        vm.FolderTree.Nodes.Add(new FolderNode("root", "a-root", true, false, []));
        _ = await vm.FolderTree.SaveTreeAsync(accountA.Id, TestContext.Current.CancellationToken);

        vm.FolderTree.Nodes.Clear();
        vm.FolderTree.Nodes.Add(new FolderNode("root", "b-root", false, true, []));
        _ = await vm.FolderTree.SaveTreeAsync(accountB.Id, TestContext.Current.CancellationToken);

        vm.Accounts.SelectedAccount = accountA;
        await WaitForConditionAsync(() => vm.FolderTree.ActiveAccountId == accountA.Id && vm.FolderTree.Nodes.Count == 1 && vm.FolderTree.Nodes[0].Name == "a-root");

        vm.Accounts.SelectedAccount = accountB;
        await WaitForConditionAsync(() => vm.FolderTree.ActiveAccountId == accountB.Id && vm.FolderTree.Nodes.Count == 1 && vm.FolderTree.Nodes[0].Name == "b-root");

        vm.FolderTree.Nodes[0].Name.ShouldBe("b-root");
    }

    [Fact]
    public async Task ClearTreeAndReportErrorWhenAccountReloadFails()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"astar-ui-main-window-tests-{Guid.NewGuid():N}", "astar-onedrive.db");
        var accounts = new AccountListViewModel(databasePath);
        var folderTree = new FolderTreeViewModel(databasePath);
        var vm = new MainWindowViewModel(accounts: accounts, folderTree: folderTree);
        var account = new AccountInfo(new string('x', 128), "broken@example.com", 1000, 10);
        FolderNode node = new(Guid.NewGuid().ToString("N"), "stale", true, true, []);

        vm.FolderTree.Nodes.Add(node);
        vm.FolderTree.SelectedNode = node;

        if(File.Exists(databasePath))
        {
            File.Delete(databasePath);
        }

        Directory.CreateDirectory(databasePath);

        try
        {
            vm.Accounts.SelectedAccount = account;
            await WaitForConditionAsync(() => !string.IsNullOrWhiteSpace(vm.Sync.SyncError));
        }
        finally
        {
            var dbDirectory = Path.GetDirectoryName(databasePath) ?? string.Empty;
            if(Directory.Exists(dbDirectory))
            {
                Directory.Delete(dbDirectory, true);
            }
        }

        vm.FolderTree.Nodes.ShouldBeEmpty();
        vm.FolderTree.SelectedNode.ShouldBeNull();
        vm.Sync.SyncError.ShouldNotBeNullOrWhiteSpace();
    }

    private static async Task WaitForConditionAsync(Func<bool> condition)
    {
        for(var attempt = 0; attempt < 50 && !condition(); attempt++)
        {
            if(!condition())
            {
                await Task.Delay(10, TestContext.Current.CancellationToken);
            }
        }

        condition().ShouldBeTrue();
    }
}
