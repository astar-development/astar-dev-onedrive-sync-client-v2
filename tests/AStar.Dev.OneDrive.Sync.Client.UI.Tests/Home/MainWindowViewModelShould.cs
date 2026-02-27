using AStar.Dev.OneDrive.Sync.Client.UI.AccountManagement;
using AStar.Dev.OneDrive.Sync.Client.UI.Common;
using AStar.Dev.OneDrive.Sync.Client.UI.FolderTrees;
using AStar.Dev.OneDrive.Sync.Client.UI.Home;
using AStar.Dev.OneDrive.Sync.Client.UI.Logs;
using AStar.Dev.OneDrive.Sync.Client.UI.Settings;
using AStar.Dev.OneDrive.Sync.Client.UI.SyncStatus;

namespace AStar.Dev.OneDrive.Sync.Client.UI.Tests.Home;

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

        _ = await vm.Settings.LoadSettingsAsync(TestContext.Current.CancellationToken);

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
}
