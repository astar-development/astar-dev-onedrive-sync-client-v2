using AstarOneDrive.UI.Common;
using ReactiveUI;

namespace AstarOneDrive.UI.Layouts;

public class ExplorerLayoutViewModel : ViewModelBase
{
    public string SyncSummary
    {
        get;
        set { field = value; this.RaiseAndSetIfChanged(ref field, value); }
    } = "Idle";

    public string CurrentTheme
    {
        get;
        set { field = value; this.RaiseAndSetIfChanged(ref field, value); }
    } = "System";
}
