using System.Diagnostics.Metrics;

namespace AStar.Dev.OneDrive.Sync.Client.UI;

public static class Metrics
{
    public static readonly Meter Meter = new("MyApp");

    public static readonly Counter<int> NodesLoaded = Meter.CreateCounter<int>("treeview.nodes_loaded");
}
