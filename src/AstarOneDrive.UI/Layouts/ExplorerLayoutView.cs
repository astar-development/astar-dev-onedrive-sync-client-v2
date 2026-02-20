using AstarOneDrive.UI.Common;

namespace AstarOneDrive.UI.Layouts;

public class ExplorerLayoutViewModel : ViewModelBase
{
    private string _syncSummary = "Idle";
    public string SyncSummary
    {
        get => _syncSummary;
        set { _syncSummary = value; RaisePropertyChanged(); }
    }

    private string _currentTheme = "System";
    public string CurrentTheme
    {
        get => _currentTheme;
        set { _currentTheme = value; RaisePropertyChanged(); }
    }
}
