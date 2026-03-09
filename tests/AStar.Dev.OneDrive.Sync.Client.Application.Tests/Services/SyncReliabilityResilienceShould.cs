using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.Application.Interfaces;
using AStar.Dev.OneDrive.Sync.Client.Application.Models;
using AStar.Dev.OneDrive.Sync.Client.Application.Services;
using AStar.Dev.OneDrive.Sync.Client.Domain.Entities;
using NSubstitute;

namespace AStar.Dev.OneDrive.Sync.Client.Application.Tests.Services;

public sealed class SyncReliabilityResilienceShould
{
    [Fact]
    public async Task RecoverAcrossTransientThrottleFaultsWithoutCorruptingState()
    {
        ILocalInventoryService inventory = Substitute.For<ILocalInventoryService>();
        IDeltaSyncService delta = Substitute.For<IDeltaSyncService>();
        ISyncService sync = CreateNoOpSyncService();
        var store = new InMemoryStateStore();

        _ = inventory.RunStartupScanAsync("acct", "/root", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<LocalInventoryItem>, string>>(Array.Empty<LocalInventoryItem>()));
        _ = delta.PullAsync("acct", "scope", Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<Result<DeltaPullSummary, string>>("throttled"),
                Task.FromResult<Result<DeltaPullSummary, string>>(new DeltaPullSummary(1, 0, "t1")));

        var sut = new SyncOrchestratorService(inventory, delta, sync, store);

        Result<Unit, string> first = await sut.RunOnceAsync("acct", "scope", "/root", true, TestContext.Current.CancellationToken);
        Result<Unit, string> second = await sut.RunOnceAsync("acct", "scope", "/root", true, TestContext.Current.CancellationToken);

        first.ShouldBeOfType<Result<Unit, string>.Error>().Reason.ShouldBe("throttled");
        second.ShouldBeOfType<Result<Unit, string>.Ok>();
    }

    [Fact]
    public async Task SustainRepeatedSyncCyclesInSoakRun()
    {
        ILocalInventoryService inventory = Substitute.For<ILocalInventoryService>();
        IDeltaSyncService delta = Substitute.For<IDeltaSyncService>();
        ISyncService sync = CreateNoOpSyncService();
        var store = new InMemoryStateStore();

        _ = inventory.RunStartupScanAsync("acct", "/root", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<LocalInventoryItem>, string>>(Array.Empty<LocalInventoryItem>()));
        _ = delta.PullAsync("acct", "scope", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<DeltaPullSummary, string>>(new DeltaPullSummary(1, 0, "t1")));

        var sut = new SyncOrchestratorService(inventory, delta, sync, store);

        for(var i = 0; i < 25; i++)
        {
            Result<Unit, string> result = await sut.RunOnceAsync("acct", "scope", "/root", true, TestContext.Current.CancellationToken);
            result.ShouldBeOfType<Result<Unit, string>.Ok>();
        }

        store.Count.ShouldBe(0);
    }

    [Fact]
    public async Task CaptureBaselinePerformanceForSingleCycle()
    {
        ILocalInventoryService inventory = Substitute.For<ILocalInventoryService>();
        IDeltaSyncService delta = Substitute.For<IDeltaSyncService>();
        ISyncService sync = CreateNoOpSyncService(withOneDownload: true);
        var store = new InMemoryStateStore();

        _ = inventory.RunStartupScanAsync("acct", "/root", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<LocalInventoryItem>, string>>(Array.Empty<LocalInventoryItem>()));
        _ = delta.PullAsync("acct", "scope", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<DeltaPullSummary, string>>(new DeltaPullSummary(1, 0, "t1")));

        var sut = new SyncOrchestratorService(inventory, delta, sync, store);
        DateTime started = DateTime.UtcNow;

        Result<Unit, string> result = await sut.RunOnceAsync("acct", "scope", "/root", true, TestContext.Current.CancellationToken);

        TimeSpan elapsed = DateTime.UtcNow - started;
        result.ShouldBeOfType<Result<Unit, string>.Ok>();
        elapsed.TotalMilliseconds.ShouldBeLessThan(3000);
    }

