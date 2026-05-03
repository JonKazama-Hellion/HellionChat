using System;
using System.Numerics;
using ChatTwo.Code;
using ChatTwo.Util;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;

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

    // Compact rendering for pop-out windows.
    //
    // Layout A (klassisch): Channel-Icon-Button links (Background-Farbe
    // aus ChatColours), Text-Input mittig, Auto-Translate-Icon-Button
    // rechts. Channel-Switch wirkt via Plugin.Functions.Chat global (das
    // ist eine FFXIV-API-Eigenschaft, kein Bug). Pro Pop-Out unabhängig
    // bleibt aber der Text-Buffer und der History-Cursor.
    public void RenderCompact()
    {
        var tab = _activeTabAccessor();
        if (tab == null)
            return;

        DrawChannelIconButton(tab);
        ImGui.SameLine();
        DrawCompactInput(tab);
        ImGui.SameLine();
        // Task 23 ergänzt hier den Auto-Translate-Icon-Button.
        ImGui.TextDisabled("[at]");
    }

    private void DrawCompactInput(Tab tab)
    {
        // Reserve room for the auto-translate icon button on the right.
        const float reservedRightWidth = 32f;
        var inputWidth = ImGui.GetContentRegionAvail().X - reservedRightWidth;
        if (inputWidth < 60f)
            inputWidth = 60f;

        ImGui.SetNextItemWidth(inputWidth);

        // CallbackHistory wires up Up/Down navigation against the shared
        // InputHistoryService. Submit is detected the same way the main
        // window does it: via IsItemDeactivated + Enter, NOT EnterReturnsTrue
        // (matching v0.5.x ChatLogWindow.cs behavior).
        const ImGuiInputTextFlags flags = ImGuiInputTextFlags.CallbackHistory;
        ImGui.InputText($"##chat-compact-input-{tab.Identifier}", ref _state.Buffer, 500, flags, CompactCallback);

        IsFocused = ImGui.IsItemActive();

        if (ImGui.IsItemDeactivated()
            && (ImGui.IsKeyDown(ImGuiKey.Enter) || ImGui.IsKeyDown(ImGuiKey.KeypadEnter)))
        {
            SubmitCompact(tab);
        }
    }

    private void SubmitCompact(Tab tab)
    {
        if (string.IsNullOrWhiteSpace(_state.Buffer))
            return;

        var text = _state.Buffer;
        _state.Buffer = string.Empty;
        _state.HistoryCursor = -1;
        _host.SendChatBoxFromExternal(tab, text);
    }

    // History-navigation callback for the compact input. Mirrors the main
    // window's logic but operates on _state.HistoryCursor and the shared
    // InputHistoryService. Index semantics match v0.5.x InputBacklog:
    // 0 = oldest, Count-1 = newest.
    private unsafe int CompactCallback(scoped ref ImGuiInputTextCallbackData data)
    {
        if (data.EventFlag != ImGuiInputTextFlags.CallbackHistory)
            return 0;

        var prev = _state.HistoryCursor;
        switch (data.EventKey)
        {
            case ImGuiKey.UpArrow:
                switch (_state.HistoryCursor)
                {
                    case -1:
                        var offset = 0;
                        if (!string.IsNullOrWhiteSpace(_state.Buffer))
                        {
                            InputHistoryService.Push(_state.Buffer);
                            offset = 1;
                        }
                        _state.HistoryCursor = InputHistoryService.Count - 1 - offset;
                        break;
                    case > 0:
                        _state.HistoryCursor--;
                        break;
                }
                break;
            case ImGuiKey.DownArrow:
                if (_state.HistoryCursor != -1)
                    if (++_state.HistoryCursor >= InputHistoryService.Count)
                        _state.HistoryCursor = -1;
                break;
        }

        if (prev == _state.HistoryCursor)
            return 0;

        var historyStr = InputHistoryService.GetByCursor(_state.HistoryCursor) ?? string.Empty;
        data.DeleteChars(0, data.BufTextLen);
        data.InsertChars(0, historyStr);

        return 0;
    }

    private void DrawChannelIconButton(Tab tab)
    {
        var inputType = tab.CurrentChannel.UseTempChannel
            ? tab.CurrentChannel.TempChannel.ToChatType()
            : tab.CurrentChannel.Channel.ToChatType();

        var rgba = Plugin.Config.ChatColours.TryGetValue(inputType, out var c)
            ? c
            : (inputType.DefaultColor() ?? 0xFFFFFFFFu);
        var v3 = ColourUtil.RgbaToVector3(rgba);
        var bg = new Vector4(v3.X, v3.Y, v3.Z, 1f);

        // Compute readable foreground — black on bright, white on dark
        var luminance = 0.2126f * v3.X + 0.7152f * v3.Y + 0.0722f * v3.Z;
        var fg = luminance > 0.55f ? new Vector4(0f, 0f, 0f, 1f) : new Vector4(1f, 1f, 1f, 1f);

        const string popupId = "chat-channel-picker-compact";
        const float buttonSize = 22f;

        using (ImRaii.PushColor(ImGuiCol.Button, bg))
        using (ImRaii.PushColor(ImGuiCol.ButtonHovered, bg))
        using (ImRaii.PushColor(ImGuiCol.ButtonActive, bg))
        using (ImRaii.PushColor(ImGuiCol.Text, fg))
        {
            // Single-letter glyph derived from the channel — quick visual cue
            // until we have a proper icon font available in the compact bar.
            var label = ChannelGlyph(inputType);
            if (ImGui.Button($"{label}##chan-compact", new Vector2(buttonSize, buttonSize)) && tab.Channel is null)
                ImGui.OpenPopup(popupId);
        }

        if (tab.Channel is not null && ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(Resources.Language.ChatLog_SwitcherDisabled);
        }
        else if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(inputType.Name());
        }

        using (var popup = ImRaii.Popup(popupId))
        {
            if (popup)
            {
                var channels = _host.GetValidChannels();
                foreach (var (name, channel) in channels)
                    if (ImGui.Selectable(name))
                        _host.SetChannel(channel);
            }
        }
    }

    private static string ChannelGlyph(ChatType type) => type switch
    {
        ChatType.Say => "S",
        ChatType.Yell => "Y",
        ChatType.Shout => "!",
        ChatType.TellIncoming or ChatType.TellOutgoing => "T",
        ChatType.Party or ChatType.CrossParty => "P",
        ChatType.Alliance => "A",
        ChatType.FreeCompany => "F",
        ChatType.NoviceNetwork => "N",
        ChatType.Linkshell1 => "1",
        ChatType.Linkshell2 => "2",
        ChatType.Linkshell3 => "3",
        ChatType.Linkshell4 => "4",
        ChatType.Linkshell5 => "5",
        ChatType.Linkshell6 => "6",
        ChatType.Linkshell7 => "7",
        ChatType.Linkshell8 => "8",
        ChatType.CrossLinkshell1 => "①",
        ChatType.CrossLinkshell2 => "②",
        ChatType.CrossLinkshell3 => "③",
        ChatType.CrossLinkshell4 => "④",
        ChatType.CrossLinkshell5 => "⑤",
        ChatType.CrossLinkshell6 => "⑥",
        ChatType.CrossLinkshell7 => "⑦",
        ChatType.CrossLinkshell8 => "⑧",
        _ => "?",
    };

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
