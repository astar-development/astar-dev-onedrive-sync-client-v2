namespace AStar.Dev.OneDrive.Sync.Client.Infrastructure.Graph;

/// <summary>
/// Default telemetry sink that discards Graph telemetry events.
/// </summary>
public sealed class NoOpOneDriveGraphTelemetry : IOneDriveGraphTelemetry
{
    /// <inheritdoc />
    public void Track(GraphRequestTelemetryEvent telemetryEvent)
    {
    }
}
