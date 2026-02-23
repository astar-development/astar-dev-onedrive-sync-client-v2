using System.Windows.Input;
using AstarOneDrive.UI.Common;
using AstarOneDrive.UI.Home;
using ReactiveUI;

namespace AstarOneDrive.UI.Layouts;

public class ExplorerLayoutViewModel : ViewModelBase
{
    public MainWindowViewModel MainWindow { get; }

    public string SyncSummary
    {
        get;
        set { field = value; this.RaiseAndSetIfChanged(ref field, value); }
    } = "Idle";

    public ICommand RefreshSummaryCommand { get; }

    public string CurrentTheme
    {
        get;
        set { field = value; this.RaiseAndSetIfChanged(ref field, value); }
    } = "System";

    public ExplorerLayoutViewModel(MainWindowViewModel mainWindow)
    {
        MainWindow = mainWindow;
        RefreshSummaryCommand = new RelayCommand(_ => SyncSummary = MainWindow.Sync.Status);
    }
}
