using System.Windows.Input;

namespace AStar.Dev.OneDrive.Sync.Client.UI.Common;

/// <summary>
/// A simple implementation of the ICommand interface that allows for delegating command execution and can-execute logic to methods passed in as parameters. This class is commonly used in MVVM applications to bind UI actions to ViewModel methods without having to create separate command classes for each action.
/// </summary>
/// <param name="execute">The action to execute when the command is invoked.</param>
/// <param name="canExecute">A function that determines whether the command can execute. If null, the command is always executable.</param>
public class RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null) : ICommand
{
    private readonly Action<object?> _execute = execute;
    private readonly Func<object?, bool>? _canExecute = canExecute;

    ///<inheritdoc/>
    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

    ///<inheritdoc/>
    public void Execute(object? parameter) => _execute(parameter);
    
    ///<inheritdoc/>
    public event EventHandler? CanExecuteChanged;

    ///<inheritdoc/>
    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
