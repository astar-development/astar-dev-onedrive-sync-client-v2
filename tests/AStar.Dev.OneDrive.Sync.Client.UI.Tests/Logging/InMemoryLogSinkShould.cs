using Serilog;

namespace AStar.Dev.OneDrive.Sync.Client.UI.Tests.Logging;

public sealed class InMemoryLogSinkShould
{
    [Fact]
    public void RedactSensitiveDataWhenLogsContainEmailsAndTokens()
    {
        var sink = new InMemoryLogSink();
        ILogger logger = new LoggerConfiguration().WriteTo.Sink(sink).CreateLogger();

        logger.Information("user={Email} token={Token}", "jason@example.com", "secret_token_123");

        var text = sink.GetText();
        text.Contains("jason@example.com", StringComparison.Ordinal).ShouldBeFalse();
        text.Contains("secret_token_123", StringComparison.Ordinal).ShouldBeFalse();
    }
}
