namespace AStar.Dev.OneDrive.Sync.Client.Application.Models;

/// <summary>
/// Represents persisted sync checkpoint state for delta progression.
/// </summary>
/// <param name="AccountId">The account identifier.</param>
/// <param name="ScopeId">The sync scope identifier.</param>
/// <param name="DeltaToken">The provider delta token.</param>
/// <param name="UpdatedUtc">The UTC timestamp when checkpoint was updated.</param>
public sealed record SyncCheckpoint(string AccountId, string ScopeId, string DeltaToken, DateTime UpdatedUtc);
