using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.Application.Interfaces;
using AStar.Dev.OneDrive.Sync.Client.Application.Models;
using AStar.Dev.OneDrive.Sync.Client.Application.Services;
using NSubstitute;

namespace AStar.Dev.OneDrive.Sync.Client.Application.Tests.Services;

public sealed class DeltaSyncServiceShould
{
    private readonly IRemoteDeltaSource _source;
    private readonly IDeltaCheckpointStore _checkpoints;
    private readonly IRemoteDeltaApplier _applier;
    private readonly IDeltaSyncService _sut;

    public DeltaSyncServiceShould()
    {
        _source = Substitute.For<IRemoteDeltaSource>();
        _checkpoints = Substitute.For<IDeltaCheckpointStore>();
        _applier = Substitute.For<IRemoteDeltaApplier>();
        _sut = new DeltaSyncService(_source, _checkpoints, _applier);
    }

    [Fact]
    public async Task CommitCheckpointAfterProcessingAllDeltaPages()
    {
        var accountId = "acct-a";
        var scopeId = "drive-root";
        var firstPage = new RemoteDeltaPage(
            [new RemoteDeltaItem("1", "/a.txt", RemoteDeltaChangeKind.Updated)],
            "cursor-2",
            null);
        var secondPage = new RemoteDeltaPage(
            [new RemoteDeltaItem("2", "/b.txt", RemoteDeltaChangeKind.Created)],
            null,
            "cursor-final");

        _ = _checkpoints.LoadAsync(accountId, scopeId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<Option<SyncCheckpoint>, string>>(Option.None<SyncCheckpoint>()));
        _ = _source.GetDeltaPageAsync(accountId, scopeId, null, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<RemoteDeltaPage, string>>(firstPage));
        _ = _source.GetDeltaPageAsync(accountId, scopeId, "cursor-2", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<RemoteDeltaPage, string>>(secondPage));
        _ = _applier.ApplyAsync(accountId, scopeId, Arg.Any<IReadOnlyList<RemoteDeltaItem>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<Unit, string>>(Unit.Value));
        _ = _checkpoints.SaveAsync(Arg.Any<SyncCheckpoint>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<Unit, string>>(Unit.Value));

        Result<DeltaPullSummary, string> result = await _sut.PullAsync(accountId, scopeId, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<Result<DeltaPullSummary, string>.Ok>().Value.ChangeCount.ShouldBe(2);
        await _checkpoints.Received(1).SaveAsync(
            Arg.Is<SyncCheckpoint>(x => x.AccountId == accountId && x.ScopeId == scopeId && x.DeltaToken == "cursor-final"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task KeepPreviousCheckpointWhenDeltaPullIsInterrupted()
    {
        var accountId = "acct-b";
        var scopeId = "drive-root";
        var existing = new SyncCheckpoint(accountId, scopeId, "cursor-prev", DateTime.UtcNow.AddMinutes(-5));
        var firstPage = new RemoteDeltaPage(
            [new RemoteDeltaItem("1", "/a.txt", RemoteDeltaChangeKind.Updated)],
            "cursor-next",
            null);

        _ = _checkpoints.LoadAsync(accountId, scopeId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<Option<SyncCheckpoint>, string>>(new Option<SyncCheckpoint>.Some(existing)));
        _ = _source.GetDeltaPageAsync(accountId, scopeId, "cursor-prev", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<RemoteDeltaPage, string>>(firstPage));
        _ = _source.GetDeltaPageAsync(accountId, scopeId, "cursor-next", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<RemoteDeltaPage, string>>("transient failure"));
        _ = _applier.ApplyAsync(accountId, scopeId, Arg.Any<IReadOnlyList<RemoteDeltaItem>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<Unit, string>>(Unit.Value));

        Result<DeltaPullSummary, string> result = await _sut.PullAsync(accountId, scopeId, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<Result<DeltaPullSummary, string>.Error>().Reason.ShouldBe("transient failure");
        await _checkpoints.DidNotReceive().SaveAsync(Arg.Any<SyncCheckpoint>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UseExistingCursorAndAvoidFullRefreshOnUnchangedRun()
    {
        var accountId = "acct-c";
        var scopeId = "drive-root";
        var existing = new SyncCheckpoint(accountId, scopeId, "cursor-current", DateTime.UtcNow.AddMinutes(-1));
        var unchangedPage = new RemoteDeltaPage([], null, "cursor-current");

        _ = _checkpoints.LoadAsync(accountId, scopeId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<Option<SyncCheckpoint>, string>>(new Option<SyncCheckpoint>.Some(existing)));
        _ = _source.GetDeltaPageAsync(accountId, scopeId, "cursor-current", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<RemoteDeltaPage, string>>(unchangedPage));

        Result<DeltaPullSummary, string> result = await _sut.PullAsync(accountId, scopeId, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<Result<DeltaPullSummary, string>.Ok>().Value.ChangeCount.ShouldBe(0);
        await _source.Received(1).GetDeltaPageAsync(accountId, scopeId, "cursor-current", Arg.Any<CancellationToken>());
        await _source.DidNotReceive().GetDeltaPageAsync(accountId, scopeId, null, Arg.Any<CancellationToken>());
        await _checkpoints.DidNotReceive().SaveAsync(Arg.Any<SyncCheckpoint>(), Arg.Any<CancellationToken>());
    }
}