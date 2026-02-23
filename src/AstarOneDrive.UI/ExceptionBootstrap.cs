using System.Diagnostics;
using Avalonia.Threading;
using Serilog;

namespace AstarOneDrive.UI;

public static class ExceptionBootstrap
{
    public static void Initialize()
    {
        // Background thread crashes
        AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
        {
            Log.Fatal(e.ExceptionObject as Exception, "Fatal unhandled exception");
        };

        // Unobserved task exceptions
        TaskScheduler.UnobservedTaskException += (sender, e) =>
        {
            Log.Error(e.Exception, "Unobserved task exception");
            e.SetObserved();
        };

        // Avalonia internal logs (binding errors, dispatcher warnings, etc.)
        Trace.Listeners.Add(new SerilogTraceListener());
    }

    public static void HookAvaloniaUIThread()
    {
        Dispatcher.UIThread.UnhandledException += (sender, e) =>
        {
            Log.Error(e.Exception, "UI thread exception");
            e.Handled = true; // optional
        };
    }
}
