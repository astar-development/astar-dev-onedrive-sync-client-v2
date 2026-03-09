using System.Diagnostics.Metrics;
using System.Collections.Concurrent;

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

    public static readonly Counter<double> QueueDepth = Meter.CreateCounter<double>("sync.queue.depth");
    public static readonly Counter<double> DurationMs = Meter.CreateCounter<double>("sync.duration.ms");
    public static readonly Counter<double> Throughput = Meter.CreateCounter<double>("sync.throughput");
    public static readonly Counter<double> Retries = Meter.CreateCounter<double>("sync.retries");
    public static readonly Counter<double> Conflicts = Meter.CreateCounter<double>("sync.conflicts");
    private static readonly ConcurrentDictionary<string, Counter<double>> CustomCounters = new(StringComparer.Ordinal);

    public static void Record(string metricName, double value, string correlationId)
    {
        switch(metricName)
        {
            case "queue.depth.upload":
            case "queue.depth.download":
                QueueDepth.Add(value, new KeyValuePair<string, object?>("correlation_id", correlationId), new KeyValuePair<string, object?>("metric", metricName));
                return;
            case "duration.upload.ms":
            case "duration.download.ms":
                DurationMs.Add(value, new KeyValuePair<string, object?>("correlation_id", correlationId), new KeyValuePair<string, object?>("metric", metricName));
                return;
            case "throughput.upload":
            case "throughput.download":
                Throughput.Add(value, new KeyValuePair<string, object?>("correlation_id", correlationId), new KeyValuePair<string, object?>("metric", metricName));
                return;
            case "retry.count":
                Retries.Add(value, new KeyValuePair<string, object?>("correlation_id", correlationId));
                return;
            case "conflict.count":
                Conflicts.Add(value, new KeyValuePair<string, object?>("correlation_id", correlationId));
                return;
            default:
                Counter<double> counter = CustomCounters.GetOrAdd(metricName, name => Meter.CreateCounter<double>($"sync.{name}"));
                counter.Add(value, new KeyValuePair<string, object?>("correlation_id", correlationId));
                return;
        }
    }
}
