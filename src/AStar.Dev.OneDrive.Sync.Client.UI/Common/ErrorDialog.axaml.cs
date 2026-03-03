using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AStar.Dev.OneDrive.Sync.Client.UI.Common;

/// <summary>
/// Error dialog window for displaying error messages to the user.
/// </summary>
public partial class ErrorDialog : Window
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorDialog"/> class.
    /// </summary>
    public ErrorDialog() => InitializeComponent();

    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorDialog"/> class with the specified title and message.
    /// </summary>
    /// <param name="title">The dialog title.</param>
    /// <param name="message">The error message.</param>
    public ErrorDialog(string title, string message) : this() => DataContext = new ErrorDialogViewModel(title, message);

    private void OnOkClick(object? sender, RoutedEventArgs e) => Close();
}

/// <summary>
/// View model for the error dialog.
/// </summary>
public class ErrorDialogViewModel(string title, string message)
{
    /// <summary>
    /// Gets the dialog title.
    /// </summary>
    public string Title { get; } = title;

    /// <summary>
    /// Gets the error message.
    /// </summary>
    public string Message { get; } = message;
}
