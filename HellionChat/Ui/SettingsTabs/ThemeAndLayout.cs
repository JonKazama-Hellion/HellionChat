using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using HellionChat.Resources;
using HellionChat.Themes;
using HellionChat.Util;

namespace HellionChat.Ui.SettingsTabs;

internal sealed class ThemeAndLayout : ISettingsTab
{
    private Plugin Plugin { get; }
    private Configuration Mutable { get; }

    private string? _applyDismissedFor;

    public string Name => HellionStrings.Settings_Card_ThemeAndLayout_Title + "###tabs-themeandlayout";

    internal ThemeAndLayout(Plugin plugin, Configuration mutable)
    {
        Plugin = plugin;
        Mutable = mutable;
    }

    public void Draw(bool changed)
    {
        DrawThemeSection();
        ImGui.Spacing();
        DrawWindowStyleSection();
        ImGui.Spacing();
        DrawTimestampStyleSection();
    }

    private void DrawThemeSection()
    {
        using var tree = ImRaii.TreeNode(HellionStrings.Settings_ThemeAndLayout_Theme_Heading);
        if (!tree.Success)
            return;

        using (ImRaii.PushIndent(ImGui.GetStyle().IndentSpacing, false))
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
    }

    private void DrawThemeGrid(IEnumerable<Theme> themes, string activeSlug)
    {
        var avail = ImGui.GetContentRegionAvail();
        var columns = avail.X >= 700f ? 3 : 2;
        var cardWidth = (avail.X - (columns - 1) * 8f) / columns;
        var cardHeight = 140f;

        var list = themes.ToList();
        for (var i = 0; i < list.Count; i++)
        {
            DrawThemeCard(list[i], activeSlug, cardWidth, cardHeight);

            if ((i + 1) % columns != 0 && i != list.Count - 1)
                ImGui.SameLine();
        }
    }

    private void DrawThemeCard(Theme theme, string activeSlug, float w, float h)
    {
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

        var mockupOrigin = cursorBefore + new Vector2(12f, 12f);
        var mockupSize = new Vector2(w - 24f, 60f);
        ThemeMockup.Draw(mockupOrigin, mockupSize, theme);

        var textColor = ColourUtil.RgbaToAbgr(theme.Colors.TextPrimary);
        var mutedColor = ColourUtil.RgbaToAbgr(theme.Colors.TextMuted);
        draw.AddText(cursorBefore + new Vector2(12f, 80f), textColor, theme.Name);
        draw.AddText(cursorBefore + new Vector2(12f, 100f), mutedColor, theme.Author);

        ImGui.EndGroup();

        if (clicked)
        {
            Mutable.Theme = theme.Slug;
            Plugin.ThemeRegistry.Switch(theme.Slug);
            _applyDismissedFor = null;
        }
    }

    private void DrawChatColorsApplyBanner(Theme active)
    {
        if (active.ChatColors is not { Channels.Count: > 0 } themeChatColors)
            return;

        if (_applyDismissedFor == active.Slug)
            return;

        var alreadyMatching = themeChatColors.Channels.All(kvp =>
            Mutable.ChatColours.TryGetValue(kvp.Key, out var current) && current == kvp.Value);
        if (alreadyMatching)
            return;

        ImGui.Spacing();

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

        ImGui.SetCursorScreenPos(origin + new Vector2(0f, height + 8f));

        ImGui.Spacing();
    }

    private void DrawWindowStyleSection()
    {
        using var tree = ImRaii.TreeNode(HellionStrings.Settings_ThemeAndLayout_WindowStyle_Heading);
        if (!tree.Success)
            return;

        using (ImRaii.PushIndent(ImGui.GetStyle().IndentSpacing, false))
        {
            ImGui.Checkbox(Language.Options_ShowTitleBar_Name, ref Mutable.ShowTitleBar);

            ImGui.Checkbox(Language.Options_ShowPopOutTitleBar_Name, ref Mutable.ShowPopOutTitleBar);

            ImGui.Checkbox(Language.Options_ShowHideButton_Name, ref Mutable.ShowHideButton);
            ImGuiUtil.HelpMarker(Language.Options_ShowHideButton_Description);

            ImGui.Checkbox(Language.Options_SidebarTabView_Name, ref Mutable.SidebarTabView);
            ImGuiUtil.HelpMarker(string.Format(Language.Options_SidebarTabView_Description, Plugin.PluginName));

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            // Slider 50–100 % UX-Range; intern 0.5–1.0 als WindowOpacity-Float.
            // Untere Schwelle 50 % verhindert versehentliches Komplett-Wegblenden
            // des Chat-Hintergrunds (war v1.2.0 Bug bei WindowAlpha=0).
            var opacityPercent = Mutable.WindowOpacity * 100f;
            if (ImGuiUtil.DragFloatVertical(
                    HellionStrings.Settings_ThemeAndLayout_WindowOpacity_Name,
                    ref opacityPercent,
                    .25f,
                    50f,
                    100f,
                    $"{opacityPercent:N0}%%",
                    ImGuiSliderFlags.AlwaysClamp))
            {
                Mutable.WindowOpacity = opacityPercent / 100f;
            }
            ImGuiUtil.HelpMarker(HellionStrings.Settings_ThemeAndLayout_WindowOpacity_Description);
        }
    }

    private void DrawTimestampStyleSection()
    {
        using var tree = ImRaii.TreeNode(HellionStrings.Settings_ThemeAndLayout_TimestampStyle_Heading);
        if (!tree.Success)
            return;

        using (ImRaii.PushIndent(ImGui.GetStyle().IndentSpacing, false))
        {
            ImGui.Checkbox(Language.Options_PrettierTimestamps_Name, ref Mutable.PrettierTimestamps);
            ImGuiUtil.HelpMarker(Language.Options_PrettierTimestamps_Description);

            if (Mutable.PrettierTimestamps)
            {
                ImGui.Checkbox(Language.Options_MoreCompactPretty_Name, ref Mutable.MoreCompactPretty);
                ImGuiUtil.HelpMarker(Language.Options_MoreCompactPretty_Description);

                ImGui.Checkbox(HellionStrings.Appearance_UseCompactDensity_Name, ref Mutable.UseCompactDensity);
                ImGuiUtil.HelpMarker(HellionStrings.Appearance_UseCompactDensity_Description);

                ImGui.Checkbox(Language.Options_HideSameTimestamps_Name, ref Mutable.HideSameTimestamps);
                ImGuiUtil.HelpMarker(Language.Options_HideSameTimestamps_Description);
            }

            ImGui.Checkbox(Language.Options_Use24HourClock_Name, ref Mutable.Use24HourClock);
            ImGuiUtil.HelpMarker(Language.Options_Use24HourClock_Description);
        }
    }
}
