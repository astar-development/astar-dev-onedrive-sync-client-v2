using AStar.Dev.OneDrive.Sync.Client.UI.Common;

namespace AStar.Dev.OneDrive.Sync.Client.UI.Tests.Common;

public class RelayCommandShould
{
    [Fact]
    public void CanExecute_WhenCanExecuteDelegateIsMissing_ReturnsTrue()
    {
        var command = new RelayCommand(_ => { });

        var result = command.CanExecute(new object());

        result.ShouldBeTrue();
    }

    [Fact]
    public void CanExecute_WhenCanExecuteDelegateReturnsTrue_ReturnsTrue()
    {
        var command = new RelayCommand(_ => { }, _ => true);

        var result = command.CanExecute(new object());

        result.ShouldBeTrue();
    }

    [Fact]
    public void CanExecute_WhenCanExecuteDelegateReturnsFalse_ReturnsFalse()
    {
        var command = new RelayCommand(_ => { }, _ => false);

        var result = command.CanExecute(new object());

        result.ShouldBeFalse();
    }

    [Fact]
    public void CanExecute_WhenCalled_PassesParameterToCanExecuteDelegate()
    {
        object? received = null;
        var parameter = new object();
        var command = new RelayCommand(_ => { }, p => { received = p; return true; });

        _ = command.CanExecute(parameter);

        received.ShouldBeSameAs(parameter);
    }

    [Fact]
    public void Execute_WhenCalled_InvokesExecuteDelegate()
    {
        var callCount = 0;
        var command = new RelayCommand(_ => callCount++);

        command.Execute(new object());

        callCount.ShouldBe(1);
    }

    [Fact]
    public void Execute_WhenParameterIsNotNull_PassesParameterToExecuteDelegate()
    {
        object? received = null;
        var parameter = new object();
        var command = new RelayCommand(p => received = p);

        command.Execute(parameter);

        received.ShouldBeSameAs(parameter);
    }

    [Fact]
    public void Execute_WhenParameterIsNull_PassesNullToExecuteDelegate()
    {
        var received = new object();
        var command = new RelayCommand(p => received = p);

        command.Execute(null);

        received.ShouldBeNull();
    }

    [Fact]
    public void RaiseCanExecuteChanged_WhenCalled_RaisesCanExecuteChangedEvent()
    {
        var callCount = 0;
        var command = new RelayCommand(_ => { });
        command.CanExecuteChanged += (_, _) => callCount++;

        command.RaiseCanExecuteChanged();

        callCount.ShouldBe(1);
    }

    [Fact]
    public void RaiseCanExecuteChanged_WhenMultipleHandlersSubscribed_InvokesAllHandlers()
    {
        var first = 0;
        var second = 0;
        var command = new RelayCommand(_ => { });
        command.CanExecuteChanged += (_, _) => first++;
        command.CanExecuteChanged += (_, _) => second++;

        command.RaiseCanExecuteChanged();

        first.ShouldBe(1);
        second.ShouldBe(1);
    }
}
