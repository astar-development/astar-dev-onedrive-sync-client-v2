using System.Windows.Input;
using AStar.Dev.OneDrive.Sync.Client.UI.Common;
using AStar.Dev.OneDrive.Sync.Client.UI.Home;
using AStar.Dev.OneDrive.Sync.Client.UI.Settings;
using AStar.Dev.OneDrive.Sync.Client.UI.SyncStatus;
using ReactiveUI;
using System.ComponentModel;

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
    /// Gets the synchronization summary text.
    /// </summary>
    public string SyncSummary => MainWindow.Sync.Status;

    /// <summary>
    /// Gets the command to start synchronization.
    /// </summary>
    public ICommand StartSyncCommand => MainWindow.Sync.StartSyncCommand;

    /// <summary>
    /// Gets the currently selected theme.
    /// </summary>
    public string CurrentTheme => MainWindow.Settings.SelectedTheme;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExplorerLayoutViewModel"/> class.
    /// </summary>
    /// <param name="mainWindow">The main window view model.</param>
    public ExplorerLayoutViewModel(MainWindowViewModel mainWindow)
    {
        MainWindow = mainWindow;
        MainWindow.Sync.PropertyChanged += OnSyncPropertyChanged;
        MainWindow.Settings.PropertyChanged += OnSettingsPropertyChanged;
    }

    private void OnSyncPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if(string.Equals(e.PropertyName, nameof(SyncStatusViewModel.Status), StringComparison.Ordinal))
        {
            this.RaisePropertyChanged(nameof(SyncSummary));
        }
    }

    private void OnSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if(string.Equals(e.PropertyName, nameof(SettingsViewModel.SelectedTheme), StringComparison.Ordinal))
        {
            this.RaisePropertyChanged(nameof(CurrentTheme));
        }
    }
}
