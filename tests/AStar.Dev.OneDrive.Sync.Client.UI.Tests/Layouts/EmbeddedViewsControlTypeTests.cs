using AStar.Dev.OneDrive.Sync.Client.UI.FolderTrees;
using AStar.Dev.OneDrive.Sync.Client.UI.Home;
using AStar.Dev.OneDrive.Sync.Client.UI.Layouts;
using AStar.Dev.OneDrive.Sync.Client.UI.Logs;
using AStar.Dev.OneDrive.Sync.Client.UI.Settings;
using AStar.Dev.OneDrive.Sync.Client.UI.SyncStatus;
using Avalonia.Controls;
using Shouldly;

namespace AStar.Dev.OneDrive.Sync.Client.UI.Tests.Layouts;

public sealed class EmbeddedViewsControlTypeTests
{
    [Fact]
    public void MainWindow_IsTopLevelControl() => typeof(TopLevel).IsAssignableFrom(typeof(MainWindow)).ShouldBeTrue();

    [Theory]
    [InlineData(typeof(ExplorerLayoutView))]
    [InlineData(typeof(DashboardLayoutView))]
    [InlineData(typeof(TerminalLayoutView))]
    [InlineData(typeof(FolderTreeView))]
    [InlineData(typeof(SyncStatusView))]
    [InlineData(typeof(SettingsView))]
    [InlineData(typeof(LogsView))]
    public void EmbeddedViews_HaveExpectedBaseType(Type embeddedViewType) => typeof(UserControl).IsAssignableFrom(embeddedViewType).ShouldBeTrue();
}
