using AStar.Dev.OneDrive.Sync.Client.UI.Home;
using AStar.Dev.OneDrive.Sync.Client.UI.Layouts;

namespace AStar.Dev.OneDrive.Sync.Client.UI.Tests.Layouts;

public sealed class LayoutViewModelShould
{
    [Fact]
    public void InstantiateWithMainWindowContextWithExplorerLayoutViewModel()
    {
        MainWindowViewModel mainWindowViewModel = CreateMainWindowContext();

        var sut = new ExplorerLayoutViewModel(mainWindowViewModel);

        sut.MainWindow.ShouldBeSameAs(mainWindowViewModel);
        sut.MainWindow.Accounts.ShouldBeSameAs(mainWindowViewModel.Accounts);
    }

    [Fact]
    public void InstantiateWithMainWindowContextWithDashboardLayoutViewModel()
    {
        MainWindowViewModel mainWindowViewModel = CreateMainWindowContext();

        var sut = new DashboardLayoutViewModel(mainWindowViewModel);

        sut.MainWindow.ShouldBeSameAs(mainWindowViewModel);
        sut.MainWindow.Settings.ShouldBeSameAs(mainWindowViewModel.Settings);
    }

    [Fact]
    public void InstantiateWithMainWindowContextWithTerminalLayoutViewModel()
    {
        MainWindowViewModel mainWindowViewModel = CreateMainWindowContext();

        var sut = new TerminalLayoutViewModel(mainWindowViewModel);

        sut.MainWindow.ShouldBeSameAs(mainWindowViewModel);
        sut.MainWindow.Sync.ShouldBeSameAs(mainWindowViewModel.Sync);
    }

    [Fact]
    public void ReflectSyncStatusWhenExplorerLayoutViewModelIsCreated()
    {
        MainWindowViewModel mainWindowViewModel = CreateMainWindowContext();
        var sut = new ExplorerLayoutViewModel(mainWindowViewModel);

        sut.SyncSummary.ShouldBe(mainWindowViewModel.Sync.Status);
    }

    [Fact]
    public void ReflectThemeWhenDashboardLayoutViewModelTracksSettings()
    {
        MainWindowViewModel mainWindowViewModel = CreateMainWindowContext();
        var sut = new DashboardLayoutViewModel(mainWindowViewModel);

        mainWindowViewModel.Settings.SelectedTheme = "Dark";

        sut.CurrentTheme.ShouldBe("Dark");
    }

    [Fact]
    public void ReflectTerminalOperationalStatusWhenSyncChanges()
    {
        MainWindowViewModel mainWindowViewModel = CreateMainWindowContext();
        var sut = new TerminalLayoutViewModel(mainWindowViewModel);

        mainWindowViewModel.Sync.Status = "Syncing...";

        sut.TerminalStatus.ShouldContain("Syncing");
    }

    private static MainWindowViewModel CreateMainWindowContext()
        => new(initializeLayoutView: false);
}
