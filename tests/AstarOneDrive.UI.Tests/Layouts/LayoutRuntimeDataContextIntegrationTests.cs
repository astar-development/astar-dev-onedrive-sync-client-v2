using AstarOneDrive.UI.AccountManagement;
using AstarOneDrive.UI.Common;
using AstarOneDrive.UI.FolderTrees;
using AstarOneDrive.UI.Home;
using AstarOneDrive.UI.Layouts;
using AstarOneDrive.UI.Logs;
using AstarOneDrive.UI.Settings;
using AstarOneDrive.UI.SyncStatus;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Shouldly;

namespace AstarOneDrive.UI.Tests.Layouts;

public sealed class LayoutRuntimeDataContextIntegrationTests
{
    [Fact]
    public void MainWindowViewModel_CurrentLayoutView_ContainsExpectedEmbeddedViewsAcrossLayouts()
    {
        var viewModel = new MainWindowViewModel();

        AssertLayoutState(
            viewModel,
            LayoutType.Explorer,
            expectedLayoutName: "Explorer",
            expectedViewType: typeof(ExplorerLayoutView),
            hasAccountView: true,
            hasFolderView: true,
            hasSyncView: true,
            hasLogsView: false,
            hasSettingsView: false);

        viewModel.CurrentLayout = LayoutType.Dashboard;
        AssertLayoutState(
            viewModel,
            LayoutType.Dashboard,
            expectedLayoutName: "Dashboard",
            expectedViewType: typeof(DashboardLayoutView),
            hasAccountView: true,
            hasFolderView: false,
            hasSyncView: true,
            hasLogsView: false,
            hasSettingsView: false);

        viewModel.CurrentLayout = LayoutType.Terminal;
        AssertLayoutState(
            viewModel,
            LayoutType.Terminal,
            expectedLayoutName: "Terminal",
            expectedViewType: typeof(TerminalLayoutView),
            hasAccountView: true,
            hasFolderView: true,
            hasSyncView: true,
            hasLogsView: true,
            hasSettingsView: true);
    }

    private static void AssertLayoutState(
        MainWindowViewModel viewModel,
        LayoutType expectedLayout,
        string expectedLayoutName,
        Type expectedViewType,
        bool hasAccountView,
        bool hasFolderView,
        bool hasSyncView,
        bool hasLogsView,
        bool hasSettingsView)
    {
        viewModel.CurrentLayout.ShouldBe(expectedLayout);
        viewModel.Settings.SelectedLayout.ShouldBe(expectedLayoutName);

        if (viewModel.CurrentLayoutView is not Control layout)
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
        var control = GetLogicalDescendants(layout).OfType<TControl>().FirstOrDefault();

        if (!shouldExist)
        {
            control.ShouldBeNull();
            return;
        }

        control.ShouldNotBeNull();
    }

    private static IEnumerable<Control> GetLogicalDescendants(Control root)
    {
        if (root is not ILogical logical)
        {
            yield break;
        }

        foreach (var child in logical.LogicalChildren)
        {
            if (child is not Control control)
            {
                continue;
            }

            yield return control;

            foreach (var descendant in GetLogicalDescendants(control))
            {
                yield return descendant;
            }
        }
    }

}