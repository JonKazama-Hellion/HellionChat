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
    // Themes ist Card-Index 2, eingeschoben zwischen Appearance und Window.
    private static readonly (FontAwesomeIcon Icon, string TitleKey, string SubtextKey)[] CardDefs =
    [
        (FontAwesomeIcon.SlidersH,        "Settings_Card_General_Title",     "Settings_Card_General_Subtext"),
        (FontAwesomeIcon.Palette,         "Settings_Card_Appearance_Title",  "Settings_Card_Appearance_Subtext"),
        (FontAwesomeIcon.Swatchbook,      "Settings_Card_Themes_Title",      "Settings_Card_Themes_Subtext"),
        (FontAwesomeIcon.WindowMaximize,  "Settings_Card_Window_Title",      "Settings_Card_Window_Subtext"),
        (FontAwesomeIcon.Comments,        "Settings_Card_Chat_Title",        "Settings_Card_Chat_Subtext"),
        (FontAwesomeIcon.FolderTree,      "Settings_Card_Tabs_Title",        "Settings_Card_Tabs_Subtext"),
        (FontAwesomeIcon.ShieldAlt,       "Settings_Card_Privacy_Title",     "Settings_Card_Privacy_Subtext"),
        (FontAwesomeIcon.Database,        "Settings_Card_Database_Title",    "Settings_Card_Database_Subtext"),
        (FontAwesomeIcon.InfoCircle,      "Settings_Card_Information_Title", "Settings_Card_Information_Subtext"),
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
        var cardHeight = 96f;

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
        var cursorBefore = ImGui.GetCursorScreenPos();
        var clicked = ImGui.InvisibleButton($"##settings-card-{index}", new Vector2(w, h));
        var hovered = ImGui.IsItemHovered();
        var bgColor = hovered ? 0xFF22303Fu : 0xFF1A2538u;

        var draw = ImGui.GetWindowDrawList();
        draw.AddRectFilled(cursorBefore, cursorBefore + new Vector2(w, h), bgColor, 4f);

        var textPos = cursorBefore + new Vector2(16f, 12f);
        ImGui.SetCursorScreenPos(textPos);
        // Plugin ist hier Instanz, nicht Static-Type — daher über _window
        // referenzieren (Codebase-Konvention, siehe ImGuiUtil.cs:22 für die
        // alternative Static-Init-Pattern, das wir hier nicht nutzen).
        using (_window.Plugin.FontManager.FontAwesome.Push())
        {
            ImGui.Text(icon.ToIconString());
        }

        ImGui.SetCursorScreenPos(textPos + new Vector2(0f, 28f));
        ImGui.TextUnformatted(title);

        ImGui.SetCursorScreenPos(textPos + new Vector2(0f, 50f));
        using (ImRaii.PushColor(ImGuiCol.Text, 0xFF8FA3B5u))
        {
            ImGui.TextUnformatted(subtext);
        }

        // Cursor unter die Card setzen für nächsten Item-Pass
        ImGui.SetCursorScreenPos(cursorBefore + new Vector2(0f, h + 8f));

        if (clicked)
        {
            _window.OpenSection(index);
        }
    }
}
