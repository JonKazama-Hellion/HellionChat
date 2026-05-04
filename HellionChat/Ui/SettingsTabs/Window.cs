using HellionChat.Resources;
using HellionChat.Util;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Bindings.ImGui;

namespace HellionChat.Ui.SettingsTabs;

internal sealed class Window : ISettingsTab
{
    private Plugin Plugin { get; }
    private Configuration Mutable { get; }

    public string Name => HellionStrings.Settings_Tab_Window + "###tabs-window";

    internal Window(Plugin plugin, Configuration mutable)
    {
        Plugin = plugin;
        Mutable = mutable;
    }

    public void Draw(bool changed)
    {
        DrawHideSection();
        ImGui.Spacing();
        DrawInactivityHideSection();
        ImGui.Spacing();
        DrawFrameSection();
        ImGui.Spacing();
        DrawTooltipsSection();
    }

    private void DrawHideSection()
    {
        using var tree = ImRaii.TreeNode(HellionStrings.Settings_Window_Hide_Heading);
        if (!tree.Success)
        {
            return;
        }

        using (ImRaii.PushIndent(ImGui.GetStyle().IndentSpacing, false))
        {
            ImGui.Checkbox(Language.Options_HideChat_Name, ref Mutable.HideChat);
            ImGuiUtil.HelpMarker(Language.Options_HideChat_Description);

            ImGui.Checkbox(Language.Options_HideDuringCutscenes_Name, ref Mutable.HideDuringCutscenes);
            ImGuiUtil.HelpMarker(string.Format(Language.Options_HideDuringCutscenes_Description, Plugin.PluginName));

            ImGui.Checkbox(Language.Options_HideWhenNotLoggedIn_Name, ref Mutable.HideWhenNotLoggedIn);
            ImGuiUtil.HelpMarker(string.Format(Language.Options_HideWhenNotLoggedIn_Description, Plugin.PluginName));

            ImGui.Checkbox(Language.Options_HideWhenUiHidden_Name, ref Mutable.HideWhenUiHidden);
            ImGuiUtil.HelpMarker(string.Format(Language.Options_HideWhenUiHidden_Description, Plugin.PluginName));

            ImGui.Checkbox(Language.Options_HideInLoadingScreens_Name, ref Mutable.HideInLoadingScreens);
            ImGuiUtil.HelpMarker(string.Format(Language.Options_HideInLoadingScreens_Description, Plugin.PluginName));

            ImGui.Checkbox(Language.Options_HideInBattle_Name, ref Mutable.HideInBattle);
            ImGuiUtil.HelpMarker(Language.Options_HideInBattle_Description);
        }
    }

    private void DrawInactivityHideSection()
    {
        using var tree = ImRaii.TreeNode(HellionStrings.Settings_Window_InactivityHide_Heading);
        if (!tree.Success)
        {
            return;
        }

        using (ImRaii.PushIndent(ImGui.GetStyle().IndentSpacing, false))
        {
            ImGui.Checkbox(Language.Options_HideWhenInactive_Name, ref Mutable.HideWhenInactive);
            ImGuiUtil.HelpMarker(Language.Options_HideWhenInactive_Description);

            if (!Mutable.HideWhenInactive)
            {
                return;
            }

            ImGuiUtil.InputIntVertical(Language.Options_InactivityHideTimeout_Name, Language.Options_InactivityHideTimeout_Description, ref Mutable.InactivityHideTimeout, 1, 10);
            // Untergrenze von 2 Sekunden gegen Selbst-Soft-Lock.
            Mutable.InactivityHideTimeout = Math.Max(2, Mutable.InactivityHideTimeout);

            using (ImRaii.Disabled(Mutable.HideInBattle))
            {
                ImGui.Checkbox(Language.Options_InactivityHideActiveDuringBattle_Name, ref Mutable.InactivityHideActiveDuringBattle);
                ImGuiUtil.HelpMarker(Language.Options_InactivityHideActiveDuringBattle_Description);
            }

            using var channelTree = ImRaii.TreeNode(Language.Options_InactivityHideChannels_Name);
            if (!channelTree.Success)
            {
                return;
            }

            if (ImGuiUtil.CtrlShiftButton(Language.Options_InactivityHideChannels_All_Label, Language.Options_InactivityHideChannels_Button_Tooltip))
            {
                Mutable.InactivityHideChannelsV2 = TabsUtil.AllChannels();
                Mutable.InactivityHideExtraChatAll = true;
                Mutable.InactivityHideExtraChatChannels = [];
            }

            ImGui.SameLine();
            if (ImGuiUtil.CtrlShiftButton(Language.Options_InactivityHideChannels_None_Label, Language.Options_InactivityHideChannels_Button_Tooltip))
            {
                Mutable.InactivityHideChannelsV2 = [];
                Mutable.InactivityHideExtraChatAll = false;
                Mutable.InactivityHideExtraChatChannels = [];
            }

            ImGui.Spacing();

            ImGuiUtil.ChannelSelector(Language.Options_Tabs_Channels, Mutable.InactivityHideChannelsV2);
            ImGuiUtil.ExtraChatSelector(Language.Options_Tabs_ExtraChatChannels, ref Mutable.InactivityHideExtraChatAll, Mutable.InactivityHideExtraChatChannels);
        }
    }

