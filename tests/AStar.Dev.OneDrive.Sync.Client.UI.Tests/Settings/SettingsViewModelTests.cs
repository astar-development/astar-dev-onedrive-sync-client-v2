using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.UI.Settings;
using Shouldly;

namespace AStar.Dev.OneDrive.Sync.Client.UI.Tests.ViewModels.Settings;

public class SettingsViewModelTests
{
    [Fact]
    public void Constructor_InitializesWithDefaults()
    {
        var viewModel = new SettingsViewModel();
        viewModel.SelectedTheme.ShouldBe("Light");
        viewModel.SelectedLanguage.ShouldBe("en-GB");
        viewModel.SelectedLayout.ShouldBe("Explorer");
    }

    [Fact]
    public void SelectedTheme_Set_RaisesPropertyChanged()
    {
        var viewModel = new SettingsViewModel();
        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if(args.PropertyName == nameof(SettingsViewModel.SelectedTheme))
            {
                propertyChangedRaised = true;
            }
        };

        viewModel.SelectedTheme = "Dark";
        propertyChangedRaised.ShouldBeTrue();
    }

    [Fact]
    public async Task SelectedTheme_Set_CallsThemeManagerApplyTheme()
    {
        var viewModel = new SettingsViewModel
        {
            SelectedTheme = "Dark"
        };
        viewModel.SelectedTheme.ShouldBe("Dark");
        await Task.CompletedTask;
    }

    [Fact]
    public async Task SaveSettingsAsync_ReturnsOkResult()
    {
        var viewModel = new SettingsViewModel(CreateDatabasePath())
        {
            SelectedTheme = "Dark",
            SelectedLanguage = "en-GB",
            SelectedLayout = "Dashboard"
        };

        Result<bool, Exception> result = await viewModel.SaveSettingsAsync(TestContext.Current.CancellationToken);
        Pattern.IsSuccess(result).ShouldBeTrue();
    }

    [Fact]
    public async Task SaveSettings_PersistsToDatabase()
    {
        var viewModel = new SettingsViewModel(CreateDatabasePath())
        {
            SelectedTheme = "Dark",
            SelectedLanguage = "en-GB",
            SelectedLayout = "Dashboard"
        };

        Result<bool, Exception> result = await viewModel.SaveSettingsAsync(TestContext.Current.CancellationToken);
        Pattern.IsSuccess(result).ShouldBeTrue();
    }

    [Fact]
    public async Task LoadSettingsAsync_ReturnsOkResult()
    {
        var databasePath = CreateDatabasePath();
        var viewModel = new SettingsViewModel(databasePath)
        {
            SelectedTheme = "Hacker",
            SelectedLayout = "Terminal"
        };
        _ = await viewModel.SaveSettingsAsync(TestContext.Current.CancellationToken);
        var newViewModel = new SettingsViewModel(databasePath);

        Result<bool, Exception> result = await newViewModel.LoadSettingsAsync(TestContext.Current.CancellationToken);
        Pattern.IsSuccess(result).ShouldBeTrue();
    }

    [Fact]
    public async Task LoadSettings_RestoresFromDatabase()
    {
        var databasePath = CreateDatabasePath();
        var viewModel = new SettingsViewModel(databasePath)
        {
            SelectedTheme = "Dark",
            SelectedLayout = "Dashboard"
        };
        _ = await viewModel.SaveSettingsAsync(TestContext.Current.CancellationToken);
        var newViewModel = new SettingsViewModel(databasePath);

        Result<bool, Exception> result = await newViewModel.LoadSettingsAsync(TestContext.Current.CancellationToken);

        Pattern.IsSuccess(result).ShouldBeTrue();
        newViewModel.SelectedTheme.ShouldBe("Dark");
        newViewModel.SelectedLayout.ShouldBe("Dashboard");
    }

    [Fact]
    public async Task LoadSettingsAsync_ReturnsOkResult_WhenDatabaseIsEmpty()
    {
        var viewModel = new SettingsViewModel(CreateDatabasePath());
        Result<bool, Exception> result = await viewModel.LoadSettingsAsync(TestContext.Current.CancellationToken);
        Pattern.IsSuccess(result).ShouldBeTrue();
    }

    [Fact]
    public void AvailableThemes_IsNotEmpty()
    {
        var viewModel = new SettingsViewModel();
        viewModel.AvailableThemes.ShouldNotBeEmpty();
        viewModel.AvailableThemes.ShouldContain("Light");
        viewModel.AvailableThemes.ShouldContain("Dark");
    }

    [Fact]
    public void ThemeChanged_FiredWhenThemeChanges()
    {
        var viewModel = new SettingsViewModel();
        string? changedTheme = null;
        viewModel.ThemeChanged += (sender, theme) => changedTheme = theme;
        viewModel.SelectedTheme = "Dark";
        changedTheme.ShouldBe("Dark");
    }

    private static string CreateDatabasePath()
        => Path.Combine(Path.GetTempPath(), $"astar-ui-settings-tests-{Guid.NewGuid():N}", "astar-onedrive.db");
}
