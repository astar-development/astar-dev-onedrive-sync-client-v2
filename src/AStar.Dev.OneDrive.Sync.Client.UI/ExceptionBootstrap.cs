using System.Diagnostics;
using Avalonia.Threading;
using Serilog;

namespace AStar.Dev.OneDrive.Sync.Client.UI;

/// <summary>
/// Configures global exception handling for the application.
/// </summary>
public static class ExceptionBootstrap
{
    /// <summary>
    /// Initializes global exception handlers for unhandled exceptions and task exceptions.
    /// </summary>
    public static void Initialize()
    {
        AppDomain.CurrentDomain.UnhandledException += (sender, e) => Log.Fatal(e.ExceptionObject as Exception, "Fatal unhandled exception");

        TaskScheduler.UnobservedTaskException += (sender, e) =>
        {
            Log.Error(e.Exception, "Unobserved task exception");
            e.SetObserved();
        };

        _ = Trace.Listeners.Add(new SerilogTraceListener());
    }

    /// <summary>
    /// Hooks the Avalonia UI thread to handle unhandled UI exceptions.
    /// </summary>
    public static void HookAvaloniaUIThread() => Dispatcher.UIThread.UnhandledException += HandleUIThreadException();
    
    private static DispatcherUnhandledExceptionEventHandler HandleUIThreadException() => (sender, e) =>
    {
        Log.Error(e.Exception, "UI thread exception");
        e.Handled = true;
    };
}
