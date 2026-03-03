using AStar.Dev.Utilities;

namespace AStar.Dev.OneDrive.Sync.Client.UI.Tests.Composition;

public sealed class AppCompositionWiringShould
{
    [Fact]
    public void InitializeCompositionRootBeforeCreatingMainWindowViewModel()
    {
        var appPath = GetRepositoryRootPath().CombinePath("src", "AStar.Dev.OneDrive.Sync.Client.UI", "App.axaml.cs");

        File.Exists(appPath).ShouldBeTrue();
        var code = File.ReadAllText(appPath);

        var initializeIndex = code.IndexOf("CompositionRoot.Initialize();", StringComparison.Ordinal);
        var mainWindowViewModelIndex = code.IndexOf("new MainWindowViewModel()", StringComparison.Ordinal);

        initializeIndex.ShouldBeGreaterThanOrEqualTo(0);
        mainWindowViewModelIndex.ShouldBeGreaterThanOrEqualTo(0);
        initializeIndex.ShouldBeLessThan(mainWindowViewModelIndex);
    }

    private static string GetRepositoryRootPath()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null)
        {
            var srcFolder = current.FullName.CombinePath("src", "AStar.Dev.OneDrive.Sync.Client.UI");
            if (Directory.Exists(srcFolder))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repository root from test base directory.");
    }
}