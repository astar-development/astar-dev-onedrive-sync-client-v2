using System.Security.Cryptography;
using System.Text;
using AStar.Dev.Functional.Extensions;

namespace AStar.Dev.OneDrive.Sync.Client.Infrastructure.Authentication;

/// <summary>
/// Provides a deterministic OneDrive authentication adapter for account link and refresh workflows.
/// </summary>
public sealed class OneDriveAuthenticationAdapter : IOneDriveAuthenticationAdapter
{
    /// <inheritdoc />
    public Task<Result<OneDriveAuthenticationResult, string>> AcquireInteractiveTokenAsync(string emailHint, CancellationToken cancellationToken = default)
    {
        if(string.IsNullOrWhiteSpace(emailHint))
        {
            return Task.FromResult<Result<OneDriveAuthenticationResult, string>>("Email is required for account linking.");
        }

        var normalizedEmail = emailHint.Trim().ToLowerInvariant();
        if(!normalizedEmail.Contains('@'))
        {
            return Task.FromResult<Result<OneDriveAuthenticationResult, string>>("Invalid email address.");
        }

        var accountId = BuildDeterministicAccountId(normalizedEmail);
        DateTime now = DateTime.UtcNow;
        var result = new OneDriveAuthenticationResult(
            accountId,
            normalizedEmail,
            1_099_511_627_776,
            Random.Shared.NextInt64(1_073_741_824, 10_737_418_240),
            BuildToken("access", accountId, now),
            BuildToken("refresh", accountId, now),
            now.AddMinutes(55));

        return Task.FromResult<Result<OneDriveAuthenticationResult, string>>(result);
    }

    /// <inheritdoc />
    public Task<Result<OneDriveAuthenticationResult, string>> RefreshTokenAsync(string accountId, string refreshToken, CancellationToken cancellationToken = default)
    {
        if(string.IsNullOrWhiteSpace(accountId) || string.IsNullOrWhiteSpace(refreshToken))
        {
            return Task.FromResult<Result<OneDriveAuthenticationResult, string>>("Refresh token is unavailable.");
        }

        if(!refreshToken.Contains("refresh", StringComparison.Ordinal))
        {
            return Task.FromResult<Result<OneDriveAuthenticationResult, string>>("Refresh token is invalid.");
        }

        DateTime now = DateTime.UtcNow;
        var result = new OneDriveAuthenticationResult(
            accountId,
            $"{accountId}@onedrive.local",
            1_099_511_627_776,
            Random.Shared.NextInt64(1_073_741_824, 10_737_418_240),
            BuildToken("access", accountId, now),
            BuildToken("refresh", accountId, now),
            now.AddMinutes(55));

        return Task.FromResult<Result<OneDriveAuthenticationResult, string>>(result);
    }

    private static string BuildDeterministicAccountId(string email)
    {
        var bytes = Encoding.UTF8.GetBytes(email);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash[..16]).ToLowerInvariant();
    }

    private static string BuildToken(string prefix, string accountId, DateTime utcNow)
        => $"{prefix}-{accountId}-{utcNow.Ticks}";
}
