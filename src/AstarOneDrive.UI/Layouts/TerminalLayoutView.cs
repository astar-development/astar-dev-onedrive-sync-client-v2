using AstarOneDrive.UI.Common;

namespace AstarOneDrive.UI.Layouts;

public class TerminalLayoutViewModel : ViewModelBase
{
    private string _terminalStatus = "Ready";
    public string TerminalStatus
    {
        get => _terminalStatus;
        set { _terminalStatus = value; RaisePropertyChanged(); }
    }
}
