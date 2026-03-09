using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.Application.Interfaces;
using AStar.Dev.OneDrive.Sync.Client.Application.Models;
using AStar.Dev.OneDrive.Sync.Client.Domain.Entities;
using AStar.Dev.OneDrive.Sync.Client.UI.SyncStatus;

namespace AStar.Dev.OneDrive.Sync.Client.UI.Tests.ViewModels.SyncStatus;

public sealed class SyncStatusConflictShould
{
    [Fact]
    public async Task LogConflictOutcomesAfterSyncCompletes()
    {
        var service = new ConflictReportingSyncService();
        var sut = new SyncStatusViewModel(service);

        sut.StartSyncCommand.Execute(null);
        await WaitForConditionAsync(() => sut.Status == "Idle" && sut.RecentActivity.Count > 0);

        sut.RecentActivity.Any(x => x.Message.Contains("Conflict", StringComparison.Ordinal)).ShouldBeTrue();
    }

    private static async Task WaitForConditionAsync(Func<bool> condition)
    {
        for(var attempt = 0; attempt < 100; attempt++)
        {
            if(condition())
            {
                return;
            }

            await Task.Delay(20, TestContext.Current.CancellationToken);
        }

        throw new TimeoutException("Condition was not met within the allowed time.");
    }

    private sealed class ConflictReportingSyncService : ISyncService
    {
        public Task<Result<IReadOnlyList<SyncFile>, string>> GetSyncFilesAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<Result<IReadOnlyList<SyncFile>, string>>(Array.Empty<SyncFile>());

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

        public Task<Result<IReadOnlyList<SyncConflict>, string>> GetConflictsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<Result<IReadOnlyList<SyncConflict>, string>>(
                new List<SyncConflict> { new("q-1", "etag", "Conflict resolved using manual queue") });

        public Task<Result<Unit, string>> RetryFailedOperationsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<Result<Unit, string>>(Unit.Value);
    }
}
