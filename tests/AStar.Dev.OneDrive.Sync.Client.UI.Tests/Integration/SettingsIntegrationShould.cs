using AStar.Dev.OneDrive.Sync.Client.UI.Settings;
using AStar.Dev.Utilities;
using Avalonia.Headless;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Themes.Fluent;

namespace AStar.Dev.OneDrive.Sync.Client.UI.Tests.Integration;

public sealed class SettingsIntegrationShould
{
    [Fact]
    public void InitializeSettingsViewModelWithDefaults()
    {
        var viewModel = new SettingsViewModel();
        
        viewModel.SelectedTheme.ShouldBe("Light");
        viewModel.SelectedLanguage.ShouldBe("en-GB");
        viewModel.SelectedLayout.ShouldBe("Explorer");
        viewModel.UserName.ShouldBe("User");
        viewModel.AvailableThemes.ShouldContain("Dark");
        viewModel.AvailableThemes.ShouldContain("Light");
        viewModel.AvailableLanguages.ShouldContain("en-GB");
        viewModel.AvailableLanguages.ShouldContain("en-US");
    }

    [Fact]
    public void RaisePropertyChangedWhenThemePropertyChanges()
    {
        var viewModel = new SettingsViewModel();
        var changedProperties = new List<string>();
        viewModel.PropertyChanged += (_, e) => changedProperties.Add(e.PropertyName ?? "");

        viewModel.SelectedTheme = "Dark";

        changedProperties.ShouldContain("SelectedTheme");
    }

    [Fact]
    public void RaisePropertyChangedWhenLanguagePropertyChanges()
    {
        var viewModel = new SettingsViewModel();
        var changedProperties = new List<string>();
        viewModel.PropertyChanged += (_, e) => changedProperties.Add(e.PropertyName ?? "");

        viewModel.SelectedLanguage = "en-US";

        changedProperties.ShouldContain("SelectedLanguage");
    }

    [Fact]
    public async Task SaveAllSettingsWhenOkCommandIsExecuted()
    {
        var databasePath = CreateDatabasePath();
        var viewModel = new SettingsViewModel(databasePath)
        {
            SelectedTheme = "Hacker",
            SelectedLanguage = "en-US",
            SelectedLayout = "Dashboard",
            UserName = "Taylor"
        };

        viewModel.OkCommand.Execute(null);
        var reloadedViewModel = new SettingsViewModel(databasePath);
        _ = await reloadedViewModel.LoadSettingsAsync(TestContext.Current.CancellationToken);
        reloadedViewModel.SelectedTheme.ShouldBe("Hacker");
        reloadedViewModel.SelectedLanguage.ShouldBe("en-US");
        reloadedViewModel.SelectedLayout.ShouldBe("Dashboard");
        reloadedViewModel.UserName.ShouldBe("Taylor");
    }

    [Fact]
    public async Task DiscardChangesWhenCancelCommandIsExecuted()
    {
        var databasePath = CreateDatabasePath();
        var viewModel = new SettingsViewModel(databasePath)
        {
            SelectedTheme = "Dark",
            SelectedLanguage = "en-GB",
            SelectedLayout = "Explorer",
            UserName = "Jordan"
        };

        _ = await viewModel.SaveSettingsAsync(TestContext.Current.CancellationToken);
        viewModel.SelectedTheme = "Hacker";
        viewModel.SelectedLanguage = "en-US";
        viewModel.SelectedLayout = "Terminal";
        viewModel.UserName = "Casey";
        viewModel.CancelCommand.Execute(null);
        viewModel.SelectedTheme.ShouldBe("Dark");
        viewModel.SelectedLanguage.ShouldBe("en-GB");
        viewModel.SelectedLayout.ShouldBe("Explorer");
        viewModel.UserName.ShouldBe("Jordan");

        var reloadedViewModel = new SettingsViewModel(databasePath);
        _ = await reloadedViewModel.LoadSettingsAsync(TestContext.Current.CancellationToken);
        reloadedViewModel.SelectedTheme.ShouldBe("Dark");
        reloadedViewModel.SelectedLanguage.ShouldBe("en-GB");
        reloadedViewModel.SelectedLayout.ShouldBe("Explorer");
        reloadedViewModel.UserName.ShouldBe("Jordan");
    }

    private static string CreateDatabasePath()
        => Path.GetTempPath().CombinePath("astar-ui-settings-integration", Guid.NewGuid().ToString("N"), "astar-onedrive.db");

    private sealed class TestApplication : Avalonia.Application;
}
