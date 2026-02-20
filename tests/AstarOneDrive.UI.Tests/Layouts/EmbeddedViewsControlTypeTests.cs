using AstarOneDrive.UI.FolderTrees;
using AstarOneDrive.UI.Home;
using AstarOneDrive.UI.Layouts;
using AstarOneDrive.UI.Logs;
using AstarOneDrive.UI.Settings;
using AstarOneDrive.UI.SyncStatus;
using Avalonia.Controls;
using Shouldly;

namespace AstarOneDrive.UI.Tests.Layouts;

public sealed class EmbeddedViewsControlTypeTests
{
    [Fact]
    public void MainWindow_IsTopLevelControl()
    {
        typeof(TopLevel).IsAssignableFrom(typeof(MainWindow)).ShouldBeTrue();
    }

    [Fact]
    public void EmbeddedViews_AreUsedAsChildren_AreNotTopLevelControls()
    {
        Type[] embeddedViewTypes =
        [
            typeof(ExplorerLayoutView),
            typeof(DashboardLayoutView),
            typeof(TerminalLayoutView),
            typeof(FolderTreeView),
            typeof(SyncStatusView),
            typeof(SettingsView),
            typeof(LogsView)
        ];

        foreach (Type embeddedViewType in embeddedViewTypes)
        {
            typeof(TopLevel).IsAssignableFrom(embeddedViewType).ShouldBeFalse();
        }
    }
}