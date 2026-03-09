using Serilog.Core;
using Serilog.Events;
using System.Text.RegularExpressions;

namespace AStar.Dev.OneDrive.Sync.Client.UI;

/// <summary>
/// An in-memory Serilog sink that stores recent log messages for display in debug windows.
/// </summary>
public class InMemoryLogSink(int maxLines = 200) : ILogEventSink
{
    private readonly Lock _syncLock = new();
    private readonly int _maxLines = maxLines;
    private readonly List<string> _lines = [];
    private static readonly Regex EmailPattern = new(@"\b[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex TokenPattern = new(@"(?i)(token\s*[=:]\s*)([^\s,;]+)", RegexOptions.Compiled);

    /// <summary>
    /// Occurs when new log events are added.
    /// </summary>
    public event Action? Updated;

    /// <inheritdoc />
    public void Emit(LogEvent logEvent)
    {
        var line = RedactSensitiveData(logEvent.RenderMessage());

        lock(_syncLock)
        {
            _lines.Add(line);
            if(_lines.Count > _maxLines)
                _lines.RemoveAt(0);
        }

        Updated?.Invoke();
    }

    /// <summary>
    /// Retrieves all stored log messages as a single text string.
    /// </summary>
    /// <returns>The concatenated log messages.</returns>
    public string GetText()
    {
        lock(_syncLock)
            return string.Join(Environment.NewLine, _lines);
    }

    public static string RedactSensitiveData(string text)
    {
        if(string.IsNullOrWhiteSpace(text))
        {
            return text;
        }

        var redacted = EmailPattern.Replace(text, "[REDACTED_EMAIL]");
        redacted = TokenPattern.Replace(redacted, "$1[REDACTED_TOKEN]");
        return redacted;
    }
}

