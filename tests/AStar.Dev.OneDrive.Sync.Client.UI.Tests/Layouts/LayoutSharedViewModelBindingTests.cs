using Shouldly;

namespace AStar.Dev.OneDrive.Sync.Client.UI.Tests.Layouts;

public sealed class LayoutSharedViewModelBindingTests
{
    [Theory]
    [InlineData("ExplorerLayoutView.axaml")]
    [InlineData("DashboardLayoutView.axaml")]
    [InlineData("TerminalLayoutView.axaml")]
    public void Layouts_UseMainWindowViewModelAsDataType(string layoutFileName)
    {
        var xaml = File.ReadAllText(GetLayoutPath(layoutFileName));

        xaml.ShouldContain("x:DataType=\"home:MainWindowViewModel\"", Case.Insensitive);
    }

    [Theory]
    [InlineData("ExplorerLayoutView.axaml", "<folder:FolderTreeView DataContext=\"{Binding FolderTree}\" />")]
    [InlineData("ExplorerLayoutView.axaml", "<account:AccountListView Grid.Row=\"0\" DataContext=\"{Binding Accounts}\" />")]
    [InlineData("DashboardLayoutView.axaml", "<account:AccountListView DataContext=\"{Binding Accounts}\" />")]
    [InlineData("TerminalLayoutView.axaml", "<account:AccountListView DataContext=\"{Binding Accounts}\" />")]
    [InlineData("TerminalLayoutView.axaml", "<folder:FolderTreeView DataContext=\"{Binding FolderTree}\" />")]
    [InlineData("TerminalLayoutView.axaml", "<logs:LogsView DataContext=\"{Binding Logs}\" />")]
    [InlineData("TerminalLayoutView.axaml", "<settings:SettingsView DataContext=\"{Binding Settings}\" />")]
    public void EmbeddedViews_BindToSharedViewModels(string layoutFileName, string expectedBindingSnippet)
    {
        var xaml = File.ReadAllText(GetLayoutPath(layoutFileName));

        xaml.ShouldContain(expectedBindingSnippet, Case.Insensitive);
    }

    private static string GetLayoutPath(string layoutFileName)
        => Path.Combine(GetRepositoryRootPath(), "src", "AStar.Dev.OneDrive.Sync.Client.UI", "Layouts", Path.GetFileName(layoutFileName));

    private static string GetRepositoryRootPath()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while(current is not null)
        {
            var srcFolder = Path.Combine(current.FullName, "src", "AStar.Dev.OneDrive.Sync.Client.UI", "Layouts");
            if(Directory.Exists(srcFolder))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repository root from test base directory.");
    }
}
