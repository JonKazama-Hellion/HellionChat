using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using HellionChat.Resources;
using HellionChat.Themes;
using HellionChat.Util;

namespace HellionChat.Ui.SettingsTabs;

internal sealed class Themes : ISettingsTab
{
    private readonly Plugin Plugin;
    private readonly Configuration Mutable;

    // Tracks ob der User die Apply-Frage für das aktive Theme bereits
    // beantwortet hat. Banner wird nur angezeigt wenn das Theme ein
    // ChatColors-Set hat UND noch keine Antwort vorliegt UND die aktuellen
    // Mutable.ChatColours davon abweichen.
    private string? _applyDismissedFor;

    public string Name => HellionStrings.ResourceManager.GetString("Settings_Tab_Themes") ?? "Themes" + "###tabs-themes";

    internal Themes(Plugin plugin, Configuration mutable)
    {
        Plugin = plugin;
        Mutable = mutable;
    }

    public void Draw(bool changed)
    {
        var registry = Plugin.ThemeRegistry;
        var active = registry.Get(Mutable.Theme);

        var activeLabelTemplate = HellionStrings.ResourceManager.GetString("Settings_Themes_Active") ?? "Active: {0}";
        ImGui.TextUnformatted(string.Format(activeLabelTemplate, active.Name));
        using (ImRaii.PushColor(ImGuiCol.Text, 0xFF8FA3B5u))
            ImGui.TextUnformatted(active.Author);

        DrawChatColorsApplyBanner(active);

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        var builtInsLabel = HellionStrings.ResourceManager.GetString("Settings_Themes_BuiltIns") ?? "Built-in themes";
        ImGui.TextUnformatted(builtInsLabel);
        ImGui.Spacing();
        DrawThemeGrid(registry.AllBuiltIns(), active.Slug);

        var customs = registry.AllCustom().ToList();
        if (customs.Count > 0)
        {
            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();
            var customLabel = HellionStrings.ResourceManager.GetString("Settings_Themes_Custom") ?? "Custom themes";
            ImGui.TextUnformatted(customLabel);
            ImGui.Spacing();
            DrawThemeGrid(customs, active.Slug);
        }

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        var openFolderLabel = HellionStrings.ResourceManager.GetString("Settings_Themes_OpenFolder") ?? "Open themes folder";
        if (ImGui.Button(openFolderLabel))
        {
            var dir = Path.Combine(Plugin.Interface.ConfigDirectory.FullName, "themes");
            Directory.CreateDirectory(dir);
            Dalamud.Utility.Util.OpenLink(dir);
        }

        ImGui.SameLine();
        var exportLabel = HellionStrings.ResourceManager.GetString("Settings_Themes_ExportActive") ?? "Export active...";
        if (ImGui.Button(exportLabel))
        {
            var dir = Path.Combine(Plugin.Interface.ConfigDirectory.FullName, "themes");
            Directory.CreateDirectory(dir);
            var fileName = $"{active.Slug}.export.json";
            var path = Path.Combine(dir, fileName);
            var json = ThemeJsonWriter.Serialize(active);
            File.WriteAllText(path, json);
            Plugin.Log.Information($"Exported active theme '{active.Slug}' to {path}");
        }
    }

    private void DrawThemeGrid(IEnumerable<Theme> themes, string activeSlug)
    {
        var avail = ImGui.GetContentRegionAvail();
        var columns = avail.X >= 700f ? 3 : 2;
        var cardWidth = (avail.X - (columns - 1) * 8f) / columns;
        var cardHeight = 140f;  // Mockup + Name + Author brauchen den Platz

        var list = themes.ToList();
        for (var i = 0; i < list.Count; i++)
        {
            DrawThemeCard(list[i], activeSlug, cardWidth, cardHeight);

            // SameLine zwischen den Cards einer Reihe; am Spalten-Ende kein
            // SameLine, dann beginnt automatisch eine neue Zeile.
            if ((i + 1) % columns != 0 && i != list.Count - 1)
                ImGui.SameLine();
        }
    }

