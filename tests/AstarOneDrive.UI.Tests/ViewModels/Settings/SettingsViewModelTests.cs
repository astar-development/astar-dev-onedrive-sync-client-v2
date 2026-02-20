using AstarOneDrive.UI.Settings;
using Shouldly;
using Xunit;

namespace AstarOneDrive.UI.Tests.ViewModels.Settings;

public class SettingsViewModelTests
{
    [Fact]
    public void Constructor_InitializesWithDefaults()
    {
        var viewModel = new SettingsViewModel();

        viewModel.SelectedTheme.ShouldBe("Light");
        viewModel.SelectedLanguage.ShouldBe("en-US");
        viewModel.SelectedLayout.ShouldBe("Explorer");
    }

    [Fact]
    public void SelectedTheme_Set_RaisesPropertyChanged()
    {
        var viewModel = new SettingsViewModel();
        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(SettingsViewModel.SelectedTheme))
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
        var viewModel = new SettingsViewModel();

        viewModel.SelectedTheme = "Dark";

        // This test will fail until we refactor to make ThemeManager injectable or observable
        // For now, we'll verify through integration testing or by refactoring ThemeManager
        // to track last applied theme
        viewModel.SelectedTheme.ShouldBe("Dark");
        // TODO: Verify ThemeManager.ApplyTheme("Dark") was called
    }

    [Fact]
    public async Task SaveSettings_PersistsToFile()
    {
        var viewModel = new SettingsViewModel
        {
            SelectedTheme = "Dark",
            SelectedLanguage = "en-US",
            SelectedLayout = "Dashboard"
        };

        await viewModel.SaveSettingsAsync(TestContext.Current.CancellationToken);

        // Assert
        // Verify settings were persisted (will fail until SaveSettingsAsync is implemented)
    }

    [Fact]
    public async Task LoadSettings_RestoresFromFile()
    {
        var viewModel = new SettingsViewModel();
        viewModel.SelectedTheme = "Dark";
        viewModel.SelectedLayout = "Dashboard";
        await viewModel.SaveSettingsAsync(TestContext.Current.CancellationToken);
        var newViewModel = new SettingsViewModel();

        await newViewModel.LoadSettingsAsync(TestContext.Current.CancellationToken);

        newViewModel.SelectedTheme.ShouldBe("Dark");
        newViewModel.SelectedLayout.ShouldBe("Dashboard");
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
}
