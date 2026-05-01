using ChatTwo.Code;
using ChatTwo.Privacy;
using ChatTwo.Util;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Bindings.ImGui;

namespace ChatTwo.Ui.SettingsTabs;

internal sealed class Privacy : ISettingsTab
{
    private Configuration Mutable { get; }

    public string Name => "Privacy###tabs-privacy";

    internal Privacy(Configuration mutable)
    {
        Mutable = mutable;
    }

    // Channels grouped for the UI. Order = display order.
    private static readonly (string Heading, ChatType[] Types)[] Groups =
    [
        ("Direct Messages", [ChatType.TellIncoming, ChatType.TellOutgoing]),
        ("Party & Alliance", [ChatType.Party, ChatType.CrossParty, ChatType.Alliance, ChatType.PvpTeam]),
        ("Free Company", [ChatType.FreeCompany, ChatType.FreeCompanyAnnouncement, ChatType.FreeCompanyLoginLogout]),
        ("Linkshells", [
            ChatType.Linkshell1, ChatType.Linkshell2, ChatType.Linkshell3, ChatType.Linkshell4,
            ChatType.Linkshell5, ChatType.Linkshell6, ChatType.Linkshell7, ChatType.Linkshell8,
        ]),
        ("Cross-World Linkshells", [
            ChatType.CrossLinkshell1, ChatType.CrossLinkshell2, ChatType.CrossLinkshell3, ChatType.CrossLinkshell4,
            ChatType.CrossLinkshell5, ChatType.CrossLinkshell6, ChatType.CrossLinkshell7, ChatType.CrossLinkshell8,
        ]),
        ("ExtraChat (Encrypted)", [
            ChatType.ExtraChatLinkshell1, ChatType.ExtraChatLinkshell2, ChatType.ExtraChatLinkshell3, ChatType.ExtraChatLinkshell4,
            ChatType.ExtraChatLinkshell5, ChatType.ExtraChatLinkshell6, ChatType.ExtraChatLinkshell7, ChatType.ExtraChatLinkshell8,
        ]),
        ("Public Chat (third-party data)", [ChatType.Say, ChatType.Shout, ChatType.Yell, ChatType.NoviceNetwork, ChatType.CustomEmote, ChatType.StandardEmote]),
        ("System & Game Logs", [
            ChatType.System, ChatType.Notice, ChatType.Urgent, ChatType.Echo,
            ChatType.NpcDialogue, ChatType.NpcAnnouncement,
            ChatType.LootNotice, ChatType.LootRoll, ChatType.RetainerSale,
            ChatType.Crafting, ChatType.Gathering, ChatType.Sign, ChatType.RandomNumber,
        ]),
    ];

    public void Draw(bool changed)
    {
        ImGuiUtil.OptionCheckbox(
            ref Mutable.PrivacyFilterEnabled,
            "Enable privacy filter",
            "When enabled, only messages from whitelisted channels are persisted to the database. " +
            "Disabling restores upstream ChatTwo behavior (everything except battle messages is stored).");

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        using (ImRaii.Disabled(!Mutable.PrivacyFilterEnabled))
        {
            ImGuiUtil.HelpText(
                "Pick which channels are stored in the local database. " +
                "Privacy-First default: only your own conversations. " +
                "Use the buttons below to apply a preset.");

            ImGui.Spacing();

            if (ImGui.Button("Privacy-First (recommended)"))
                Mutable.PrivacyPersistChannels = [..PrivacyDefaults.PrivacyFirstWhitelist];

            ImGui.SameLine();
            if (ImGui.Button("Clear all"))
                Mutable.PrivacyPersistChannels.Clear();

            ImGui.SameLine();
            if (ImGui.Button("Select all"))
                foreach (var group in Groups)
                foreach (var t in group.Types)
                    Mutable.PrivacyPersistChannels.Add(t);

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            foreach (var (heading, types) in Groups)
            {
                using var tree = ImRaii.TreeNode(heading);
                if (!tree.Success)
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
                "Persist unknown channel types",
                "Failsafe for ChatTypes added by future FFXIV patches that this plugin does not yet know about. " +
                "Default OFF (Privacy-First). Turn ON if you want a complete log including future channels.");
        }
    }
}
