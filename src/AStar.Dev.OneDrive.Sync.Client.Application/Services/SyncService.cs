using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.Application.Interfaces;
using AStar.Dev.OneDrive.Sync.Client.Application.Models;
using AStar.Dev.OneDrive.Sync.Client.Domain.Entities;
using AStar.Dev.OneDrive.Sync.Client.Domain.Interfaces;
using System.Collections.Concurrent;

namespace AStar.Dev.OneDrive.Sync.Client.Application.Services;

/// <summary>
/// Orchestrates file synchronisation operations.
/// </summary>
public sealed class SyncService : ISyncService
{
    private readonly ISyncFileRepository _syncFileRepository;
    private readonly IDownloadTransferClient _downloadTransferClient;
    private readonly IDownloadFileSystem _downloadFileSystem;
    private readonly IUploadTransferClient _uploadTransferClient;
    private readonly IUploadFileSystem _uploadFileSystem;
    private readonly ConcurrentQueue<SyncQueueItem> _downloadQueue = [];
    private readonly ConcurrentQueue<SyncQueueItem> _uploadQueue = [];
    private readonly SemaphoreSlim _downloadSignal = new(0);
    private readonly SemaphoreSlim _uploadSignal = new(0);
    private readonly SemaphoreSlim _downloadParallelism;
    private readonly SemaphoreSlim _uploadParallelism;
    private readonly Lock _workerSync = new();
    private readonly Lock _failedSync = new();
    private readonly List<FailedOperation> _failedOperations = [];
    private CancellationTokenSource _workerCts = new();
    private Task? _downloadWorkerTask;
    private Task? _uploadWorkerTask;
    private bool _isPaused;
    private readonly long _chunkedUploadThresholdBytes;

    public SyncService(
        ISyncFileRepository syncFileRepository,
        IDownloadTransferClient? downloadTransferClient = null,
        IDownloadFileSystem? downloadFileSystem = null,
        IUploadTransferClient? uploadTransferClient = null,
        IUploadFileSystem? uploadFileSystem = null,
        int maxConcurrentDownloads = 2,
        int maxConcurrentUploads = 2,
        long chunkedUploadThresholdBytes = 8L * 1024L * 1024L)
    {
        _syncFileRepository = syncFileRepository;
        _downloadTransferClient = downloadTransferClient ?? new NullDownloadTransferClient();
        _downloadFileSystem = downloadFileSystem ?? new NullDownloadFileSystem();
        _uploadTransferClient = uploadTransferClient ?? new NullUploadTransferClient();
        _uploadFileSystem = uploadFileSystem ?? new NullUploadFileSystem();
        _downloadParallelism = new SemaphoreSlim(Math.Max(1, maxConcurrentDownloads));
        _uploadParallelism = new SemaphoreSlim(Math.Max(1, maxConcurrentUploads));
        _chunkedUploadThresholdBytes = Math.Max(1, chunkedUploadThresholdBytes);
    }

    ///  <inheritdoc/>
    public Task<Result<IReadOnlyList<SyncFile>, string>> GetSyncFilesAsync(CancellationToken cancellationToken = default)
        => _syncFileRepository.GetAllAsync(cancellationToken);

    ///  <inheritdoc/>
    public Task<Result<Unit, string>> PauseSyncAsync(CancellationToken cancellationToken = default)
    {
        _isPaused = true;
        return Task.FromResult<Result<Unit, string>>(Unit.Value);
    }

    ///  <inheritdoc/>
    public Task<Result<Unit, string>> ResumeSyncAsync(CancellationToken cancellationToken = default)
    {
        _isPaused = false;
        return Task.FromResult<Result<Unit, string>>(Unit.Value);
    }

