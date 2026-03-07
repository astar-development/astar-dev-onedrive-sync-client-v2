namespace AStar.Dev.OneDrive.Sync.Client.Infrastructure.Graph;

/// <summary>
/// Represents request-level Graph telemetry.
/// </summary>
/// <param name="Method">The HTTP method name.</param>
/// <param name="Path">The Graph API path.</param>
/// <param name="Duration">The total request duration including retries.</param>
/// <param name="RetryCount">The number of retries executed.</param>
/// <param name="ErrorKind">The mapped error category, if the request failed.</param>
/// <param name="StatusCode">The terminal HTTP status code, if available.</param>
public sealed record GraphRequestTelemetryEvent(string Method, string Path, TimeSpan Duration, int RetryCount, SyncErrorKind? ErrorKind, int? StatusCode);
