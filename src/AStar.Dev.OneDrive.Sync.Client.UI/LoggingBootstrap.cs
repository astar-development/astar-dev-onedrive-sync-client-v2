using OpenTelemetry;
using OpenTelemetry.Metrics;
using Serilog;

namespace AStar.Dev.OneDrive.Sync.Client.UI;

public static class LoggingBootstrap
{
    /// <summary>Gets the default log file path used when <c>ASTAR_LOG_PATH</c> is not set.</summary>
    public const string DefaultLogPath = "astar-logs/app.log";

    /// <summary>Gets the default retained file count used when <c>ASTAR_LOG_RETENTION_DAYS</c> is not set.</summary>
    public const int DefaultRetentionDays = 7;

    /// <summary>Gets the log file path, resolved from the <c>ASTAR_LOG_PATH</c> environment variable or the default.</summary>
    public static string LogPath => Environment.GetEnvironmentVariable("ASTAR_LOG_PATH") ?? DefaultLogPath;

    /// <summary>Gets the retained file count, resolved from the <c>ASTAR_LOG_RETENTION_DAYS</c> environment variable or the default.</summary>
    public static int RetentionDays => int.TryParse(Environment.GetEnvironmentVariable("ASTAR_LOG_RETENTION_DAYS"), out var days) ? days : DefaultRetentionDays;

    public static InMemoryLogSink DebugLogSink { get; private set; } = new();

    public static void Initialize()
    {
        // --- Serilog ---
        LoggerConfiguration config = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File(
                LogPath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: RetentionDays);

        config = config.WriteTo.Sink(DebugLogSink);

        Log.Logger = config.CreateLogger();

        // --- OpenTelemetry Metrics ---
        MeterProvider = Sdk.CreateMeterProviderBuilder()
            .AddMeter("MyApp")
            .AddRuntimeInstrumentation()
            .AddProcessInstrumentation()
            // No exporters → stays self-contained
            .Build();
    }

    public static MeterProvider? MeterProvider { get; private set; }
}
