using AStar.Dev.Utilities;

namespace AStar.Dev.OneDrive.Sync.Client.UI.Tests.Layouts;

public sealed class SyncStatusDataContextBindingShould
{
    [Theory]
    [InlineData("ExplorerLayoutView.axaml")]
    [InlineData("DashboardLayoutView.axaml")]
    [InlineData("TerminalLayoutView.axaml")]
    public void BindDataContextToSyncViewModel(string layoutFileName)
    {
        var filePath = GetRepositoryRootPath().CombinePath("src", "AStar.Dev.OneDrive.Sync.Client.UI", "Layouts", Path.GetFileName(layoutFileName));

        File.Exists(filePath).ShouldBeTrue();

        var xaml = File.ReadAllText(filePath);

        xaml.ShouldContain("<sync:SyncStatusView", Case.Insensitive);
        xaml.ShouldContain("DataContext=\"{Binding Sync}\"", Case.Insensitive);
    }

    private static string GetRepositoryRootPath()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while(current is not null)
        {
            var srcFolder = current.FullName.CombinePath("src", "AStar.Dev.OneDrive.Sync.Client.UI", "Layouts");
            if(Directory.Exists(srcFolder))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repository root from test base directory.");
    }
}
