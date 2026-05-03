using System.Collections.Generic;

namespace HellionChat;

// Hellion Chat — v0.6.0 shared input history. Replaces the embedded
// ChatLogWindow.InputBacklog so that pop-out windows with their own
// ChatInputBar can navigate the same Up/Down history as the main window.
// Index semantics are kept identical to the v0.5.x InputBacklog:
//   index 0          = oldest entry
//   index Count - 1  = newest entry
// Push performs move-to-newest deduplication: existing entries are
// removed before the new one is appended at the end.
public static class InputHistoryService
{
    private const int MaxSize = 30;
    private static readonly List<string> _entries = new();

    public static IReadOnlyList<string> Entries => _entries;

    public static int Count => _entries.Count;

    public static void Push(string entry)
    {
        if (string.IsNullOrWhiteSpace(entry))
            return;

        var trimmed = entry.Trim();

        // Move-to-newest: existing entries are removed before the append
        // so the same line typed twice does not occupy two history slots.
        for (var i = 0; i < _entries.Count; i++)
        {
            if (_entries[i] == trimmed)
            {
                _entries.RemoveAt(i);
                break;
            }
        }

        _entries.Add(trimmed);
        if (_entries.Count > MaxSize)
            _entries.RemoveAt(0);
    }

    public static string? GetByCursor(int cursor)
    {
        if (cursor < 0 || cursor >= _entries.Count)
            return null;
        return _entries[cursor];
    }
}
