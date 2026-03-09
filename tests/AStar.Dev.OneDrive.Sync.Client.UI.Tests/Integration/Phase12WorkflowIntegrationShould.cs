using AStar.Dev.OneDrive.Sync.Client.UI.Common;
using AStar.Dev.OneDrive.Sync.Client.UI.Home;

namespace AStar.Dev.OneDrive.Sync.Client.UI.Tests.Integration;

public sealed class Phase12WorkflowIntegrationShould
{
    [Fact]
    public void ProjectRealWorkflowStateAcrossExplorerDashboardAndTerminal()
    {
        var sut = new MainWindowViewModel(initializeLayoutView: false);
        sut.Accounts.Accounts.Clear();
        sut.Accounts.Accounts.Add(new("acc-1", "one@example.com", 500, 120));
        sut.Accounts.SelectedAccount = sut.Accounts.Accounts[0];
        sut.Sync.ProgressPercentage = 52;
        sut.Sync.Status = "Syncing...";
        sut.Sync.RecentActivity.Add(new(DateTime.UtcNow, "Info", "Upload queued: /local/a.txt"));

        sut.SwitchToExplorerCommand.Execute(null);
        sut.CurrentLayout.ShouldBe(LayoutType.Explorer);
        sut.SwitchToDashboardCommand.Execute(null);
        sut.CurrentLayout.ShouldBe(LayoutType.Dashboard);
        sut.LinkedAccountCount.ShouldBe(1);
        sut.SelectedAccountEmail.ShouldBe("one@example.com");
        sut.SyncProgressMetric.ShouldBe(52);

        sut.SwitchToTerminalCommand.Execute(null);
        sut.CurrentLayout.ShouldBe(LayoutType.Terminal);
        sut.TerminalOperationalStatus.ShouldContain("Syncing");
        sut.Logs.LogText.ShouldContain("Upload queued");
    }
}
