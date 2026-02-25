using OpenTelemetry;
using OpenTelemetry.Metrics;
using Serilog;

namespace AStar.Dev.OneDrive.Sync.Client.UI;

public static class LoggingBootstrap
{

    public static InMemoryLogSink DebugLogSink { get; private set; } = new();

    public static void Initialize()
    {
        // --- Serilog ---
        LoggerConfiguration config = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File(
                "logs/app.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7);

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
