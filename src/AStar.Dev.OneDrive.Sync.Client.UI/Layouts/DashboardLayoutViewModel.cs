using System.Collections.ObjectModel;
using System.Windows.Input;
using AStar.Dev.OneDrive.Sync.Client.UI.Common;
using AStar.Dev.OneDrive.Sync.Client.UI.Home;
using AStar.Dev.OneDrive.Sync.Client.UI.Settings;
using ReactiveUI;
using System.ComponentModel;

namespace AStar.Dev.OneDrive.Sync.Client.UI.Layouts;

/// <summary>
/// ViewModel for the dashboard layout view.
/// </summary>
public class DashboardLayoutViewModel : ViewModelBase
{
    /// <summary>
    /// Gets the main window view model.
    /// </summary>
    public MainWindowViewModel MainWindow { get; }

    /// <summary>
    /// Gets the available themes.
    /// </summary>
    public ObservableCollection<string> Themes => MainWindow.Settings.AvailableThemes;

    /// <summary>
    /// Gets the command to open sync activity in terminal layout.
    /// </summary>
    public ICommand OpenSyncActivityCommand => MainWindow.SwitchToTerminalCommand;

    /// <summary>
    /// Gets the currently selected theme.
    /// </summary>
    public string CurrentTheme => MainWindow.Settings.SelectedTheme;

    /// <summary>
    /// Gets linked account count for dashboard metric cards.
    /// </summary>
    public int LinkedAccountCount => MainWindow.LinkedAccountCount;

    /// <summary>
    /// Gets selected account email for dashboard metric cards.
    /// </summary>
    public string SelectedAccountEmail => MainWindow.SelectedAccountEmail;

    /// <summary>
    /// Gets sync progress metric for dashboard metric cards.
    /// </summary>
    public int SyncProgressMetric => MainWindow.SyncProgressMetric;

    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardLayoutViewModel"/> class.
    /// </summary>
    /// <param name="mainWindow">The main window view model.</param>
    public DashboardLayoutViewModel(MainWindowViewModel mainWindow)
    {
        MainWindow = mainWindow;
        MainWindow.PropertyChanged += OnMainWindowPropertyChanged;
        MainWindow.Settings.PropertyChanged += OnSettingsPropertyChanged;
    }

    private void OnMainWindowPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if(string.Equals(e.PropertyName, nameof(MainWindowViewModel.LinkedAccountCount), StringComparison.Ordinal)
            || string.Equals(e.PropertyName, nameof(MainWindowViewModel.SelectedAccountEmail), StringComparison.Ordinal)
            || string.Equals(e.PropertyName, nameof(MainWindowViewModel.SyncProgressMetric), StringComparison.Ordinal))
        {
            this.RaisePropertyChanged(e.PropertyName);
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
