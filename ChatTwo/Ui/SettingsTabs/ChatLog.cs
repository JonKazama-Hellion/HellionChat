using ChatTwo.Resources;
using ChatTwo.Util;
using Dalamud.Interface.Style;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Bindings.ImGui;

namespace ChatTwo.Ui.SettingsTabs;

internal sealed class ChatLog : ISettingsTab
{
    private readonly Plugin Plugin;
    private Configuration Mutable { get; }

    public string Name => Language.Options_ChatLog_Tab + "###tabs-chatlog";

    internal ChatLog(Plugin plugin, Configuration mutable)
    {
        Plugin = plugin;
        Mutable = mutable;
    }

    public void Draw(bool changed)
    {
        using (ImRaii.TextWrapPos(0.0f))
        {
            ImGuiUtil.OptionCheckbox(ref Mutable.KeepInputFocus, Language.Options_KeepInputFocus_Name, Language.Options_KeepInputFocus_Description);
            ImGui.Spacing();

            ImGuiUtil.OptionCheckbox(ref Mutable.PlaySounds, Language.Options_PlaySounds_Name, Language.Options_PlaySounds_Description);
            ImGui.Spacing();

            ImGuiUtil.OptionCheckbox(ref Mutable.SidebarTabView, Language.Options_SidebarTabView_Name, string.Format(Language.Options_SidebarTabView_Description, Plugin.PluginName));
            ImGui.Spacing();

            ImGuiUtil.OptionCheckbox(ref Mutable.ShowNoviceNetwork, Language.Options_ShowNoviceNetwork_Name, Language.Options_ShowNoviceNetwork_Description);
            ImGui.Spacing();

            ImGuiUtil.OptionCheckbox(ref Mutable.ShowHideButton, Language.Options_ShowHideButton_Name, Language.Options_ShowHideButton_Description);
            ImGui.Spacing();

            ImGuiUtil.OptionCheckbox(ref Mutable.NativeItemTooltips, Language.Options_NativeItemTooltips_Name, string.Format(Language.Options_NativeItemTooltips_Description, Plugin.PluginName));
            ImGui.Spacing();

            if (Mutable.NativeItemTooltips)
            {
                ImGuiUtil.DragFloatVertical(Language.Options_TooltipOffset_Name, Language.Options_TooltipOffset_Desc, ref Mutable.TooltipOffset, 1, 0f, 400f, $"{Mutable.TooltipOffset:N0}px", ImGuiSliderFlags.AlwaysClamp);
                ImGui.Spacing();
            }

            ImGuiUtil.DragFloatVertical(Language.Options_WindowOpacity_Name, ref Mutable.WindowAlpha, .25f, 0f, 100f, $"{Mutable.WindowAlpha:N2}%%", ImGuiSliderFlags.AlwaysClamp);
            ImGui.Spacing();

            if (ImGuiUtil.InputIntVertical(Language.Options_MaxLinesToShow_Name, Language.Options_MaxLinesToShow_Description, ref Mutable.MaxLinesToRender))
                Mutable.MaxLinesToRender = Math.Clamp(Mutable.MaxLinesToRender, 1, 10_000);
            ImGui.Spacing();

            ImGuiUtil.OptionCheckbox(ref Mutable.CanMove, Language.Options_CanMove_Name);
            ImGui.Spacing();

            ImGuiUtil.OptionCheckbox(ref Mutable.CanResize, Language.Options_CanResize_Name);
            ImGui.Spacing();

            ImGuiUtil.OptionCheckbox(ref Mutable.ShowTitleBar, Language.Options_ShowTitleBar_Name);
            ImGui.Spacing();

            ImGuiUtil.OptionCheckbox(ref Mutable.ShowPopOutTitleBar, Language.Options_ShowPopOutTitleBar_Name);
            ImGui.Spacing();

            ImGuiUtil.OptionCheckbox(ref Mutable.OverrideStyle, Language.Options_OverrideStyle_Name, Language.Options_OverrideStyle_Name_Desc);
            ImGui.Spacing();

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            ImGui.TextUnformatted(Language.Options_ChatTabForwardKeybind_Name);
            ImGui.SetNextItemWidth(-1);
            ImGuiUtil.KeybindInput("ChatTabForwardKeybind", ref Mutable.ChatTabForward);

            ImGui.TextUnformatted(Language.Options_ChatTabBackwardKeybind_Name);
            ImGui.SetNextItemWidth(-1);
            ImGuiUtil.KeybindInput("ChatTabBackwardKeybind", ref Mutable.ChatTabBackward);

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            ImGui.TextUnformatted(Language.Options_AdjustPosition_Name);
            ImGui.SetNextItemWidth(-1);
            var pos = Plugin.ChatLogWindow.LastWindowPos;
            if (ImGui.DragFloat2($"##{Language.Options_AdjustPosition_Name}", ref pos, 1, 0, float.MaxValue, "%.0fpx"))
                Plugin.ChatLogWindow.Position = pos;
            ImGuiUtil.WarningText(Language.Options_AdjustPosition_Warning);
            ImGui.Spacing();

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            DrawAutoTellTabsSection();
        }

        if (!Mutable.OverrideStyle)
            return;

        var styles = StyleModel.GetConfiguredStyles();
        if (styles == null)
        {
            ImGui.TextUnformatted(Language.Options_OverrideStyle_NotAvailable);
            ImGui.Spacing();
            return;
        }

        var currentStyle = Mutable.ChosenStyle ?? Language.Options_OverrideStyle_NotSelected;
        using var combo = ImRaii.Combo(Language.Options_OverrideStyleDropdown_Name, currentStyle);
        if (combo)
        {
            foreach (var style in styles)
                if (ImGui.Selectable(style.Name, Mutable.ChosenStyle == style.Name))
                    Mutable.ChosenStyle = style.Name;
        }

        ImGui.Spacing();
    }

    private void DrawAutoTellTabsSection()
    {
        using var tree = ImRaii.TreeNode(HellionStrings.ChatLog_AutoTellTabs_Section_Title);
        if (!tree.Success)
            return;

        using (ImRaii.PushIndent(ImGui.GetStyle().IndentSpacing, false))
        {
            ImGui.Checkbox(HellionStrings.ChatLog_AutoTellTabs_Enable_Name, ref Mutable.EnableAutoTellTabs);
            ImGuiUtil.HelpMarker(HellionStrings.ChatLog_AutoTellTabs_Enable_Description);

            ImGui.SetNextItemWidth(200f * ImGuiHelpers.GlobalScale);
            var limit = Mutable.AutoTellTabsLimit;
            if (ImGui.SliderInt(HellionStrings.ChatLog_AutoTellTabs_Limit_Name, ref limit, 1, 50))
            {
                Mutable.AutoTellTabsLimit = limit;
            }
            ImGuiUtil.HelpMarker(HellionStrings.ChatLog_AutoTellTabs_Limit_Description);

            ImGui.Checkbox(HellionStrings.ChatLog_AutoTellTabs_Compact_Name, ref Mutable.AutoTellTabsCompactDisplay);
            ImGuiUtil.HelpMarker(HellionStrings.ChatLog_AutoTellTabs_Compact_Description);

            ImGui.Spacing();
            ImGuiUtil.HelpText(HellionStrings.ChatLog_AutoTellTabs_PreloadHint);

            ImGui.Spacing();
            ImGuiUtil.WarningText(HellionStrings.ChatLog_AutoTellTabs_ConflictHint);
        }
    }
}
