using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.Application.Interfaces;
using AStar.Dev.OneDrive.Sync.Client.Application.Models;
using Serilog;

namespace AStar.Dev.OneDrive.Sync.Client.UI;

/// <summary>
/// Emits sync diagnostics as structured logs and metrics.
/// </summary>
public sealed class SyncDiagnosticsSink : ISyncDiagnosticsSink
{
    /// <inheritdoc />
    public Task<Result<Unit, string>> RecordEventAsync(SyncDiagnosticEvent diagnosticEvent, CancellationToken cancellationToken = default)
    {
        Log.Information(
            "{EventName} op={Operation} corr={CorrelationId} outcome={Outcome} msg={Message}",
            diagnosticEvent.EventName,
            diagnosticEvent.Operation,
            diagnosticEvent.CorrelationId,
            diagnosticEvent.Outcome,
            Redact(diagnosticEvent.Message));
        return Task.FromResult<Result<Unit, string>>(Unit.Value);
    }

    /// <inheritdoc />
    public Task<Result<Unit, string>> RecordMetricAsync(SyncMetricPoint metricPoint, CancellationToken cancellationToken = default)
    {
        Metrics.Record(metricPoint.Name, metricPoint.Value, metricPoint.CorrelationId);
        return Task.FromResult<Result<Unit, string>>(Unit.Value);
    }

    private static string Redact(string text)
        => InMemoryLogSink.RedactSensitiveData(text);
}
