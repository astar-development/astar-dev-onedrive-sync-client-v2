namespace AStar.Dev.OneDrive.Sync.Client.Application.Models;

/// <summary>
/// Represents the result summary for a completed delta pull.
/// </summary>
/// <param name="PageCount">The number of pages pulled.</param>
/// <param name="ChangeCount">The number of change entries processed.</param>
/// <param name="AppliedDeltaToken">The resulting delta token used for the checkpoint.</param>
public sealed record DeltaPullSummary(int PageCount, int ChangeCount, string AppliedDeltaToken);