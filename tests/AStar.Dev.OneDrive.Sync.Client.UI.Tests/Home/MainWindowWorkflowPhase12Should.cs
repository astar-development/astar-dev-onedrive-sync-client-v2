using AStar.Dev.OneDrive.Sync.Client.UI.AccountManagement;
using AStar.Dev.OneDrive.Sync.Client.UI.Home;

namespace AStar.Dev.OneDrive.Sync.Client.UI.Tests.Home;

public sealed class MainWindowWorkflowPhase12Should
{
    [Fact]
    public void ExposeRealDashboardMetricsFromAccountAndSyncState()
    {
        var sut = new MainWindowViewModel(initializeLayoutView: false);
        sut.Accounts.Accounts.Clear();
        sut.Accounts.Accounts.Add(new AccountInfo("a1", "alpha@example.com", 200, 20));
        sut.Accounts.Accounts.Add(new AccountInfo("a2", "beta@example.com", 300, 120));
        sut.Accounts.SelectedAccount = sut.Accounts.Accounts[1];
        sut.Sync.ProgressPercentage = 67;

        sut.LinkedAccountCount.ShouldBe(2);
        sut.SelectedAccountEmail.ShouldBe("beta@example.com");
        sut.SyncProgressMetric.ShouldBe(67);
    }

    [Fact]
    public void AppendOperationalLogsWhenSyncActivityChanges()
    {
        var sut = new MainWindowViewModel(initializeLayoutView: false);

        sut.Sync.Status = "Syncing...";
        sut.Sync.RecentActivity.Add(new(DateTime.UtcNow, "Info", "Download queued: /remote/a.txt"));

        sut.TerminalOperationalStatus.ShouldContain("Syncing");
        sut.Logs.LogText.ShouldContain("Download queued");
    }
}