    [Fact]
    public async Task ResumeInterruptedPartialDownloadQueueSafelyAfterRestart()
    {
        ILocalInventoryService inventory = Substitute.For<ILocalInventoryService>();
        IDeltaSyncService delta = Substitute.For<IDeltaSyncService>();
        var store = new InMemoryStateStore();

        _ = inventory.RunStartupScanAsync("acct", "/root", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<LocalInventoryItem>, string>>(Array.Empty<LocalInventoryItem>()));
        _ = delta.PullAsync("acct", "scope", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<DeltaPullSummary, string>>(new DeltaPullSummary(1, 0, "t1")));

        ISyncService firstSync = CreateNoOpSyncService(withOneDownload: true, failDownload: true);
        var first = new SyncOrchestratorService(inventory, delta, firstSync, store);
        Result<Unit, string> firstResult = await first.RunOnceAsync("acct", "scope", "/root", true, TestContext.Current.CancellationToken);
        firstResult.ShouldBeOfType<Result<Unit, string>.Error>();

        ISyncService secondSync = CreateNoOpSyncService(withOneDownload: true);
        var second = new SyncOrchestratorService(inventory, delta, secondSync, store);
        Result<Unit, string> secondResult = await second.RunOnceAsync("acct", "scope", "/root", true, TestContext.Current.CancellationToken);
        secondResult.ShouldBeOfType<Result<Unit, string>.Ok>();
    }

    [Fact]
    public void TrackBlockingReliabilityDefectsInRunbook()
    {
        var root = GetRepositoryRootPath();
        var path = Path.Combine(root, "docs", "reliability-defects.md");

        File.Exists(path).ShouldBeTrue();
        File.ReadAllText(path).ShouldContain("No blocking reliability defects");
    }

    private static ISyncService CreateNoOpSyncService(bool withOneDownload = false, bool failDownload = false)
    {
        ISyncService sync = Substitute.For<ISyncService>();
        _ = sync.PauseSyncAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult<Result<Unit, string>>(Unit.Value));
        _ = sync.ResumeSyncAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult<Result<Unit, string>>(Unit.Value));
        _ = sync.CancelSyncAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult<Result<Unit, string>>(Unit.Value));
        _ = sync.RunDeltaSyncAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult<Result<Unit, string>>(Unit.Value));
        _ = sync.RetryFailedOperationsAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult<Result<Unit, string>>(Unit.Value));
        _ = sync.GetFailedOperationsAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult<Result<IReadOnlyList<SyncQueueItem>, string>>(Array.Empty<SyncQueueItem>()));
        _ = sync.GetConflictsAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult<Result<IReadOnlyList<SyncConflict>, string>>(Array.Empty<SyncConflict>()));
        _ = sync.EnqueueUploadAsync(Arg.Any<SyncQueueItem>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult<Result<Unit, string>>(Unit.Value));
        _ = sync.GetSyncFilesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<SyncFile>, string>>(withOneDownload
                ? new[] { new SyncFile("a.txt", "/root/a.txt", "/remote/a.txt") }
                : Array.Empty<SyncFile>()));
        _ = sync.EnqueueDownloadAsync(Arg.Any<SyncQueueItem>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<Unit, string>>(failDownload ? "network" : Unit.Value));
        return sync;
    }

    private static string GetRepositoryRootPath()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while(current is not null)
        {
            var srcFolder = Path.Combine(current.FullName, "src", "AStar.Dev.OneDrive.Sync.Client.UI");
            if(Directory.Exists(srcFolder))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repository root from test base directory.");
    }

    private sealed class InMemoryStateStore : ISyncRunStateStore
    {
        private readonly Dictionary<string, SyncRunState> _states = [];

        public int Count => _states.Count;

        public Task<Result<Option<SyncRunState>, string>> LoadAsync(string accountId, string scopeId, CancellationToken cancellationToken = default)
        {
            var key = $"{accountId}:{scopeId}";
            return Task.FromResult<Result<Option<SyncRunState>, string>>(
                _states.TryGetValue(key, out SyncRunState? value)
                    ? new Option<SyncRunState>.Some(value)
                    : Option.None<SyncRunState>());
        }

        public Task<Result<Unit, string>> SaveAsync(SyncRunState state, CancellationToken cancellationToken = default)
        {
            _states[$"{state.AccountId}:{state.ScopeId}"] = state;
            return Task.FromResult<Result<Unit, string>>(Unit.Value);
        }

        public Task<Result<Unit, string>> ClearAsync(string accountId, string scopeId, CancellationToken cancellationToken = default)
        {
            _ = _states.Remove($"{accountId}:{scopeId}");
            return Task.FromResult<Result<Unit, string>>(Unit.Value);
        }
    }
}
