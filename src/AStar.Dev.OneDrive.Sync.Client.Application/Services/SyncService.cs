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
    private readonly ConcurrentQueue<SyncQueueItem> _downloadQueue = [];
    private readonly SemaphoreSlim _downloadSignal = new(0);
    private readonly SemaphoreSlim _downloadParallelism;
    private readonly Lock _workerSync = new();
    private readonly Lock _failedSync = new();
    private CancellationTokenSource _workerCts = new();
    private Task? _workerTask;
    private readonly List<SyncQueueItem> _failedOperations = [];
    private bool _isPaused;

    public SyncService(
        ISyncFileRepository syncFileRepository,
        IDownloadTransferClient? downloadTransferClient = null,
        IDownloadFileSystem? downloadFileSystem = null,
        int maxConcurrentDownloads = 2)
    {
        _syncFileRepository = syncFileRepository;
        _downloadTransferClient = downloadTransferClient ?? new NullDownloadTransferClient();
        _downloadFileSystem = downloadFileSystem ?? new NullDownloadFileSystem();
        _downloadParallelism = new SemaphoreSlim(Math.Max(1, maxConcurrentDownloads));
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
        while(_downloadQueue.TryDequeue(out SyncQueueItem? queued))
        {
            AddFailed(queued);
        }

        _workerCts = new CancellationTokenSource();
        lock(_workerSync)
        {
            _workerTask = null;
        }

        return Task.FromResult<Result<Unit, string>>(Unit.Value);
    }

    ///  <inheritdoc/>
    public Task<Result<Unit, string>> RunDeltaSyncAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<Result<Unit, string>>(Unit.Value);

    ///  <inheritdoc/>
    public Task<Result<Unit, string>> EnqueueUploadAsync(SyncQueueItem queueItem, CancellationToken cancellationToken = default)
        => Task.FromResult<Result<Unit, string>>(Unit.Value);

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
        EnsureWorkerStarted();
        return Unit.Value;
    }

    ///  <inheritdoc/>
    public Task<Result<IReadOnlyList<SyncQueueItem>, string>> GetFailedOperationsAsync(CancellationToken cancellationToken = default)
    {
        lock(_failedSync)
        {
            Result<IReadOnlyList<SyncQueueItem>, string> result = _failedOperations.ToList();
            return Task.FromResult(result);
        }
    }

    ///  <inheritdoc/>
    public async Task<Result<Unit, string>> RetryFailedOperationsAsync(CancellationToken cancellationToken = default)
    {
        var retryItems = new List<SyncQueueItem>();
        lock(_failedSync)
        {
            retryItems.AddRange(_failedOperations);
            _failedOperations.Clear();
        }

        foreach(SyncQueueItem item in retryItems)
        {
            Result<Unit, string> enqueueResult = await EnqueueDownloadAsync(item, cancellationToken);
            if(enqueueResult is Result<Unit, string>.Error)
            {
                AddFailed(item);
            }
        }

        return Unit.Value;
    }

    private void EnsureWorkerStarted()
    {
        lock(_workerSync)
        {
            if(_workerTask is { IsCompleted: false })
            {
                return;
            }

            _workerTask = Task.Run(() => WorkerLoopAsync(_workerCts.Token));
        }
    }

    private async Task WorkerLoopAsync(CancellationToken cancellationToken)
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
                AddFailed(item);
                return;
            }

            Result<Unit, string> finalizeResult = await _downloadFileSystem.FinalizeAtomicAsync(tempPath, item.LocalPath, cancellationToken);
            if(finalizeResult is Result<Unit, string>.Error)
            {
                _ = await _downloadFileSystem.CleanupTempAsync(tempPath, cancellationToken);
                AddFailed(item);
            }
        }
        catch(OperationCanceledException)
        {
            _ = await _downloadFileSystem.CleanupTempAsync(tempPath, CancellationToken.None);
            AddFailed(item);
        }
    }

    private void AddFailed(SyncQueueItem item)
    {
        lock(_failedSync)
        {
            _failedOperations.Add(item);
        }
    }

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
        }
