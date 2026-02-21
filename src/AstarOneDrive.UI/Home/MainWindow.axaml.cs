using System.Diagnostics.CodeAnalysis;
using AstarOneDrive.UI.Logs;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.ReactiveUI;

namespace AstarOneDrive.UI.Home;

[ExcludeFromCodeCoverage]
public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        InitializeComponent();

        #if DEBUG 
        var overlay = new DebugLogOverlay 
        {
             HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, 
             VerticalAlignment = Avalonia.Layout.VerticalAlignment.Bottom 
        };
        this.FindControl<Panel>("RootPanel")?.Children.Add(overlay);
    
        #endif
    }

    #if DEBUG
    private bool _overlayVisible = true;

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.KeyModifiers == (KeyModifiers.Control | KeyModifiers.Shift) &&
            e.Key == Key.L)
        {
            _overlayVisible = !_overlayVisible;
            var overlay = this.FindControl<DebugLogOverlay>("DebugOverlay");
            overlay?.IsVisible = _overlayVisible;
            overlay?.Name = "DebugOverlay";
        }
    }
    #endif
}
