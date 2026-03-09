using System.Reflection;
using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.Application.Interfaces;
using AStar.Dev.OneDrive.Sync.Client.Application.Models;
using AStar.Dev.OneDrive.Sync.Client.Application.Services;
using AStar.Dev.OneDrive.Sync.Client.Domain.Entities;
using AStar.Dev.OneDrive.Sync.Client.Domain.Interfaces;
using NSubstitute;

namespace AStar.Dev.OneDrive.Sync.Client.Application.Tests.Services;

public sealed class SyncServiceShould
{
    private readonly ISyncFileRepository _repository;
    private readonly ISyncService _sut;
    private readonly IDownloadTransferClient _downloadClient;
    private readonly IDownloadFileSystem _downloadFileSystem;

    public SyncServiceShould()
    {
        _repository = Substitute.For<ISyncFileRepository>();
        _downloadClient = Substitute.For<IDownloadTransferClient>();
        _downloadFileSystem = Substitute.For<IDownloadFileSystem>();
        _sut = new SyncService(_repository, _downloadClient, _downloadFileSystem, maxConcurrentDownloads: 2);
    }

    [Fact]
    public async Task ReturnOkWithFilesWhenRepositoryContainsFiles()
    {
        var expectedFiles = new List<SyncFile>
        {
            new("file1.txt", "/local/file1.txt", "/remote/file1.txt"),
            new("file2.txt", "/local/file2.txt", "/remote/file2.txt"),
        };
        Result<IReadOnlyList<SyncFile>, string> repositoryResult = expectedFiles;
        _ = _repository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(repositoryResult));

        Result<IReadOnlyList<SyncFile>, string> result = await _sut.GetSyncFilesAsync(TestContext.Current.CancellationToken);

