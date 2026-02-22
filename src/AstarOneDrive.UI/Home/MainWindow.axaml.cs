using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using AstarOneDrive.UI.Logs;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.ReactiveUI;
using ReactiveUI;
using Serilog;

namespace AstarOneDrive.UI.Home;

[ExcludeFromCodeCoverage]
public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{

public ReactiveCommand<Unit, Unit> ToggleDebugWindowCommand { get; }
private DebugLogWindow? _debugWindow;



    public MainWindow()
    {
        InitializeComponent();

    
        this.Opened += (_, _) =>
        {
            this.FindControl<Panel>("RootPanel")?.Focus();
            Log.Information("MainWindow opened and focused.");
        };

        ToggleDebugWindowCommand = ReactiveCommand.Create(ToggleDebugWindow);
    
    }

    
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        Log.Information("Key pressed: {Key} with modifiers {Modifiers} - main window", e.Key, e.KeyModifiers);

        if (e.Key == Key.L &&
            e.KeyModifiers.HasFlag(KeyModifiers.Control) &&
            e.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            if (_debugWindow == null)
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
        if (_debugWindow == null)
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
