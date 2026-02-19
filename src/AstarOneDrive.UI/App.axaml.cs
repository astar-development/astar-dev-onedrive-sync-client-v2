using AstarOneDrive.UI.Home;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace AstarOneDrive.UI;

public partial class App : Avalonia.Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        ThemeManager.ThemeManager.ApplyTheme("Light");

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new Home.MainWindowViewModel()
            };
        }
ThemeManager.ThemeManager.ApplyTheme("Dark"); // or load from config

        base.OnFrameworkInitializationCompleted();
    }
}
