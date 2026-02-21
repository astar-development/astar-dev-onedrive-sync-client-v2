using System.Windows.Input;
using ReactiveUI;
using AstarOneDrive.UI.Common;

namespace AstarOneDrive.UI.SyncStatus;

public class SyncStatusViewModel : ViewModelBase
{
    public string CurrentStatus
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = "Idle";
    public int Progress
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
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

    private void PauseSync() => CurrentStatus = "Paused";
}
