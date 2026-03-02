using System.Diagnostics.Metrics;

namespace AStar.Dev.OneDrive.Sync.Client.UI;

/// <summary>
/// Provides application metrics for monitoring and observability.
/// </summary>
public static class Metrics
{
    /// <summary>
    /// Gets the main meter for the application.
    /// </summary>
    public static readonly Meter Meter = new(typeof(Metrics).Assembly.GetName().Name!);

    /// <summary>
    /// Gets the counter for tracking loaded tree nodes.
    /// </summary>
    public static readonly Counter<int> NodesLoaded = Meter.CreateCounter<int>("treeview.nodes_loaded");
}
