using AStar.Dev.OneDrive.Sync.Client.UI.Home;
using AStar.Dev.OneDrive.Sync.Client.UI.Layouts;
using Shouldly;

namespace AStar.Dev.OneDrive.Sync.Client.UI.Tests.ViewModels;

public sealed class MainWindowViewModelTests
{
    [Fact]
    public void CurrentLayout_StartsAsExplorer()
    {
        var vm = new MainWindowViewModel();
        vm.CurrentLayout.ShouldBe(LayoutType.Explorer);
    }

    [Fact]
    public void SwitchToExplorerCommand_SetsLayoutToExplorer()
    {
        var vm = new MainWindowViewModel
        {
            CurrentLayout = LayoutType.Dashboard
        };
        vm.SwitchToExplorerCommand.Execute(null);
        vm.CurrentLayout.ShouldBe(LayoutType.Explorer);
    }

    [Fact]
    public void SwitchToDashboardCommand_SetsLayoutToDashboard()
    {
        var vm = new MainWindowViewModel
        {
            CurrentLayout = LayoutType.Explorer
        };
        vm.SwitchToDashboardCommand.Execute(null);
        vm.CurrentLayout.ShouldBe(LayoutType.Dashboard);
    }

    [Fact]
    public void SwitchToTerminalCommand_SetsLayoutToTerminal()
    {
        var vm = new MainWindowViewModel
        {
            CurrentLayout = LayoutType.Explorer
        };
        vm.SwitchToTerminalCommand.Execute(null);
        vm.CurrentLayout.ShouldBe(LayoutType.Terminal);
    }

    [Fact]
    public void SharedVMs_RemainSameInstanceAcrossLayoutSwaps()
    {
        var vm = new MainWindowViewModel();
        var accountsInstance = vm.Accounts;
        var folderTreeInstance = vm.FolderTree;
        var syncInstance = vm.Sync;
        var logsInstance = vm.Logs;
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
    public void LayoutChange_UpdatesSettingsViewModel()
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
    public async Task SettingsViewModel_LayoutChanged_UpdatesMainWindowLayout()
    {
        var vm = new MainWindowViewModel();
        await vm.Settings.LoadSettingsAsync();
        vm.CurrentLayout.ShouldBe(LayoutType.Explorer);

        vm.Settings.SelectedLayout = "Dashboard";
        vm.CurrentLayout.ShouldBe(LayoutType.Dashboard);

        vm.Settings.SelectedLayout = "Terminal";
        vm.CurrentLayout.ShouldBe(LayoutType.Terminal);
    }
}
