using System.Collections.Generic;

namespace ChatTwo;

// Hellion Chat — v0.6.0 shared input history. Replaces the embedded
// ChatLogWindow.InputBacklog so that pop-out windows with their own
// ChatInputBar can navigate the same Up/Down history as the main window.
// Newest entry at index 0; consecutive duplicates are collapsed.
public static class InputHistoryService
{
    private const int MaxSize = 30;
    private static readonly List<string> _entries = new();

    public static IReadOnlyList<string> Entries => _entries;

    public static void Push(string entry)
    {
        if (string.IsNullOrWhiteSpace(entry))
            return;

        var trimmed = entry.Trim();

        // Drop consecutive duplicates so spamming the same line does not
        // pollute the history with repeats.
        if (_entries.Count > 0 && _entries[0] == trimmed)
            return;

        _entries.Insert(0, trimmed);
        if (_entries.Count > MaxSize)
            _entries.RemoveAt(_entries.Count - 1);
    }

    public static string? GetByCursor(int cursor)
    {
        if (cursor < 0 || cursor >= _entries.Count)
            return null;
        return _entries[cursor];
    }

    public static int Count => _entries.Count;
}
