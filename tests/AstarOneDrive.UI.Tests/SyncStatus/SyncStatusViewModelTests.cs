using AstarOneDrive.UI.SyncStatus;
using Shouldly;

namespace AstarOneDrive.UI.Tests.ViewModels.SyncStatus;

public sealed class SyncStatusViewModelTests
{
    [Fact]
    public void Constructor_InitializesStatusToIdle()
    {
        var viewModel = new SyncStatusViewModel();

        viewModel.Status.ShouldBe("Idle");
    }

    [Fact]
    public void StartSyncCommand_SetsStatusToSyncing()
    {
        var viewModel = new SyncStatusViewModel();

        viewModel.StartSyncCommand.Execute(null);

        viewModel.Status.ShouldBe("Syncing...");
    }

    [Fact]
    public void PauseSyncCommand_SetsStatusToPaused()
    {
        var viewModel = new SyncStatusViewModel();

        viewModel.PauseSyncCommand.Execute(null);

        viewModel.Status.ShouldBe("Paused");
    }

    [Fact]
    public void ProgressPercentage_Set_UpdatesValue()
    {
        var viewModel = new SyncStatusViewModel();

        viewModel.ProgressPercentage = 42;

        viewModel.ProgressPercentage.ShouldBe(42);
    }

    [Fact]
    public void RecentActivity_LogsEntries_OnStateChanges()
    {
        var viewModel = new SyncStatusViewModel();

        viewModel.StartSyncCommand.Execute(null);

        viewModel.RecentActivity.ShouldNotBeEmpty();
    }

    [Fact]
    public void SyncError_Set_StoresMessage()
    {
        var viewModel = new SyncStatusViewModel();

        viewModel.SyncError = "Network unavailable";

        viewModel.SyncError.ShouldBe("Network unavailable");
    }

    [Fact]
    public void PropertyChanged_FiresOnStateChanges()
    {
        var viewModel = new SyncStatusViewModel();
        var raised = false;
        viewModel.PropertyChanged += (_, args) => raised |= args.PropertyName == nameof(SyncStatusViewModel.Status);

        viewModel.StartSyncCommand.Execute(null);

        raised.ShouldBeTrue();
    }
}