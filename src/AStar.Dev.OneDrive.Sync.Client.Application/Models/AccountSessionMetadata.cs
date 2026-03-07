namespace AStar.Dev.OneDrive.Sync.Client.Application.Models;

/// <summary>
/// Represents persisted non-secret metadata for an account session.
/// </summary>
/// <param name="AccountId">The linked account identifier.</param>
/// <param name="AccessTokenExpiresUtc">The UTC timestamp when the access token expires.</param>
/// <param name="LastAuthenticatedUtc">The UTC timestamp when interactive authentication last succeeded.</param>
/// <param name="LastTokenRefreshUtc">The UTC timestamp when token refresh last succeeded.</param>
/// <param name="RequiresReauthentication">Whether reauthentication is required before sync operations continue.</param>
public sealed record AccountSessionMetadata(string AccountId, DateTime AccessTokenExpiresUtc, DateTime LastAuthenticatedUtc, DateTime? LastTokenRefreshUtc, bool RequiresReauthentication);
