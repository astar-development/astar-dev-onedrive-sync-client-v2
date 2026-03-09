using AStar.Dev.OneDrive.Sync.Client.Application.Models;

namespace AStar.Dev.OneDrive.Sync.Client.Application.Services;

/// <summary>
/// Classifies conflict metadata and maps it to deterministic policy outcomes.
/// </summary>
public sealed class SyncConflictPolicyEngine
{
    /// <summary>
    /// Classifies a conflict based on eTag, timestamp, rename, and delete metadata.
    /// </summary>
    public SyncConflictKind Classify(SyncConflictContext context)
        => (context.LocalRenamed || context.RemoteRenamed) && (context.LocalDeleted || context.RemoteDeleted)
            ? SyncConflictKind.RenameDelete
            : !string.IsNullOrWhiteSpace(context.LocalETag)
                && !string.IsNullOrWhiteSpace(context.RemoteETag)
                && !string.Equals(context.LocalETag, context.RemoteETag, StringComparison.Ordinal)
                ? SyncConflictKind.EtagMismatch
                : context.LocalTimestampUtc.HasValue
                    && context.RemoteTimestampUtc.HasValue
                    && context.LocalTimestampUtc.Value != context.RemoteTimestampUtc.Value
                    ? SyncConflictKind.TimestampDrift
                    : SyncConflictKind.None;

    /// <summary>
    /// Resolves a conflict using the supplied policy.
    /// </summary>
    public ConflictPolicyDecision Resolve(SyncConflictContext context, SyncConflictResolutionPolicy policy)
    {
        SyncConflictKind kind = Classify(context);
        if(kind == SyncConflictKind.None)
        {
            return new ConflictPolicyDecision(kind, policy, SyncConflictResolutionOutcome.ProceedWithRemote, "No conflict detected.");
        }

        SyncConflictResolutionOutcome outcome = policy switch
        {
            SyncConflictResolutionPolicy.RemoteWins => SyncConflictResolutionOutcome.ProceedWithRemote,
            SyncConflictResolutionPolicy.LocalWins => SyncConflictResolutionOutcome.ProceedWithLocal,
            SyncConflictResolutionPolicy.RenameCopy => SyncConflictResolutionOutcome.RenameAndProceed,
            _ => SyncConflictResolutionOutcome.QueueForManualResolution
        };

        var reason = $"Conflict detected ({kind}) resolved via {policy}.";
        return new ConflictPolicyDecision(kind, policy, outcome, reason);
    }
}
