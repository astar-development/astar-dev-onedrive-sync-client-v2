using System.Collections.ObjectModel;
using System.Windows.Input;
using AStar.Dev.OneDrive.Sync.Client.UI.Common;
using AStar.Dev.OneDrive.Sync.Client.UI.Home;
using ReactiveUI;

namespace AStar.Dev.OneDrive.Sync.Client.UI.Layouts;

public class DashboardLayoutViewModel : ViewModelBase
{
    public MainWindowViewModel MainWindow { get; }

    public ObservableCollection<string> Themes { get; } =
        ["Light", "Dark", "System"];

    public ICommand CycleThemeCommand { get; }

    public string CurrentTheme
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = "System";

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
