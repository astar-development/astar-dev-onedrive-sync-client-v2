using System.Collections.ObjectModel;
using System.Windows.Input;
using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.Application.Interfaces;
using AStar.Dev.OneDrive.Sync.Client.Domain.Entities;
using AStar.Dev.OneDrive.Sync.Client.UI.Composition;
using AStar.Dev.OneDrive.Sync.Client.UI.Common;
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
            if (field == value)
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
            if (string.IsNullOrWhiteSpace(value))
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

    public ICommand SyncNowCommand => StartSyncCommand;
    public ICommand PauseCommand => PauseSyncCommand;

    public SyncStatusViewModel(ISyncService? syncService = null)
    {
        _syncService = syncService ?? CompositionRoot.Resolve<ISyncService>();
        StartSyncCommand = new RelayCommand(_ => _ = StartSyncAsync());
        PauseSyncCommand = new RelayCommand(_ => PauseSync());
    }

    private async Task StartSyncAsync(CancellationToken cancellationToken = default)
    {
        Status = SyncingStatus;
        ProgressPercentage = 0;
        SyncError = string.Empty;
        await Task.Yield();

        Result<IReadOnlyList<SyncFile>, string> result = await _syncService.GetSyncFilesAsync(cancellationToken);
        switch (result)
        {
            case Result<IReadOnlyList<SyncFile>, string>.Ok ok:
                ProgressPercentage = 100;
                Status = IdleStatus;
                AddActivity("Info", $"Sync completed: {ok.Value.Count} item(s)");
                return;

            case Result<IReadOnlyList<SyncFile>, string>.Error error:
                ProgressPercentage = 0;
                SyncError = error.Reason;
                return;
        }
    }

    private void PauseSync() => Status = PausedStatus;

    private void AddActivity(string level, string message)
        => RecentActivity.Add(new SyncActivityEntry(DateTime.UtcNow, level, message));
}
