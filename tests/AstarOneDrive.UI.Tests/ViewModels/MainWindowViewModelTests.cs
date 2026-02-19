using AstarOneDrive.UI.ViewModels;
using Shouldly;

namespace AstarOneDrive.UI.Tests.ViewModels;

public sealed class MainWindowViewModelTests
{
    [Fact]
    public void Greeting_ReturnsExpectedValue()
    {
        var sut = new MainWindowViewModel();

        sut.Greeting.ShouldBe("Welcome to AstarOneDrive!");
    }
}
