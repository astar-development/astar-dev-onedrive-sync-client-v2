using Avalonia.Controls;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Threading;

namespace AStar.Dev.OneDrive.Sync.Client.UI.ThemeManager;

public static class ThemeManager
{
    private static readonly HashSet<string> SupportedThemes =
    [
with(StringComparer.OrdinalIgnoreCase),
        "Light",
        "Dark",
        "Colorful",
        "Professional",
        "Hacker",
        "HighContrast"
    ];

    public static void ApplyTheme(string themeName)
    {
        Avalonia.Application? app = Avalonia.Application.Current;
        if(app is null)
            return;

        if(!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => ApplyTheme(themeName));
            return;
        }

        if(!SupportedThemes.Contains(themeName))
        {
            throw new InvalidOperationException($"Theme '{themeName}' was not found.");
        }

        var themeUri = new Uri($"avares://AStar.Dev.OneDrive.Sync.Client.UI/Themes/{themeName}.axaml");

        for(var index = app.Styles.Count - 1; index >= 0; index--)
        {
            if(app.Styles[index] is not StyleInclude styleInclude || styleInclude.Source is null)
            {
                continue;
            }

            if(styleInclude.Source.OriginalString.Equals($"{ApplicationMetadata.AvaresPrefix}://{ApplicationMetadata.UiProject}/Themes/Base.axaml", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if(styleInclude.Source.OriginalString.StartsWith($"{ApplicationMetadata.AvaresPrefix}://{ApplicationMetadata.UiProject}/Themes/", StringComparison.OrdinalIgnoreCase))
            {
                app.Styles.RemoveAt(index);
            }
        }

        app.Styles.Add(new StyleInclude(new Uri($"{ApplicationMetadata.AvaresPrefix}://{ApplicationMetadata.UiProject}/"))
        {
            Source = themeUri
        });

        Dispatcher.UIThread.Post(InvalidateAllVisuals, DispatcherPriority.Render);
    }

    private static void InvalidateAllVisuals()
    {
        if(Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow is { })
        {
            InvalidateVisualTree(desktop.MainWindow);
        }
    }

    private static void InvalidateVisualTree(Control control)
    {
        control.InvalidateMeasure();
        control.InvalidateArrange();
        control.InvalidateVisual();

        if(control is Panel panel)
        {
            foreach(Control child in panel.Children.OfType<Control>())
            {
                InvalidateVisualTree(child);
            }
        }
        else if(control is Decorator decorator && decorator.Child is Control childControl)
        {
            InvalidateVisualTree(childControl);
        }
        else if(control is ContentControl contentControl && contentControl.Content is Control contentChild)
        {
            InvalidateVisualTree(contentChild);
        }
    }
}
