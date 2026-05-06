using HellionChat.Resources;
using HellionChat.Util;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Bindings.ImGui;

namespace HellionChat.Ui.SettingsTabs;

internal sealed class General : ISettingsTab
{
    private Plugin Plugin { get; }
    private Configuration Mutable { get; }

    public string Name => HellionStrings.Settings_Tab_General + "###tabs-general";

    internal General(Plugin plugin, Configuration mutable)
    {
        Plugin = plugin;
        Mutable = mutable;
    }

    public void Draw(bool changed)
    {
        DrawInputSection();
        ImGui.Spacing();
        DrawAudioSection();
        ImGui.Spacing();
        DrawPerformanceSection();
        ImGui.Spacing();
        DrawLanguageSection();
    }

    private void DrawInputSection()
    {
        using var tree = ImRaii.TreeNode(HellionStrings.Settings_General_Input_Heading);
        if (!tree.Success)
        {
            return;
        }

        using (ImRaii.PushIndent(ImGui.GetStyle().IndentSpacing, false))
        {
            ImGui.Checkbox(Language.Options_KeepInputFocus_Name, ref Mutable.KeepInputFocus);
            ImGuiUtil.HelpMarker(Language.Options_KeepInputFocus_Description);

            ImGui.Spacing();
            ImGui.TextUnformatted(Language.Options_ChatTabForwardKeybind_Name);
            ImGui.SetNextItemWidth(-1);
            ImGuiUtil.KeybindInput("ChatTabForwardKeybind", ref Mutable.ChatTabForward);

            ImGui.TextUnformatted(Language.Options_ChatTabBackwardKeybind_Name);
            ImGui.SetNextItemWidth(-1);
            ImGuiUtil.KeybindInput("ChatTabBackwardKeybind", ref Mutable.ChatTabBackward);

            ImGui.Spacing();

            using (var combo = ImGuiUtil.BeginComboVertical(Language.Options_KeybindMode_Name, Mutable.KeybindMode.Name()))
            {
                if (combo.Success)
                {
                    foreach (var mode in Enum.GetValues<KeybindMode>())
                    {
                        if (ImGui.Selectable(mode.Name(), Mutable.KeybindMode == mode))
                        {
                            Mutable.KeybindMode = mode;
                        }

                        if (ImGui.IsItemHovered())
                        {
                            ImGuiUtil.Tooltip(mode.Tooltip() ?? "");
                        }
                    }
                }
            }
            ImGuiUtil.HelpMarker(string.Format(Language.Options_KeybindMode_Description, Plugin.PluginName));
        }
    }

    private void DrawAudioSection()
    {
        using var tree = ImRaii.TreeNode(HellionStrings.Settings_General_Audio_Heading);
        if (!tree.Success)
        {
            return;
        }

        using (ImRaii.PushIndent(ImGui.GetStyle().IndentSpacing, false))
        {
            ImGui.Checkbox(Language.Options_PlaySounds_Name, ref Mutable.PlaySounds);
            ImGuiUtil.HelpMarker(Language.Options_PlaySounds_Description);

            ImGui.Checkbox(Language.Options_ShowNoviceNetwork_Name, ref Mutable.ShowNoviceNetwork);
            ImGuiUtil.HelpMarker(Language.Options_ShowNoviceNetwork_Description);
        }
    }

    private void DrawPerformanceSection()
    {
        using var tree = ImRaii.TreeNode(HellionStrings.Settings_General_Performance_Heading);
        if (!tree.Success)
        {
            return;
        }

        using (ImRaii.PushIndent(ImGui.GetStyle().IndentSpacing, false))
        {
            ImGui.SetNextItemWidth(200f * ImGuiHelpers.GlobalScale);
            if (ImGui.InputInt(Language.Options_MaxLinesToShow_Name, ref Mutable.MaxLinesToRender))
            {
                Mutable.MaxLinesToRender = Math.Clamp(Mutable.MaxLinesToRender, 1, 10_000);
            }
            ImGuiUtil.HelpMarker(Language.Options_MaxLinesToShow_Description);
        }
    }

    private void DrawLanguageSection()
    {
        using var tree = ImRaii.TreeNode(HellionStrings.Settings_General_Language_Heading);
        if (!tree.Success)
        {
            return;
        }

        using (ImRaii.PushIndent(ImGui.GetStyle().IndentSpacing, false))
        {
            using (var combo = ImGuiUtil.BeginComboVertical(Language.Options_Language_Name, Mutable.LanguageOverride.Name()))
            {
                if (combo.Success)
                {
                    foreach (var language in Enum.GetValues<LanguageOverride>())
                    {
                        if (ImGui.Selectable(language.Name()))
                        {
                            Mutable.LanguageOverride = language;
                        }
                    }
                }
            }
            ImGuiUtil.HelpMarker(string.Format(Language.Options_Language_Description, Plugin.PluginName));
            ImGui.Spacing();

            using (var combo = ImGuiUtil.BeginComboVertical(Language.Options_CommandHelpSide_Name, Mutable.CommandHelpSide.Name()))
            {
                if (combo.Success)
                {
                    foreach (var side in Enum.GetValues<CommandHelpSide>())
                    {
                        if (ImGui.Selectable(side.Name(), Mutable.CommandHelpSide == side))
                        {
                            Mutable.CommandHelpSide = side;
                        }
                    }
                }
            }
            ImGuiUtil.HelpMarker(string.Format(Language.Options_CommandHelpSide_Description, Plugin.PluginName));
            ImGui.Spacing();

            ImGui.Checkbox(Language.Options_SortAutoTranslate_Name, ref Mutable.SortAutoTranslate);
            ImGuiUtil.HelpMarker(Language.Options_SortAutoTranslate_Description);
        }
    }
}
