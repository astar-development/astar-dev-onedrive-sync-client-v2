using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.Application.Models;

namespace AStar.Dev.OneDrive.Sync.Client.Application.Interfaces;

/// <summary>
/// Defines structured event and metric emission for sync diagnostics.
/// </summary>
public interface ISyncDiagnosticsSink
{
    /// <summary>
    /// Records a structured lifecycle event.
    /// </summary>
    Task<Result<Unit, string>> RecordEventAsync(SyncDiagnosticEvent diagnosticEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Records a structured metric point.
    /// </summary>
    Task<Result<Unit, string>> RecordMetricAsync(SyncMetricPoint metricPoint, CancellationToken cancellationToken = default);
}
