using System.Globalization;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using HellionChat.Code;
using HellionChat.Resources;
using HellionChat.Util;

namespace HellionChat.Ui;

/// <summary>
/// Bottom-Status-Bar (v1.2.0). Fix 22 px hoch, BorderTop als Trenner.
/// Slots links → rechts: Channel-Indicator (Color-Dot + Channel-Name),
/// Privacy-Badge (Lock-Icon + Privacy-Label), Counts (Tabs + Msgs),
/// Tells (Auto-Tell-Counter, hidden bei 0), Version (rechtsbündig, muted).
///
/// Update-Frequenz: 1×/Sekunde. Format-Strings werden zwischen Updates
/// gecached, damit kein Per-Frame-Format-Allocation entsteht.
/// </summary>
internal sealed class StatusBar
{
    public const float Height = 22f;
    private const long UpdateIntervalMs = 1000;

    // Cache-State — initial outdated, damit der erste Frame frisch berechnet.
    private long _lastUpdateMs = -UpdateIntervalMs;
    private string _cachedCountsText = string.Empty;
    private string _cachedTellsText = string.Empty;

    /// <summary>
    /// Reine String-Logik — testbar ohne ImGui-Init.
    /// </summary>
    public static string FormatCounts(int tabs, int messages)
    {
        // InvariantCulture: User-System-Locale darf das Format nicht
        // verändern (de_DE würde sonst "1,2k" statt "1.2k" liefern).
        var msgPart = messages >= 1000
            ? string.Format(CultureInfo.InvariantCulture, "{0:0.0}k msg", messages / 1000.0)
            : $"{messages} msg";
        var tabsPart = $"{tabs} {(tabs == 1 ? "tab" : "tabs")}";
        return $"{tabsPart} · {msgPart}";
    }

    /// <summary>
    /// Reine String-Logik — testbar ohne ImGui-Init.
    /// 0 Tells → Leerstring (Slot wird ausgeblendet).
    /// </summary>
    public static string FormatTells(int count)
    {
        if (count <= 0) return string.Empty;
        return $"{count} {(count == 1 ? "tell" : "tells")}";
    }

    /// <summary>
    /// Test-Hook: Cache-Logic ohne reale Time-Source verifizieren.
    /// Nicht für Production-Render.
    /// </summary>
    internal (string counts, string tells) SnapshotForTest(long now, int tabs, int messages, int tells)
    {
        UpdateCacheIfDue(now, tabs, messages, tells);
        return (_cachedCountsText, _cachedTellsText);
    }

    private void UpdateCacheIfDue(long now, int tabs, int messages, int tells)
    {
        if (now - _lastUpdateMs < UpdateIntervalMs)
            return;
        _cachedCountsText = FormatCounts(tabs, messages);
        _cachedTellsText = FormatTells(tells);
        _lastUpdateMs = now;
    }

    /// <summary>
    /// Render-Pfad. Aufrufer pusht bereits den HellionStyle/Theme;
    /// wir lesen nur die aktiven Theme-Farben und zeichnen.
    /// </summary>
    public void Draw(Plugin plugin)
    {
        var theme = plugin.ThemeRegistry.Active;
        var now = Environment.TickCount64;

        // Counts pro Frame berechnen ist günstig (List<>.Count, kleine
        // Sums); Format-String wird gecached.
        var tabs = Plugin.Config.Tabs.Count;
        var messages = Plugin.Config.Tabs.Sum(t => t.Messages.Count);
        var tells = Plugin.Config.Tabs.Count(t => t.IsTempTab);
        UpdateCacheIfDue(now, tabs, messages, tells);

        // BorderTop als Trenner — DrawList-Line, ImGui-Separator hat zu viel Padding.
        var cursorY = ImGui.GetCursorScreenPos().Y;
        var winLeft = ImGui.GetWindowPos().X;
        var winRight = winLeft + ImGui.GetWindowSize().X;
        ImGui.GetWindowDrawList().AddLine(
            new Vector2(winLeft, cursorY),
            new Vector2(winRight, cursorY),
            ColourUtil.RgbaToAbgr(theme.Colors.Border),
            1f);

        ImGui.Dummy(new Vector2(0, 2)); // BorderTop-Spacing

        // Slot 1: Active-Channel-Indicator
        var inputCh = plugin.CurrentTab?.CurrentChannel?.Channel ?? InputChannel.Invalid;
        var hasChannel = inputCh != InputChannel.Invalid;
        var chatType = inputCh.ToChatType();
        var channelName = hasChannel ? chatType.Name() : "—";
        var channelColor = hasChannel
            ? (plugin.Functions.Chat.GetChannelColor(chatType) ?? theme.Colors.TextMuted)
            : theme.Colors.TextMuted;
        DrawDot(channelColor);
        ImGui.SameLine();
        ImGui.TextUnformatted(channelName);

        // Slot 2: Privacy-Badge — abgeleitet aus PrivacyFilterEnabled.
        ImGui.SameLine();
        DrawSeparator();
        ImGui.SameLine();
        using (plugin.FontManager.FontAwesome.Push())
        {
            ImGui.TextUnformatted(FontAwesomeIcon.Lock.ToIconString());
        }
        ImGui.SameLine();
        var privacyLabel = Plugin.Config.PrivacyFilterEnabled
            ? HellionStrings.StatusBar_Privacy_Enabled
            : HellionStrings.StatusBar_Privacy_Open;
        ImGui.TextUnformatted(privacyLabel);

        // Slot 3: Counts
        ImGui.SameLine();
        DrawSeparator();
        ImGui.SameLine();
        ImGui.TextUnformatted(_cachedCountsText);

        // Slot 4: Tells (nur wenn > 0)
        if (!string.IsNullOrEmpty(_cachedTellsText))
        {
            ImGui.SameLine();
            DrawSeparator();
            ImGui.SameLine();
            ImGui.TextUnformatted(_cachedTellsText);
        }

        // Slot 5: Version (rechtsbündig, muted)
        var versionText = $"v{Plugin.Interface.Manifest.AssemblyVersion} · Hellion";
        var versionWidth = ImGui.CalcTextSize(versionText).X;
        var contentRegionMax = ImGui.GetContentRegionMax().X;
        ImGui.SameLine(contentRegionMax - versionWidth);
        using (ImRaii.PushColor(ImGuiCol.Text, ColourUtil.RgbaToAbgr(theme.Colors.TextMuted)))
        {
            ImGui.TextUnformatted(versionText);
        }
    }

    private static void DrawDot(uint rgba)
    {
        var pos = ImGui.GetCursorScreenPos();
        const float radius = 4f;
        ImGui.GetWindowDrawList().AddCircleFilled(
            new Vector2(pos.X + radius, pos.Y + ImGui.GetTextLineHeight() / 2f),
            radius,
            ColourUtil.RgbaToAbgr(rgba));
        ImGui.Dummy(new Vector2(radius * 2 + 4, ImGui.GetTextLineHeight()));
    }

    private static void DrawSeparator()
    {
        ImGui.TextDisabled("·");
    }
}
