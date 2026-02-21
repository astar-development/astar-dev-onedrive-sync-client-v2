using System.Diagnostics;
using Serilog;

namespace AstarOneDrive.UI;

public class SerilogTraceListener : TraceListener
{
    public override void Write(string? message)
    {
        Log.Debug("[Avalonia] {Message}", message);
    }

    public override void WriteLine(string? message)
    {
        Log.Debug("[Avalonia] {Message}", message);
    }
}
