using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.Application.Models;

namespace AStar.Dev.OneDrive.Sync.Client.Application.Interfaces;

/// <summary>
/// Defines account linking and session lifecycle operations for OneDrive authentication.
/// </summary>
public interface IAccountSessionService
{
    /// <summary>
    /// Performs interactive account linking and persists account session state.
    /// </summary>
    /// <param name="emailHint">Optional email hint for interactive sign-in.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The linked account session state.</returns>
    Task<Result<AccountSessionState, string>> LinkAccountAsync(string emailHint, CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs interactive reauthentication for an existing account.
    /// </summary>
    /// <param name="accountId">The account identifier.</param>
    /// <param name="emailHint">Optional email hint for interactive sign-in.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The refreshed account session state.</returns>
    Task<Result<AccountSessionState, string>> ReauthenticateAsync(string accountId, string emailHint, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a valid account session, refreshing tokens when needed.
    /// </summary>
    /// <param name="accountId">The account identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The valid account session state.</returns>
    Task<Result<AccountSessionState, string>> GetValidSessionAsync(string accountId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Unlinks an account and removes stored session state and tokens.
    /// </summary>
    /// <param name="accountId">The account identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success when the account session has been removed.</returns>
    Task<Result<Unit, string>> UnlinkAccountAsync(string accountId, CancellationToken cancellationToken = default);
}