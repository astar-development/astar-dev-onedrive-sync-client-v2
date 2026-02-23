using System.Collections.ObjectModel;
using System.Windows.Input;
using ReactiveUI;
using AstarOneDrive.UI.Common;

namespace AstarOneDrive.UI.SyncStatus;

public class SyncStatusViewModel : ViewModelBase
{
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
            RecentActivity.Add(new SyncActivityEntry(DateTime.UtcNow, "Info", value));
        }
    } = "Idle";

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

            Status = "Error";
            RecentActivity.Add(new SyncActivityEntry(DateTime.UtcNow, "Error", value));
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
        Status = "Syncing...";
        ProgressPercentage = 0;
    }

    private void PauseSync() => Status = "Paused";
}
