using System;

namespace HellionChat._Helpers;

// Pure-helper mirror of the compact pop-out history-navigation cursor
// math. The original CompactCallback was tangled with ImGuiInputTextCallbackData
// (DeleteChars/InsertChars), which can't be exercised in xUnit. The
// ImGui buffer mutation stays at the call site; only the deterministic
// cursor-and-replacement decision lives here.
//
// Index semantics match InputHistoryService:
//   index 0          = oldest entry
//   index Count - 1  = newest entry
//   cursor == -1     = "not browsing history"
//
// TEST-MIRROR: ../../../Hellion Build test/Ui/CompactInputHistoryNavigatorTests.cs
public static class CompactInputHistoryNavigator
{
    public enum Direction { Up, Down }

    // replacement == null means: caller must NOT touch the buffer. This
    // distinguishes "cursor unchanged, leave the user's typing alone"
    // from "cursor moved to an empty slot, clear the buffer".
    public static (int cursor, string? replacement) Navigate(
        Direction direction,
        int currentCursor,
        string currentBuffer,
        Func<int> getCount,
        Action<string> push,
        Func<int, string?> getByCursor)
    {
        ArgumentNullException.ThrowIfNull(getCount);
        ArgumentNullException.ThrowIfNull(push);
        ArgumentNullException.ThrowIfNull(getByCursor);

        var prev = currentCursor;
        var next = currentCursor;

        switch (direction)
        {
            case Direction.Up:
                if (currentCursor == -1)
                {
                    // First Up press from a fresh buffer: stash whatever
                    // the user typed so they can recover it after browsing.
                    var offset = 0;
                    if (!string.IsNullOrWhiteSpace(currentBuffer))
                    {
                        push(currentBuffer);
                        offset = 1;
                    }
                    next = getCount() - 1 - offset;
                }
                else if (currentCursor > 0)
                {
                    next--;
                }
                break;
            case Direction.Down:
                if (currentCursor != -1)
                {
                    next++;
                    if (next >= getCount())
                        next = -1;
                }
                break;
        }

        if (prev == next)
            return (next, null);

        var replacement = getByCursor(next) ?? string.Empty;
        return (next, replacement);
    }
}
