using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.Application.Interfaces;
using AStar.Dev.OneDrive.Sync.Client.Application.Models;
using AStar.Dev.OneDrive.Sync.Client.Application.Services;
using AStar.Dev.OneDrive.Sync.Client.Domain.Interfaces;
using NSubstitute;

namespace AStar.Dev.OneDrive.Sync.Client.Application.Tests.Services;

public sealed class SyncServiceConflictShould
{
    [Fact]
    public async Task AvoidSilentOverwriteWhenConflictRequiresManualResolution()
    {
        ISyncFileRepository repo = Substitute.For<ISyncFileRepository>();
        IDownloadTransferClient downloadClient = Substitute.For<IDownloadTransferClient>();
        IDownloadFileSystem downloadFileSystem = Substitute.For<IDownloadFileSystem>();
        _ = downloadFileSystem.ValidatePathAndDiskAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<Unit, string>>(Unit.Value));

        var sut = new SyncService(
            repo,
            downloadTransferClient: downloadClient,
            downloadFileSystem: downloadFileSystem,
            defaultConflictResolutionPolicy: SyncConflictResolutionPolicy.Manual);

        var queueItem = new SyncQueueItem(
            "conflict-1",
            "/tmp/local/a.txt",
            "/remote/a.txt",
            SyncOperationType.Update,
            ConflictContext: new SyncConflictContext("a", "b", null, null, false, false, false, false));

        Result<Unit, string> result = await sut.EnqueueDownloadAsync(queueItem, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<Result<Unit, string>.Error>();
        await downloadClient.DidNotReceive().DownloadAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());

        Result<IReadOnlyList<SyncConflict>, string> conflictsResult = await sut.GetConflictsAsync(TestContext.Current.CancellationToken);
        conflictsResult.ShouldBeOfType<Result<IReadOnlyList<SyncConflict>, string>.Ok>().Value.Count.ShouldBe(1);
    }
}
