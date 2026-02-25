using AstarOneDrive.UI.Home;
using AstarOneDrive.Infrastructure.Data;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Serilog;

namespace AstarOneDrive.UI;

public partial class App : Avalonia.Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            Dispatcher.UIThread.UnhandledException += (sender, e) => { Log.Error(e.Exception, "UI thread exception"); 
                e.Handled = true;
            };
            ApplyDatabaseMigrations();

            var mainViewModel = new MainWindowViewModel();

            desktop.MainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };

            _ = LoadSettingsAndApplyThemeAsync(mainViewModel);
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static async Task LoadSettingsAndApplyThemeAsync(MainWindowViewModel mainViewModel)
    {
         ExceptionBootstrap.HookAvaloniaUIThread();
        var loadResult = await mainViewModel.Settings.LoadSettingsAsync();

        if (loadResult is AStar.Dev.Functional.Extensions.Result<bool, Exception>.Error error)
        {
            Log.Error(error.Reason, "Failed to load settings on startup");
            ThemeManager.ThemeManager.ApplyTheme(mainViewModel.Settings.SelectedTheme);
            return;
        }

        ThemeManager.ThemeManager.ApplyTheme(mainViewModel.Settings.SelectedTheme);
    }

    private static void ApplyDatabaseMigrations()
    {
        var migrator = new SqliteDatabaseMigrator();
        try
        {
            migrator.EnsureMigrated();
        }
        catch (Exception exception)
        {
            Log.Error(exception, "Failed to apply database migrations on startup");
        }
    }
}
