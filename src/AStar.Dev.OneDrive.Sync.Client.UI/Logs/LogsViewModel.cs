using AStar.Dev.OneDrive.Sync.Client.UI.Common;
using ReactiveUI;

namespace AStar.Dev.OneDrive.Sync.Client.UI.Logs;

public class LogsViewModel : ViewModelBase
{
    public string LogText
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = "Logs will appear here...";

    public void Append(string message) => LogText += "\n" + message;
}
