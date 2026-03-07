using AStar.Dev.Functional.Extensions;

namespace AStar.Dev.OneDrive.Sync.Client.Infrastructure.Authentication;

/// <summary>
/// Defines OneDrive authentication operations for interactive sign-in and token refresh.
/// </summary>
public interface IOneDriveAuthenticationAdapter
{
    /// <summary>
    /// Acquires tokens interactively using an email hint.
    /// </summary>
    /// <param name="emailHint">Optional email hint for sign-in.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The authentication result.</returns>
    Task<Result<OneDriveAuthenticationResult, string>> AcquireInteractiveTokenAsync(string emailHint, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes tokens for an existing account session.
    /// </summary>
    /// <param name="accountId">The account identifier.</param>
    /// <param name="refreshToken">The refresh token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The refreshed authentication result.</returns>
    Task<Result<OneDriveAuthenticationResult, string>> RefreshTokenAsync(string accountId, string refreshToken, CancellationToken cancellationToken = default);
}
