using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using AStar.Dev.OneDrive.Sync.Client.UI.Logs;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.ReactiveUI;
using ReactiveUI;
using Serilog;

namespace AStar.Dev.OneDrive.Sync.Client.UI.Home;

[ExcludeFromCodeCoverage]
public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public ReactiveCommand<Unit, Unit> ToggleDebugWindowCommand { get; }
    private DebugLogWindow? _debugWindow;

    public MainWindow()
    {
        InitializeComponent();

        Opened += (_, _) =>
        {
            _ = (this.FindControl<Panel>("RootPanel")?.Focus());
            Log.Information("MainWindow opened and focused.");
        };

        ToggleDebugWindowCommand = ReactiveCommand.Create(ToggleDebugWindow);

    }

    /// <summary>
    /// Overrides the OnKeyDown method to listen for specific key combinations. When the user presses Ctrl+Shift+L, it toggles the visibility of a debug log window. If the debug window is not currently open, it creates a new instance of DebugLogWindow, shows it, and sets up an event handler to nullify the reference when the window is closed. If the debug window is already open, it closes the window and nullifies the reference. This allows developers to quickly access logging information while using the application without needing to navigate through menus or settings.
    /// </summary>
    /// <param name="e">The KeyEventArgs instance containing information about the key event.</param>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        Log.Information("Key pressed: {Key} with modifiers {Modifiers} - main window", e.Key, e.KeyModifiers);

        if(e.Key == Key.L &&
            e.KeyModifiers.HasFlag(KeyModifiers.Control) &&
            e.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            if(_debugWindow == null)
            {
                _debugWindow = new DebugLogWindow();
                Log.Information("Created debug window of type {Type}", _debugWindow.GetType().FullName);

                _debugWindow.Closed += (_, _) => _debugWindow = null;
                _debugWindow.Show();
            }
            else
            {
                _debugWindow.Close();
                _debugWindow = null;
            }
        }
    }

    private void ToggleDebugWindow()
    {
        Log.Information("Toggling debug window - main window command");
        if(_debugWindow == null)
        {
            _debugWindow = new DebugLogWindow();
            _debugWindow.Closed += (_, _) => _debugWindow = null;
            _debugWindow.Show();
            _debugWindow.Activate();
        }
        else
        {
            _debugWindow.Close();
            _debugWindow = null;
        }
    }
}
