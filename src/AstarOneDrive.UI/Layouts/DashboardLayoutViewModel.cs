using System.Collections.ObjectModel;
using System.Windows.Input;
using AstarOneDrive.UI.Common;
using AstarOneDrive.UI.Home;
using ReactiveUI;

namespace AstarOneDrive.UI.Layouts;

public class DashboardLayoutViewModel : ViewModelBase
{
    public MainWindowViewModel MainWindow { get; }

    public ObservableCollection<string> Themes { get; } =
        new() { "Light", "Dark", "System" };

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
        if (currentIndex < 0)
        {
            return Themes[0];
        }

        return Themes[(currentIndex + 1) % Themes.Count];
    }
}
