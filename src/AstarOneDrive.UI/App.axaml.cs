using AstarOneDrive.UI.Home;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace AstarOneDrive.UI;

public partial class App : Avalonia.Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainViewModel = new MainWindowViewModel();
            mainViewModel.Settings.LoadSettingsAsync().GetAwaiter().GetResult();
            ThemeManager.ThemeManager.ApplyTheme(mainViewModel.Settings.SelectedTheme);

            desktop.MainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
