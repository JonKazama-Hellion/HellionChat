using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using HellionChat.Resources;
using HellionChat.Util;

namespace HellionChat.Ui;

internal sealed class SettingsOverview
{
    private readonly SettingsWindow _window;

    // Card-Reihenfolge entspricht 1:1 dem Tabs-Index in SettingsWindow.
    // v1.2.1: Cards thematisch re-sortiert. Theme & Layout vereint Theme-
    // Picker + Frame-Style + Timestamps; Fonts & Colours vereint Schriften
    // + Chat-Farben; Data Management vereint Storage + Retention + Cleanup
    // + Export + DB-Viewer + Advanced.
    private static readonly (FontAwesomeIcon Icon, string TitleKey, string SubtextKey)[] CardDefs =
    [
        (FontAwesomeIcon.SlidersH,        "Settings_Card_General_Title",         "Settings_Card_General_Subtext"),
        (FontAwesomeIcon.Palette,         "Settings_Card_ThemeAndLayout_Title",  "Settings_Card_ThemeAndLayout_Subtext"),
        (FontAwesomeIcon.Font,            "Settings_Card_FontsAndColours_Title", "Settings_Card_FontsAndColours_Subtext"),
        (FontAwesomeIcon.WindowMaximize,  "Settings_Card_Window_Title",          "Settings_Card_Window_Subtext"),
        (FontAwesomeIcon.Comments,        "Settings_Card_Chat_Title",            "Settings_Card_Chat_Subtext"),
        (FontAwesomeIcon.FolderTree,      "Settings_Card_Tabs_Title",            "Settings_Card_Tabs_Subtext"),
        (FontAwesomeIcon.ShieldAlt,       "Settings_Card_Privacy_Title",         "Settings_Card_Privacy_Subtext"),
        (FontAwesomeIcon.Database,        "Settings_Card_DataManagement_Title",  "Settings_Card_DataManagement_Subtext"),
        (FontAwesomeIcon.Plug,            "Settings_Card_Integrations_Title",    "Settings_Card_Integrations_Subtext"),
        (FontAwesomeIcon.InfoCircle,      "Settings_Card_Information_Title",     "Settings_Card_Information_Subtext"),
    ];

    public SettingsOverview(SettingsWindow window)
    {
        _window = window;
    }

    public void Draw()
    {
        var avail = ImGui.GetContentRegionAvail();
        var columns = avail.X >= 700f ? 3 : 2;
        var cardWidth = (avail.X - (columns - 1) * 8f) / columns;
        // v1.2.1 — Subtexte wrappen jetzt auf zwei Zeilen, daher 110f statt der
        // v1.1.0-Höhe 96f. Wrap-Breite + Y-Position der Subtext-Zeile sind in
        // DrawCard auf den Card-Innenrand abgestimmt.
        var cardHeight = 110f;

        for (var i = 0; i < CardDefs.Length; i++)
        {
            var (icon, titleKey, subtextKey) = CardDefs[i];
            var title = HellionStrings.ResourceManager.GetString(titleKey) ?? titleKey;
            var subtext = HellionStrings.ResourceManager.GetString(subtextKey) ?? subtextKey;
            DrawCard(i, icon, title, subtext, cardWidth, cardHeight);

            if ((i + 1) % columns != 0 && i != CardDefs.Length - 1)
                ImGui.SameLine();
        }
    }

    private void DrawCard(int index, FontAwesomeIcon icon, string title, string subtext, float w, float h)
    {
        // BeginGroup macht den Card-Bereich zu einem einzelnen ImGui-Layout-Item.
        // Damit funktioniert SameLine() im Caller-Loop — sonst tracked ImGui die
        // einzelnen InvisibleButton/Text-Items separat und das Wrapping bricht.
        ImGui.BeginGroup();

        var cursorBefore = ImGui.GetCursorScreenPos();
        var clicked = ImGui.InvisibleButton($"##settings-card-{index}", new Vector2(w, h));
        var hovered = ImGui.IsItemHovered();
        var bgColor = hovered ? 0xFF22303Fu : 0xFF1A2538u;

        var draw = ImGui.GetWindowDrawList();
        draw.AddRectFilled(cursorBefore, cursorBefore + new Vector2(w, h), bgColor, 4f);

        // Inhalts-Overlay: Icon + Title via DrawList (kein Wrap nötig). Subtext
        // läuft über ImGui-Cursor + PushTextWrapPos damit der Text bei
        // Card-Innenbreite umbricht statt rechts geclippt zu werden.
        var iconPos = cursorBefore + new Vector2(16f, 12f);
        var titlePos = cursorBefore + new Vector2(16f, 40f);
        var subtextPos = cursorBefore + new Vector2(16f, 62f);

        var titleColor = ColourUtil.RgbaToAbgr(0xE6F4F1FFu);
        var subtextColor = ColourUtil.RgbaToAbgr(0x8FA3B5FFu);

        using (_window.Plugin.FontManager.FontAwesome.Push())
        {
            draw.AddText(iconPos, titleColor, icon.ToIconString());
        }

        draw.AddText(titlePos, titleColor, title);

        // Subtext mit Wrap auf Card-Innenbreite (16 px Padding links + rechts).
        // Cursor-basiertes TextUnformatted würde die ImGui-Group-Bounds
        // erweitern und das SameLine-Wrapping in der Card-Reihe brechen, daher
        // bleibt der Subtext bewusst beim DrawList-Overlay-Pattern.
        var subtextWrapWidth = w - 32f;
        draw.AddText(ImGui.GetFont(), ImGui.GetFontSize(), subtextPos, subtextColor, subtext, subtextWrapWidth);

        ImGui.EndGroup();

        if (clicked)
        {
            _window.OpenSection(index);
        }
    }
}
