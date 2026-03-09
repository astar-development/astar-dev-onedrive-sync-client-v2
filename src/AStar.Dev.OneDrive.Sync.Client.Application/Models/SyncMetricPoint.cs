namespace AStar.Dev.OneDrive.Sync.Client.Application.Models;

/// <summary>
/// Represents a metric point emitted from sync pipeline execution.
/// </summary>
/// <param name="Name">The metric name.</param>
/// <param name="Value">The metric numeric value.</param>
/// <param name="CorrelationId">The propagated correlation identifier.</param>
/// <param name="CapturedUtc">The UTC capture timestamp.</param>
public sealed record SyncMetricPoint(
    string Name,
    double Value,
    string CorrelationId,
    DateTime CapturedUtc);
