namespace AStar.Dev.OneDrive.Sync.Client.Infrastructure.Graph;

/// <summary>
/// Records telemetry around OneDrive Graph requests.
/// </summary>
public interface IOneDriveGraphTelemetry
{
    /// <summary>
    /// Records a Graph request telemetry event.
    /// </summary>
    void Track(GraphRequestTelemetryEvent telemetryEvent);
}
