using HellionChat.Code;
using HellionChat.Privacy;
using HellionChat.Resources;
using HellionChat.Util;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Bindings.ImGui;

namespace HellionChat.Ui.SettingsTabs;

internal sealed class Privacy : ISettingsTab
{
    private Plugin Plugin { get; }
    private Configuration Mutable { get; }

    public string Name => HellionStrings.Privacy_Tab_Title + "###tabs-privacy";

    internal Privacy(Plugin plugin, Configuration mutable)
    {
        Plugin = plugin;
        Mutable = mutable;
    }

    // (HeadingKey lookup, ChatType list). Heading is resolved per-frame so
    // a runtime LanguageChanged call updates the labels immediately.
    private static readonly (Func<string> Heading, ChatType[] Types)[] Groups =
    [
        (() => HellionStrings.Privacy_Group_DirectMessages, [ChatType.TellIncoming, ChatType.TellOutgoing]),
        (() => HellionStrings.Privacy_Group_PartyAlliance, [ChatType.Party, ChatType.CrossParty, ChatType.Alliance, ChatType.PvpTeam]),
        (() => HellionStrings.Privacy_Group_FreeCompany, [ChatType.FreeCompany, ChatType.FreeCompanyAnnouncement, ChatType.FreeCompanyLoginLogout]),
        (() => HellionStrings.Privacy_Group_Linkshells, [
            ChatType.Linkshell1, ChatType.Linkshell2, ChatType.Linkshell3, ChatType.Linkshell4,
            ChatType.Linkshell5, ChatType.Linkshell6, ChatType.Linkshell7, ChatType.Linkshell8,
        ]),
        (() => HellionStrings.Privacy_Group_CrossLinkshells, [
            ChatType.CrossLinkshell1, ChatType.CrossLinkshell2, ChatType.CrossLinkshell3, ChatType.CrossLinkshell4,
            ChatType.CrossLinkshell5, ChatType.CrossLinkshell6, ChatType.CrossLinkshell7, ChatType.CrossLinkshell8,
        ]),
        (() => HellionStrings.Privacy_Group_ExtraChat, [
            ChatType.ExtraChatLinkshell1, ChatType.ExtraChatLinkshell2, ChatType.ExtraChatLinkshell3, ChatType.ExtraChatLinkshell4,
            ChatType.ExtraChatLinkshell5, ChatType.ExtraChatLinkshell6, ChatType.ExtraChatLinkshell7, ChatType.ExtraChatLinkshell8,
        ]),
        (() => HellionStrings.Privacy_Group_PublicChat, [ChatType.Say, ChatType.Shout, ChatType.Yell, ChatType.NoviceNetwork, ChatType.CustomEmote, ChatType.StandardEmote]),
        (() => HellionStrings.Privacy_Group_SystemLogs, [
            ChatType.System, ChatType.Notice, ChatType.Urgent, ChatType.Echo,
            ChatType.NpcDialogue, ChatType.NpcAnnouncement,
            ChatType.LootNotice, ChatType.LootRoll, ChatType.RetainerSale,
            ChatType.Crafting, ChatType.Gathering, ChatType.Sign, ChatType.RandomNumber,
        ]),
    ];

    public void Draw(bool changed)
    {
        if (ImGui.Button(HellionStrings.Wizard_Reopen_Button))
            Plugin.FirstRunWizard.IsOpen = true;
        ImGui.Spacing();

        DrawPrivacyFilterSection();
    }

    private void DrawPrivacyFilterSection()
    {
        using var tree = ImRaii.TreeNode(HellionStrings.Privacy_Filter_Tree_Heading);
        if (!tree.Success)
            return;

        using (ImRaii.PushIndent(ImGui.GetStyle().IndentSpacing, false))
        {
            ImGuiUtil.OptionCheckbox(
                ref Mutable.PrivacyFilterEnabled,
                HellionStrings.Privacy_FilterEnabled_Name,
                HellionStrings.Privacy_FilterEnabled_Description);
            ImGuiUtil.HelpMarker(HellionStrings.Privacy_FilterEnabled_StorageOnly_Help);

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            using (ImRaii.Disabled(!Mutable.PrivacyFilterEnabled))
            {
                ImGuiUtil.HelpText(HellionStrings.Privacy_Whitelist_Help);

                ImGui.Spacing();

                if (ImGui.Button(HellionStrings.Privacy_Preset_PrivacyFirst))
                    Mutable.PrivacyPersistChannels = [..PrivacyDefaults.PrivacyFirstWhitelist];

                ImGui.SameLine();
                if (ImGui.Button(HellionStrings.Privacy_Preset_ClearAll))
                    Mutable.PrivacyPersistChannels.Clear();

                ImGui.SameLine();
                if (ImGui.Button(HellionStrings.Privacy_Preset_SelectAll))
                    foreach (var group in Groups)
                    foreach (var t in group.Types)
                        Mutable.PrivacyPersistChannels.Add(t);

                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();

                foreach (var (heading, types) in Groups)
                {
                    using var groupTree = ImRaii.TreeNode(heading());
                    if (!groupTree.Success)
                        continue;

                    using (ImRaii.PushIndent(ImGui.GetStyle().IndentSpacing, false))
                    {
                        foreach (var type in types)
                        {
                            var enabled = Mutable.PrivacyPersistChannels.Contains(type);
                            var label = type.ToString();
                            if (ImGui.Checkbox($"{label}##privacy-{(int)type}", ref enabled))
                            {
                                if (enabled)
                                    Mutable.PrivacyPersistChannels.Add(type);
                                else
                                    Mutable.PrivacyPersistChannels.Remove(type);
                            }
                        }
                    }
                }

                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();

                ImGuiUtil.OptionCheckbox(
                    ref Mutable.PrivacyPersistUnknownChannels,
                    HellionStrings.Privacy_PersistUnknown_Name,
                    HellionStrings.Privacy_PersistUnknown_Description);
            }
        }
    }
}
