using System.Net;
using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Graph;

namespace AStar.Dev.OneDrive.Sync.Client.Infrastructure.Tests.Graph;

public sealed class OneDriveGraphClientShould
{
    [Fact]
    public async Task ReturnSuccessResponse()
    {
        SequenceHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("ok") });
        RecordingGraphTelemetry telemetry = new();
        using DisposableOneDriveGraphClient client = CreateClient(handler, telemetry);

        Result<OneDriveGraphResponse, SyncError> result = await client.SendAsync(
            OneDriveGraphRequest.Get("/me/drive/root", "token"),
            TestContext.Current.CancellationToken);

        result.Match(ok => ok.Content, error => error.Message).ShouldBe("ok");
        telemetry.Events.Count.ShouldBe(1);
        telemetry.Events[0].RetryCount.ShouldBe(0);
        telemetry.Events[0].ErrorKind.ShouldBeNull();
    }

    [Fact]
    public async Task MapNotFoundToTypedErrorWithoutRetry()
    {
        SequenceHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
        RecordingGraphTelemetry telemetry = new();
        using DisposableOneDriveGraphClient client = CreateClient(handler, telemetry);

        Result<OneDriveGraphResponse, SyncError> result = await client.SendAsync(
            OneDriveGraphRequest.Get("/me/drive/items/missing", "token"),
            TestContext.Current.CancellationToken);

        result.Match(_ => false, error => error.Kind == SyncErrorKind.NotFound && !error.IsTransient).ShouldBeTrue();
        telemetry.Events.Single().RetryCount.ShouldBe(0);
        telemetry.Events.Single().ErrorKind.ShouldBe(SyncErrorKind.NotFound);
    }

    [Fact]
    public async Task MapNetworkFailureToTypedError()
    {
        SequenceHttpMessageHandler handler = new(_ => throw new HttpRequestException("offline"));
        RecordingGraphTelemetry telemetry = new();
        using DisposableOneDriveGraphClient client = CreateClient(handler, telemetry);

        Result<OneDriveGraphResponse, SyncError> result = await client.SendAsync(
            OneDriveGraphRequest.Get("/me/drive/root", "token"),
            TestContext.Current.CancellationToken);

        result.Match(_ => false, error => error.Kind == SyncErrorKind.Network && error.IsTransient).ShouldBeTrue();
        telemetry.Events.Single().RetryCount.ShouldBe(0);
    }

    private static DisposableOneDriveGraphClient CreateClient(HttpMessageHandler handler, IOneDriveGraphTelemetry telemetry)
    {
        HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://graph.microsoft.com/v1.0/") };
        return new DisposableOneDriveGraphClient(httpClient, telemetry);
    }

    private sealed class RecordingGraphTelemetry : IOneDriveGraphTelemetry
    {
        public List<GraphRequestTelemetryEvent> Events { get; } = [];

        public void Track(GraphRequestTelemetryEvent telemetryEvent) => Events.Add(telemetryEvent);
    }

    private sealed class DisposableOneDriveGraphClient : OneDriveGraphClient, IDisposable
    {
        private readonly HttpClient _httpClient;

        public DisposableOneDriveGraphClient(HttpClient httpClient, IOneDriveGraphTelemetry telemetry)
            : base(httpClient, telemetry) => _httpClient = httpClient;

        public void Dispose() => _httpClient.Dispose();
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
