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
            // Export-Logik wird in Phase L (Task 21) ergänzt — Stub belassen, Button bleibt sichtbar.
        }
    }

    private void DrawThemeGrid(IEnumerable<Theme> themes, string activeSlug)
    {
        var avail = ImGui.GetContentRegionAvail();
        var columns = avail.X >= 700f ? 3 : 2;
        var cardWidth = (avail.X - (columns - 1) * 8f) / columns;
        var cardHeight = 110f;
        var i = 0;
        foreach (var theme in themes)
        {
            DrawThemeCard(theme, activeSlug, cardWidth, cardHeight);
            i++;
            if (i % columns != 0)
                ImGui.SameLine();
            else
                ImGui.NewLine();
        }
    }

    private void DrawThemeCard(Theme theme, string activeSlug, float w, float h)
    {
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

        // Akzent-Swatch links oben
        var swatchPos = cursorBefore + new Vector2(12f, 12f);
        var swatchSize = new Vector2(20f, 20f);
        draw.AddRectFilled(swatchPos, swatchPos + swatchSize, ColourUtil.RgbaToAbgr(theme.Colors.Primary), 3f);

        // Name
        ImGui.SetCursorScreenPos(cursorBefore + new Vector2(40f, 12f));
        var textColor = ColourUtil.RgbaToAbgr(theme.Colors.TextPrimary);
        using (ImRaii.PushColor(ImGuiCol.Text, textColor))
            ImGui.TextUnformatted(theme.Name);

        // Author
        ImGui.SetCursorScreenPos(cursorBefore + new Vector2(40f, 32f));
        var mutedColor = ColourUtil.RgbaToAbgr(theme.Colors.TextMuted);
        using (ImRaii.PushColor(ImGuiCol.Text, mutedColor))
            ImGui.TextUnformatted(theme.Author);

        // Description (wrapped, falls zu lang)
        ImGui.SetCursorScreenPos(cursorBefore + new Vector2(12f, 60f));
        ImGui.PushTextWrapPos(cursorBefore.X + w - 12f);
        using (ImRaii.PushColor(ImGuiCol.Text, mutedColor))
            ImGui.TextUnformatted(theme.Description);
        ImGui.PopTextWrapPos();

        // Cursor unter die Card setzen
        ImGui.SetCursorScreenPos(cursorBefore + new Vector2(0f, h + 8f));

        if (clicked)
        {
            Mutable.Theme = theme.Slug;
            Plugin.ThemeRegistry.Switch(theme.Slug);
        }
    }
}
