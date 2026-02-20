using System;
using Avalonia;
using Avalonia.Markup.Xaml.Styling;

namespace AstarOneDrive.UI.ThemeManager;

public static class ThemeManager
{
    public static void ApplyTheme(string themeName)
    {
        var app = Avalonia.Application.Current;
        if (app is null) return;

        // Clear existing theme styles but keep other styles if you add them later
        app.Styles.Clear();

        // Base style (optional, if you create one)
        // app.Styles.Add(new StyleInclude(new Uri("avares://AstarOneDrive.UI/"))
        // {
        //     Source = new Uri("avares://AstarOneDrive.UI/Themes/Base.axaml")
        // });

        var themeUri = new Uri($"avares://AstarOneDrive.UI/Themes/{themeName}.axaml");

        app.Styles.Add(new StyleInclude(new Uri("avares://AstarOneDrive.UI/"))
        {
            Source = themeUri
        });
    }
}
