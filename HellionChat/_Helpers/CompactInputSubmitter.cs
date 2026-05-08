using System;
using HellionChat.Ui;

namespace HellionChat._Helpers;

// Pure-helper mirror of the compact pop-out submit flow. ChatInputBar's
// SubmitCompact used to inline this against a sealed ChatLogWindow, which
// blocks Moq-based isolation. Lifting the deterministic part into a POCO
// keeps the production call site a one-liner while letting xUnit assert
// the buffer/cursor reset and the sender contract directly.
// TEST-MIRROR: ../../../Hellion Build test/Ui/CompactInputSubmitterTests.cs
public static class CompactInputSubmitter
{
    public static bool TrySubmit(InputState state, Tab tab, Action<Tab, string> sender)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(tab);
        ArgumentNullException.ThrowIfNull(sender);

        if (string.IsNullOrWhiteSpace(state.Buffer))
            return false;

        var text = state.Buffer;
        state.Buffer = string.Empty;
        state.HistoryCursor = -1;
        sender(tab, text);
        return true;
    }
}
