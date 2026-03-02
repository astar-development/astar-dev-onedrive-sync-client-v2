namespace AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Contracts;

/// <summary>
/// Represents the state of a OneDrive account for persistence.
/// </summary>
/// <param name="Id">The unique identifier of the account.</param>
/// <param name="Email">The email address associated with the account.</param>
/// <param name="QuotaBytes">The total storage quota in bytes.</param>
/// <param name="UsedBytes">The amount of storage used in bytes.</param>
public sealed record AccountState(string Id, string Email, long QuotaBytes, long UsedBytes);
