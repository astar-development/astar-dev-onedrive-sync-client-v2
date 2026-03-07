using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using AStar.Dev.Functional.Extensions;

namespace AStar.Dev.OneDrive.Sync.Client.Infrastructure.Graph;

/// <summary>
/// Provides resilient access to OneDrive Graph API endpoints.
/// </summary>
public sealed class OneDriveGraphClient(HttpClient httpClient, OneDriveGraphClientOptions options, IGraphDelayStrategy delayStrategy, IOneDriveGraphTelemetry telemetry) : IOneDriveGraphClient
{
    /// <inheritdoc />
    public async Task<Result<OneDriveGraphResponse, SyncError>> SendAsync(OneDriveGraphRequest request, CancellationToken cancellationToken = default)
    {
        var startTime = Stopwatch.GetTimestamp();
        var retries = 0;

        while(true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                using HttpRequestMessage message = request.BuildHttpRequest(options.BaseUri);
                using HttpResponseMessage response = await httpClient.SendAsync(message, cancellationToken);
                if(response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(cancellationToken);
                    TrackTelemetry(request, Stopwatch.GetElapsedTime(startTime), retries, null, (int)response.StatusCode);
                    return new OneDriveGraphResponse(response.StatusCode, content);
                }

                SyncError error = MapHttpError(response.StatusCode);
                if(ShouldRetry(error, retries))
                {
                    await DelayBeforeRetryAsync(response.Headers.RetryAfter, retries, cancellationToken);
                    retries++;
                    continue;
                }

                TrackTelemetry(request, Stopwatch.GetElapsedTime(startTime), retries, error.Kind, (int)response.StatusCode);
                return error;
            }
            catch(HttpRequestException exception)
            {
                var error = new SyncError(SyncErrorKind.Network, exception.Message, true);
                if(ShouldRetry(error, retries))
                {
                    await DelayBeforeRetryAsync(null, retries, cancellationToken);
                    retries++;
                    continue;
                }

                TrackTelemetry(request, Stopwatch.GetElapsedTime(startTime), retries, error.Kind, null);
                return error;
            }
        }
    }

    private bool ShouldRetry(SyncError error, int retries)
        => error.IsTransient && retries < options.MaximumRetryAttempts;

    private async Task DelayBeforeRetryAsync(RetryConditionHeaderValue? retryAfter, int retries, CancellationToken cancellationToken)
    {
        if(retryAfter?.Delta is TimeSpan delta)
        {
            await delayStrategy.DelayAsync(delta, cancellationToken);
            return;
        }

        var factor = Math.Pow(2, retries);
        var duration = TimeSpan.FromMilliseconds(options.InitialBackoff.TotalMilliseconds * factor);
        await delayStrategy.DelayAsync(duration, cancellationToken);
    }

    private static SyncError MapHttpError(HttpStatusCode statusCode)
        => statusCode switch
        {
            HttpStatusCode.Unauthorized => new SyncError(SyncErrorKind.Authentication, "Authentication failed.", false, statusCode),
            HttpStatusCode.Forbidden => new SyncError(SyncErrorKind.Authentication, "Access to resource denied.", false, statusCode),
            HttpStatusCode.NotFound => new SyncError(SyncErrorKind.NotFound, "Resource not found.", false, statusCode),
            HttpStatusCode.Conflict => new SyncError(SyncErrorKind.Conflict, "Conflict detected.", false, statusCode),
            HttpStatusCode.RequestTimeout => new SyncError(SyncErrorKind.Network, "Request timed out.", true, statusCode),
            (HttpStatusCode)429 => new SyncError(SyncErrorKind.Throttled, "Request throttled.", true, statusCode),
            >= HttpStatusCode.InternalServerError => new SyncError(SyncErrorKind.Network, "Remote service temporary failure.", true, statusCode),
            _ => new SyncError(SyncErrorKind.Api, "Graph API request failed.", false, statusCode)
        };

    private void TrackTelemetry(
        OneDriveGraphRequest request,
        TimeSpan duration,
        int retries,
        SyncErrorKind? errorKind,
        int? statusCode)
        => telemetry.Track(new GraphRequestTelemetryEvent(
            request.Method.Method,
            request.Path,
            duration,
            retries,
            errorKind,
            statusCode));
}