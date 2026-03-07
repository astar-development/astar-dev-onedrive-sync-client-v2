namespace AStar.Dev.OneDrive.Sync.Client.Infrastructure.Authentication;

/// <summary>
/// Represents OneDrive authentication output including token details.
/// </summary>
/// <param name="AccountId">The account identifier.</param>
/// <param name="Email">The account email address.</param>
/// <param name="QuotaBytes">The account quota in bytes.</param>
/// <param name="UsedBytes">The used storage in bytes.</param>
/// <param name="AccessToken">The access token.</param>
/// <param name="RefreshToken">The refresh token.</param>
/// <param name="AccessTokenExpiresUtc">The UTC expiration of the access token.</param>
public sealed record OneDriveAuthenticationResult(string AccountId, string Email, long QuotaBytes, long UsedBytes, string AccessToken, string RefreshToken, DateTime AccessTokenExpiresUtc);
