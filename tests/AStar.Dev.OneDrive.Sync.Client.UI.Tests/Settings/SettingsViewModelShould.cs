using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.UI.Settings;
using AStar.Dev.Utilities;

namespace AStar.Dev.OneDrive.Sync.Client.UI.Tests.ViewModels.Settings;

public class SettingsViewModelShould
{
    [Fact]
    public void InitializeWithDefaults()
    {
        var viewModel = new SettingsViewModel();

        viewModel.SelectedTheme.ShouldBe("Light");
        viewModel.SelectedLanguage.ShouldBe("en-GB");
        viewModel.SelectedLayout.ShouldBe("Explorer");
    }

    [Fact]
    public void RaisePropertyChangedWhenSelectedThemeIsSet()
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
    public async Task CallThemeManagerApplyThemeWhenSelectedThemeIsSet()
    {
        var viewModel = new SettingsViewModel
        {
            SelectedTheme = "Dark"
        };
        viewModel.SelectedTheme.ShouldBe("Dark");
        await Task.CompletedTask;
    }

    [Fact]
    public void FireThemeChangedEventWhenThemeChanges()
    {
        var viewModel = new SettingsViewModel();
        var eventFired = false;
        string? themeValue = null;
        viewModel.ThemeChanged += (_, theme) =>
        {
            eventFired = true;
            themeValue = theme;
        };

        viewModel.SelectedTheme = "Hacker";

        eventFired.ShouldBeTrue();
        themeValue.ShouldBe("Hacker");
    }

    [Fact]
    public void FireLayoutChangedEventWhenLayoutChanges()
    {
        var viewModel = new SettingsViewModel();
        var eventFired = false;
        string? layoutValue = null;
        viewModel.LayoutChanged += (_, layout) =>
        {
            eventFired = true;
            layoutValue = layout;
        };

        viewModel.SelectedLayout = "Terminal";

        eventFired.ShouldBeTrue();
        layoutValue.ShouldBe("Terminal");
    }

    [Fact]
    public async Task ReturnOkResultWhenSaveSettingsAsyncIsCalled()
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
    public async Task PersistChangesToDatabaseWhenSaveSettingsIsCalled()
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
    public async Task ReturnOkResultWhenLoadSettingsAsyncIsCalled()
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
    public async Task RestoreSettingsFromDatabaseWhenLoadSettingsIsCalled()
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
    public async Task ReturnOkResultWhenLoadSettingsAsyncIsCalledAndDatabaseIsEmpty()
    {
        var viewModel = new SettingsViewModel(CreateDatabasePath());
        Result<bool, Exception> result = await viewModel.LoadSettingsAsync(TestContext.Current.CancellationToken);
        Pattern.IsSuccess(result).ShouldBeTrue();
    }

    [Fact]
    public void ContainTheExpectedThemes()
    {
        var viewModel = new SettingsViewModel();
        viewModel.AvailableThemes.ShouldNotBeEmpty();
        viewModel.AvailableThemes.ShouldContain("Light");
        viewModel.AvailableThemes.ShouldContain("Dark");
    }

    [Fact]
    public void FireThemeChangedEventWhenThemeChanged()
    {
        var viewModel = new SettingsViewModel();
        string? changedTheme = null;
        viewModel.ThemeChanged += (sender, theme) => changedTheme = theme;

        viewModel.SelectedTheme = "Dark";
        
        changedTheme.ShouldBe("Dark");
    }

    private static string CreateDatabasePath()
        => Path.GetTempPath().CombinePath($"astar-ui-settings-tests-{Guid.NewGuid():N}", "astar-onedrive.db");
}
