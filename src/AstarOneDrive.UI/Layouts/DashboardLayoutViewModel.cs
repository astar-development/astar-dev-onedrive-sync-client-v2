using System.Collections.ObjectModel;
using AstarOneDrive.UI.Common;
using ReactiveUI;

namespace AstarOneDrive.UI.Layouts;

public class DashboardLayoutViewModel : ViewModelBase
{
    public ObservableCollection<string> Themes { get; } =
        new() { "Light", "Dark", "System" };
    public string CurrentTheme
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = "System";
}
