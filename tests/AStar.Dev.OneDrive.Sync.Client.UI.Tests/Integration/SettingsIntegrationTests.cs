using System.Linq;
using AStar.Dev.OneDrive.Sync.Client.UI.Localization;
using AStar.Dev.OneDrive.Sync.Client.UI.Settings;
using Avalonia.Headless;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Themes.Fluent;
using Shouldly;

namespace AStar.Dev.OneDrive.Sync.Client.UI.Tests.Integration;

public sealed class SettingsIntegrationTests
{
    private static bool _isAvaloniaInitialized;

    [Fact]
    public void ChangingTheme_UpdatesAppTheme()
    {
        var app = GetAppWithThemeSupport();
        var viewModel = new SettingsViewModel
        {
            SelectedTheme = "Dark"
        };
        GetAppThemeSource(app).ShouldBe("avares://AStar.Dev.OneDrive.Sync.Client.UI/Themes/Dark.axaml");
    }

    [Fact]
    public void ChangingLanguage_UpdatesLocalizedStrings()
    {
        EnsureAvaloniaInitialized();
        var app = Avalonia.Application.Current ?? new TestApplication();
        app.Resources.MergedDictionaries.Clear();
        var viewModel = new SettingsViewModel
        {
            SelectedLanguage = "en-US"
        };
        LocalizationManager.CurrentLanguage.ShouldBe("en-US");
        LocalizationManager.GetString("Menu_File").ShouldBe("File (US)");
    }

    [Fact]
    public async Task OkCommand_SavesAllSettings()
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
    public async Task CancelCommand_DiscardsChanges()
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
        => Path.Combine(Path.GetTempPath(), "astar-ui-settings-integration", Guid.NewGuid().ToString("N"), "astar-onedrive.db");

    private static void EnsureAvaloniaInitialized()
    {
        if(_isAvaloniaInitialized)
        {
            return;
        }

        _ = Avalonia.AppBuilder.Configure<TestApplication>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions())
            .SetupWithoutStarting();

        _isAvaloniaInitialized = true;
    }

    private static Avalonia.Application GetAppWithThemeSupport()
    {
        EnsureAvaloniaInitialized();
        Avalonia.Application app = Avalonia.Application.Current ?? new TestApplication();
        app.Styles.Clear();
        app.Styles.Add(new FluentTheme());
        return app;
    }

    private static string GetAppThemeSource(Avalonia.Application app)
        => app.Styles
            .OfType<StyleInclude>()
            .Single(style => style.Source?.OriginalString.Contains("/Themes/", StringComparison.OrdinalIgnoreCase) == true)
            .Source!.OriginalString;

    private sealed class TestApplication : Avalonia.Application;
}
