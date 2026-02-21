using OpenTelemetry;
using OpenTelemetry.Metrics;
using Serilog;

namespace AstarOneDrive.UI;

public static class LoggingBootstrap
{
    #if DEBUG 
    public static InMemoryLogSink DebugLogSink { get; private set; } = new();
    #endif

    public static void Initialize()
    {
        // --- Serilog ---
        var config = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File(
                "logs/app.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7);
#if DEBUG
        config = config.WriteTo.Sink(DebugLogSink);
#endif
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
