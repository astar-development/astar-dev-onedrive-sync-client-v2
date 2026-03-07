using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.Application.Models;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Authentication;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Repositories;
using AStar.Dev.Utilities;

namespace AStar.Dev.OneDrive.Sync.Client.Infrastructure.Tests.Authentication;

public sealed class OneDriveAccountSessionServiceShould
{
    [Fact]
    public async Task PersistLinkedSessionAndRestoreWithoutReauthenticationWhenTokenIsValid()
    {
        var databasePath = CreateDatabasePath();
        var tokenStorePath = Path.GetTempPath().CombinePath($"astar-onedrive-token-store-{Guid.NewGuid():N}");
        var authClient = new DeterministicOneDriveAuthenticationAdapter(
            Acquire: (_, _) => Task.FromResult<Result<OneDriveAuthenticationResult, string>>(CreateAuthResult("acct-1", "linked@example.com", DateTime.UtcNow.AddMinutes(30))),
            Refresh: (_, _, _) => Task.FromResult<Result<OneDriveAuthenticationResult, string>>("refresh should not be called"));

        OneDriveAccountSessionService service = CreateSessionService(databasePath, tokenStorePath, authClient);
        Result<AccountSessionState, string> linkResult = await service.LinkAccountAsync("linked@example.com", TestContext.Current.CancellationToken);

        OneDriveAccountSessionService reloadedService = CreateSessionService(databasePath, tokenStorePath, authClient);
        Result<AccountSessionState, string> validSessionResult = await reloadedService.GetValidSessionAsync("acct-1", TestContext.Current.CancellationToken);

        linkResult.Map(_ => true).Match(ok => ok, _ => false).ShouldBeTrue();
        validSessionResult.Map(_ => true).Match(ok => ok, _ => false).ShouldBeTrue();
        authClient.RefreshCalls.ShouldBe(0);
    }

    [Fact]
    public async Task RefreshExpiredTokenWhenValidSessionIsRequested()
    {
        var databasePath = CreateDatabasePath();
        var tokenStorePath = Path.GetTempPath().CombinePath($"astar-onedrive-token-store-{Guid.NewGuid():N}");
        var authClient = new DeterministicOneDriveAuthenticationAdapter(
            Acquire: (_, _) => Task.FromResult<Result<OneDriveAuthenticationResult, string>>(CreateAuthResult("acct-2", "refresh@example.com", DateTime.UtcNow.AddMinutes(-1))),
            Refresh: (_, _, _) => Task.FromResult<Result<OneDriveAuthenticationResult, string>>(CreateAuthResult("acct-2", "refresh@example.com", DateTime.UtcNow.AddMinutes(60))));

        OneDriveAccountSessionService service = CreateSessionService(databasePath, tokenStorePath, authClient);
        _ = await service.LinkAccountAsync("refresh@example.com", TestContext.Current.CancellationToken);
        Result<AccountSessionState, string> sessionResult = await service.GetValidSessionAsync("acct-2", TestContext.Current.CancellationToken);

        sessionResult.Map(_ => true).Match(ok => ok, _ => false).ShouldBeTrue();
        authClient.RefreshCalls.ShouldBe(1);
    }

    [Fact]
    public async Task ReturnErrorWhenTokenRefreshFails()
    {
        var databasePath = CreateDatabasePath();
        var tokenStorePath = Path.GetTempPath().CombinePath($"astar-onedrive-token-store-{Guid.NewGuid():N}");
        var authClient = new DeterministicOneDriveAuthenticationAdapter(
            Acquire: (_, _) => Task.FromResult<Result<OneDriveAuthenticationResult, string>>(CreateAuthResult("acct-3", "reauth@example.com", DateTime.UtcNow.AddMinutes(-1))),
            Refresh: (_, _, _) => Task.FromResult<Result<OneDriveAuthenticationResult, string>>("refresh failed"));

        OneDriveAccountSessionService service = CreateSessionService(databasePath, tokenStorePath, authClient);
        _ = await service.LinkAccountAsync("reauth@example.com", TestContext.Current.CancellationToken);
        Result<AccountSessionState, string> sessionResult = await service.GetValidSessionAsync("acct-3", TestContext.Current.CancellationToken);

        sessionResult.Map(_ => false).Match(ok => ok, _ => true).ShouldBeTrue();
    }

    [Fact]
    public async Task RemoveSessionAndSecretsWhenAccountIsUnlinked()
    {
        var databasePath = CreateDatabasePath();
        var tokenStorePath = Path.GetTempPath().CombinePath($"astar-onedrive-token-store-{Guid.NewGuid():N}");
        var authClient = new DeterministicOneDriveAuthenticationAdapter(
            Acquire: (_, _) => Task.FromResult<Result<OneDriveAuthenticationResult, string>>(CreateAuthResult("acct-4", "unlink@example.com", DateTime.UtcNow.AddMinutes(30))),
            Refresh: (_, _, _) => Task.FromResult<Result<OneDriveAuthenticationResult, string>>(CreateAuthResult("acct-4", "unlink@example.com", DateTime.UtcNow.AddMinutes(60))));

        OneDriveAccountSessionService service = CreateSessionService(databasePath, tokenStorePath, authClient);
        _ = await service.LinkAccountAsync("unlink@example.com", TestContext.Current.CancellationToken);
        Result<Unit, string> unlinkResult = await service.UnlinkAccountAsync("acct-4", TestContext.Current.CancellationToken);
        Result<AccountSessionState, string> sessionResult = await service.GetValidSessionAsync("acct-4", TestContext.Current.CancellationToken);

        unlinkResult.Map(_ => true).Match(ok => ok, _ => false).ShouldBeTrue();
        sessionResult.Map(_ => false).Match(ok => ok, _ => true).ShouldBeTrue();
    }

    private static OneDriveAccountSessionService CreateSessionService(string databasePath, string tokenStorePath, DeterministicOneDriveAuthenticationAdapter authClient)
        => new(
            authClient,
            new FileBackedSecureAccountTokenStore(tokenStorePath),
            new SqliteAccountSessionMetadataRepository(databasePath));

    private static OneDriveAuthenticationResult CreateAuthResult(string accountId, string email, DateTime expiresUtc)
        => new(
            accountId,
            email,
            12_000,
            1_500,
            $"access-{Guid.NewGuid():N}",
            $"refresh-{Guid.NewGuid():N}",
            expiresUtc);

    private static string CreateDatabasePath()
        => Path.GetTempPath().CombinePath($"astar-onedrive-auth-tests-{Guid.NewGuid():N}", "astar-onedrive.db");

    private sealed class DeterministicOneDriveAuthenticationAdapter(
        Func<string, CancellationToken, Task<Result<OneDriveAuthenticationResult, string>>> Acquire,
        Func<string, string, CancellationToken, Task<Result<OneDriveAuthenticationResult, string>>> Refresh) : IOneDriveAuthenticationAdapter
    {
        public int RefreshCalls { get; private set; }

        public Task<Result<OneDriveAuthenticationResult, string>> AcquireInteractiveTokenAsync(string emailHint, CancellationToken cancellationToken = default)
            => Acquire(emailHint, cancellationToken);

        public Task<Result<OneDriveAuthenticationResult, string>> RefreshTokenAsync(string accountId, string refreshToken, CancellationToken cancellationToken = default)
        {
            RefreshCalls++;
            return Refresh(accountId, refreshToken, cancellationToken);
        }
    }
}