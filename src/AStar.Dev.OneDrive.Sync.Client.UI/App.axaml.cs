using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data;
using AStar.Dev.OneDrive.Sync.Client.UI.Home;
using AStar.Dev.OneDrive.Sync.Client.UI.Localization;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Serilog;

namespace AStar.Dev.OneDrive.Sync.Client.UI;

public partial class App : Avalonia.Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if(ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            Dispatcher.UIThread.UnhandledException += (sender, e) =>
            {
                Log.Error(e.Exception, "UI thread exception");
                e.Handled = true;
            };
            ApplyDatabaseMigrations();
            ExceptionBootstrap.HookAvaloniaUIThread();

            var mainViewModel = new MainWindowViewModel();

            desktop.MainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };

            LoadSettingsAndApplyThemeSync(mainViewModel);
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void LoadSettingsAndApplyThemeSync(MainWindowViewModel mainViewModel)
    {
        LocalizationManager.SetLanguage(mainViewModel.Settings.SelectedLanguage);

        Result<bool, Exception> loadResult = mainViewModel.Settings.LoadSettingsAsync().Result;

        if(loadResult is Result<bool, Exception>.Error error)
        {
            Log.Error(error.Reason, "Failed to load settings on startup");
            LocalizationManager.SetLanguage(mainViewModel.Settings.SelectedLanguage);
            ThemeManager.ThemeManager.ApplyTheme(mainViewModel.Settings.SelectedTheme);
            return;
        }

        LocalizationManager.SetLanguage(mainViewModel.Settings.SelectedLanguage);
        ThemeManager.ThemeManager.ApplyTheme(mainViewModel.Settings.SelectedTheme);
    }

    private static void ApplyDatabaseMigrations()
    {
        var migrator = new SqliteDatabaseMigrator();
        try
        {
            migrator.EnsureMigrated();
        }
        catch(Exception exception)
        {
            Log.Error(exception, "Failed to apply database migrations on startup");
        }
    }
}
