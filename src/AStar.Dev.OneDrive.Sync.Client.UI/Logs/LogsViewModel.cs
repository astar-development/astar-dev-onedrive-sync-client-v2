using AStar.Dev.OneDrive.Sync.Client.UI.Common;
using ReactiveUI;

namespace AStar.Dev.OneDrive.Sync.Client.UI.Logs;

/// <summary>
/// ViewModel for displaying application logs.
/// </summary>
public class LogsViewModel : ViewModelBase
{
    /// <summary>
    /// Gets or sets the log text to display.
    /// </summary>
    public string LogText
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = "Logs will appear here...";

    /// <summary>
    /// Appends a message to the log text.
    /// </summary>
    /// <param name="message">The message to append.</param>
    public void Append(string message) => LogText += "\n" + message;
}
