using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.Application.Interfaces;
using AStar.Dev.OneDrive.Sync.Client.Application.Models;
using AStar.Dev.OneDrive.Sync.Client.Application.Services;
using AStar.Dev.OneDrive.Sync.Client.Domain.Entities;
using AStar.Dev.OneDrive.Sync.Client.Domain.Interfaces;
using NSubstitute;

namespace AStar.Dev.OneDrive.Sync.Client.Application.Tests.Services;

public sealed class UploadPipelineIntegrationShould
{
    [Fact]
    public async Task ReplayFailedUploadAfterServiceRestart()
    {
        ISyncFileRepository repo = Substitute.For<ISyncFileRepository>();
        var fileSystem = new AcceptingUploadFileSystem();
        var failingClient = new FailingUploadTransferClient();
        var first = new SyncService(repo, uploadTransferClient: failingClient, uploadFileSystem: fileSystem, chunkedUploadThresholdBytes: 100);
        var queueItem = new SyncQueueItem("restart-1", "/tmp/local/restart.txt", "/remote/restart.txt", SyncOperationType.Update);

        _ = await first.EnqueueUploadAsync(queueItem, TestContext.Current.CancellationToken);
        await WaitForConditionAsync(async () =>
        {
            Result<IReadOnlyList<SyncQueueItem>, string> failed = await first.GetFailedOperationsAsync(TestContext.Current.CancellationToken);
            return failed.ShouldBeOfType<Result<IReadOnlyList<SyncQueueItem>, string>.Ok>().Value.Count == 1;
        });

        var succeedingClient = new SucceedingUploadTransferClient();
        var second = new SyncService(repo, uploadTransferClient: succeedingClient, uploadFileSystem: fileSystem, chunkedUploadThresholdBytes: 100);
        _ = await second.EnqueueUploadAsync(queueItem, TestContext.Current.CancellationToken);
        await WaitForConditionAsync(() => Task.FromResult(succeedingClient.UploadCalls > 0));
        Result<IReadOnlyList<SyncQueueItem>, string> secondFailed = await second.GetFailedOperationsAsync(TestContext.Current.CancellationToken);

        secondFailed.ShouldBeOfType<Result<IReadOnlyList<SyncQueueItem>, string>.Ok>().Value.ShouldBeEmpty();
    }

    private static async Task WaitForConditionAsync(Func<Task<bool>> condition)
    {
        for(var attempt = 0; attempt < 100; attempt++)
        {
            if(await condition())
            {
                return;
            }

            await Task.Delay(20, TestContext.Current.CancellationToken);
        }

        throw new TimeoutException("Condition was not met within the allowed time.");
    }

    private sealed class AcceptingUploadFileSystem : IUploadFileSystem
    {
        public Task<Result<Unit, string>> ValidateUploadPathAsync(string localPath, CancellationToken cancellationToken = default)
            => Task.FromResult<Result<Unit, string>>(Unit.Value);

        public Task<Result<long, string>> GetFileSizeBytesAsync(string localPath, CancellationToken cancellationToken = default)
            => Task.FromResult<Result<long, string>>(10L);
    }

    private sealed class FailingUploadTransferClient : IUploadTransferClient
    {
        public Task<Result<Unit, string>> UploadAsync(string localPath, string remotePath, string correlationId, CancellationToken cancellationToken = default)
            => Task.FromResult<Result<Unit, string>>("network");

        public Task<Result<Unit, string>> UploadChunkedAsync(string localPath, string remotePath, string correlationId, CancellationToken cancellationToken = default)
            => Task.FromResult<Result<Unit, string>>("network");

        public Task<Result<Unit, string>> DeleteAsync(string remotePath, string correlationId, CancellationToken cancellationToken = default)
            => Task.FromResult<Result<Unit, string>>("network");
    }

    private sealed class SucceedingUploadTransferClient : IUploadTransferClient
    {
        public int UploadCalls { get; private set; }

        public Task<Result<Unit, string>> UploadAsync(string localPath, string remotePath, string correlationId, CancellationToken cancellationToken = default)
        {
            UploadCalls++;
            return Task.FromResult<Result<Unit, string>>(Unit.Value);
        }

        public Task<Result<Unit, string>> UploadChunkedAsync(string localPath, string remotePath, string correlationId, CancellationToken cancellationToken = default)
        {
            UploadCalls++;
            return Task.FromResult<Result<Unit, string>>(Unit.Value);
        }

        public Task<Result<Unit, string>> DeleteAsync(string remotePath, string correlationId, CancellationToken cancellationToken = default)
        {
            UploadCalls++;
            return Task.FromResult<Result<Unit, string>>(Unit.Value);
        }
    }
}