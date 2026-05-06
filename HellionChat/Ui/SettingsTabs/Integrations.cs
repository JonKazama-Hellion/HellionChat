using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using HellionChat.Branding;
using HellionChat.Integrations;
using HellionChat.Resources;
using HellionChat.Util;

namespace HellionChat.Ui.SettingsTabs;

// First settings tab introduced in v1.3.0 (Plugin Integrations Cycle 1).
// Designed to grow organically: each future cycle adds a new section above
// the "Coming soon" block and removes the corresponding stub item.
internal sealed class Integrations : ISettingsTab
{
    private Plugin Plugin { get; }
    private Configuration Mutable { get; }

    public string Name => HellionStrings.Settings_Tab_Integrations + "###tabs-integrations";

    internal Integrations(Plugin plugin, Configuration mutable)
    {
        Plugin = plugin;
        Mutable = mutable;
    }

    public void Draw(bool changed)
    {
        ImGui.TextWrapped(HellionStrings.Settings_Integrations_Intro);
        ImGui.Spacing();
        ImGui.Spacing();

        DrawHonorificSection();
        ImGui.Spacing();
        ImGui.Spacing();

        DrawComingSoonSection();
        ImGui.Spacing();
        ImGui.Spacing();

        DrawGotAnIdeaSection();
    }

    private void DrawHonorificSection()
    {
        DrawSectionHeader(HellionStrings.Settings_Integrations_Honorific_SectionHeader);

        DrawHonorificStatus();
        ImGui.Spacing();

        // The toggle is enabled regardless of detection state — leaving it
        // on means "render when available, hide otherwise". Disabling the
        // toggle when Honorific is missing would force the user to retoggle
        // it every time Honorific is reloaded, which is worse UX than the
        // silent auto-hide.
        if (ImGui.Checkbox(
            HellionStrings.Settings_Integrations_Honorific_Toggle,
            ref Mutable.ShowHonorificTitleInHeader))
        {
            Plugin.SaveConfig();
        }

        using (ImRaii.PushIndent())
        {
            using (ImRaii.PushColor(ImGuiCol.Text, ColourUtil.RgbaToAbgr(Plugin.ThemeRegistry.Active.Colors.TextMuted)))
            {
                ImGui.TextWrapped(HellionStrings.Settings_Integrations_Honorific_ToggleHint);
            }
        }

        // Maintainer attribution. Honorific has no LICENSE in its repo so we
        // can't bundle its assets, but linking to the upstream and the
        // author's profile is the polite minimum. Plain ImGui buttons keep
        // the visual weight modest, the FontAwesome Brands subset is not
        // guaranteed in Dalamud's font set so we use text labels.
        ImGui.Spacing();
        if (ImGui.Button(HellionStrings.Settings_Integrations_Honorific_LinkRepo))
        {
            Dalamud.Utility.Util.OpenLink(IntegrationLinks.Honorific_Repo);
        }
        ImGui.SameLine();
        if (ImGui.Button(HellionStrings.Settings_Integrations_Honorific_LinkAuthor))
        {
            Dalamud.Utility.Util.OpenLink(IntegrationLinks.Honorific_Author);
        }
    }

    private void DrawHonorificStatus()
    {
        var theme = Plugin.ThemeRegistry.Active;
        var service = Plugin.HonorificService;

        if (service.IsAvailable && service.DetectedApiVersion is { } version)
        {
            DrawStatusGlyph('●', theme.Colors.StatusSuccess);
            ImGui.SameLine();
            ImGui.TextUnformatted(string.Format(
                HellionStrings.Settings_Integrations_Honorific_Status_Detected,
                version.Major, version.Minor));
        }
        else if (service.DetectedApiVersion is { } incompatibleVersion)
        {
            DrawStatusGlyph('⚠', theme.Colors.StatusWarning);
            ImGui.SameLine();
            ImGui.TextUnformatted(string.Format(
                HellionStrings.Settings_Integrations_Honorific_Status_Incompatible,
                3, incompatibleVersion.Major, incompatibleVersion.Minor));
        }
        else
        {
            DrawStatusGlyph('○', theme.Colors.TextMuted);
            ImGui.SameLine();
            ImGui.TextUnformatted(HellionStrings.Settings_Integrations_Honorific_Status_NotInstalled);
        }
    }

