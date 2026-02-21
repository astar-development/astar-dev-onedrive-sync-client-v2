using System.Windows.Input;
using AstarOneDrive.UI.Common;

namespace AstarOneDrive.UI.SyncStatus;

public class SyncStatusViewModel : ViewModelBase
{
    private string _currentStatus = "Idle";
    public string CurrentStatus
    {
        get => _currentStatus;
        set => this.RaiseAndSetIfChanged(ref _currentStatus, value);
    }

    private int _progress;
    public int Progress
    {
        get => _progress;
        set => this.RaiseAndSetIfChanged(ref _progress, value);
    }

    public ICommand SyncNowCommand { get; }
    public ICommand PauseCommand { get; }

    public SyncStatusViewModel()
    {
        SyncNowCommand = new RelayCommand(_ => StartSync());
        PauseCommand = new RelayCommand(_ => PauseSync());
    }

    private void StartSync()
    {
        CurrentStatus = "Syncing...";
        Progress = 0;

        // TODO: Implement real sync logic
    }

    private void PauseSync()
    {
        CurrentStatus = "Paused";
    }
}
