using System.Collections.ObjectModel;
using System.Windows.Input;
using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.Application.Interfaces;
using AStar.Dev.OneDrive.Sync.Client.Application.Models;
using AStar.Dev.OneDrive.Sync.Client.Domain.Entities;
using AStar.Dev.OneDrive.Sync.Client.UI.Common;
using AStar.Dev.OneDrive.Sync.Client.UI.Composition;
using ReactiveUI;

namespace AStar.Dev.OneDrive.Sync.Client.UI.SyncStatus;

/// <summary>
/// ViewModel for displaying and managing synchronization status and activity.
/// </summary>
public class SyncStatusViewModel : ViewModelBase
{
    private const string IdleStatus = "Idle";
    private const string SyncingStatus = "Syncing...";
    private const string PausedStatus = "Paused";
    private const string ErrorStatus = "Error";
    private readonly ISyncService _syncService;

    /// <summary>
    /// Gets or sets the current synchronization status text.
    /// </summary>
    public string Status
    {
        get;
        set
        {
            if(field == value)
            {
                return;
            }

            _ = this.RaiseAndSetIfChanged(ref field, value);
            AddActivity("Info", value);
        }
    } = IdleStatus;

    /// <summary>
    /// Gets or sets the current synchronization status text (alias for Status).
    /// </summary>
    public string CurrentStatus
    {
        get => Status;
        set => Status = value;
    }

    /// <summary>
    /// Gets or sets the synchronization progress percentage (0-100).
    /// </summary>
    public int ProgressPercentage
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>
    /// Gets or sets the synchronization progress (alias for ProgressPercentage).
    /// </summary>
    public int Progress
    {
        get => ProgressPercentage;
        set => ProgressPercentage = value;
    }

    public string SyncError
    {
        get;
        set
        {
            _ = this.RaiseAndSetIfChanged(ref field, value);
            if(string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            Status = ErrorStatus;
            AddActivity("Error", value);
        }
    } = string.Empty;

    public ObservableCollection<SyncActivityEntry> RecentActivity { get; } = [];

    public ICommand StartSyncCommand { get; }
    public ICommand PauseSyncCommand { get; }
    public ICommand ResumeSyncCommand { get; }

    public ICommand SyncNowCommand => StartSyncCommand;
    public ICommand PauseCommand => PauseSyncCommand;

    public SyncStatusViewModel(ISyncService? syncService = null)
    {
        _syncService = syncService ?? CompositionRoot.Resolve<ISyncService>();
        StartSyncCommand = new RelayCommand(_ => _ = StartSyncAsync());
        PauseSyncCommand = new RelayCommand(_ => _ = PauseSyncAsync());
        ResumeSyncCommand = new RelayCommand(_ => _ = ResumeSyncAsync());
    }

    private async Task StartSyncAsync(CancellationToken cancellationToken = default)
    {
        Status = SyncingStatus;
        ProgressPercentage = 0;
        SyncError = string.Empty;
        await Task.Yield();

        _ = await _syncService.GetSyncFilesAsync(cancellationToken)
            .MatchAsync(
                syncedFiles => QueueDownloadsAsync(syncedFiles, cancellationToken),
                failureMessage => Task.FromResult(FailSync(failureMessage)));
    }

    private async Task<Unit> QueueDownloadsAsync(IReadOnlyList<SyncFile> syncedFiles, CancellationToken cancellationToken)
    {
        if(syncedFiles.Count == 0)
        {
            ProgressPercentage = 100;
            Status = IdleStatus;
            AddActivity("Info", "Sync completed: 0 item(s)");
            await LogConflictOutcomesAsync(cancellationToken);
            return Unit.Value;
        }

        for(var index = 0; index < syncedFiles.Count; index++)
        {
            SyncFile file = syncedFiles[index];
            var queueItem = new SyncQueueItem(Guid.NewGuid().ToString("N"), file.LocalPath, file.RemotePath);
            Result<Unit, string> enqueueResult = await _syncService.EnqueueDownloadAsync(queueItem, cancellationToken);
            if(enqueueResult is Result<Unit, string>.Error enqueueError)
            {
                return FailSync(enqueueError.Reason);
            }

            ProgressPercentage = (index + 1) * 100 / syncedFiles.Count;
        }

        Status = IdleStatus;
        AddActivity("Info", $"Sync completed: {syncedFiles.Count} item(s)");
        await LogConflictOutcomesAsync(cancellationToken);
        return Unit.Value;
    }

    private async Task LogConflictOutcomesAsync(CancellationToken cancellationToken)
    {
        Result<IReadOnlyList<SyncConflict>, string> conflictsResult = await _syncService.GetConflictsAsync(cancellationToken);
        if(conflictsResult is not Result<IReadOnlyList<SyncConflict>, string>.Ok conflictsOk)
        {
            return;
        }

        foreach(SyncConflict conflict in conflictsOk.Value)
        {
            AddActivity("Warning", $"Conflict ({conflict.ConflictType}): {conflict.Reason}");
        }
    }

    private Unit FailSync(string error)
    {
        ProgressPercentage = 0;
        SyncError = error;
        return Unit.Value;
    }

    private async Task PauseSyncAsync()
    {
        Result<Unit, string> pauseResult = await _syncService.PauseSyncAsync();
        if(pauseResult is Result<Unit, string>.Ok)
        {
            Status = PausedStatus;
        }
    }

    private async Task ResumeSyncAsync()
    {
        Result<Unit, string> resumeResult = await _syncService.ResumeSyncAsync();
        if(resumeResult is Result<Unit, string>.Ok)
        {
            Status = IdleStatus;
        }
    }

    private void AddActivity(string level, string message)
        => RecentActivity.Add(new SyncActivityEntry(DateTime.UtcNow, level, message));
}