    private void DrawFrameSection()
    {
        using var tree = ImRaii.TreeNode(HellionStrings.Settings_Window_Frame_Heading);
        if (!tree.Success)
        {
            return;
        }

        using (ImRaii.PushIndent(ImGui.GetStyle().IndentSpacing, false))
        {
            ImGui.Checkbox(Language.Options_CanMove_Name, ref Mutable.CanMove);

            ImGui.Checkbox(Language.Options_CanResize_Name, ref Mutable.CanResize);

            ImGui.Checkbox(Language.Options_ShowTitleBar_Name, ref Mutable.ShowTitleBar);

            ImGui.Checkbox(Language.Options_ShowPopOutTitleBar_Name, ref Mutable.ShowPopOutTitleBar);

            // v0.6.0 — global master switch for the pop-out input bar.
            ImGui.Checkbox(HellionStrings.Settings_Window_PopOutInputEnabled_Name, ref Mutable.PopOutInputEnabled);
            ImGuiUtil.HelpMarker(HellionStrings.Settings_Window_PopOutInputEnabled_Description);

            ImGui.Checkbox(Language.Options_ShowHideButton_Name, ref Mutable.ShowHideButton);
            ImGuiUtil.HelpMarker(Language.Options_ShowHideButton_Description);

            ImGui.Checkbox(Language.Options_SidebarTabView_Name, ref Mutable.SidebarTabView);
            ImGuiUtil.HelpMarker(string.Format(Language.Options_SidebarTabView_Description, Plugin.PluginName));

            ImGui.Spacing();

            // Manual escape hatch for off-screen windows. The plugin already
            // runs an automatic bounds check once per session, but a button
            // is the user-friendly fallback after a display layout change.
            if (ImGui.Button(HellionStrings.Settings_Window_ResetPosition_Name))
                Plugin.ChatLogWindow.RequestPositionReset = true;
            ImGuiUtil.HelpMarker(HellionStrings.Settings_Window_ResetPosition_Description);
        }
    }

    private void DrawTooltipsSection()
    {
        using var tree = ImRaii.TreeNode(HellionStrings.Settings_Window_Tooltips_Heading);
        if (!tree.Success)
        {
            return;
        }

        using (ImRaii.PushIndent(ImGui.GetStyle().IndentSpacing, false))
        {
            ImGui.Checkbox(Language.Options_NativeItemTooltips_Name, ref Mutable.NativeItemTooltips);
            ImGuiUtil.HelpMarker(string.Format(Language.Options_NativeItemTooltips_Description, Plugin.PluginName));

            if (Mutable.NativeItemTooltips)
            {
                ImGuiUtil.DragFloatVertical(Language.Options_TooltipOffset_Name, Language.Options_TooltipOffset_Desc, ref Mutable.TooltipOffset, 1, 0f, 400f, $"{Mutable.TooltipOffset:N0}px", ImGuiSliderFlags.AlwaysClamp);
            }
        }
    }
}
