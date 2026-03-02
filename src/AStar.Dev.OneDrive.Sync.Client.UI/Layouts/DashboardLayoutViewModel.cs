using System.Collections.ObjectModel;
using System.Windows.Input;
using AStar.Dev.OneDrive.Sync.Client.UI.Common;
using AStar.Dev.OneDrive.Sync.Client.UI.Home;
using ReactiveUI;

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
    public ObservableCollection<string> Themes { get; } =
        ["Light", "Dark", "System"];

    /// <summary>
    /// Gets the command to cycle through available themes.
    /// </summary>
    public ICommand CycleThemeCommand { get; }

    /// <summary>
    /// Gets or sets the currently selected theme.
    /// </summary>
    public string CurrentTheme
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = "System";

    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardLayoutViewModel"/> class.
    /// </summary>
    /// <param name="mainWindow">The main window view model.</param>
    public DashboardLayoutViewModel(MainWindowViewModel mainWindow)
    {
        MainWindow = mainWindow;
        CycleThemeCommand = new RelayCommand(_ => CurrentTheme = GetNextTheme(CurrentTheme));
    }

    private string GetNextTheme(string currentTheme)
    {
        var currentIndex = Themes.IndexOf(currentTheme);
        return currentIndex < 0 ? Themes[0] : Themes[(currentIndex + 1) % Themes.Count];
    }
}
