using AstarOneDrive.UI.Common;
using ReactiveUI;


namespace AstarOneDrive.UI.Layouts;

public class TerminalLayoutViewModel : ViewModelBase
{
    public string TerminalStatus
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = "Ready";
}
