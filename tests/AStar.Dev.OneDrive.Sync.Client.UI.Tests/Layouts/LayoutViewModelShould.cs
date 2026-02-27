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
    public void UpdateSummaryWhenExplorerLayoutCommandIsExecuted()
    {
        MainWindowViewModel mainWindowViewModel = CreateMainWindowContext();
        var sut = new ExplorerLayoutViewModel(mainWindowViewModel);

        sut.RefreshSummaryCommand.Execute(null);

        sut.SyncSummary.ShouldBe(mainWindowViewModel.Sync.Status);
    }

    [Fact]
    public void ChangeThemeWhenDashboardLayoutCommandIsExecuted()
    {
        MainWindowViewModel mainWindowViewModel = CreateMainWindowContext();
        var sut = new DashboardLayoutViewModel(mainWindowViewModel);
        var initialTheme = sut.CurrentTheme;

        sut.CycleThemeCommand.Execute(null);

        sut.CurrentTheme.ShouldNotBe(initialTheme);
    }

    [Fact]
    public void UpdateTerminalStatusWhenTerminalLayoutCommandIsExecuted()
    {
        MainWindowViewModel mainWindowViewModel = CreateMainWindowContext();
        var sut = new TerminalLayoutViewModel(mainWindowViewModel);

        sut.RunHealthCheckCommand.Execute(null);

        sut.TerminalStatus.ShouldBe("Connected");
    }

    private static MainWindowViewModel CreateMainWindowContext()
        => new(initializeLayoutView: false);
}