    private void DrawThemeCard(Theme theme, string activeSlug, float w, float h)
    {
        // BeginGroup macht den Card-Bereich zu einem einzelnen ImGui-Layout-Item.
        // Damit funktioniert SameLine() im Caller-Loop — sonst tracked ImGui die
        // einzelnen InvisibleButton-Items separat und das Wrapping bricht.
        ImGui.BeginGroup();

        var isActive = string.Equals(theme.Slug, activeSlug, StringComparison.OrdinalIgnoreCase);
        var cursorBefore = ImGui.GetCursorScreenPos();
        var clicked = ImGui.InvisibleButton($"##theme-card-{theme.Slug}", new Vector2(w, h));
        var hovered = ImGui.IsItemHovered();

        var draw = ImGui.GetWindowDrawList();
        var bg = ColourUtil.RgbaToAbgr(theme.Colors.WindowBg | 0xFFu);
        draw.AddRectFilled(cursorBefore, cursorBefore + new Vector2(w, h), bg, 4f);

        if (isActive)
        {
            var border = ColourUtil.RgbaToAbgr(theme.Colors.Primary);
            draw.AddRect(cursorBefore, cursorBefore + new Vector2(w, h), border, 4f, ImDrawFlags.None, 2f);
        }
        else if (hovered)
        {
            var border = ColourUtil.RgbaToAbgr(theme.Colors.PrimaryLight & 0xFFFFFF99u);
            draw.AddRect(cursorBefore, cursorBefore + new Vector2(w, h), border, 4f, ImDrawFlags.None, 1f);
        }

        // Mini-Mockup oben — DrawList-Operation, kein Cursor-Hopping
        var mockupOrigin = cursorBefore + new Vector2(12f, 12f);
        var mockupSize = new Vector2(w - 24f, 60f);
        ThemeMockup.Draw(mockupOrigin, mockupSize, theme);

        // Name + Author direkt via DrawList (statt SetCursorScreenPos +
        // TextUnformatted), damit der ImGui-Layout-Cursor stabil bleibt
        // und die BeginGroup/EndGroup-Klammer den Card-Bereich als ein
        // Layout-Item führt.
        var textColor = ColourUtil.RgbaToAbgr(theme.Colors.TextPrimary);
        var mutedColor = ColourUtil.RgbaToAbgr(theme.Colors.TextMuted);
        draw.AddText(cursorBefore + new Vector2(12f, 80f), textColor, theme.Name);
        draw.AddText(cursorBefore + new Vector2(12f, 100f), mutedColor, theme.Author);

        ImGui.EndGroup();

        if (clicked)
        {
            Mutable.Theme = theme.Slug;
            Plugin.ThemeRegistry.Switch(theme.Slug);
            _applyDismissedFor = null;  // Banner für neues Theme wieder zeigen
        }
    }

    private void DrawChatColorsApplyBanner(Theme active)
    {
        // Klassik hat per Design keine ChatColors — kein Banner.
        if (active.ChatColors is not { Channels.Count: > 0 } themeChatColors)
            return;

        // User hat die Frage bereits für genau dieses Theme beantwortet.
        if (_applyDismissedFor == active.Slug)
            return;

        // Wenn die aktuellen Channel-Colors bereits exakt mit dem Theme-Vorschlag
        // übereinstimmen, gibt's nichts zu tun.
        var alreadyMatching = themeChatColors.Channels.All(kvp =>
            Mutable.ChatColours.TryGetValue(kvp.Key, out var current) && current == kvp.Value);
        if (alreadyMatching)
            return;

        ImGui.Spacing();

        // Dezent-Akzent-Banner mit Border in Theme-Primary
        var border = ColourUtil.RgbaToAbgr(active.Colors.Primary);
        var bgFill = ColourUtil.RgbaToAbgr((active.Colors.Surface & 0xFFFFFF00u) | 0xCCu);
        var origin = ImGui.GetCursorScreenPos();
        var width = ImGui.GetContentRegionAvail().X;
        var height = 64f;
        var draw = ImGui.GetWindowDrawList();
        draw.AddRectFilled(origin, origin + new Vector2(width, height), bgFill, 4f);
        draw.AddRect(origin, origin + new Vector2(width, height), border, 4f, ImDrawFlags.None, 1f);

        var hint = HellionStrings.ResourceManager.GetString("Settings_Themes_ApplyChatColors_Hint")
                   ?? "This theme suggests its own chat channel colours.";
        var applyLabel = HellionStrings.ResourceManager.GetString("Settings_Themes_ApplyChatColors_Apply")
                         ?? "Apply";
        var keepLabel = HellionStrings.ResourceManager.GetString("Settings_Themes_ApplyChatColors_Keep")
                        ?? "Keep current";

        var textColor = ColourUtil.RgbaToAbgr(active.Colors.TextPrimary);
        draw.AddText(origin + new Vector2(12f, 10f), textColor, hint);

        // Buttons als InvisibleButton + DrawList-Overlay, damit sie konsistent
        // zum Banner-Look bleiben.
        using (ImRaii.PushColor(ImGuiCol.Button, active.Colors.Primary))
        using (ImRaii.PushColor(ImGuiCol.ButtonHovered, active.Colors.PrimaryLight))
        using (ImRaii.PushColor(ImGuiCol.ButtonActive, active.Colors.PrimaryDark))
        {
            ImGui.SetCursorScreenPos(origin + new Vector2(12f, 32f));
            if (ImGui.Button(applyLabel))
            {
                foreach (var kvp in themeChatColors.Channels)
                    Mutable.ChatColours[kvp.Key] = kvp.Value;
                _applyDismissedFor = active.Slug;
            }
        }

        ImGui.SameLine();
        if (ImGui.Button(keepLabel))
        {
            _applyDismissedFor = active.Slug;
        }

        // Cursor unter den Banner setzen
        ImGui.SetCursorScreenPos(origin + new Vector2(0f, height + 8f));

        ImGui.Spacing();
    }
}
