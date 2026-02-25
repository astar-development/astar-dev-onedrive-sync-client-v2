using Serilog.Core;
using Serilog.Events;

namespace AStar.Dev.OneDrive.Sync.Client.UI;

public class InMemoryLogSink : ILogEventSink
{
    private readonly Lock _lock = new();
    private readonly int _maxLines;
    private readonly List<string> _lines = new();

    public event Action? Updated;

    public InMemoryLogSink(int maxLines = 200) => _maxLines = maxLines;

    public void Emit(LogEvent logEvent)
    {
        var line = logEvent.RenderMessage();

        lock(_lock)
        {
            _lines.Add(line);
            if(_lines.Count > _maxLines)
                _lines.RemoveAt(0);
        }

        Updated?.Invoke();
    }

    public string GetText()
    {
        lock(_lock)
            return string.Join(Environment.NewLine, _lines);
    }
}

