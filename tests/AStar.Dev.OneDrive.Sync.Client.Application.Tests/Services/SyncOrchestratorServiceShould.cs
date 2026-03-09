using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.Application.Interfaces;
using AStar.Dev.OneDrive.Sync.Client.Application.Models;
using AStar.Dev.OneDrive.Sync.Client.Application.Services;
using AStar.Dev.OneDrive.Sync.Client.Domain.Entities;
using NSubstitute;

namespace AStar.Dev.OneDrive.Sync.Client.Application.Tests.Services;

public sealed class SyncOrchestratorServiceShould
{
    [Fact]
    public async Task RunStateMachineInScanDeltaUploadDownloadOrder()
    {
        var events = new List<string>();
        ILocalInventoryService inventory = Substitute.For<ILocalInventoryService>();
        IDeltaSyncService delta = Substitute.For<IDeltaSyncService>();
        ISyncService sync = Substitute.For<ISyncService>();
        var store = new InMemorySyncRunStateStore();

        _ = inventory.RunStartupScanAsync("acct", "/root", Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                events.Add("scan");
                return Task.FromResult<Result<IReadOnlyList<LocalInventoryItem>, string>>(Array.Empty<LocalInventoryItem>());
            });
        _ = delta.PullAsync("acct", "scope", Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                events.Add("delta");
                return Task.FromResult<Result<DeltaPullSummary, string>>(new DeltaPullSummary(1, 0, "t1"));
            });
        _ = sync.RetryFailedOperationsAsync(Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                events.Add("upload");
                return Task.FromResult<Result<Unit, string>>(Unit.Value);
            });
        _ = sync.GetSyncFilesAsync(Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromResult<Result<IReadOnlyList<SyncFile>, string>>(new[] { new SyncFile("a.txt", "/root/a.txt", "/remote/a.txt") }));
        _ = sync.EnqueueDownloadAsync(Arg.Any<SyncQueueItem>(), Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                events.Add("download");
                return Task.FromResult<Result<Unit, string>>(Unit.Value);
            });
        _ = sync.GetFailedOperationsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<SyncQueueItem>, string>>(Array.Empty<SyncQueueItem>()));
        _ = sync.GetConflictsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<SyncConflict>, string>>(Array.Empty<SyncConflict>()));

        var sut = new SyncOrchestratorService(inventory, delta, sync, store);

        Result<Unit, string> result = await sut.RunOnceAsync("acct", "scope", "/root", true, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<Result<Unit, string>.Ok>();
        events.ShouldBe(["scan", "delta", "upload", "download"]);
    }

    [Fact]
    public async Task PauseAndResumeRunWithoutLosingWork()
    {
        ILocalInventoryService inventory = Substitute.For<ILocalInventoryService>();
        IDeltaSyncService delta = Substitute.For<IDeltaSyncService>();
        ISyncService sync = Substitute.For<ISyncService>();
        var store = new InMemorySyncRunStateStore();
        var gate = new TaskCompletionSource<bool>();

        _ = inventory.RunStartupScanAsync("acct", "/root", Arg.Any<CancellationToken>())
            .Returns(async _ =>
            {
                await gate.Task;
                return (Result<IReadOnlyList<LocalInventoryItem>, string>)Array.Empty<LocalInventoryItem>();
            });
        _ = delta.PullAsync("acct", "scope", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<DeltaPullSummary, string>>(new DeltaPullSummary(1, 0, "t1")));
        _ = sync.RetryFailedOperationsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<Unit, string>>(Unit.Value));
        _ = sync.GetSyncFilesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<SyncFile>, string>>(Array.Empty<SyncFile>()));
        _ = sync.GetFailedOperationsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<SyncQueueItem>, string>>(Array.Empty<SyncQueueItem>()));
        _ = sync.GetConflictsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<SyncConflict>, string>>(Array.Empty<SyncConflict>()));

        var sut = new SyncOrchestratorService(inventory, delta, sync, store);
        _ = await sut.PauseAsync(TestContext.Current.CancellationToken);
        Task<Result<Unit, string>> runTask = sut.RunOnceAsync("acct", "scope", "/root", true, TestContext.Current.CancellationToken);

        await Task.Delay(60, TestContext.Current.CancellationToken);
        runTask.IsCompleted.ShouldBeFalse();

        _ = await sut.ResumeAsync(TestContext.Current.CancellationToken);
        _ = gate.TrySetResult(true);
        Result<Unit, string> result = await runTask;

        result.ShouldBeOfType<Result<Unit, string>.Ok>();
    }

    [Fact]
    public async Task ResumeInterruptedRunFromPersistedStageAndQueue()
    {
        ILocalInventoryService inventory = Substitute.For<ILocalInventoryService>();
        IDeltaSyncService delta = Substitute.For<IDeltaSyncService>();
        ISyncService firstSync = Substitute.For<ISyncService>();
        var store = new InMemorySyncRunStateStore();

        _ = inventory.RunStartupScanAsync("acct", "/root", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<LocalInventoryItem>, string>>(Array.Empty<LocalInventoryItem>()));
        _ = delta.PullAsync("acct", "scope", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<DeltaPullSummary, string>>(new DeltaPullSummary(1, 0, "t1")));
        _ = firstSync.RetryFailedOperationsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<Unit, string>>(Unit.Value));
        _ = firstSync.GetSyncFilesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<SyncFile>, string>>(new[] { new SyncFile("a.txt", "/root/a.txt", "/remote/a.txt") }));
        _ = firstSync.EnqueueDownloadAsync(Arg.Any<SyncQueueItem>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<Unit, string>>("disk-full"));
        _ = firstSync.GetFailedOperationsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<SyncQueueItem>, string>>(Array.Empty<SyncQueueItem>()));
        _ = firstSync.GetConflictsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<SyncConflict>, string>>(Array.Empty<SyncConflict>()));

        var first = new SyncOrchestratorService(inventory, delta, firstSync, store);
        Result<Unit, string> firstResult = await first.RunOnceAsync("acct", "scope", "/root", true, TestContext.Current.CancellationToken);
        firstResult.ShouldBeOfType<Result<Unit, string>.Error>();

        ISyncService secondSync = Substitute.For<ISyncService>();
        _ = secondSync.RetryFailedOperationsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<Unit, string>>(Unit.Value));
        _ = secondSync.EnqueueDownloadAsync(Arg.Any<SyncQueueItem>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<Unit, string>>(Unit.Value));
        _ = secondSync.GetSyncFilesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<SyncFile>, string>>(Array.Empty<SyncFile>()));
        _ = secondSync.GetFailedOperationsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<SyncQueueItem>, string>>(Array.Empty<SyncQueueItem>()));
        _ = secondSync.GetConflictsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<SyncConflict>, string>>(Array.Empty<SyncConflict>()));

        var second = new SyncOrchestratorService(inventory, delta, secondSync, store);
        Result<Unit, string> secondResult = await second.RunOnceAsync("acct", "scope", "/root", true, TestContext.Current.CancellationToken);

        secondResult.ShouldBeOfType<Result<Unit, string>.Ok>();
        await inventory.Received(1).RunStartupScanAsync("acct", "/root", Arg.Any<CancellationToken>());
        await delta.Received(1).PullAsync("acct", "scope", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TriggerStartupAndBackgroundRuns()
    {
        ILocalInventoryService inventory = Substitute.For<ILocalInventoryService>();
        IDeltaSyncService delta = Substitute.For<IDeltaSyncService>();
        ISyncService sync = Substitute.For<ISyncService>();
        var store = new InMemorySyncRunStateStore();
        var startupCount = 0;
        var manualCount = 0;

        _ = inventory.RunStartupScanAsync("acct", "/root", Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                startupCount++;
                return Task.FromResult<Result<IReadOnlyList<LocalInventoryItem>, string>>(Array.Empty<LocalInventoryItem>());
            });
        _ = inventory.RunManualScanAsync("acct", "/root", Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                manualCount++;
                return Task.FromResult<Result<IReadOnlyList<LocalInventoryItem>, string>>(Array.Empty<LocalInventoryItem>());
            });
        _ = delta.PullAsync("acct", "scope", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<DeltaPullSummary, string>>(new DeltaPullSummary(1, 0, "t1")));
        _ = sync.RetryFailedOperationsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<Unit, string>>(Unit.Value));
        _ = sync.GetSyncFilesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<SyncFile>, string>>(Array.Empty<SyncFile>()));
        _ = sync.GetFailedOperationsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<SyncQueueItem>, string>>(Array.Empty<SyncQueueItem>()));
        _ = sync.GetConflictsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<SyncConflict>, string>>(Array.Empty<SyncConflict>()));

        var sut = new SyncOrchestratorService(inventory, delta, sync, store);
        _ = await sut.StartBackgroundSchedulingAsync("acct", "scope", "/root", TimeSpan.FromMilliseconds(80), TestContext.Current.CancellationToken);

        await WaitForConditionAsync(() => Task.FromResult(startupCount >= 1 && manualCount >= 1));

        _ = await sut.StopBackgroundSchedulingAsync(TestContext.Current.CancellationToken);
    }

    private static async Task WaitForConditionAsync(Func<Task<bool>> condition)
    {
        for(var attempt = 0; attempt < 120; attempt++)
        {
            if(await condition())
            {
                return;
            }

            await Task.Delay(20, TestContext.Current.CancellationToken);
        }

        throw new TimeoutException("Condition was not met within the allowed time.");
    }

    private sealed class InMemorySyncRunStateStore : ISyncRunStateStore
    {
        private readonly Dictionary<string, SyncRunState> _states = [];

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
