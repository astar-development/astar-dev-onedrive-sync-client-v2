using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using Serilog;
using System.Diagnostics;

namespace AstarOneDrive.UI.Logs;

public partial class DebugLogWindow : Window
{
    private readonly Stopwatch _uptime = Stopwatch.StartNew();
    private readonly DispatcherTimer _metricsTimer;
    private readonly Action _onLogUpdated;

    public DebugLogWindow()
    {
        InitializeComponent();

        // Live log updates
        _onLogUpdated = () =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                LogText!.Text = LoggingBootstrap.DebugLogSink.GetText();
                Scroller.ScrollToEnd();
            });
        };
        LoggingBootstrap.DebugLogSink.Updated += _onLogUpdated;

        _metricsTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _metricsTimer.Tick += (_, _) => UpdateMetrics();
        _metricsTimer.Start();

        this.Opened += (_, _) =>
        {
            this.Topmost = true;
            this.Activate();
        };

        Log.Information("DebugLogWindow opened - debug window");

        var initialLogs = LoggingBootstrap.DebugLogSink.GetText();
        if (!string.IsNullOrWhiteSpace(initialLogs))
        {
            LogText?.Text = initialLogs;
        }

    }


    private void UpdateMetrics()
    {
        CpuText.Text = $"CPU: {GetCpuUsage():F2}%";
        MemoryText.Text = $"Memory: {GetMemoryUsage():F2} MB";
        GcText.Text = $"GC: {GetGcInfo()}";
        ThreadsText.Text = $"Threads: {GetThreadCount()}";
        UptimeText.Text = $"Uptime: {_uptime.Elapsed}";
    }

    private double GetCpuUsage() => 0;
    private double GetMemoryUsage() => 0;
    private string GetGcInfo() => string.Empty;
    private int GetThreadCount() => 0;

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        Log.Information("Key pressed: {Key} with modifiers {Modifiers} - debug window", e.Key, e.KeyModifiers);

        if (e.Key == Key.L &&
            e.KeyModifiers.HasFlag(KeyModifiers.Control) &&
            e.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            this.Close();
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        _metricsTimer.Stop();
        LoggingBootstrap.DebugLogSink.Updated -= _onLogUpdated;
    }


    private void DragWindow(object? sender, PointerPressedEventArgs e) => BeginMoveDrag(e);
}
