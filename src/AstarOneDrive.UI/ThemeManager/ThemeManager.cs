using Avalonia.Markup.Xaml.Styling;
using Avalonia.Threading;

namespace AstarOneDrive.UI.ThemeManager;

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
        var app = Avalonia.Application.Current;
        if (app is null) return;

        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => ApplyTheme(themeName));
            return;
        }

        if (!SupportedThemes.Contains(themeName))
        {
            throw new InvalidOperationException($"Theme '{themeName}' was not found.");
        }

        var themeUri = new Uri($"avares://AstarOneDrive.UI/Themes/{themeName}.axaml");

        for (var index = app.Styles.Count - 1; index >= 0; index--)
        {
            if (app.Styles[index] is not StyleInclude styleInclude || styleInclude.Source is null)
            {
                continue;
            }

            if (styleInclude.Source.OriginalString.StartsWith("avares://AstarOneDrive.UI/Themes/", StringComparison.OrdinalIgnoreCase))
            {
                app.Styles.RemoveAt(index);
            }
        }

        app.Styles.Add(new StyleInclude(new Uri("avares://AstarOneDrive.UI/"))
        {
            Source = themeUri
        });
    }
}
