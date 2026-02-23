using System.Linq;
using Avalonia.Headless;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Themes.Fluent;
using Shouldly;

namespace AstarOneDrive.UI.Tests.ThemeManager;

[Collection(ThemeManagerTestCollection.Name)]
public sealed class ThemeManagerTests
{
    private static bool _isAvaloniaInitialized;

    [Fact]
    public void ApplyTheme_Dark_LoadsWithoutError()
    {
        EnsureAvaloniaInitialized();

        var app = global::Avalonia.Application.Current ?? new TestApplication();
        app.Styles.Clear();
        app.Styles.Add(new FluentTheme());

        Should.NotThrow(() => global::AstarOneDrive.UI.ThemeManager.ThemeManager.ApplyTheme("Dark"));

        var appThemeInclude = app.Styles
            .OfType<StyleInclude>()
            .Single(static style => style.Source?.OriginalString.StartsWith("avares://AstarOneDrive.UI/Themes/", StringComparison.OrdinalIgnoreCase) == true);

        appThemeInclude.Source!.OriginalString.ShouldBe("avares://AstarOneDrive.UI/Themes/Dark.axaml");
    }

    [Fact]
    public void ApplyTheme_ReplacesOnlyAppThemeInclude_AndPreservesFluentTheme()
    {
        EnsureAvaloniaInitialized();

        var app = global::Avalonia.Application.Current ?? new TestApplication();
        app.Styles.Clear();
        app.Styles.Add(new FluentTheme());
        app.Styles.Add(new StyleInclude(new Uri("avares://AstarOneDrive.UI/"))
        {
            Source = new Uri("avares://AstarOneDrive.UI/Themes/Hacker.axaml")
        });

        global::AstarOneDrive.UI.ThemeManager.ThemeManager.ApplyTheme("Light");

        app.Styles.OfType<FluentTheme>().Any().ShouldBeTrue();

        var appThemeIncludes = app.Styles
            .OfType<StyleInclude>()
            .Where(static style => style.Source?.OriginalString.StartsWith("avares://AstarOneDrive.UI/Themes/", StringComparison.OrdinalIgnoreCase) == true)
            .ToList();

        appThemeIncludes.Count.ShouldBe(1);
        appThemeIncludes[0].Source!.OriginalString.ShouldBe("avares://AstarOneDrive.UI/Themes/Light.axaml");
    }

    [Fact]
    public void ApplyTheme_InvalidTheme_ThrowsInvalidOperationException()
    {
        EnsureAvaloniaInitialized();

        var app = global::Avalonia.Application.Current ?? new TestApplication();
        app.Styles.Clear();
        app.Styles.Add(new FluentTheme());

        Should.Throw<InvalidOperationException>(() => global::AstarOneDrive.UI.ThemeManager.ThemeManager.ApplyTheme("DoesNotExist"));
    }

    private static void EnsureAvaloniaInitialized()
    {
        if (_isAvaloniaInitialized)
        {
            return;
        }

        global::Avalonia.AppBuilder.Configure<TestApplication>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions())
            .SetupWithoutStarting();

        _isAvaloniaInitialized = true;
    }

    private sealed class TestApplication : global::Avalonia.Application;
}
