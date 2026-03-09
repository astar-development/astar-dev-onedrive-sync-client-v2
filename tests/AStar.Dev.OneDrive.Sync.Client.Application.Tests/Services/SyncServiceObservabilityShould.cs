using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.Application.Interfaces;
using AStar.Dev.OneDrive.Sync.Client.Application.Models;
using AStar.Dev.OneDrive.Sync.Client.Application.Services;
using AStar.Dev.OneDrive.Sync.Client.Domain.Interfaces;
using NSubstitute;

namespace AStar.Dev.OneDrive.Sync.Client.Application.Tests.Services;

public sealed class SyncServiceObservabilityShould
{
    [Fact]
    public async Task EmitLifecycleMetricsAndCorrelationForSuccessfulUpload()
    {
        ISyncFileRepository repo = Substitute.For<ISyncFileRepository>();
        IUploadTransferClient upload = Substitute.For<IUploadTransferClient>();
        IUploadFileSystem fileSystem = Substitute.For<IUploadFileSystem>();
        var diagnostics = new InMemorySyncDiagnosticsSink();
        _ = fileSystem.ValidateUploadPathAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<Unit, string>>(Unit.Value));
        _ = fileSystem.GetFileSizeBytesAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<long, string>>(10L));
        _ = upload.UploadAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<Unit, string>>(Unit.Value));

        var sut = new SyncService(repo, uploadTransferClient: upload, uploadFileSystem: fileSystem, diagnosticsSink: diagnostics, chunkedUploadThresholdBytes: 100);
        var item = new SyncQueueItem("u-1", "/tmp/local/a.txt", "/remote/a.txt", SyncOperationType.Update, "corr-u-1");

        _ = await sut.EnqueueUploadAsync(item, TestContext.Current.CancellationToken);

        await WaitForConditionAsync(() => diagnostics.Events.Any(e => e.EventName == "upload.completed"));
        diagnostics.Events.Any(e => e.EventName == "upload.enqueued" && e.CorrelationId == "corr-u-1").ShouldBeTrue();
        diagnostics.Events.Any(e => e.EventName == "upload.completed" && e.CorrelationId == "corr-u-1").ShouldBeTrue();
        diagnostics.Metrics.Any(m => m.Name == "queue.depth.upload").ShouldBeTrue();
        diagnostics.Metrics.Any(m => m.Name == "throughput.upload").ShouldBeTrue();
        diagnostics.Metrics.Any(m => m.Name == "duration.upload.ms").ShouldBeTrue();
    }

    [Fact]
    public async Task EmitFailureLifecycleAndRetryMetricsWhenDownloadFailsAndIsRetried()
    {
        ISyncFileRepository repo = Substitute.For<ISyncFileRepository>();
        IDownloadTransferClient transfer = Substitute.For<IDownloadTransferClient>();
        IDownloadFileSystem fileSystem = Substitute.For<IDownloadFileSystem>();
        var diagnostics = new InMemorySyncDiagnosticsSink();

        _ = fileSystem.ValidatePathAndDiskAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<Unit, string>>(Unit.Value));
        _ = fileSystem.GetTempPath(Arg.Any<string>()).Returns("/tmp/local/a.txt.download");
        _ = fileSystem.CleanupTempAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<Unit, string>>(Unit.Value));
        _ = transfer.DownloadAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<Unit, string>>("network"), Task.FromResult<Result<Unit, string>>(Unit.Value));
        _ = fileSystem.FinalizeAtomicAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<Unit, string>>(Unit.Value));

        var sut = new SyncService(repo, downloadTransferClient: transfer, downloadFileSystem: fileSystem, diagnosticsSink: diagnostics);
        var item = new SyncQueueItem("d-1", "/tmp/local/a.txt", "/remote/a.txt", SyncOperationType.Update, "corr-d-1");

        _ = await sut.EnqueueDownloadAsync(item, TestContext.Current.CancellationToken);
        await WaitForConditionAsync(() => diagnostics.Events.Any(e => e.EventName == "download.failed"));
        _ = await sut.RetryFailedOperationsAsync(TestContext.Current.CancellationToken);
        await WaitForConditionAsync(() => diagnostics.Events.Count(e => e.EventName == "download.completed") == 1);

        diagnostics.Events.Any(e => e.EventName == "download.failed" && e.CorrelationId == "corr-d-1").ShouldBeTrue();
        diagnostics.Metrics.Any(m => m.Name == "retry.count").ShouldBeTrue();
    }

    private static async Task WaitForConditionAsync(Func<bool> condition)
    {
        for(var attempt = 0; attempt < 120; attempt++)
        {
            if(condition())
            {
                return;
            }

            await Task.Delay(20, TestContext.Current.CancellationToken);
        }

        throw new TimeoutException("Condition was not met within the allowed time.");
    }

    private sealed class InMemorySyncDiagnosticsSink : ISyncDiagnosticsSink
    {
        public List<SyncDiagnosticEvent> Events { get; } = [];
        public List<SyncMetricPoint> Metrics { get; } = [];

        public Task<Result<Unit, string>> RecordEventAsync(SyncDiagnosticEvent diagnosticEvent, CancellationToken cancellationToken = default)
        {
            Events.Add(diagnosticEvent);
            return Task.FromResult<Result<Unit, string>>(Unit.Value);
        }

        public Task<Result<Unit, string>> RecordMetricAsync(SyncMetricPoint metricPoint, CancellationToken cancellationToken = default)
        {
            Metrics.Add(metricPoint);
            return Task.FromResult<Result<Unit, string>>(Unit.Value);
        }
    }
}
