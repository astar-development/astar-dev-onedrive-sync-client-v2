using System.Net;

namespace AStar.Dev.OneDrive.Sync.Client.Infrastructure.Graph;

/// <summary>
/// Represents a typed sync error returned from OneDrive Graph operations.
/// </summary>
/// <param name="Kind">The error category.</param>
/// <param name="Message">A human-readable error message.</param>
/// <param name="IsTransient">Indicates whether retrying might succeed.</param>
/// <param name="StatusCode">The HTTP status code, if available.</param>
public sealed record SyncError(SyncErrorKind Kind, string Message, bool IsTransient, HttpStatusCode? StatusCode = null);