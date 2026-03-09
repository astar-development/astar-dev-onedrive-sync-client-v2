using System.Windows.Input;
using AStar.Dev.OneDrive.Sync.Client.UI.Common;
using AStar.Dev.OneDrive.Sync.Client.UI.Home;
using ReactiveUI;
using System.ComponentModel;

namespace AStar.Dev.OneDrive.Sync.Client.UI.Layouts;

/// <summary>
/// ViewModel for the Terminal layout view.
/// </summary>
public class TerminalLayoutViewModel : ViewModelBase
{
    /// <summary>
    /// Gets the main window view model.
    /// </summary>
    public MainWindowViewModel MainWindow { get; }

    /// <summary>
    /// Gets the terminal status text.
    /// </summary>
    public string TerminalStatus => MainWindow.TerminalOperationalStatus;

    /// <summary>
    /// Gets the command to trigger sync now.
    /// </summary>
    public ICommand SyncNowCommand => MainWindow.Sync.StartSyncCommand;

    /// <summary>
    /// Gets the command to pause sync.
    /// </summary>
    public ICommand PauseSyncCommand => MainWindow.Sync.PauseSyncCommand;

    /// <summary>
    /// Gets the command to resume sync.
    /// </summary>
    public ICommand ResumeSyncCommand => MainWindow.Sync.ResumeSyncCommand;

    /// <summary>
    /// Initializes a new instance of the <see cref="TerminalLayoutViewModel"/> class.
    /// </summary>
    /// <param name="mainWindow">The main window view model.</param>
    public TerminalLayoutViewModel(MainWindowViewModel mainWindow)
    {
        MainWindow = mainWindow;
        MainWindow.PropertyChanged += OnMainWindowPropertyChanged;
    }

    private void OnMainWindowPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if(string.Equals(e.PropertyName, nameof(MainWindowViewModel.TerminalOperationalStatus), StringComparison.Ordinal))
        {
            this.RaisePropertyChanged(nameof(TerminalStatus));
        }
    }
}
