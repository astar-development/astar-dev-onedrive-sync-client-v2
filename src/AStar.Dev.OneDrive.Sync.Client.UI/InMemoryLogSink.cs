using Serilog.Core;
using Serilog.Events;

namespace AStar.Dev.OneDrive.Sync.Client.UI;

public class InMemoryLogSink(int maxLines = 200) : ILogEventSink
{
    private readonly Lock _syncLock = new();
    private readonly int _maxLines = maxLines;
    private readonly List<string> _lines = [];

    public event Action? Updated;

    public void Emit(LogEvent logEvent)
    {
        var line = logEvent.RenderMessage();

        lock(_syncLock)
        {
            _lines.Add(line);
            if(_lines.Count > _maxLines)
                _lines.RemoveAt(0);
        }

        Updated?.Invoke();
    }

    public string GetText()
    {
        lock(_syncLock)
            return string.Join(Environment.NewLine, _lines);
    }
}