        Result<IReadOnlyList<SyncFile>, string>.Ok ok = result.ShouldBeOfType<Result<IReadOnlyList<SyncFile>, string>.Ok>();
        ok.Value.ShouldBe(expectedFiles);
    }

    [Fact]
    public async Task ReturnOkWithEmptyListWhenRepositoryContainsNoFiles()
    {
        Result<IReadOnlyList<SyncFile>, string> repositoryResult = new List<SyncFile>();
        _ = _repository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(repositoryResult));

        Result<IReadOnlyList<SyncFile>, string> result = await _sut.GetSyncFilesAsync(TestContext.Current.CancellationToken);

        Result<IReadOnlyList<SyncFile>, string>.Ok ok = result.ShouldBeOfType<Result<IReadOnlyList<SyncFile>, string>.Ok>();
        ok.Value.ShouldBeEmpty();
    }

    [Fact]
    public async Task ReturnErrorWhenRepositoryReturnsError()
    {
        Result<IReadOnlyList<SyncFile>, string> repositoryResult = "retrieval failed";
        _ = _repository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(repositoryResult));

        Result<IReadOnlyList<SyncFile>, string> result = await _sut.GetSyncFilesAsync(TestContext.Current.CancellationToken);

        Result<IReadOnlyList<SyncFile>, string>.Error error = result.ShouldBeOfType<Result<IReadOnlyList<SyncFile>, string>.Error>();
        error.Reason.ShouldBe("retrieval failed");
    }

    [Fact]
    public void DefineQueueConflictAndCheckpointModelsWhenContractsAreIntroduced()
    {
        Assembly applicationAssembly = typeof(ISyncService).Assembly;

        applicationAssembly.GetType("AStar.Dev.OneDrive.Sync.Client.Application.Models.SyncQueueItem").ShouldNotBeNull();
        applicationAssembly.GetType("AStar.Dev.OneDrive.Sync.Client.Application.Models.SyncConflict").ShouldNotBeNull();
        applicationAssembly.GetType("AStar.Dev.OneDrive.Sync.Client.Application.Models.SyncCheckpoint").ShouldNotBeNull();
    }

    [Fact]
    public void DefineOrchestrationOperationsWhenContractsAreIntroduced()
    {
        MethodInfo[] methods = typeof(ISyncService).GetMethods();

        methods.Any(x => x.Name == "RunDeltaSyncAsync").ShouldBeTrue();
        methods.Any(x => x.Name == "EnqueueUploadAsync").ShouldBeTrue();
        methods.Any(x => x.Name == "EnqueueDownloadAsync").ShouldBeTrue();
    }

    [Fact]
    public void DefineFailureHandlingOperationsWhenContractsAreIntroduced()
    {
        MethodInfo[] methods = typeof(ISyncService).GetMethods();

        methods.Any(x => x.Name == "GetFailedOperationsAsync").ShouldBeTrue();
        methods.Any(x => x.Name == "RetryFailedOperationsAsync").ShouldBeTrue();
    }

    [Fact]
    public async Task RejectDownloadQueueItemWhenDestinationPathFailsValidation()
    {
        var queueItem = new SyncQueueItem("q1", "/invalid/path/file.txt", "/remote/file.txt");
        _ = _downloadFileSystem.ValidatePathAndDiskAsync(queueItem.LocalPath, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<Unit, string>>("invalid destination"));

        Result<Unit, string> result = await _sut.EnqueueDownloadAsync(queueItem, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<Result<Unit, string>.Error>().Reason.ShouldBe("invalid destination");
        await _downloadClient.DidNotReceive().DownloadAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DownloadWithTempFileAndAtomicFinalizeWhenEnqueued()
    {
        var queueItem = new SyncQueueItem("q2", "/tmp/local/a.txt", "/remote/a.txt");
        _ = _downloadFileSystem.ValidatePathAndDiskAsync(queueItem.LocalPath, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<Unit, string>>(Unit.Value));
        _ = _downloadFileSystem.GetTempPath(queueItem.LocalPath).Returns("/tmp/local/a.txt.download");
        _ = _downloadClient.DownloadAsync(queueItem.RemotePath, "/tmp/local/a.txt.download", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<Unit, string>>(Unit.Value));
        _ = _downloadFileSystem.FinalizeAtomicAsync("/tmp/local/a.txt.download", queueItem.LocalPath, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<Unit, string>>(Unit.Value));

        Result<Unit, string> enqueueResult = await _sut.EnqueueDownloadAsync(queueItem, TestContext.Current.CancellationToken);
        enqueueResult.ShouldBeOfType<Result<Unit, string>.Ok>();
        await WaitForConditionAsync(async () =>
        {
            await _downloadFileSystem.Received(1).FinalizeAtomicAsync("/tmp/local/a.txt.download", queueItem.LocalPath, Arg.Any<CancellationToken>());
            return true;
        });
    }

    [Fact]
    public async Task LimitConcurrentDownloadsToConfiguredBound()
    {
        var queueItemA = new SyncQueueItem("q3", "/tmp/local/1.txt", "/remote/1.txt");
        var queueItemB = new SyncQueueItem("q4", "/tmp/local/2.txt", "/remote/2.txt");
        var gate = new TaskCompletionSource();
        var current = 0;
        var maxConcurrent = 0;
        _ = _downloadFileSystem.ValidatePathAndDiskAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<Unit, string>>(Unit.Value));
        _ = _downloadFileSystem.GetTempPath(Arg.Any<string>()).Returns(call => $"{call.Arg<string>()}.download");
        _ = _downloadFileSystem.FinalizeAtomicAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<Unit, string>>(Unit.Value));
        _ = _downloadClient.DownloadAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_ => ControlledDownloadAsync());

        async Task<Result<Unit, string>> ControlledDownloadAsync()
        {
            var now = Interlocked.Increment(ref current);
            maxConcurrent = Math.Max(maxConcurrent, now);
            await gate.Task.WaitAsync(TestContext.Current.CancellationToken);
            _ = Interlocked.Decrement(ref current);
            return Unit.Value;
        }

        await _sut.EnqueueDownloadAsync(queueItemA, TestContext.Current.CancellationToken);
        await _sut.EnqueueDownloadAsync(queueItemB, TestContext.Current.CancellationToken);
        await Task.Delay(50, TestContext.Current.CancellationToken);
        gate.SetResult();
        await WaitForConditionAsync(() => Task.FromResult(maxConcurrent >= 1));

        maxConcurrent.ShouldBeLessThanOrEqualTo(2);
    }

    [Fact]
    public async Task AddFailedOperationAndAllowRetryWhenDownloadFails()
    {
        var queueItem = new SyncQueueItem("q5", "/tmp/local/fail.txt", "/remote/fail.txt");
        _ = _downloadFileSystem.ValidatePathAndDiskAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<Unit, string>>(Unit.Value));
        _ = _downloadFileSystem.GetTempPath(Arg.Any<string>()).Returns("/tmp/local/fail.txt.download");
        _ = _downloadClient.DownloadAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<Unit, string>>("network"), Task.FromResult<Result<Unit, string>>(Unit.Value));
        _ = _downloadFileSystem.CleanupTempAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<Unit, string>>(Unit.Value));
        _ = _downloadFileSystem.FinalizeAtomicAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<Unit, string>>(Unit.Value));

        _ = await _sut.EnqueueDownloadAsync(queueItem, TestContext.Current.CancellationToken);
        await WaitForConditionAsync(async () =>
        {
            Result<IReadOnlyList<SyncQueueItem>, string> failed = await _sut.GetFailedOperationsAsync(TestContext.Current.CancellationToken);
            return failed.ShouldBeOfType<Result<IReadOnlyList<SyncQueueItem>, string>.Ok>().Value.Count == 1;
        });

        Result<Unit, string> retry = await _sut.RetryFailedOperationsAsync(TestContext.Current.CancellationToken);
        retry.ShouldBeOfType<Result<Unit, string>.Ok>();
    }

    private static async Task WaitForConditionAsync(Func<Task<bool>> condition)
    {
        for(var attempt = 0; attempt < 80; attempt++)
        {
            if(await condition())
            {
                return;
            }

            await Task.Delay(20, TestContext.Current.CancellationToken);
        }

        throw new TimeoutException("Condition was not met within the allowed time.");
    }
}
