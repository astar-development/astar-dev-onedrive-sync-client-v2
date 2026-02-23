using Shouldly;

namespace AstarOneDrive.UI.Tests.Layouts;

public sealed class SyncStatusDataContextBindingTests
{
    [Theory]
    [InlineData("ExplorerLayoutView.axaml")]
    [InlineData("DashboardLayoutView.axaml")]
    [InlineData("TerminalLayoutView.axaml")]
    public void SyncStatusView_BindsDataContextToSyncViewModel(string layoutFileName)
    {
        var filePath = Path.Combine(GetRepositoryRootPath(), "src", "AstarOneDrive.UI", "Layouts", Path.GetFileName(layoutFileName));

        File.Exists(filePath).ShouldBeTrue();

        var xaml = File.ReadAllText(filePath);

        xaml.ShouldContain("<sync:SyncStatusView", Case.Insensitive);
        xaml.ShouldContain("DataContext=\"{Binding Sync}\"", Case.Insensitive);
    }

    private static string GetRepositoryRootPath()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null)
        {
            var srcFolder = Path.Combine(current.FullName, "src", "AstarOneDrive.UI", "Layouts");
            if (Directory.Exists(srcFolder))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repository root from test base directory.");
    }
}