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