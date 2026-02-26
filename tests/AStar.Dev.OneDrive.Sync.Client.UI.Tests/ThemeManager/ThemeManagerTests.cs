using System.Linq;
using Avalonia.Headless;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Themes.Fluent;
using Shouldly;

namespace AStar.Dev.OneDrive.Sync.Client.UI.Tests.ThemeManager;

[Collection(ThemeManagerTestCollection.Name)]
public sealed class ThemeManagerTests
{
    private static bool _isAvaloniaInitialized;

    [Fact]
    public void ApplyTheme_Dark_LoadsWithoutError()
    {
        EnsureAvaloniaInitialized();

        Avalonia.Application app = global::Avalonia.Application.Current ?? new TestApplication();
        app.Styles.Clear();
        app.Styles.Add(new FluentTheme());

        Should.NotThrow(() => global::AStar.Dev.OneDrive.Sync.Client.UI.ThemeManager.ThemeManager.ApplyTheme("Dark"));

        StyleInclude appThemeInclude = app.Styles
            .OfType<StyleInclude>()
            .Single(static style => style.Source?.OriginalString.StartsWith("avares://AStar.Dev.OneDrive.Sync.Client.UI/Themes/", StringComparison.OrdinalIgnoreCase) == true);

        appThemeInclude.Source!.OriginalString.ShouldBe("avares://AStar.Dev.OneDrive.Sync.Client.UI/Themes/Dark.axaml");
    }

    [Fact]
    public void ApplyTheme_ReplacesOnlyAppThemeInclude_AndPreservesFluentTheme()
    {
        EnsureAvaloniaInitialized();

        Avalonia.Application app = global::Avalonia.Application.Current ?? new TestApplication();
        app.Styles.Clear();
        app.Styles.Add(new FluentTheme());
        app.Styles.Add(new StyleInclude(new Uri("avares://AStar.Dev.OneDrive.Sync.Client.UI/"))
        {
            Source = new Uri("avares://AStar.Dev.OneDrive.Sync.Client.UI/Themes/Hacker.axaml")
        });

        global::AStar.Dev.OneDrive.Sync.Client.UI.ThemeManager.ThemeManager.ApplyTheme("Light");

        app.Styles.OfType<FluentTheme>().Any().ShouldBeTrue();

        var appThemeIncludes = app.Styles
            .OfType<StyleInclude>()
            .Where(static style => style.Source?.OriginalString.StartsWith("avares://AStar.Dev.OneDrive.Sync.Client.UI/Themes/", StringComparison.OrdinalIgnoreCase) == true)
            .ToList();

        appThemeIncludes.Count.ShouldBe(1);
        appThemeIncludes[0].Source!.OriginalString.ShouldBe("avares://AStar.Dev.OneDrive.Sync.Client.UI/Themes/Light.axaml");
    }

    [Fact]
    public void ApplyTheme_InvalidTheme_ThrowsInvalidOperationException()
    {
        EnsureAvaloniaInitialized();

        Avalonia.Application app = global::Avalonia.Application.Current ?? new TestApplication();
        app.Styles.Clear();
        app.Styles.Add(new FluentTheme());

        _ = Should.Throw<InvalidOperationException>(() => global::AStar.Dev.OneDrive.Sync.Client.UI.ThemeManager.ThemeManager.ApplyTheme("DoesNotExist"));
    }

    private static void EnsureAvaloniaInitialized()
    {
        if(_isAvaloniaInitialized)
        {
            return;
        }

        _ = global::Avalonia.AppBuilder.Configure<TestApplication>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions())
            .SetupWithoutStarting();

        _isAvaloniaInitialized = true;
    }

    private sealed class TestApplication : global::Avalonia.Application;
}
