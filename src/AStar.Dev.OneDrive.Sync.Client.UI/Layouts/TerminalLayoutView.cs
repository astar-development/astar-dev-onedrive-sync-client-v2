using System.Windows.Input;
using AStar.Dev.OneDrive.Sync.Client.UI.Common;
using AStar.Dev.OneDrive.Sync.Client.UI.Home;
using ReactiveUI;

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
    /// Gets or sets the terminal status text.
    /// </summary>
    public string TerminalStatus
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = "Ready";

    /// <summary>
    /// Gets the command to run a health check.
    /// </summary>
    public ICommand RunHealthCheckCommand { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TerminalLayoutViewModel"/> class.
    /// </summary>
    /// <param name="mainWindow">The main window view model.</param>
    public TerminalLayoutViewModel(MainWindowViewModel mainWindow)
    {
        MainWindow = mainWindow;
        RunHealthCheckCommand = new RelayCommand(_ => TerminalStatus = "Connected");
    }
}
