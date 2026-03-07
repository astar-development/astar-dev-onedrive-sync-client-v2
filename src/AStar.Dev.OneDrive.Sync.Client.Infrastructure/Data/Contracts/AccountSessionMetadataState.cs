namespace AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Contracts;

/// <summary>
/// Represents persisted non-secret session metadata for an account.
/// </summary>
/// <param name="AccountId">The account identifier.</param>
/// <param name="Email">The account email address.</param>
/// <param name="QuotaBytes">The account quota in bytes.</param>
/// <param name="UsedBytes">The used storage in bytes.</param>
/// <param name="AccessTokenExpiresUtc">The UTC access token expiration timestamp.</param>
/// <param name="LastAuthenticatedUtc">The UTC timestamp of the last successful interactive authentication.</param>
/// <param name="LastTokenRefreshUtc">The UTC timestamp of the last successful refresh.</param>
/// <param name="RequiresReauthentication">Whether reauthentication is required.</param>
public sealed record AccountSessionMetadataState(
    string AccountId,
    string Email,
    long QuotaBytes,
    long UsedBytes,
    DateTime AccessTokenExpiresUtc,
    DateTime LastAuthenticatedUtc,
    DateTime? LastTokenRefreshUtc,
    bool RequiresReauthentication);
