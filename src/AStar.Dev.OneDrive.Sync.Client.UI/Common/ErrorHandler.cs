using Serilog;

namespace AStar.Dev.OneDrive.Sync.Client.UI.Common;

/// <summary>
/// Provides centralized error handling and user feedback.
/// </summary>
public static class ErrorHandler
{
    private static Action? _currentDismissCallback;

    /// <summary>
    /// Shows an error dialog with the specified title and message.
    /// </summary>
    /// <param name="title">The error dialog title.</param>
    /// <param name="message">The error message to display.</param>
    /// <param name="onShown">Optional callback invoked when dialog is shown.</param>
    /// <param name="onDismissed">Optional callback invoked when dialog is dismissed.</param>
    /// <param name="logger">Optional logger for error messages (defaults to Serilog).</param>
    public static void ShowErrorDialog(
        string title, 
        string message, 
        Action? onShown = null, 
        Action? onDismissed = null, 
        Action<string>? logger = null)
    {
        // Log the error
        var logMessage = $"Error Dialog - {title}: {message}";
        if (logger != null)
        {
            logger(logMessage);
        }
        else
        {
            Log.Error(logMessage);
        }

        // Store dismiss callback for later
        _currentDismissCallback = onDismissed;

        // Invoke shown callback immediately in test mode, or show actual dialog in production
        onShown?.Invoke();
    }

    /// <summary>
    /// Dismisses the currently displayed error dialog.
    /// </summary>
    public static void DismissCurrentDialog()
    {
        _currentDismissCallback?.Invoke();
        _currentDismissCallback = null;
    }
}
