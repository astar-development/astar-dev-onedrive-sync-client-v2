using AStar.Dev.OneDrive.Sync.Client.UI.AccountManagement;
using AStar.Dev.OneDrive.Sync.Client.UI.Common;
using AStar.Dev.OneDrive.Sync.Client.UI.FolderTrees;
using AStar.Dev.OneDrive.Sync.Client.UI.Home;
using AStar.Dev.OneDrive.Sync.Client.UI.Layouts;
using AStar.Dev.OneDrive.Sync.Client.UI.Logs;
using AStar.Dev.OneDrive.Sync.Client.UI.Settings;
using AStar.Dev.OneDrive.Sync.Client.UI.SyncStatus;
using Avalonia.Controls;
using Avalonia.LogicalTree;

namespace AStar.Dev.OneDrive.Sync.Client.UI.Tests.Layouts;

public sealed class LayoutsShould
{
    [Fact]
    public void ContainExpectedEmbeddedViewsAcrossLayouts()
    {
        var viewModel = new MainWindowViewModel();

        AssertLayoutState(viewModel, LayoutType.Explorer, expectedLayoutName: "Explorer", expectedViewType: typeof(ExplorerLayoutView), hasAccountView: true, hasFolderView: true,
            hasSyncView: true, hasLogsView: false, hasSettingsView: false);

        viewModel.CurrentLayout = LayoutType.Dashboard;
        AssertLayoutState(viewModel, LayoutType.Dashboard, expectedLayoutName: "Dashboard", expectedViewType: typeof(DashboardLayoutView), hasAccountView: true,
            hasFolderView: false, hasSyncView: true, hasLogsView: false, hasSettingsView: false);

        viewModel.CurrentLayout = LayoutType.Terminal;
        AssertLayoutState(viewModel, LayoutType.Terminal, expectedLayoutName: "Terminal", expectedViewType: typeof(TerminalLayoutView), hasAccountView: true,
            hasFolderView: true, hasSyncView: true, hasLogsView: true, hasSettingsView: true);
    }

    [Fact]
    public void DefineMainWindowAsTheTopLevelControl() => typeof(TopLevel).IsAssignableFrom(typeof(MainWindow)).ShouldBeTrue();

    [Theory]
    [InlineData(typeof(ExplorerLayoutView))]
    [InlineData(typeof(DashboardLayoutView))]
    [InlineData(typeof(TerminalLayoutView))]
    [InlineData(typeof(FolderTreeView))]
    [InlineData(typeof(SyncStatusView))]
    [InlineData(typeof(SettingsView))]
    [InlineData(typeof(LogsView))]
    public void HaveTheExpectedBaseTypeForViews(Type embeddedViewType) => typeof(UserControl).IsAssignableFrom(embeddedViewType).ShouldBeTrue();

    private static void AssertLayoutState(MainWindowViewModel viewModel, LayoutType expectedLayout, string expectedLayoutName, Type expectedViewType,
        bool hasAccountView, bool hasFolderView, bool hasSyncView, bool hasLogsView, bool hasSettingsView)
    {
        viewModel.CurrentLayout.ShouldBe(expectedLayout);
        viewModel.Settings.SelectedLayout.ShouldBe(expectedLayoutName);

        if(viewModel.CurrentLayoutView is not Control layout)
        {
            return;
        }

        layout.GetType().ShouldBe(expectedViewType);

        _ = new ContentControl
        {
            Content = layout,
            DataContext = viewModel
        };

        layout.DataContext = null;
        layout.DataContext = viewModel;
        layout.DataContext.ShouldBeSameAs(viewModel);

        AssertControlPresence<AccountListView>(layout, hasAccountView);
        AssertControlPresence<FolderTreeView>(layout, hasFolderView);
        AssertControlPresence<SyncStatusView>(layout, hasSyncView);
        AssertControlPresence<LogsView>(layout, hasLogsView);
        AssertControlPresence<SettingsView>(layout, hasSettingsView);
    }

    private static void AssertControlPresence<TControl>(Control layout, bool shouldExist)
        where TControl : Control
    {
        TControl? control = GetLogicalDescendants(layout).OfType<TControl>().FirstOrDefault();

        if(!shouldExist)
        {
            control.ShouldBeNull();
            return;
        }

        _ = control.ShouldNotBeNull();
    }

    private static IEnumerable<Control> GetLogicalDescendants(Control root)
    {
        if(root is not ILogical logical)
        {
            yield break;
        }

        foreach(ILogical child in logical.LogicalChildren)
        {
            if(child is not Control control)
            {
                continue;
            }

            yield return control;

            foreach(Control descendant in GetLogicalDescendants(control))
            {
                yield return descendant;
            }
        }
    }
}
