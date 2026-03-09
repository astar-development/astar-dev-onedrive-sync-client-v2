namespace AStar.Dev.OneDrive.Sync.Client.Application.Models;

/// <summary>
/// Captures the conflict classification and selected policy outcome.
/// </summary>
/// <param name="ConflictKind">The classified conflict type.</param>
/// <param name="Policy">The policy used to resolve the conflict.</param>
/// <param name="Outcome">The deterministic policy outcome.</param>
/// <param name="Reason">A user-facing reason for diagnostics and UI logs.</param>
public sealed record ConflictPolicyDecision(
    SyncConflictKind ConflictKind,
    SyncConflictResolutionPolicy Policy,
    SyncConflictResolutionOutcome Outcome,
    string Reason);