    ///  <inheritdoc/>
    public Task<Result<Unit, string>> CancelSyncAsync(CancellationToken cancellationToken = default)
    {
        _workerCts.Cancel();
        _isPaused = false;

        while(_downloadQueue.TryDequeue(out SyncQueueItem? downloadItem))
        {
            AddFailed(downloadItem, isUpload: false);
        }

        while(_uploadQueue.TryDequeue(out SyncQueueItem? uploadItem))
        {
            AddFailed(uploadItem, isUpload: true);
        }

        _workerCts = new CancellationTokenSource();
        lock(_workerSync)
        {
            _downloadWorkerTask = null;
            _uploadWorkerTask = null;
        }

        return Task.FromResult<Result<Unit, string>>(Unit.Value);
    }

    ///  <inheritdoc/>
    public Task<Result<Unit, string>> RunDeltaSyncAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<Result<Unit, string>>(Unit.Value);

    ///  <inheritdoc/>
    public async Task<Result<Unit, string>> EnqueueUploadAsync(SyncQueueItem queueItem, CancellationToken cancellationToken = default)
    {
        if(queueItem.OperationType is SyncOperationType.Create or SyncOperationType.Update)
        {
            Result<Unit, string> validation = await _uploadFileSystem.ValidateUploadPathAsync(queueItem.LocalPath, cancellationToken);
            if(validation is Result<Unit, string>.Error validationError)
            {
                return validationError.Reason;
            }
        }

        _uploadQueue.Enqueue(NormalizeCorrelationId(queueItem));
        _ = _uploadSignal.Release();
        EnsureUploadWorkerStarted();
        return Unit.Value;
    }

    ///  <inheritdoc/>
    public async Task<Result<Unit, string>> EnqueueDownloadAsync(SyncQueueItem queueItem, CancellationToken cancellationToken = default)
    {
        Result<Unit, string> validationResult = await _downloadFileSystem.ValidatePathAndDiskAsync(queueItem.LocalPath, cancellationToken);
        if(validationResult is Result<Unit, string>.Error validationError)
        {
            return validationError.Reason;
        }

        _downloadQueue.Enqueue(queueItem);
        _ = _downloadSignal.Release();
        EnsureDownloadWorkerStarted();
        return Unit.Value;
    }

    ///  <inheritdoc/>
    public Task<Result<IReadOnlyList<SyncQueueItem>, string>> GetFailedOperationsAsync(CancellationToken cancellationToken = default)
    {
        lock(_failedSync)
        {
            Result<IReadOnlyList<SyncQueueItem>, string> result = _failedOperations.Select(x => x.Item).ToList();
            return Task.FromResult(result);
        }
    }

    ///  <inheritdoc/>
    public async Task<Result<Unit, string>> RetryFailedOperationsAsync(CancellationToken cancellationToken = default)
    {
        List<FailedOperation> retryItems = [];
        lock(_failedSync)
        {
            retryItems.AddRange(_failedOperations);
            _failedOperations.Clear();
        }

        foreach(FailedOperation retry in retryItems)
        {
            Result<Unit, string> result = retry.IsUpload
                ? await EnqueueUploadAsync(retry.Item, cancellationToken)
                : await EnqueueDownloadAsync(retry.Item, cancellationToken);
            if(result is Result<Unit, string>.Error)
            {
                AddFailed(retry.Item, retry.IsUpload);
            }
        }

        return Unit.Value;
    }

    private void EnsureDownloadWorkerStarted()
    {
        lock(_workerSync)
        {
            if(_downloadWorkerTask is { IsCompleted: false })
            {
                return;
            }

            _downloadWorkerTask = Task.Run(() => DownloadWorkerLoopAsync(_workerCts.Token));
        }
    }

    private void EnsureUploadWorkerStarted()
    {
        lock(_workerSync)
        {
            if(_uploadWorkerTask is { IsCompleted: false })
            {
                return;
            }

            _uploadWorkerTask = Task.Run(() => UploadWorkerLoopAsync(_workerCts.Token));
        }
    }

