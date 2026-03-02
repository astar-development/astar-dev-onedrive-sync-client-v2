using AStar.Dev.OneDrive.Sync.Client.UI.AccountManagement;
using AStar.Dev.OneDrive.Sync.Client.UI.Common;
using AStar.Dev.OneDrive.Sync.Client.UI.FolderTrees;
using AStar.Dev.OneDrive.Sync.Client.UI.Home;
using AStar.Dev.OneDrive.Sync.Client.UI.Layouts;
using AStar.Dev.OneDrive.Sync.Client.UI.Settings;
using AStar.Dev.OneDrive.Sync.Client.UI.SyncStatus;
using AStar.Dev.OneDrive.Sync.Client.UI.Tests.ThemeManager;
using Shouldly;
using Xunit;
using ThemeManagerStatic = AStar.Dev.OneDrive.Sync.Client.UI.ThemeManager.ThemeManager;

#pragma warning disable IDE0007, IDE0017

namespace AStar.Dev.OneDrive.Sync.Client.UI.Tests.Layouts;

[Collection(ThemeManagerTestCollection.Name)]
public sealed class LayoutEdgeCasesShould
{
    [Fact]
    public void PreserveLayoutStateWhenThemeSwitchesRapidly()
    {
        var viewModel = new MainWindowViewModel();
        viewModel.CurrentLayout = LayoutType.Explorer;

        var themes = new[] { "Light", "Dark" };

        foreach (string theme in themes)
        {
            foreach (int _ in Enumerable.Range(0, 5))
            {
                ThemeManagerStatic.ApplyTheme(theme);
                viewModel.CurrentLayout.ShouldBe(LayoutType.Explorer);
                viewModel.Settings.SelectedLayout.ShouldBe("Explorer");
            }
        }
    }

    [Fact]
    public void MaintainLayoutViewTypeAfterMultipleLayoutSwitches()
    {
        MainWindowViewModel viewModel = new();
        LayoutType[] layoutSequence = [LayoutType.Explorer, LayoutType.Dashboard, LayoutType.Terminal, LayoutType.Explorer, LayoutType.Dashboard];

        var expectedViewTypes = new Dictionary<LayoutType, Type>
        {
            [LayoutType.Explorer] = typeof(ExplorerLayoutView),
            [LayoutType.Dashboard] = typeof(DashboardLayoutView),
            [LayoutType.Terminal] = typeof(TerminalLayoutView)
        };

        foreach (LayoutType layout in layoutSequence)
        {
            viewModel.CurrentLayout = layout;
            viewModel.CurrentLayoutView?.GetType().ShouldBe(expectedViewTypes[layout]);
        }
    }

    [Fact]
    public void PreserveViewModelInstancesAcrossLayoutSwitches()
    {
        MainWindowViewModel viewModel = new();
        AccountListViewModel originalAccounts = viewModel.Accounts;
        FolderTreeViewModel originalFolderTree = viewModel.FolderTree;
        SyncStatusViewModel originalSync = viewModel.Sync;

        viewModel.CurrentLayout = LayoutType.Dashboard;
        viewModel.Accounts.ShouldBeSameAs(originalAccounts);
        viewModel.FolderTree.ShouldBeSameAs(originalFolderTree);
        viewModel.Sync.ShouldBeSameAs(originalSync);

        viewModel.CurrentLayout = LayoutType.Terminal;
        viewModel.Accounts.ShouldBeSameAs(originalAccounts);
        viewModel.FolderTree.ShouldBeSameAs(originalFolderTree);
        viewModel.Sync.ShouldBeSameAs(originalSync);
    }

    [Fact]
    public void HandleRapidThemeSwitchesAcrossMultipleLayouts()
    {
        MainWindowViewModel viewModel = new();

        viewModel.CurrentLayout = LayoutType.Dashboard;
        ThemeManagerStatic.ApplyTheme("Dark");

        viewModel.CurrentLayout = LayoutType.Terminal;
        ThemeManagerStatic.ApplyTheme("Light");

        viewModel.CurrentLayout = LayoutType.Explorer;
        ThemeManagerStatic.ApplyTheme("Dark");

        viewModel.CurrentLayout.ShouldBe(LayoutType.Explorer);
    }

    [Fact]
    public void NotThrowWhenSwitchingLayoutsDuringSettingsChanges()
    {
        MainWindowViewModel viewModel = new();
        SettingsViewModel settingsVm = viewModel.Settings;
        string originalTheme = settingsVm.SelectedTheme;

        foreach (LayoutType layout in new[] { LayoutType.Explorer, LayoutType.Dashboard, LayoutType.Terminal })
        {
            viewModel.CurrentLayout = layout;
            settingsVm.SelectedTheme = "Light";
            settingsVm.SelectedTheme = "Dark";
            settingsVm.SelectedTheme = originalTheme;
        }

        viewModel.CurrentLayout.ShouldBe(LayoutType.Terminal);
    }
}
