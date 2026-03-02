using AStar.Dev.OneDrive.Sync.Client.UI.Tests.ThemeManager;

namespace AStar.Dev.OneDrive.Sync.Client.UI.Tests.Logging;

[Collection(ThemeManagerTestCollection.Name)]
[TestSubject(typeof(LoggingBootstrap))]
public sealed class LoggingBootstrapShould : IDisposable
{
    public void Dispose()
    {
        Environment.SetEnvironmentVariable("ASTAR_LOG_PATH", null);
        Environment.SetEnvironmentVariable("ASTAR_LOG_RETENTION_DAYS", null);
    }

    [Fact]
    public void UseDefaultLogPathWhenEnvironmentVariableIsNotSet()
    {
        Environment.SetEnvironmentVariable("ASTAR_LOG_PATH", null);

        LoggingBootstrap.LogPath.ShouldBe(LoggingBootstrap.DefaultLogPath);
    }

    [Fact]
    public void UseEnvironmentVariableLogPathWhenSet()
    {
        Environment.SetEnvironmentVariable("ASTAR_LOG_PATH", "/custom/log/path.log");

        LoggingBootstrap.LogPath.ShouldBe("/custom/log/path.log");
    }

    [Fact]
    public void UseDefaultRetentionDaysWhenEnvironmentVariableIsNotSet()
    {
        Environment.SetEnvironmentVariable("ASTAR_LOG_RETENTION_DAYS", null);

        LoggingBootstrap.RetentionDays.ShouldBe(LoggingBootstrap.DefaultRetentionDays);
    }

    [Fact]
    public void UseEnvironmentVariableRetentionDaysWhenSet()
    {
        Environment.SetEnvironmentVariable("ASTAR_LOG_RETENTION_DAYS", "14");

        LoggingBootstrap.RetentionDays.ShouldBe(14);
    }

    [Fact]
    public void UseDefaultRetentionDaysWhenEnvironmentVariableIsInvalid()
    {
        Environment.SetEnvironmentVariable("ASTAR_LOG_RETENTION_DAYS", "not-a-number");

        LoggingBootstrap.RetentionDays.ShouldBe(LoggingBootstrap.DefaultRetentionDays);
    }
}
