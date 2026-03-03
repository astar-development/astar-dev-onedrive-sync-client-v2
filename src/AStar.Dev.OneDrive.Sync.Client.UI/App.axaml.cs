using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.Application.Interfaces;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure;
using AStar.Dev.OneDrive.Sync.Client.UI.Composition;
using AStar.Dev.OneDrive.Sync.Client.UI.Home;
using AStar.Dev.OneDrive.Sync.Client.UI.Localization;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace AStar.Dev.OneDrive.Sync.Client.UI;

/// <summary>
/// The main Avalonia application class.
/// </summary>
public partial class App : Avalonia.Application
{
    /// <inheritdoc />
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    /// <inheritdoc />
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            
            ApplyDatabaseMigrations();
            CompositionRoot.Initialize();
            ExceptionBootstrap.HookAvaloniaUIThread();

            var mainViewModel = new MainWindowViewModel();

            desktop.MainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };

            LoadSettingsAndApplyThemeSync(mainViewModel);
        }
        else
        {
            ApplyDatabaseMigrations();
            ExceptionBootstrap.HookAvaloniaUIThread();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void LoadSettingsAndApplyThemeSync(MainWindowViewModel mainViewModel)
        => _ = mainViewModel.Settings.LoadSettings()
                .Map(success =>
                {
                    ApplyCurrentLocalizationAndTheme(mainViewModel);
                    return success;
                })
                .TapError(exception => Log.Error("Failed to load settings on startup"));

    private static void ApplyDatabaseMigrations()
    {
        IServiceCollection services = new ServiceCollection().AddInfrastructure();
        using ServiceProvider provider = services.BuildServiceProvider();
        IMigrationService migrationService = provider.GetRequiredService<IMigrationService>();
        _ = Try.Run(migrationService.EnsureMigrated)
            .TapError(exception => Log.Error(exception, "Failed to apply database migrations on startup"));
    }

    private static void ApplyCurrentLocalizationAndTheme(MainWindowViewModel mainViewModel)
    {
        LocalizationManager.SetLanguage(mainViewModel.Settings.SelectedLanguage);
        ThemeManager.ThemeManager.ApplyTheme(mainViewModel.Settings.SelectedTheme);
    }
}