    private async Task DownloadWorkerLoopAsync(CancellationToken cancellationToken)
    {
        while(!cancellationToken.IsCancellationRequested)
        {
            await _downloadSignal.WaitAsync(cancellationToken);
            if(!_downloadQueue.TryDequeue(out SyncQueueItem? item))
            {
                continue;
            }

            await _downloadParallelism.WaitAsync(cancellationToken);
            _ = Task.Run(async () =>
            {
                try
                {
                    await ProcessDownloadAsync(item, cancellationToken);
                }
                finally
                {
                    _ = _downloadParallelism.Release();
                }
            }, cancellationToken);
        }
    }

    private async Task UploadWorkerLoopAsync(CancellationToken cancellationToken)
    {
        while(!cancellationToken.IsCancellationRequested)
        {
            await _uploadSignal.WaitAsync(cancellationToken);
            if(!_uploadQueue.TryDequeue(out SyncQueueItem? item))
            {
                continue;
            }

            await _uploadParallelism.WaitAsync(cancellationToken);
            _ = Task.Run(async () =>
            {
                try
                {
                    await ProcessUploadAsync(item, cancellationToken);
                }
                finally
                {
                    _ = _uploadParallelism.Release();
                }
            }, cancellationToken);
        }
    }

    private async Task ProcessDownloadAsync(SyncQueueItem item, CancellationToken cancellationToken)
    {
        var tempPath = _downloadFileSystem.GetTempPath(item.LocalPath);
        try
        {
            while(_isPaused && !cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(25, cancellationToken);
            }

            Result<Unit, string> downloadResult = await _downloadTransferClient.DownloadAsync(item.RemotePath, tempPath, cancellationToken);
            if(downloadResult is Result<Unit, string>.Error)
            {
                _ = await _downloadFileSystem.CleanupTempAsync(tempPath, cancellationToken);
                AddFailed(item, isUpload: false);
                return;
            }

            Result<Unit, string> finalizeResult = await _downloadFileSystem.FinalizeAtomicAsync(tempPath, item.LocalPath, cancellationToken);
            if(finalizeResult is Result<Unit, string>.Error)
            {
                _ = await _downloadFileSystem.CleanupTempAsync(tempPath, cancellationToken);
                AddFailed(item, isUpload: false);
            }
        }
        catch(OperationCanceledException)
        {
            _ = await _downloadFileSystem.CleanupTempAsync(tempPath, CancellationToken.None);
            AddFailed(item, isUpload: false);
        }
    }

    private async Task ProcessUploadAsync(SyncQueueItem item, CancellationToken cancellationToken)
    {
        try
        {
            while(_isPaused && !cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(25, cancellationToken);
            }

            Result<Unit, string> transferResult = item.OperationType switch
            {
                SyncOperationType.Delete => await _uploadTransferClient.DeleteAsync(item.RemotePath, item.CorrelationId!, cancellationToken),
                _ => await UploadCreateOrUpdateAsync(item, cancellationToken)
            };

            if(transferResult is Result<Unit, string>.Error)
            {
                AddFailed(item, isUpload: true);
            }
        }
        catch(OperationCanceledException)
        {
            AddFailed(item, isUpload: true);
        }
    }

    private async Task<Result<Unit, string>> UploadCreateOrUpdateAsync(SyncQueueItem item, CancellationToken cancellationToken)
    {
        Result<long, string> sizeResult = await _uploadFileSystem.GetFileSizeBytesAsync(item.LocalPath, cancellationToken);
        if(sizeResult is Result<long, string>.Error sizeError)
        {
            return sizeError.Reason;
        }

        var sizeBytes = ((Result<long, string>.Ok)sizeResult).Value;
        return sizeBytes > _chunkedUploadThresholdBytes
            ? await _uploadTransferClient.UploadChunkedAsync(item.LocalPath, item.RemotePath, item.CorrelationId!, cancellationToken)
            : await _uploadTransferClient.UploadAsync(item.LocalPath, item.RemotePath, item.CorrelationId!, cancellationToken);
    }

    private SyncQueueItem NormalizeCorrelationId(SyncQueueItem item)
    {
        if(!string.IsNullOrWhiteSpace(item.CorrelationId))
        {
            return item;
        }

        var fallback = string.IsNullOrWhiteSpace(item.Id) ? Guid.NewGuid().ToString("N") : item.Id;
        return item with { CorrelationId = fallback };
    }

