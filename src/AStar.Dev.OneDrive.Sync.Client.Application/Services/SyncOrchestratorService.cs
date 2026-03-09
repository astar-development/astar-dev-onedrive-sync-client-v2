using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.Application.Interfaces;
using AStar.Dev.OneDrive.Sync.Client.Application.Models;
using AStar.Dev.OneDrive.Sync.Client.Domain.Entities;

namespace AStar.Dev.OneDrive.Sync.Client.Application.Services;

/// <summary>
/// Coordinates scan, delta, upload, and download stages with crash-safe resume and scheduling.
/// </summary>
public sealed class SyncOrchestratorService(
    ILocalInventoryService inventory,
    IDeltaSyncService delta,
    ISyncService sync,
    ISyncRunStateStore stateStore) : ISyncOrchestratorService
{
    private readonly SemaphoreSlim _runLock = new(1, 1);
    private readonly Lock _stateLock = new();
    private CancellationTokenSource _runCts = new();
    private bool _isPaused;
    private PeriodicTimer? _timer;
    private CancellationTokenSource? _timerCts;

    /// <inheritdoc />
    public Task<Result<Unit, string>> PauseAsync(CancellationToken cancellationToken = default)
    {
        _isPaused = true;
        return Task.FromResult<Result<Unit, string>>(Unit.Value);
    }

    /// <inheritdoc />
    public Task<Result<Unit, string>> ResumeAsync(CancellationToken cancellationToken = default)
    {
        _isPaused = false;
        return Task.FromResult<Result<Unit, string>>(Unit.Value);
    }

    /// <inheritdoc />
    public Task<Result<Unit, string>> CancelAsync(CancellationToken cancellationToken = default)
    {
        lock(_stateLock)
        {
            _runCts.Cancel();
            _runCts.Dispose();
            _runCts = new CancellationTokenSource();
            _isPaused = false;
        }

        return Task.FromResult<Result<Unit, string>>(Unit.Value);
    }

    /// <inheritdoc />
    public async Task<Result<Unit, string>> RunOnceAsync(string accountId, string scopeId, string rootPath, bool useStartupScan, CancellationToken cancellationToken = default)
    {
        await _runLock.WaitAsync(cancellationToken);
        try
        {
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _runCts.Token);
            Result<Option<SyncRunState>, string> loadResult = await stateStore.LoadAsync(accountId, scopeId, linked.Token);
            if(loadResult is Result<Option<SyncRunState>, string>.Error loadError)
            {
                return loadError.Reason;
            }

            SyncRunState state = ((Result<Option<SyncRunState>, string>.Ok)loadResult).Value is Option<SyncRunState>.Some some
                ? some.Value
                : NewState(accountId, scopeId, rootPath, useStartupScan, SyncRunStage.Scan, [], []);
            return await ExecuteAsync(state, linked.Token);
        }
        finally
        {
            _ = _runLock.Release();
        }
    }

    /// <inheritdoc />
    public Task<Result<Unit, string>> StartBackgroundSchedulingAsync(string accountId, string scopeId, string rootPath, TimeSpan interval, CancellationToken cancellationToken = default)
    {
        _ = StopBackgroundSchedulingAsync(cancellationToken);
        _timerCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _timer = new PeriodicTimer(interval);
        _ = Task.Run(() => SchedulerLoopAsync(accountId, scopeId, rootPath, _timer, _timerCts.Token), CancellationToken.None);
        _ = Task.Run(() => RunOnceAsync(accountId, scopeId, rootPath, useStartupScan: true, _timerCts.Token), CancellationToken.None);
        return Task.FromResult<Result<Unit, string>>(Unit.Value);
    }

    /// <inheritdoc />
    public Task<Result<Unit, string>> StopBackgroundSchedulingAsync(CancellationToken cancellationToken = default)
    {
        _timerCts?.Cancel();
        _timer?.Dispose();
        _timerCts?.Dispose();
        _timer = null;
        _timerCts = null;
        return Task.FromResult<Result<Unit, string>>(Unit.Value);
    }

    private async Task SchedulerLoopAsync(string accountId, string scopeId, string rootPath, PeriodicTimer timer, CancellationToken cancellationToken)
    {
        try
        {
            while(await timer.WaitForNextTickAsync(cancellationToken))
            {
                _ = await RunOnceAsync(accountId, scopeId, rootPath, useStartupScan: false, cancellationToken);
            }
        }
        catch(OperationCanceledException)
        {
        }
    }

    private async Task<Result<Unit, string>> ExecuteAsync(SyncRunState state, CancellationToken cancellationToken)
    {
        while(state.Stage != SyncRunStage.Completed)
        {
            await WaitIfPausedAsync(cancellationToken);
            Result<SyncRunState, string> stageResult = state.Stage switch
            {
                SyncRunStage.Scan => await RunScanAsync(state, cancellationToken),
                SyncRunStage.Delta => await RunDeltaAsync(state, cancellationToken),
                SyncRunStage.Upload => await RunUploadAsync(state, cancellationToken),
                SyncRunStage.Download => await RunDownloadAsync(state, cancellationToken),
                _ => state with { Stage = SyncRunStage.Completed }
            };

            if(stageResult is Result<SyncRunState, string>.Error stageError)
            {
                return stageError.Reason;
            }

            state = ((Result<SyncRunState, string>.Ok)stageResult).Value;
        }

        Result<Unit, string> clearResult = await stateStore.ClearAsync(state.AccountId, state.ScopeId, cancellationToken);
        return clearResult is Result<Unit, string>.Error clearError ? clearError.Reason : Unit.Value;
    }

    private async Task<Result<SyncRunState, string>> RunScanAsync(SyncRunState state, CancellationToken cancellationToken)
    {
        Result<IReadOnlyList<LocalInventoryItem>, string> scanResult = state.UseStartupScan
            ? await inventory.RunStartupScanAsync(state.AccountId, state.RootPath, cancellationToken)
            : await inventory.RunManualScanAsync(state.AccountId, state.RootPath, cancellationToken);
        if(scanResult is Result<IReadOnlyList<LocalInventoryItem>, string>.Error scanError)
        {
            return scanError.Reason;
        }

        SyncRunState next = state with { Stage = SyncRunStage.Delta, UpdatedUtc = DateTime.UtcNow };
        return await PersistStateAsync(next, cancellationToken);
    }

    private async Task<Result<SyncRunState, string>> RunDeltaAsync(SyncRunState state, CancellationToken cancellationToken)
    {
        Result<DeltaPullSummary, string> deltaResult = await delta.PullAsync(state.AccountId, state.ScopeId, cancellationToken);
        if(deltaResult is Result<DeltaPullSummary, string>.Error deltaError)
        {
            return deltaError.Reason;
        }

        SyncRunState next = state with { Stage = SyncRunStage.Upload, UpdatedUtc = DateTime.UtcNow };
        return await PersistStateAsync(next, cancellationToken);
    }

    private async Task<Result<SyncRunState, string>> RunUploadAsync(SyncRunState state, CancellationToken cancellationToken)
    {
        foreach(SyncQueueItem item in state.PendingUploads)
        {
            Result<Unit, string> enqueueResult = await sync.EnqueueUploadAsync(item, cancellationToken);
            if(enqueueResult is Result<Unit, string>.Error enqueueError)
            {
                return enqueueError.Reason;
            }
        }

        Result<Unit, string> retryResult = await sync.RetryFailedOperationsAsync(cancellationToken);
        if(retryResult is Result<Unit, string>.Error retryError)
        {
            return retryError.Reason;
        }

        SyncRunState next = state with { Stage = SyncRunStage.Download, PendingUploads = [], UpdatedUtc = DateTime.UtcNow };
        return await PersistStateAsync(next, cancellationToken);
    }

    private async Task<Result<SyncRunState, string>> RunDownloadAsync(SyncRunState state, CancellationToken cancellationToken)
    {
        List<SyncQueueItem> pending = [.. state.PendingDownloads];
        if(pending.Count == 0)
        {
            Result<IReadOnlyList<SyncFile>, string> filesResult = await sync.GetSyncFilesAsync(cancellationToken);
            if(filesResult is Result<IReadOnlyList<SyncFile>, string>.Error filesError)
            {
                return filesError.Reason;
            }

            IReadOnlyList<SyncFile> files = ((Result<IReadOnlyList<SyncFile>, string>.Ok)filesResult).Value;
            pending.AddRange(files.Select(x => new SyncQueueItem(Guid.NewGuid().ToString("N"), x.LocalPath, x.RemotePath, SyncOperationType.Update)));
        }

        SyncRunState withQueue = state with { PendingDownloads = pending, UpdatedUtc = DateTime.UtcNow };
        Result<SyncRunState, string> saveQueueResult = await PersistStateAsync(withQueue, cancellationToken);
        if(saveQueueResult is Result<SyncRunState, string>.Error saveQueueError)
        {
            return saveQueueError.Reason;
        }

        for(var index = 0; index < pending.Count; index++)
        {
            Result<Unit, string> enqueueResult = await sync.EnqueueDownloadAsync(pending[index], cancellationToken);
            if(enqueueResult is Result<Unit, string>.Error enqueueError)
            {
                return enqueueError.Reason;
            }

            IReadOnlyList<SyncQueueItem> remaining = pending.Skip(index + 1).ToList();
            SyncRunState progressed = withQueue with { PendingDownloads = remaining, UpdatedUtc = DateTime.UtcNow };
            Result<SyncRunState, string> saveProgressResult = await PersistStateAsync(progressed, cancellationToken);
            if(saveProgressResult is Result<SyncRunState, string>.Error saveProgressError)
            {
                return saveProgressError.Reason;
            }

            withQueue = progressed;
        }

        SyncRunState next = withQueue with { Stage = SyncRunStage.Completed, PendingDownloads = [], UpdatedUtc = DateTime.UtcNow };
        return await PersistStateAsync(next, cancellationToken);
    }

    private async Task<Result<SyncRunState, string>> PersistStateAsync(SyncRunState state, CancellationToken cancellationToken)
    {
        Result<Unit, string> saveResult = await stateStore.SaveAsync(state, cancellationToken);
        return saveResult is Result<Unit, string>.Error saveError ? saveError.Reason : state;
    }

    private async Task WaitIfPausedAsync(CancellationToken cancellationToken)
    {
        while(_isPaused)
        {
            await Task.Delay(20, cancellationToken);
        }
    }

    private static SyncRunState NewState(
        string accountId,
        string scopeId,
        string rootPath,
        bool useStartupScan,
        SyncRunStage stage,
        IReadOnlyList<SyncQueueItem> pendingUploads,
        IReadOnlyList<SyncQueueItem> pendingDownloads)
        => new(accountId, scopeId, rootPath, useStartupScan, stage, pendingUploads, pendingDownloads, DateTime.UtcNow);
}
