using AStar.Dev.OneDrive.Sync.Client.UI.SyncStatus;

namespace AStar.Dev.OneDrive.Sync.Client.UI.Tests.ViewModels.SyncStatus;

public sealed class SyncStatusViewModelShould
{
    [Fact]
    public void InitializeStatusToIdle()
    {
        var viewModel = new SyncStatusViewModel();

        viewModel.Status.ShouldBe("Idle");
    }

    [Fact]
    public void SetStatusToSyncingWhenStartSyncCommandIsExecuted()
    {
        var viewModel = new SyncStatusViewModel();

        viewModel.StartSyncCommand.Execute(null);

        viewModel.Status.ShouldBe("Syncing...");
    }

    [Fact]
    public void SetStatusToPausedWhenPauseSyncCommandIsExecuted()
    {
        var viewModel = new SyncStatusViewModel();

        viewModel.PauseSyncCommand.Execute(null);

        viewModel.Status.ShouldBe("Paused");
    }

    [Fact]
    public void UpdateProgressPercentageWhenSet()
    {
        var viewModel = new SyncStatusViewModel
        {
            ProgressPercentage = 42
        };

        viewModel.ProgressPercentage.ShouldBe(42);
    }

    [Fact]
    public void LogRecentActivityOnStateChanges()
    {
        var viewModel = new SyncStatusViewModel();

        viewModel.StartSyncCommand.Execute(null);

        viewModel.RecentActivity.ShouldNotBeEmpty();
    }

    [Fact]
    public void StoreSyncErrorMessageWhenSet()
    {
        var viewModel = new SyncStatusViewModel
        {
            SyncError = "Network unavailable"
        };

        viewModel.SyncError.ShouldBe("Network unavailable");
    }

    [Fact]
    public void FirePropertyChangedEventOnStateChanges()
    {
        var viewModel = new SyncStatusViewModel();
        var raised = false;
        viewModel.PropertyChanged += (_, args) => raised |= args.PropertyName == nameof(SyncStatusViewModel.Status);

        viewModel.StartSyncCommand.Execute(null);

        raised.ShouldBeTrue();
    }
}
