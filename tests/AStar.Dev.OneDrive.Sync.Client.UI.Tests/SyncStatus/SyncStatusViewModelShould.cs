using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.Application.Interfaces;
using AStar.Dev.OneDrive.Sync.Client.Application.Models;
using AStar.Dev.OneDrive.Sync.Client.Domain.Entities;
using AStar.Dev.OneDrive.Sync.Client.UI.SyncStatus;

namespace AStar.Dev.OneDrive.Sync.Client.UI.Tests.ViewModels.SyncStatus;

public sealed class SyncStatusViewModelShould
{
    [Fact]
    public void InitializeStatusToIdle()
    {
        var viewModel = new SyncStatusViewModel();

        viewModel.Status.ShouldBe("Idle");
    }

    [Fact]
    public void SetStatusToSyncingWhenStartSyncCommandIsExecuted()
    {
        var completion = new TaskCompletionSource<Result<IReadOnlyList<SyncFile>, string>>();
        var viewModel = new SyncStatusViewModel(new DeferredSyncService(completion.Task));

        viewModel.StartSyncCommand.Execute(null);

        viewModel.Status.ShouldBe("Syncing...");

        _ = completion.TrySetResult(new List<SyncFile>());
    }

    [Fact]
    public void SetStatusToPausedWhenPauseSyncCommandIsExecuted()
    {
        var syncService = new TrackingSyncService();
        var viewModel = new SyncStatusViewModel(syncService);

        viewModel.PauseSyncCommand.Execute(null);

        viewModel.Status.ShouldBe("Paused");
        syncService.PauseCalls.ShouldBe(1);
    }

    [Fact]
    public async Task QueueDownloadsAndUpdateProgressWhenSyncStarts()
    {
        var syncService = new TrackingSyncService
        {
            Files =
            [
                new SyncFile("a.txt", "/local/a.txt", "/remote/a.txt"),
                new SyncFile("b.txt", "/local/b.txt", "/remote/b.txt")
            ]
        };
        var viewModel = new SyncStatusViewModel(syncService);

        viewModel.StartSyncCommand.Execute(null);
        await WaitForConditionAsync(() => syncService.DownloadEnqueueCount == 2 && viewModel.ProgressPercentage == 100 && viewModel.Status == "Idle");

        syncService.DownloadEnqueueCount.ShouldBe(2);
        viewModel.ProgressPercentage.ShouldBe(100);
    }

    [Fact]
    public void UpdateProgressPercentageWhenSet()
    {
        var viewModel = new SyncStatusViewModel
        {
            ProgressPercentage = 42
        };

        viewModel.ProgressPercentage.ShouldBe(42);
    }

    [Fact]
    public void LogRecentActivityOnStateChanges()
    {
        var viewModel = new SyncStatusViewModel();

        viewModel.StartSyncCommand.Execute(null);

        viewModel.RecentActivity.ShouldNotBeEmpty();
    }

    [Fact]
    public void StoreSyncErrorMessageWhenSet()
    {
        var viewModel = new SyncStatusViewModel
        {
            SyncError = "Network unavailable"
        };

        viewModel.SyncError.ShouldBe("Network unavailable");
    }

    [Fact]
    public void FirePropertyChangedEventOnStateChanges()
    {
        var viewModel = new SyncStatusViewModel();
        var raised = false;
        viewModel.PropertyChanged += (_, args) => raised |= args.PropertyName == nameof(SyncStatusViewModel.Status);

        viewModel.StartSyncCommand.Execute(null);

        raised.ShouldBeTrue();
    }

    private sealed class DeferredSyncService(Task<Result<IReadOnlyList<SyncFile>, string>> resultTask) : ISyncService
    {
        public Task<Result<IReadOnlyList<SyncFile>, string>> GetSyncFilesAsync(CancellationToken cancellationToken = default)
            => resultTask;

        public Task<Result<Unit, string>> PauseSyncAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<Result<Unit, string>>(Unit.Value);

        public Task<Result<Unit, string>> ResumeSyncAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<Result<Unit, string>>(Unit.Value);

        public Task<Result<Unit, string>> CancelSyncAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<Result<Unit, string>>(Unit.Value);

        public Task<Result<Unit, string>> RunDeltaSyncAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<Result<Unit, string>>(Unit.Value);

        public Task<Result<Unit, string>> EnqueueUploadAsync(SyncQueueItem queueItem, CancellationToken cancellationToken = default)
            => Task.FromResult<Result<Unit, string>>(Unit.Value);

        public Task<Result<Unit, string>> EnqueueDownloadAsync(SyncQueueItem queueItem, CancellationToken cancellationToken = default)
            => Task.FromResult<Result<Unit, string>>(Unit.Value);

        public Task<Result<IReadOnlyList<SyncQueueItem>, string>> GetFailedOperationsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<Result<IReadOnlyList<SyncQueueItem>, string>>(Array.Empty<SyncQueueItem>());

        public Task<Result<Unit, string>> RetryFailedOperationsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<Result<Unit, string>>(Unit.Value);
    }

    private sealed class TrackingSyncService : ISyncService
    {
        public int PauseCalls { get; private set; }
        public int DownloadEnqueueCount { get; private set; }
        public IReadOnlyList<SyncFile> Files { get; set; } = [];

        public Task<Result<IReadOnlyList<SyncFile>, string>> GetSyncFilesAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<Result<IReadOnlyList<SyncFile>, string>>(Files.ToList());

        public Task<Result<Unit, string>> PauseSyncAsync(CancellationToken cancellationToken = default)
        {
            PauseCalls++;
            return Task.FromResult<Result<Unit, string>>(Unit.Value);
        }

        public Task<Result<Unit, string>> ResumeSyncAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<Result<Unit, string>>(Unit.Value);

        public Task<Result<Unit, string>> CancelSyncAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<Result<Unit, string>>(Unit.Value);

        public Task<Result<Unit, string>> RunDeltaSyncAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<Result<Unit, string>>(Unit.Value);

        public Task<Result<Unit, string>> EnqueueUploadAsync(SyncQueueItem queueItem, CancellationToken cancellationToken = default)
            => Task.FromResult<Result<Unit, string>>(Unit.Value);

        public Task<Result<Unit, string>> EnqueueDownloadAsync(SyncQueueItem queueItem, CancellationToken cancellationToken = default)
        {
            DownloadEnqueueCount++;
            return Task.FromResult<Result<Unit, string>>(Unit.Value);
        }

        public Task<Result<IReadOnlyList<SyncQueueItem>, string>> GetFailedOperationsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<Result<IReadOnlyList<SyncQueueItem>, string>>(Array.Empty<SyncQueueItem>());

        public Task<Result<Unit, string>> RetryFailedOperationsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<Result<Unit, string>>(Unit.Value);
    }

    private static async Task WaitForConditionAsync(Func<bool> condition)
    {
        for(var attempt = 0; attempt < 80; attempt++)
        {
            if(condition())
            {
                return;
            }

            await Task.Delay(20, TestContext.Current.CancellationToken);
        }

        throw new TimeoutException("Condition was not met within the allowed time.");
    }
}
