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
    /// Gets or sets the pooled connection lifetime used to refresh DNS endpoints.
    /// </summary>
    public TimeSpan PooledConnectionLifetime { get; init; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Gets or sets the request timeout for outgoing Graph HTTP calls.
    /// </summary>
    public TimeSpan RequestTimeout { get; init; } = TimeSpan.FromSeconds(30);
}