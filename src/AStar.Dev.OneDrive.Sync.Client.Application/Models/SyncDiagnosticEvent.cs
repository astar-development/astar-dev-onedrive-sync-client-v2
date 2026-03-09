namespace AStar.Dev.OneDrive.Sync.Client.Application.Models;

/// <summary>
/// Represents a structured lifecycle diagnostic event.
/// </summary>
/// <param name="EventName">The lifecycle event name.</param>
/// <param name="Operation">The operation category.</param>
/// <param name="CorrelationId">The propagated operation correlation identifier.</param>
/// <param name="Outcome">The event outcome.</param>
/// <param name="Message">The structured diagnostic message.</param>
/// <param name="OccurredUtc">The UTC timestamp of the event.</param>
public sealed record SyncDiagnosticEvent(
    string EventName,
    string Operation,
    string CorrelationId,
    string Outcome,
    string Message,
    DateTime OccurredUtc);
