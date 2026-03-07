using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.Application.Interfaces;
using AStar.Dev.OneDrive.Sync.Client.Application.Models;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Contracts;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Repositories;

namespace AStar.Dev.OneDrive.Sync.Client.Infrastructure.Authentication;

/// <summary>
/// Coordinates OneDrive account link, token lifecycle, and session persistence.
/// </summary>
public sealed class OneDriveAccountSessionService(
    IOneDriveAuthenticationAdapter authenticationAdapter,
    ISecureAccountTokenStore secureAccountTokenStore,
    SqliteAccountSessionMetadataRepository metadataRepository) : IAccountSessionService
{
    private static readonly TimeSpan RefreshThreshold = TimeSpan.FromMinutes(2);

    /// <inheritdoc />
    public async Task<Result<AccountSessionState, string>> LinkAccountAsync(string emailHint, CancellationToken cancellationToken = default)
        => await authenticationAdapter.AcquireInteractiveTokenAsync(emailHint, cancellationToken)
            .BindAsync(result => PersistLinkedStateAsync(result, null, cancellationToken));

    /// <inheritdoc />
    public async Task<Result<AccountSessionState, string>> ReauthenticateAsync(string accountId, string emailHint, CancellationToken cancellationToken = default)
        => await authenticationAdapter.AcquireInteractiveTokenAsync(emailHint, cancellationToken)
            .BindAsync(result => PersistLinkedStateAsync(result with { AccountId = accountId }, DateTime.UtcNow, cancellationToken));

    /// <inheritdoc />
    public async Task<Result<AccountSessionState, string>> GetValidSessionAsync(string accountId, CancellationToken cancellationToken = default)
    {
        AccountSessionMetadataState? metadata = await metadataRepository.LoadAsync(accountId, cancellationToken);
        if(metadata is null)
        {
            return "No account session metadata found.";
        }

        if(metadata.RequiresReauthentication)
        {
            return "Reauthentication required.";
        }

        Result<Option<SecureAccountTokens>, string> tokenLoadResult = await secureAccountTokenStore.LoadAsync(accountId, cancellationToken);
        return await tokenLoadResult.MatchAsync(
            async option => await option.MatchAsync(
                async tokens => await ResolveSessionStateAsync(metadata, tokens, cancellationToken),
                () => Task.FromResult<Result<AccountSessionState, string>>("No secure tokens are available for this account.")),
            error => Task.FromResult<Result<AccountSessionState, string>>(error));
    }

    /// <inheritdoc />
    public async Task<Result<Unit, string>> UnlinkAccountAsync(string accountId, CancellationToken cancellationToken = default)
    {
        await metadataRepository.RemoveAsync(accountId, cancellationToken);
        return await secureAccountTokenStore.RemoveAsync(accountId, cancellationToken);
    }

    private async Task<Result<AccountSessionState, string>> ResolveSessionStateAsync(AccountSessionMetadataState metadata, SecureAccountTokens tokens, CancellationToken cancellationToken)
    {
        if(!ShouldRefresh(metadata.AccessTokenExpiresUtc))
        {
            return ToSessionState(metadata);
        }

        Result<OneDriveAuthenticationResult, string> refreshResult = await authenticationAdapter.RefreshTokenAsync(metadata.AccountId, tokens.RefreshToken, cancellationToken);
        return await refreshResult.MatchAsync(
            async refreshed => await PersistLinkedStateAsync(refreshed with { AccountId = metadata.AccountId }, DateTime.UtcNow, cancellationToken),
            async _ =>
            {
                AccountSessionMetadataState updatedMetadata = metadata with { RequiresReauthentication = true };
                await metadataRepository.SaveAsync(updatedMetadata, cancellationToken);
                return (Result<AccountSessionState, string>)"Reauthentication required.";
            });
    }

    private async Task<Result<AccountSessionState, string>> PersistLinkedStateAsync(OneDriveAuthenticationResult authResult, DateTime? lastRefreshUtc, CancellationToken cancellationToken)
    {
        Result<Unit, string> saveTokenResult = await secureAccountTokenStore.SaveAsync(
            authResult.AccountId,
            new SecureAccountTokens(authResult.AccessToken, authResult.RefreshToken),
            cancellationToken);

        return await saveTokenResult.MatchAsync(
            async _ =>
            {
                DateTime now = DateTime.UtcNow;
                var metadata = new AccountSessionMetadataState(
                    authResult.AccountId,
                    authResult.Email,
                    authResult.QuotaBytes,
                    authResult.UsedBytes,
                    authResult.AccessTokenExpiresUtc,
                    now,
                    lastRefreshUtc,
                    false);

                await metadataRepository.SaveAsync(metadata, cancellationToken);
                return (Result<AccountSessionState, string>)ToSessionState(metadata);
            },
            error => Task.FromResult<Result<AccountSessionState, string>>(error));
    }

    private static AccountSessionState ToSessionState(AccountSessionMetadataState metadata)
        => new(
            new OneDriveAccountProfile(metadata.AccountId, metadata.Email, metadata.QuotaBytes, metadata.UsedBytes),
            new AccountSessionMetadata(metadata.AccountId, metadata.AccessTokenExpiresUtc, metadata.LastAuthenticatedUtc, metadata.LastTokenRefreshUtc, metadata.RequiresReauthentication));

    private static bool ShouldRefresh(DateTime accessTokenExpiresUtc)
        => accessTokenExpiresUtc <= DateTime.UtcNow.Add(RefreshThreshold);
}
