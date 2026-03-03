using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.Application.Interfaces;
using AStar.Dev.OneDrive.Sync.Client.Domain.Entities;
using AStar.Dev.OneDrive.Sync.Client.UI.SyncStatus;

namespace AStar.Dev.OneDrive.Sync.Client.UI.Tests.Integration;

public sealed class SyncIntegrationShould
{
    [Fact]
    public void SetStatusToSyncingWhenSyncNowIsClicked()
    {
        var completion = new TaskCompletionSource<Result<IReadOnlyList<SyncFile>, string>>();
        var service = new DeferredSyncService(completion.Task);
        var viewModel = new SyncStatusViewModel(service);

        viewModel.SyncNowCommand.Execute(null);

        viewModel.Status.ShouldBe("Syncing...");
        _ = completion.TrySetResult(new List<SyncFile>());
    }

    [Fact]
    public async Task SetStatusToIdleWhenSyncCompletes()
    {
        var service = new FixedSyncService(new List<SyncFile>());
        var viewModel = new SyncStatusViewModel(service);

        viewModel.SyncNowCommand.Execute(null);
        await WaitForConditionAsync(() => viewModel.Status == "Idle");

        viewModel.Status.ShouldBe("Idle");
    }

    [Fact]
    public async Task SetStatusToErrorAndMessageWhenSyncFails()
    {
        const string expectedError = "sync failed";
        var service = new FixedSyncService(expectedError);
        var viewModel = new SyncStatusViewModel(service);

        viewModel.SyncNowCommand.Execute(null);
        await WaitForConditionAsync(() => viewModel.Status == "Error");

        viewModel.Status.ShouldBe("Error");
        viewModel.SyncError.ShouldBe(expectedError);
    }

    private static async Task WaitForConditionAsync(Func<bool> condition)
    {
        for (var attempt = 0; attempt < 50 && !condition(); attempt++)
        {
            await Task.Delay(10, TestContext.Current.CancellationToken);
        }

        condition().ShouldBeTrue();
    }

    private sealed class DeferredSyncService(Task<Result<IReadOnlyList<SyncFile>, string>> resultTask) : ISyncService
    {
        public Task<Result<IReadOnlyList<SyncFile>, string>> GetSyncFilesAsync(CancellationToken cancellationToken = default)
            => resultTask;
    }

    private sealed class FixedSyncService : ISyncService
    {
        private readonly Result<IReadOnlyList<SyncFile>, string> _result;

        public FixedSyncService(IReadOnlyList<SyncFile> files)
            => _result = new Result<IReadOnlyList<SyncFile>, string>.Ok(files);

        public FixedSyncService(string error)
            => _result = new Result<IReadOnlyList<SyncFile>, string>.Error(error);

        public Task<Result<IReadOnlyList<SyncFile>, string>> GetSyncFilesAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(_result);
    }
}