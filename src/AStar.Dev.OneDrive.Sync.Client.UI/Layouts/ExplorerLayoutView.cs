using System.Windows.Input;
using AStar.Dev.OneDrive.Sync.Client.UI.Common;
using AStar.Dev.OneDrive.Sync.Client.UI.Home;
using ReactiveUI;

namespace AStar.Dev.OneDrive.Sync.Client.UI.Layouts;

/// <summary>
/// ViewModel for the Explorer layout view.
/// </summary>
public class ExplorerLayoutViewModel : ViewModelBase
{
    /// <summary>
    /// Gets the main window view model.
    /// </summary>
    public MainWindowViewModel MainWindow { get; }

    /// <summary>
    /// Gets or sets the synchronization summary text.
    /// </summary>
    public string SyncSummary
    {
        get;
        set { field = value; _ = this.RaiseAndSetIfChanged(ref field, value); }
    } = "Idle";

    /// <summary>
    /// Gets the command to refresh the synchronization summary.
    /// </summary>
    public ICommand RefreshSummaryCommand { get; }

    /// <summary>
    /// Gets or sets the currently selected theme.
    /// </summary>
    public string CurrentTheme
    {
        get;
        set { field = value; _ = this.RaiseAndSetIfChanged(ref field, value); }
    } = "System";

    /// <summary>
    /// Initializes a new instance of the <see cref="ExplorerLayoutViewModel"/> class.
    /// </summary>
    /// <param name="mainWindow">The main window view model.</param>
    public ExplorerLayoutViewModel(MainWindowViewModel mainWindow)
    {
        MainWindow = mainWindow;
        RefreshSummaryCommand = new RelayCommand(_ => SyncSummary = MainWindow.Sync.Status);
    }
}
