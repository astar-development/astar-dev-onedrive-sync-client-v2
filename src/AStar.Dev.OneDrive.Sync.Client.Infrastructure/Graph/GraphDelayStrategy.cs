namespace AStar.Dev.OneDrive.Sync.Client.Infrastructure.Graph;

/// <summary>
/// Uses <see cref="Task.Delay(TimeSpan, CancellationToken)"/> for retry delays.
/// </summary>
public sealed class GraphDelayStrategy : IGraphDelayStrategy
{
    /// <inheritdoc />
    public Task DelayAsync(TimeSpan duration, CancellationToken cancellationToken)
        => Task.Delay(duration, cancellationToken);
}