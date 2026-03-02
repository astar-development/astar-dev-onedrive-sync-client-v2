using System.Diagnostics;
using Serilog;

namespace AStar.Dev.OneDrive.Sync.Client.UI;

/// <summary>
/// A trace listener that forwards trace messages to Serilog.
/// </summary>
public class SerilogTraceListener : TraceListener
{
    /// <inheritdoc />
    public override void Write(string? message) => Log.Debug("[Avalonia] {Message}", message);

    /// <inheritdoc />
    public override void WriteLine(string? message) => Log.Debug("[Avalonia] {Message}", message);
}
