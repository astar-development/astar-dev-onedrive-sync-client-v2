namespace AStar.Dev.OneDrive.Sync.Client.Infrastructure.Graph;

/// <summary>
/// Configures OneDrive Graph client retry behavior and base endpoint.
/// </summary>
public sealed class OneDriveGraphClientOptions
{
    /// <summary>
    /// Gets or sets the Graph API base URI.
    /// </summary>
    public Uri BaseUri { get; init; } = new("https://graph.microsoft.com/v1.0/");

    /// <summary>
    /// Gets or sets the maximum number of retries after the initial request.
    /// </summary>
    public int MaximumRetryAttempts { get; init; } = 3;

    /// <summary>
    /// Gets or sets the initial backoff duration used for exponential retry delays.
    /// </summary>
    public TimeSpan InitialBackoff { get; init; } = TimeSpan.FromMilliseconds(200);
}