using System.Net;
using System.Net.Http.Headers;
using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Graph;

namespace AStar.Dev.OneDrive.Sync.Client.Infrastructure.Tests.Graph;

public sealed class OneDriveGraphClientShould
{
    [Fact]
    public async Task RetryTransientResponsesThenReturnSuccess()
    {
        SequenceHttpMessageHandler handler = new(
            _ => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable),
            _ =>
            {
                HttpResponseMessage response = new((HttpStatusCode)429);
                response.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromMilliseconds(25));
                return response;
            },
            _ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("ok") });
        RecordingDelayStrategy delays = new();
        RecordingGraphTelemetry telemetry = new();
        OneDriveGraphClient client = CreateClient(handler, delays, telemetry, maximumRetryAttempts: 3);

        Result<OneDriveGraphResponse, SyncError> result = await client.SendAsync(
            OneDriveGraphRequest.Get("/me/drive/root", "token"),
            TestContext.Current.CancellationToken);

        result.Match(ok => ok.Content, error => error.Message).ShouldBe("ok");
        delays.Delays.Count.ShouldBe(2);
        telemetry.Events.Count.ShouldBe(1);
        telemetry.Events[0].RetryCount.ShouldBe(2);
        telemetry.Events[0].ErrorKind.ShouldBeNull();
    }

    [Fact]
    public async Task MapNotFoundToTypedErrorWithoutRetry()
    {
        SequenceHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
        RecordingDelayStrategy delays = new();
        RecordingGraphTelemetry telemetry = new();
        OneDriveGraphClient client = CreateClient(handler, delays, telemetry, maximumRetryAttempts: 3);

        Result<OneDriveGraphResponse, SyncError> result = await client.SendAsync(
            OneDriveGraphRequest.Get("/me/drive/items/missing", "token"),
            TestContext.Current.CancellationToken);

        result.Match(_ => false, error => error.Kind == SyncErrorKind.NotFound && !error.IsTransient).ShouldBeTrue();
        delays.Delays.Count.ShouldBe(0);
        telemetry.Events.Single().ErrorKind.ShouldBe(SyncErrorKind.NotFound);
    }

    [Fact]
    public async Task RetryNetworkFailuresUntilExhaustedThenReturnTypedError()
    {
        SequenceHttpMessageHandler handler = new(
            _ => throw new HttpRequestException("offline"),
            _ => throw new HttpRequestException("offline"),
            _ => throw new HttpRequestException("offline"));
        RecordingDelayStrategy delays = new();
        RecordingGraphTelemetry telemetry = new();
        OneDriveGraphClient client = CreateClient(handler, delays, telemetry, maximumRetryAttempts: 2);

        Result<OneDriveGraphResponse, SyncError> result = await client.SendAsync(
            OneDriveGraphRequest.Get("/me/drive/root", "token"),
            TestContext.Current.CancellationToken);

        result.Match(_ => false, error => error.Kind == SyncErrorKind.Network && error.IsTransient).ShouldBeTrue();
        delays.Delays.Count.ShouldBe(2);
        telemetry.Events.Single().RetryCount.ShouldBe(2);
    }

    [Fact]
    public async Task RetryThrottleResponsesUntilExhaustedThenReturnThrottleError()
    {
        SequenceHttpMessageHandler handler = new(
            _ => new HttpResponseMessage((HttpStatusCode)429),
            _ => new HttpResponseMessage((HttpStatusCode)429),
            _ => new HttpResponseMessage((HttpStatusCode)429));
        RecordingDelayStrategy delays = new();
        RecordingGraphTelemetry telemetry = new();
        OneDriveGraphClient client = CreateClient(handler, delays, telemetry, maximumRetryAttempts: 2);

        Result<OneDriveGraphResponse, SyncError> result = await client.SendAsync(
            OneDriveGraphRequest.Get("/me/drive/root", "token"),
            TestContext.Current.CancellationToken);

        result.Match(_ => false, error => error.Kind == SyncErrorKind.Throttled && error.IsTransient).ShouldBeTrue();
        delays.Delays.Count.ShouldBe(2);
        telemetry.Events.Single().RetryCount.ShouldBe(2);
    }

    private static OneDriveGraphClient CreateClient(
        HttpMessageHandler handler,
        IGraphDelayStrategy delayStrategy,
        IOneDriveGraphTelemetry telemetry,
        int maximumRetryAttempts)
    {
        var options = new OneDriveGraphClientOptions
        {
            BaseUri = new Uri("https://graph.microsoft.com/v1.0"),
            MaximumRetryAttempts = maximumRetryAttempts,
            InitialBackoff = TimeSpan.FromMilliseconds(10)
        };

        return new OneDriveGraphClient(new HttpClient(handler), options, delayStrategy, telemetry);
    }

    private sealed class RecordingDelayStrategy : IGraphDelayStrategy
    {
        public List<TimeSpan> Delays { get; } = [];

        public Task DelayAsync(TimeSpan duration, CancellationToken cancellationToken)
        {
            Delays.Add(duration);
            return Task.CompletedTask;
        }
    }

    private sealed class RecordingGraphTelemetry : IOneDriveGraphTelemetry
    {
        public List<GraphRequestTelemetryEvent> Events { get; } = [];

        public void Track(GraphRequestTelemetryEvent telemetryEvent) => Events.Add(telemetryEvent);
    }

    private sealed class SequenceHttpMessageHandler(params Func<HttpRequestMessage, HttpResponseMessage>[] sequence)
        : HttpMessageHandler
    {
        private readonly Queue<Func<HttpRequestMessage, HttpResponseMessage>> _handlers = new(sequence);

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => _handlers.Count == 0
                ? throw new InvalidOperationException("No configured HTTP response remains.")
                : Task.FromResult(_handlers.Dequeue().Invoke(request));
    }
}