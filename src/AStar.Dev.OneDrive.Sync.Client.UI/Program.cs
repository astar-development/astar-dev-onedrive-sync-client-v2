using System.Diagnostics;
using Avalonia;
using Avalonia.ReactiveUI;
using Serilog;

namespace AStar.Dev.OneDrive.Sync.Client.UI;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        LoggingBootstrap.Initialize();
        ExceptionBootstrap.Initialize();

        AppDomain.CurrentDomain.UnhandledException += (sender, e)
            => Log.Fatal(e.ExceptionObject as Exception, "Fatal unhandled exception");

        TaskScheduler.UnobservedTaskException += (sender, e)
            =>
        {
            Log.Error(e.Exception, "Unobserved task exception");
            e.SetObserved();
        };
        _ = Trace.Listeners.Add(new SerilogTraceListener());

        try
        {
            _ = BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        catch(Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
}
