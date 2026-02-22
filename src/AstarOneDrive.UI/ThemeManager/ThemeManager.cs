using Avalonia.Markup.Xaml.Styling;

namespace AstarOneDrive.UI.ThemeManager;

public static class ThemeManager
{
    public static void ApplyTheme(string themeName)
    {
        var app = Avalonia.Application.Current;
        if (app is null) return;

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
