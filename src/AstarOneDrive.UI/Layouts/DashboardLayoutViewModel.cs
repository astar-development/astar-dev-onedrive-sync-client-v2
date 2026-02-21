using System.Collections.ObjectModel;
using AstarOneDrive.UI.Common;

namespace AstarOneDrive.UI.Layouts;

public class DashboardLayoutViewModel : ViewModelBase
{
    public ObservableCollection<string> Themes { get; } =
        new() { "Light", "Dark", "System" };

    private string _currentTheme = "System";
    public string CurrentTheme
    {
        get => _currentTheme;
        set => this.RaiseAndSetIfChanged(ref _currentTheme, value);
    }
}
