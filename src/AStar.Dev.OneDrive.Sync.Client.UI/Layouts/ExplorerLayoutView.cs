using System.Windows.Input;
using AStar.Dev.OneDrive.Sync.Client.UI.Common;
using AStar.Dev.OneDrive.Sync.Client.UI.Home;
using ReactiveUI;

namespace AStar.Dev.OneDrive.Sync.Client.UI.Layouts;

public class ExplorerLayoutViewModel : ViewModelBase
{
    public MainWindowViewModel MainWindow { get; }

    public string SyncSummary
    {
        get;
        set { field = value; _ = this.RaiseAndSetIfChanged(ref field, value); }
    } = "Idle";

    public ICommand RefreshSummaryCommand { get; }

    public string CurrentTheme
    {
        get;
        set { field = value; _ = this.RaiseAndSetIfChanged(ref field, value); }
    } = "System";

    public ExplorerLayoutViewModel(MainWindowViewModel mainWindow)
    {
        MainWindow = mainWindow;
        RefreshSummaryCommand = new RelayCommand(_ => SyncSummary = MainWindow.Sync.Status);
    }
}
