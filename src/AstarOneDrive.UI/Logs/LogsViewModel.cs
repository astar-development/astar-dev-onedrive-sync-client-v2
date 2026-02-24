using AstarOneDrive.UI.Common;
using ReactiveUI;

namespace AstarOneDrive.UI.Logs;

public class LogsViewModel : ViewModelBase
{
    public string LogText
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = "Logs will appear here...";

    public void Append(string message) => LogText += "\n" + message;
}
