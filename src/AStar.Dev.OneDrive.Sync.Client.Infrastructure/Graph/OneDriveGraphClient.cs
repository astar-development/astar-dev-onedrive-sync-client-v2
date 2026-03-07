using System.Diagnostics;
using System.Net;
using AStar.Dev.Functional.Extensions;

namespace AStar.Dev.OneDrive.Sync.Client.Infrastructure.Graph;

/// <summary>
/// Provides resilient access to OneDrive Graph API endpoints.
/// </summary>
public sealed class OneDriveGraphClient(HttpClient httpClient, IOneDriveGraphTelemetry telemetry) : IOneDriveGraphClient
{
    /// <inheritdoc />
    public async Task<Result<OneDriveGraphResponse, SyncError>> SendAsync(OneDriveGraphRequest request, CancellationToken cancellationToken = default)
    {
        var startTime = Stopwatch.GetTimestamp();
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            using HttpRequestMessage message = request.BuildHttpRequest();
            using HttpResponseMessage response = await httpClient.SendAsync(message, cancellationToken);
            if(response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                TrackTelemetry(request, Stopwatch.GetElapsedTime(startTime), null, (int)response.StatusCode);
                return new OneDriveGraphResponse(response.StatusCode, content);
            }

            SyncError error = MapHttpError(response.StatusCode);
            TrackTelemetry(request, Stopwatch.GetElapsedTime(startTime), error.Kind, (int)response.StatusCode);
            return error;
        }
        catch(HttpRequestException exception)
        {
            var error = new SyncError(SyncErrorKind.Network, exception.Message, true);
            TrackTelemetry(request, Stopwatch.GetElapsedTime(startTime), error.Kind, null);
            return error;
        }
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
        SyncErrorKind? errorKind,
        int? statusCode)
        => telemetry.Track(new GraphRequestTelemetryEvent(
            request.Method.Method,
            request.Path,
            duration,
            0,
            errorKind,
            statusCode));
}