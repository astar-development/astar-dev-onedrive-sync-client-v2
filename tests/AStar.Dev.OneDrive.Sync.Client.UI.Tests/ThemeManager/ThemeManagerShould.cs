using Avalonia.Markup.Xaml.Styling;
using Avalonia.Themes.Fluent;

namespace AStar.Dev.OneDrive.Sync.Client.UI.Tests.ThemeManager;

[Collection(ThemeManagerTestCollection.Name)]
public sealed class ThemeManagerShould
{
    [Fact]
    public void LoadWithoutErrorWhenApplyThemeDarkIsApplied()
    {
        Avalonia.Application app = Avalonia.Application.Current ?? new TestApplication();
        app.Styles.Clear();
        app.Styles.Add(new FluentTheme());

        Should.NotThrow(() => UI.ThemeManager.ThemeManager.ApplyTheme("Dark"));

        StyleInclude appThemeInclude = app.Styles
            .OfType<StyleInclude>()
            .Single(static style => style.Source?.OriginalString.StartsWith("avares://AStar.Dev.OneDrive.Sync.Client.UI/Themes/", StringComparison.OrdinalIgnoreCase) == true);

        appThemeInclude.Source!.OriginalString.ShouldBe("avares://AStar.Dev.OneDrive.Sync.Client.UI/Themes/Dark.axaml");
    }

    [Fact]
    public void ReplaceOnlyAppThemeIncludeAndPreserveFluentThemeWhenApplyThemeIsCalled()
    {
        Avalonia.Application app = Avalonia.Application.Current ?? new TestApplication();
        app.Styles.Clear();
        app.Styles.Add(new FluentTheme());
        app.Styles.Add(new StyleInclude(new Uri($"{ApplicationMetadata.AvaresPrefix}://{ApplicationMetadata.UiProject}/"))
        {
            Source = new Uri($"{ApplicationMetadata.AvaresPrefix}://{ApplicationMetadata.UiProject}/Themes/Hacker.axaml")
        });

        UI.ThemeManager.ThemeManager.ApplyTheme("Light");

        app.Styles.OfType<FluentTheme>().Any().ShouldBeTrue();

        var appThemeIncludes = app.Styles
            .OfType<StyleInclude>()
            .Where(static style => style.Source?.OriginalString.StartsWith($"{ApplicationMetadata.AvaresPrefix}://{ApplicationMetadata.UiProject}/Themes/", StringComparison.OrdinalIgnoreCase) == true)
            .ToList();

        appThemeIncludes.Count.ShouldBe(1);
        appThemeIncludes[0].Source!.OriginalString.ShouldBe($"{ApplicationMetadata.AvaresPrefix}://{ApplicationMetadata.UiProject}/Themes/Light.axaml");
    }

    [Fact]
    public void ThrowInvalidOperationExceptionWhenApplyThemeWithInvalidThemeIsCalled()
    {
        Avalonia.Application app = Avalonia.Application.Current ?? new TestApplication();
        app.Styles.Clear();
        app.Styles.Add(new FluentTheme());

        _ = Should.Throw<InvalidOperationException>(() => UI.ThemeManager.ThemeManager.ApplyTheme("DoesNotExist"));
    }

    [Fact]
    public void PreserveBaseThemeStylesWhenApplyThemeIsCalled()
    {
        Avalonia.Application app = Avalonia.Application.Current ?? new TestApplication();
        app.Styles.Clear();
        app.Styles.Add(new FluentTheme());
        app.Styles.Add(new StyleInclude(new Uri($"{ApplicationMetadata.AvaresPrefix}://{ApplicationMetadata.UiProject}/"))
        {
            Source = new Uri($"{ApplicationMetadata.AvaresPrefix}://{ApplicationMetadata.UiProject}/Themes/Base.axaml")
        });

        UI.ThemeManager.ThemeManager.ApplyTheme("Professional");

        StyleInclude? baseThemeInclude = app.Styles
            .OfType<StyleInclude>()
            .SingleOrDefault(static style => style.Source?.OriginalString == $"{ApplicationMetadata.AvaresPrefix}://{ApplicationMetadata.UiProject}/Themes/Base.axaml");

        _ = baseThemeInclude.ShouldNotBeNull("Base.axaml should be preserved when swapping themes");
    }

    private sealed class TestApplication : Avalonia.Application;
}
