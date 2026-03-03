using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Serilog;

namespace AStar.Dev.OneDrive.Sync.Client.UI.Common;

/// <summary>
/// Provides centralized error handling and user feedback.
/// </summary>
public static class ErrorHandler
{
    private static Action? _currentDismissCallback;
    private static Window? _currentDialog;

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
        var logMessage = $"Error Dialog - {title}: {message}";
        if (logger != null)
        {
            logger(logMessage);
        }
        else
        {
            Log.Error(logMessage);
        }

        _currentDismissCallback = onDismissed;

        if (Avalonia.Application.Current is null || onShown != null)
        {
            onShown?.Invoke();
            return;
        }

        Dispatcher.UIThread.Post(() => ShowErrorDialogInternal(title, message));
    }

    /// <summary>
    /// Dismisses the currently displayed error dialog.
    /// </summary>
    public static void DismissCurrentDialog()
    {
        _currentDismissCallback?.Invoke();
        _currentDismissCallback = null;
        
        if (_currentDialog != null)
        {
            Dispatcher.UIThread.Post(() =>
            {
                _currentDialog?.Close();
                _currentDialog = null;
            });
        }
    }

    private static void ShowErrorDialogInternal(string title, string message)
    {
        ErrorDialog dialog = new(title, message);
        _currentDialog = dialog;

        dialog.Closed += (_, _) =>
        {
            _currentDialog = null;
            _currentDismissCallback?.Invoke();
            _currentDismissCallback = null;
        };

        Window? owner = GetMainWindow();

        if (owner != null)
        {
            _ = dialog.ShowDialog(owner);
        }
        else
        {
            dialog.Show();
        }
    }

    private static Window? GetMainWindow()
        => Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.MainWindow
            : null;
}
