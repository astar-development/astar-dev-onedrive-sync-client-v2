using System.Net;

namespace AStar.Dev.OneDrive.Sync.Client.Infrastructure.Graph;

/// <summary>
/// Represents the response payload from a OneDrive Graph request.
/// </summary>
/// <param name="StatusCode">The HTTP status code.</param>
/// <param name="Content">The response body as text.</param>
public sealed record OneDriveGraphResponse(HttpStatusCode StatusCode, string Content);
