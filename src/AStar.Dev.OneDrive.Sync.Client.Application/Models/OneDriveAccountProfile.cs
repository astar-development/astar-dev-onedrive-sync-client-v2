namespace AStar.Dev.OneDrive.Sync.Client.Application.Models;

/// <summary>
/// Represents account profile details returned by authentication flows.
/// </summary>
/// <param name="AccountId">The linked account identifier.</param>
/// <param name="Email">The account email address.</param>
/// <param name="QuotaBytes">The account quota in bytes.</param>
/// <param name="UsedBytes">The used storage in bytes.</param>
public sealed record OneDriveAccountProfile(string AccountId, string Email, long QuotaBytes, long UsedBytes);
