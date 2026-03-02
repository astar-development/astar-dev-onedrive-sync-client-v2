using Avalonia;
using Avalonia.ReactiveUI;
using Serilog;

namespace AStar.Dev.OneDrive.Sync.Client.UI;

/// <summary>
/// Main entry point for the application.
/// </summary>
sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    /// <summary>
    /// Application entry point.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    [STAThread]
    public static void Main(string[] args)
    {
        LoggingBootstrap.Initialize();
        ExceptionBootstrap.Initialize();

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

    /// <summary>
    /// Configures and builds the Avalonia application.
    /// </summary>
    /// <returns>The configured Avalonia application builder.</returns>
    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
}