    private void AddFailed(SyncQueueItem item, bool isUpload)
    {
        lock(_failedSync)
        {
            if(_failedOperations.Any(x => x.IsUpload == isUpload && string.Equals(x.Item.Id, item.Id, StringComparison.Ordinal)))
            {
                return;
            }

            _failedOperations.Add(new FailedOperation(item, isUpload));
        }
    }

    private sealed record FailedOperation(SyncQueueItem Item, bool IsUpload);

    private sealed class NullDownloadTransferClient : IDownloadTransferClient
    {
        public Task<Result<Unit, string>> DownloadAsync(string remotePath, string tempPath, CancellationToken cancellationToken = default)
            => Try.RunAsync(async () =>
            {
                var directory = Path.GetDirectoryName(tempPath);
                if(!string.IsNullOrWhiteSpace(directory))
                {
                    _ = Directory.CreateDirectory(directory);
                }

                await File.WriteAllTextAsync(tempPath, string.Empty, cancellationToken);
                return Unit.Value;
            }).MapFailureAsync(error => error.Message);
    }

    private sealed class NullDownloadFileSystem : IDownloadFileSystem
    {
        public Task<Result<Unit, string>> ValidatePathAndDiskAsync(string localPath, CancellationToken cancellationToken = default)
            => string.IsNullOrWhiteSpace(localPath)
                ? Task.FromResult<Result<Unit, string>>("invalid destination")
                : Task.FromResult<Result<Unit, string>>(Unit.Value);

        public string GetTempPath(string localPath)
            => $"{localPath}.download";

        public Task<Result<Unit, string>> FinalizeAtomicAsync(string tempPath, string localPath, CancellationToken cancellationToken = default)
            => Try.RunAsync(() =>
            {
                var directory = Path.GetDirectoryName(localPath);
                if(!string.IsNullOrWhiteSpace(directory))
                {
                    _ = Directory.CreateDirectory(directory);
                }

                if(File.Exists(localPath))
                {
                    File.Delete(localPath);
                }

                File.Move(tempPath, localPath, overwrite: false);
                return Task.FromResult(Unit.Value);
            }).MapFailureAsync(error => error.Message);

        public Task<Result<Unit, string>> CleanupTempAsync(string tempPath, CancellationToken cancellationToken = default)
            => Try.RunAsync(() =>
            {
                if(File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }

                return Task.FromResult(Unit.Value);
            }).MapFailureAsync(error => error.Message);
    }

    private sealed class NullUploadTransferClient : IUploadTransferClient
    {
        public Task<Result<Unit, string>> UploadAsync(string localPath, string remotePath, string correlationId, CancellationToken cancellationToken = default)
            => Task.FromResult<Result<Unit, string>>(Unit.Value);

        public Task<Result<Unit, string>> UploadChunkedAsync(string localPath, string remotePath, string correlationId, CancellationToken cancellationToken = default)
            => Task.FromResult<Result<Unit, string>>(Unit.Value);

        public Task<Result<Unit, string>> DeleteAsync(string remotePath, string correlationId, CancellationToken cancellationToken = default)
            => Task.FromResult<Result<Unit, string>>(Unit.Value);
    }

    private sealed class NullUploadFileSystem : IUploadFileSystem
    {
        public Task<Result<Unit, string>> ValidateUploadPathAsync(string localPath, CancellationToken cancellationToken = default)
            => string.IsNullOrWhiteSpace(localPath)
                ? Task.FromResult<Result<Unit, string>>("invalid upload source")
                : Task.FromResult<Result<Unit, string>>(Unit.Value);

        public Task<Result<long, string>> GetFileSizeBytesAsync(string localPath, CancellationToken cancellationToken = default)
            => Try.RunAsync(() => Task.FromResult(new FileInfo(localPath).Length)).MapFailureAsync(error => error.Message);
    }
}