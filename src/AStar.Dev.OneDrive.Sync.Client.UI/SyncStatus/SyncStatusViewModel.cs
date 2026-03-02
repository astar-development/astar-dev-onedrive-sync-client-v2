using System.Collections.ObjectModel;
using System.Windows.Input;
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

    public SyncStatusViewModel()
    {
        StartSyncCommand = new RelayCommand(_ => StartSync());
        PauseSyncCommand = new RelayCommand(_ => PauseSync());
    }

    private void StartSync()
    {
        Status = SyncingStatus;
        ProgressPercentage = 0;
    }

    private void PauseSync() => Status = PausedStatus;

    private void AddActivity(string level, string message)
        => RecentActivity.Add(new SyncActivityEntry(DateTime.UtcNow, level, message));
}
