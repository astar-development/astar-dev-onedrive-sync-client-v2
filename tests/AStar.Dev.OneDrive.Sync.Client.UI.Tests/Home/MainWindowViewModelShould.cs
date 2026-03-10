using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.UI.AccountManagement;
using AStar.Dev.OneDrive.Sync.Client.UI.Common;
using AStar.Dev.OneDrive.Sync.Client.UI.FolderTrees;
using AStar.Dev.OneDrive.Sync.Client.UI.Home;
using AStar.Dev.OneDrive.Sync.Client.UI.Logs;
using AStar.Dev.OneDrive.Sync.Client.UI.Settings;
using AStar.Dev.OneDrive.Sync.Client.UI.SyncStatus;
using AStar.Dev.OneDrive.Sync.Client.UI.Tests.ThemeManager;
using AStar.Dev.Utilities;

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
    public async Task ReloadFolderTreeWhenSelectedAccountChanges()
    {
        var databasePath = Path.GetTempPath().CombinePath($"astar-ui-main-window-tests-{Guid.NewGuid():N}", "astar-onedrive.db");
        var accountA = new AccountInfo("acct-a", "a@example.com", 0, 0);
        var accountB = new AccountInfo("acct-b", "b@example.com", 0, 0);

        var seedA = new FolderTreeViewModel(databasePath);
        seedA.Nodes.Add(new FolderNode(Guid.NewGuid().ToString("N"), "A Root", true, false, []));
        Result<bool, Exception> saveA = await seedA.SaveTreeAsync(accountA.Id, TestContext.Current.CancellationToken);
        Pattern.IsSuccess(saveA).ShouldBeTrue();

        var seedB = new FolderTreeViewModel(databasePath);
        seedB.Nodes.Add(new FolderNode(Guid.NewGuid().ToString("N"), "B Root", false, true, []));
        Result<bool, Exception> saveB = await seedB.SaveTreeAsync(accountB.Id, TestContext.Current.CancellationToken);
        Pattern.IsSuccess(saveB).ShouldBeTrue();

        var vm = new MainWindowViewModel(databasePath, initializeLayoutView: false);
        vm.Accounts.Accounts.Clear();
        vm.Accounts.Accounts.Add(accountA);
        vm.Accounts.Accounts.Add(accountB);

        vm.Accounts.SelectedAccount = accountA;
        await WaitForConditionAsync(() => vm.FolderTree.Nodes.Count == 1 && vm.FolderTree.Nodes[0].Name == "A Root");
        vm.FolderTree.Nodes[0].Name.ShouldBe("A Root");

        vm.Accounts.SelectedAccount = accountB;
        await WaitForConditionAsync(() => vm.FolderTree.Nodes.Count == 1 && vm.FolderTree.Nodes[0].Name == "B Root");
        vm.FolderTree.Nodes[0].Name.ShouldBe("B Root");
    }

    [Fact]
    public async Task LoadFolderTreeOnStartupForInitiallySelectedAccount()
    {
        var databasePath = Path.GetTempPath().CombinePath($"astar-ui-main-window-tests-{Guid.NewGuid():N}", "astar-onedrive.db");
        var selectedAccount = new AccountInfo("acct-startup", "startup@example.com", 0, 0);

        var seededAccounts = new AccountListViewModel(databasePath);
        seededAccounts.Accounts.Clear();
        seededAccounts.Accounts.Add(selectedAccount);
        Result<bool, Exception> saveAccounts = await seededAccounts.SaveAccountsAsync(TestContext.Current.CancellationToken);
        Pattern.IsSuccess(saveAccounts).ShouldBeTrue();

        var seededTree = new FolderTreeViewModel(databasePath);
        seededTree.Nodes.Add(new FolderNode(Guid.NewGuid().ToString("N"), "Startup Root", true, true, []));
        Result<bool, Exception> saveTree = await seededTree.SaveTreeAsync(selectedAccount.Id, TestContext.Current.CancellationToken);
        Pattern.IsSuccess(saveTree).ShouldBeTrue();

        var vm = new MainWindowViewModel(databasePath, initializeLayoutView: true);
        await WaitForConditionAsync(() => vm.FolderTree.Nodes.Count == 1 && vm.FolderTree.Nodes[0].Name == "Startup Root");

        vm.FolderTree.Nodes[0].Name.ShouldBe("Startup Root");
    }

    [Fact]
    public void UseSelectedAccountLocalSyncRootWhenUpdatingSyncRunContext()
    {
        var vm = new MainWindowViewModel(initializeLayoutView: false);
        vm.Accounts.Accounts.Clear();
        vm.Accounts.Accounts.Add(new AccountInfo("acct-root", "root@example.com", 0, 0, "/tmp/astar-sync/root-account"));

        vm.Accounts.SelectedAccount = vm.Accounts.Accounts[0];

        System.Reflection.FieldInfo? rootPathField = typeof(SyncStatusViewModel).GetField("_rootPath", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        rootPathField.ShouldNotBeNull();
        rootPathField!.GetValue(vm.Sync).ShouldBe("/tmp/astar-sync/root-account");
    }

    private static async Task WaitForConditionAsync(Func<bool> condition)
    {
        for(var attempt = 0; attempt < 60; attempt++)
        {
            if(condition())
            {
                return;
            }

            await Task.Delay(20, TestContext.Current.CancellationToken);
        }

        throw new TimeoutException("Condition was not met within the allowed time.");
    }
}
