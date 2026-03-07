using AStar.Dev.Functional.Extensions;

namespace AStar.Dev.OneDrive.Sync.Client.Infrastructure.Authentication;

/// <summary>
/// Defines secure storage operations for account access and refresh tokens.
/// </summary>
public interface ISecureAccountTokenStore
{
    /// <summary>
    /// Saves account tokens securely.
    /// </summary>
    /// <param name="accountId">The account identifier.</param>
    /// <param name="tokens">The tokens to persist.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success when tokens are saved.</returns>
    Task<Result<Unit, string>> SaveAsync(string accountId, SecureAccountTokens tokens, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads account tokens from secure storage.
    /// </summary>
    /// <param name="accountId">The account identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The stored tokens when available.</returns>
    Task<Result<Option<SecureAccountTokens>, string>> LoadAsync(string accountId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes account tokens from secure storage.
    /// </summary>
    /// <param name="accountId">The account identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success when tokens are removed.</returns>
    Task<Result<Unit, string>> RemoveAsync(string accountId, CancellationToken cancellationToken = default);
}