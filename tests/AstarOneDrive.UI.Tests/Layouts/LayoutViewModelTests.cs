using AstarOneDrive.UI.Home;
using AstarOneDrive.UI.Layouts;
using Shouldly;

namespace AstarOneDrive.UI.Tests.ViewModels.Layouts;

public sealed class LayoutViewModelTests
{
    [Fact]
    public void ExplorerLayoutViewModel_CanInstantiateWithMainWindowContext()
    {
        var mainWindowViewModel = CreateMainWindowContext();

        var sut = new ExplorerLayoutViewModel(mainWindowViewModel);

        sut.MainWindow.ShouldBeSameAs(mainWindowViewModel);
        sut.MainWindow.Accounts.ShouldBeSameAs(mainWindowViewModel.Accounts);
    }

    [Fact]
    public void DashboardLayoutViewModel_CanInstantiateWithMainWindowContext()
    {
        var mainWindowViewModel = CreateMainWindowContext();

        var sut = new DashboardLayoutViewModel(mainWindowViewModel);

        sut.MainWindow.ShouldBeSameAs(mainWindowViewModel);
        sut.MainWindow.Settings.ShouldBeSameAs(mainWindowViewModel.Settings);
    }

    [Fact]
    public void TerminalLayoutViewModel_CanInstantiateWithMainWindowContext()
    {
        var mainWindowViewModel = CreateMainWindowContext();

        var sut = new TerminalLayoutViewModel(mainWindowViewModel);

        sut.MainWindow.ShouldBeSameAs(mainWindowViewModel);
        sut.MainWindow.Sync.ShouldBeSameAs(mainWindowViewModel.Sync);
    }

    [Fact]
    public void ExplorerLayoutCommand_Execute_UpdatesSummary()
    {
        var mainWindowViewModel = CreateMainWindowContext();
        var sut = new ExplorerLayoutViewModel(mainWindowViewModel);

        sut.RefreshSummaryCommand.Execute(null);

        sut.SyncSummary.ShouldBe(mainWindowViewModel.Sync.Status);
    }

    [Fact]
    public void DashboardLayoutCommand_Execute_ChangesThemeSelection()
    {
        var mainWindowViewModel = CreateMainWindowContext();
        var sut = new DashboardLayoutViewModel(mainWindowViewModel);
        var initialTheme = sut.CurrentTheme;

        sut.CycleThemeCommand.Execute(null);

        sut.CurrentTheme.ShouldNotBe(initialTheme);
    }

    [Fact]
    public void TerminalLayoutCommand_Execute_UpdatesTerminalStatus()
    {
        var mainWindowViewModel = CreateMainWindowContext();
        var sut = new TerminalLayoutViewModel(mainWindowViewModel);

        sut.RunHealthCheckCommand.Execute(null);

        sut.TerminalStatus.ShouldBe("Connected");
    }

    private static MainWindowViewModel CreateMainWindowContext() =>
        new(initializeLayoutView: false);
}