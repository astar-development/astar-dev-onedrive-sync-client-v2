using Shouldly;
using Xunit;
using AStar.Dev.OneDrive.Sync.Client.UI.Common;

namespace AStar.Dev.OneDrive.Sync.Client.UI.Tests.Common;

public class ErrorHandlerShould
{
    [Fact]
    public void ShowErrorDialogWithTitleAndMessage()
    {
        // Arrange
        const string title = "Test Error";
        const string message = "This is a test error message";
        var wasShown = false;

        // Act
        ErrorHandler.ShowErrorDialog(title, message, () => wasShown = true);

        // Assert
        wasShown.ShouldBeTrue();
    }

    [Fact]
    public void LogErrorsWhenDisplayed()
    {
        // Arrange
        const string title = "Test Error";
        const string message = "This error should be logged";
        var loggedErrors = new List<string>();

        // Act
        ErrorHandler.ShowErrorDialog(title, message, onShown: null, logger: loggedErrors.Add);

        // Assert
        loggedErrors.ShouldContain(error => error.Contains(title) && error.Contains(message));
    }

    [Fact]
    public void AllowUserToDismissDialog()
    {
        // Arrange
        const string title = "Dismissible Error";
        const string message = "This error can be dismissed";
        var dismissed = false;

        // Act
        ErrorHandler.ShowErrorDialog(title, message, onDismissed: () => dismissed = true);

        // Assert - Verify that dismissal callback can be invoked
        dismissed.ShouldBeFalse(); // Not dismissed yet
        ErrorHandler.DismissCurrentDialog();
        dismissed.ShouldBeTrue(); // Now dismissed
    }
}
