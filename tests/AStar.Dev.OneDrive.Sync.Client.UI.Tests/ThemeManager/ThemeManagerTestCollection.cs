using Avalonia.Headless;

namespace AStar.Dev.OneDrive.Sync.Client.UI.Tests.ThemeManager;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class ThemeManagerTestCollection : ICollectionFixture<ThemeManagerFixture>
{
    public const string Name = "UiGlobalStateTestCollection";
}

public sealed class ThemeManagerFixture
{
    public ThemeManagerFixture()
    {
        if (Avalonia.Application.Current is not null)
        {
            return;
        }

        _ = Avalonia.AppBuilder.Configure<TestApplication>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions())
            .SetupWithoutStarting();
    }

    private sealed class TestApplication : Avalonia.Application;
}
