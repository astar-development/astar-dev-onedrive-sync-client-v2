using System.Net;
using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.Application.Interfaces;
using AStar.Dev.OneDrive.Sync.Client.Application.Models;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Authentication;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Graph;
using NSubstitute;

namespace AStar.Dev.OneDrive.Sync.Client.Infrastructure.Tests.Graph;

public sealed class OneDriveRemoteDeltaSourceShould
{
    [Fact]
    public async Task RequestInitialDeltaPageAndMapChanges()
    {
        IOneDriveGraphClient graphClient = Substitute.For<IOneDriveGraphClient>();
        IAccountSessionService accountSessionService = Substitute.For<IAccountSessionService>();
        ISecureAccountTokenStore tokenStore = Substitute.For<ISecureAccountTokenStore>();
        var sut = new OneDriveRemoteDeltaSource(graphClient, accountSessionService, tokenStore);
        var accountId = "acct-a";
        var scopeId = "drive-root";
        var session = new AccountSessionState(
            new OneDriveAccountProfile(accountId, "a@example.com", 0, 0),
            new AccountSessionMetadata(accountId, DateTime.UtcNow.AddMinutes(30), DateTime.UtcNow, DateTime.UtcNow, false));
        const string payload = "{\"value\":[{\"id\":\"f1\",\"name\":\"report.txt\",\"parentReference\":{\"path\":\"/drive/root:/Docs\"}}],\"@odata.nextLink\":\"https://graph.microsoft.com/v1.0/me/drive/root/delta?$skiptoken=abc\"}";

        _ = accountSessionService.GetValidSessionAsync(accountId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<AccountSessionState, string>>(session));
        _ = tokenStore.LoadAsync(accountId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<Option<SecureAccountTokens>, string>>(new Option<SecureAccountTokens>.Some(new SecureAccountTokens("access-token", "refresh-token"))));
        _ = graphClient.SendAsync(Arg.Any<OneDriveGraphRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<OneDriveGraphResponse, SyncError>>(new OneDriveGraphResponse(HttpStatusCode.OK, payload)));

        Result<RemoteDeltaPage, string> result = await sut.GetDeltaPageAsync(accountId, scopeId, null, TestContext.Current.CancellationToken);

        RemoteDeltaPage page = result.ShouldBeOfType<Result<RemoteDeltaPage, string>.Ok>().Value;
        page.NextPageToken.ShouldBe("https://graph.microsoft.com/v1.0/me/drive/root/delta?$skiptoken=abc");
        page.DeltaToken.ShouldBeNull();
        page.Changes.Count.ShouldBe(1);
        page.Changes[0].Id.ShouldBe("f1");
        page.Changes[0].Path.ShouldBe("/Docs/report.txt");
        page.Changes[0].ChangeKind.ShouldBe(RemoteDeltaChangeKind.Updated);
        await graphClient.Received(1)
            .SendAsync(Arg.Is<OneDriveGraphRequest>(x => x.Path == "/me/drive/root/delta" && x.AccessToken == "access-token"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ContinueFromCursorAndMapDeletedItems()
    {
        IOneDriveGraphClient graphClient = Substitute.For<IOneDriveGraphClient>();
        IAccountSessionService accountSessionService = Substitute.For<IAccountSessionService>();
        ISecureAccountTokenStore tokenStore = Substitute.For<ISecureAccountTokenStore>();
        var sut = new OneDriveRemoteDeltaSource(graphClient, accountSessionService, tokenStore);
        var accountId = "acct-b";
        var session = new AccountSessionState(
            new OneDriveAccountProfile(accountId, "b@example.com", 0, 0),
            new AccountSessionMetadata(accountId, DateTime.UtcNow.AddMinutes(30), DateTime.UtcNow, DateTime.UtcNow, false));
        var cursor = "https://graph.microsoft.com/v1.0/me/drive/root/delta?$skiptoken=next";
        const string payload = "{\"value\":[{\"id\":\"f2\",\"name\":\"old.txt\",\"deleted\":{}}],\"@odata.deltaLink\":\"https://graph.microsoft.com/v1.0/me/drive/root/delta?$deltatoken=final\"}";

        _ = accountSessionService.GetValidSessionAsync(accountId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<AccountSessionState, string>>(session));
        _ = tokenStore.LoadAsync(accountId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<Option<SecureAccountTokens>, string>>(new Option<SecureAccountTokens>.Some(new SecureAccountTokens("token-2", "refresh-token"))));
        _ = graphClient.SendAsync(Arg.Any<OneDriveGraphRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<OneDriveGraphResponse, SyncError>>(new OneDriveGraphResponse(HttpStatusCode.OK, payload)));

        Result<RemoteDeltaPage, string> result = await sut.GetDeltaPageAsync(accountId, "drive-root", cursor, TestContext.Current.CancellationToken);

        RemoteDeltaPage page = result.ShouldBeOfType<Result<RemoteDeltaPage, string>.Ok>().Value;
        page.NextPageToken.ShouldBeNull();
        page.DeltaToken.ShouldBe("https://graph.microsoft.com/v1.0/me/drive/root/delta?$deltatoken=final");
        page.Changes.Count.ShouldBe(1);
        page.Changes[0].ChangeKind.ShouldBe(RemoteDeltaChangeKind.Deleted);
        await graphClient.Received(1)
            .SendAsync(Arg.Is<OneDriveGraphRequest>(x => x.Path == cursor && x.AccessToken == "token-2"), Arg.Any<CancellationToken>());
    }
}