    private static void DrawStatusGlyph(char glyph, uint rgba)
    {
        using (ImRaii.PushColor(ImGuiCol.Text, ColourUtil.RgbaToAbgr(rgba)))
        {
            ImGui.TextUnformatted(glyph.ToString());
        }
    }

    private void DrawComingSoonSection()
    {
        DrawSectionHeader(HellionStrings.Settings_Integrations_ComingSoon_SectionHeader);
        ImGui.TextWrapped(HellionStrings.Settings_Integrations_ComingSoon_Intro);
        ImGui.Spacing();

        // Static list maintained in code (not Configuration). Each cycle
        // that lands a real integration removes its stub here and adds a
        // full section above the Coming Soon block.
        DrawComingSoonItem(
            HellionStrings.Settings_Integrations_ComingSoon_ContextMenu_Title,
            HellionStrings.Settings_Integrations_ComingSoon_ContextMenu_Description);
        DrawComingSoonItem(
            HellionStrings.Settings_Integrations_ComingSoon_Notifications_Title,
            HellionStrings.Settings_Integrations_ComingSoon_Notifications_Description);
        DrawComingSoonItem(
            HellionStrings.Settings_Integrations_ComingSoon_RPStatus_Title,
            HellionStrings.Settings_Integrations_ComingSoon_RPStatus_Description);
        DrawComingSoonItem(
            HellionStrings.Settings_Integrations_ComingSoon_ExtraChat_Title,
            HellionStrings.Settings_Integrations_ComingSoon_ExtraChat_Description);
        DrawComingSoonItem(
            HellionStrings.Settings_Integrations_ComingSoon_QuickDM_Title,
            HellionStrings.Settings_Integrations_ComingSoon_QuickDM_Description);
    }

    private void DrawComingSoonItem(string title, string description)
    {
        var theme = Plugin.ThemeRegistry.Active;
        using (ImRaii.PushColor(ImGuiCol.Text, ColourUtil.RgbaToAbgr(theme.Colors.TextMuted)))
        using (Plugin.FontManager.FontAwesome.Push())
        {
            ImGui.TextUnformatted(FontAwesomeIcon.Hourglass.ToIconString());
        }
        ImGui.SameLine();
        ImGui.TextUnformatted(title);
        using (ImRaii.PushIndent())
        {
            using (ImRaii.PushColor(ImGuiCol.Text, ColourUtil.RgbaToAbgr(theme.Colors.TextMuted)))
            {
                ImGui.TextWrapped(description);
            }
        }
        ImGui.Spacing();
    }

    private void DrawGotAnIdeaSection()
    {
        DrawSectionHeader(HellionStrings.Settings_Integrations_GotAnIdea_SectionHeader);
        ImGui.TextWrapped(HellionStrings.Settings_Integrations_GotAnIdea_Body);
        ImGui.Spacing();

        var theme = Plugin.ThemeRegistry.Active;
        using (ImRaii.PushColor(ImGuiCol.Text, ColourUtil.RgbaToAbgr(theme.Colors.Primary)))
        {
            // Selectable so the whole "→ link label" line is clickable and
            // shows a hover state, matching the affordance users expect from
            // hyperlinks in ImGui-driven plugins. Fully-qualified
            // Dalamud.Utility.Util.OpenLink because HellionChat.Util is in
            // scope here and an unqualified Util would clash.
            if (ImGui.Selectable("→ " + HellionStrings.Settings_Integrations_GotAnIdea_LinkLabel))
            {
                Dalamud.Utility.Util.OpenLink(BrandingLinks.HellionForgeDiscordInvite);
            }
        }
        if (ImGui.IsItemHovered())
        {
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
        }
    }

    private void DrawSectionHeader(string label)
    {
        var theme = Plugin.ThemeRegistry.Active;
        using (ImRaii.PushColor(ImGuiCol.Text, ColourUtil.RgbaToAbgr(theme.Colors.Primary)))
        {
            ImGui.TextUnformatted("── " + label + " ──");
        }
    }
}
