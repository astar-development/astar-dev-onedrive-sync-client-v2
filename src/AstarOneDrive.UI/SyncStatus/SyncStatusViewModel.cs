using System.Collections.ObjectModel;
using System.Windows.Input;
using ReactiveUI;
using AstarOneDrive.UI.Common;

namespace AstarOneDrive.UI.SyncStatus;

public class SyncStatusViewModel : ViewModelBase
{
    private const string IdleStatus = "Idle";
    private const string SyncingStatus = "Syncing...";
    private const string PausedStatus = "Paused";
    private const string ErrorStatus = "Error";

    public string Status
    {
        get;
        set
        {
            if (field == value)
            {
                return;
            }

            this.RaiseAndSetIfChanged(ref field, value);
            AddActivity("Info", value);
        }
    } = IdleStatus;

    public string CurrentStatus
    {
        get => Status;
        set => Status = value;
    }

    public int ProgressPercentage
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

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
            this.RaiseAndSetIfChanged(ref field, value);
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

    private void AddActivity(string level, string message) =>
        RecentActivity.Add(new SyncActivityEntry(DateTime.UtcNow, level, message));
}
