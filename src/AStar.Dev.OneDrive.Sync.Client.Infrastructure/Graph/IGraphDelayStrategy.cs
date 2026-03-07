namespace AStar.Dev.OneDrive.Sync.Client.Infrastructure.Graph;

/// <summary>
/// Provides delay behavior used by Graph retry handling.
/// </summary>
public interface IGraphDelayStrategy
{
    /// <summary>
    /// Delays execution for the specified duration.
    /// </summary>
    Task DelayAsync(TimeSpan duration, CancellationToken cancellationToken);
}