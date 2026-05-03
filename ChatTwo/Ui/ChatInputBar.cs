using System;
using ChatTwo.Code;
using Dalamud.Bindings.ImGui;

namespace ChatTwo.Ui;

// Hellion Chat — v0.6.0 input bar component for pop-out windows.
//
// Pragmatischer Refactor-Scope: Render() ist ein leerer Marker-Stub für
// das Hauptfenster — der bestehende Input-Layer in ChatLogWindow bleibt
// unangetastet, weil ein 400-Zeilen-Extract aus einem 1926-Zeilen-File
// das v0.6.0-Risiko unverhältnismäßig erhöhen würde. Pop-Outs nutzen
// ausschließlich RenderCompact(), das ist der ganze v0.6.0-Mehrwert.
// Sollte das Hauptfenster selber später eine Compact-Variante brauchen
// (oder das große Extract sich aus anderem Grund lohnen), kann Render()
// in einem späteren Cycle gefüllt werden.
public sealed class ChatInputBar
{
    private readonly Plugin _plugin;
    private readonly ChatLogWindow _host;
    private readonly Func<Tab?> _activeTabAccessor;
    private readonly InputState _state = new();

    public ChatInputBar(Plugin plugin, ChatLogWindow host, Func<Tab?> activeTabAccessor)
    {
        _plugin = plugin;
        _host = host;
        _activeTabAccessor = activeTabAccessor;
    }

    public InputState State => _state;
    public bool IsFocused { get; private set; }

    // Stub. v0.6.0 belässt den Hauptfenster-Input wie er ist.
    public void Render()
    {
    }

    // Compact rendering for pop-out windows — implemented in Task 21–23.
    public void RenderCompact()
    {
        ImGui.TextDisabled("[ChatInputBar Compact — Stub]");
    }

    // Forwards a tab-cycle keybind delta to the host so all windows
    // navigate the same active-tab pointer (single source of truth).
    public void HandleKeybindForward(int delta)
    {
        _host.ChangeTabDelta(delta);
    }
}

// Per-window input state. Each ChatInputBar instance owns one of these
// so pop-outs and the main window keep independent buffers and channels
// (State-Sync-Entscheidung A in the v0.6.0 spec).
public sealed class InputState
{
    public string Buffer = string.Empty;
    public InputChannel? Channel;
    public int HistoryCursor = -1;
}
