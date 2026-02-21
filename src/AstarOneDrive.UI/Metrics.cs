using System.Diagnostics.Metrics;

namespace AstarOneDrive.UI;

public static class Metrics
{
    public static readonly Meter Meter = new("MyApp");

    public static readonly Counter<int> NodesLoaded = Meter.CreateCounter<int>("treeview.nodes_loaded");
}
