using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Threading;

namespace AStar.Dev.OneDrive.Sync.Client.UI.ThemeManager;

public static class ThemeManager
{
    private static readonly HashSet<string> SupportedThemes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Light",
        "Dark",
        "Colorful",
        "Professional",
        "Hacker",
        "HighContrast"
    };

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

            if(styleInclude.Source.OriginalString.Equals("avares://AStar.Dev.OneDrive.Sync.Client.UI/Themes/Base.axaml", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if(styleInclude.Source.OriginalString.StartsWith("avares://AStar.Dev.OneDrive.Sync.Client.UI/Themes/", StringComparison.OrdinalIgnoreCase))
            {
                app.Styles.RemoveAt(index);
            }
        }

#pragma warning disable R57
        app.Styles.Add(new StyleInclude(new Uri("avares://AStar.Dev.OneDrive.Sync.Client.UI/"))
        {
            Source = themeUri
        });
#pragma warning restore R57

        // Schedule visual refresh on the UI thread after theme is applied
        // Use BeginInvoke to ensure the change is processed before refresh
        Dispatcher.UIThread.Post(InvalidateAllVisuals, DispatcherPriority.Render);
    }

    private static void InvalidateAllVisuals()
    {
        if(Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow is { })
        {
            // Clear resource caches and force re-layout
            InvalidateVisualTree(desktop.MainWindow);
        }
    }

    private static void InvalidateVisualTree(Control control)
    {
        // Invalidate measure and arrange to force layout recalculation
        control.InvalidateMeasure();
        control.InvalidateArrange();
        control.InvalidateVisual();

        // Process all children
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
