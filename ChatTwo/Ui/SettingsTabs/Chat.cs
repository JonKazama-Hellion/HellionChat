using System.Numerics;
using ChatTwo.Resources;
using ChatTwo.Util;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Bindings.ImGui;

namespace ChatTwo.Ui.SettingsTabs;

internal sealed class Chat : ISettingsTab
{
    private Plugin Plugin { get; }
    private Configuration Mutable { get; }

    public string Name => HellionStrings.Settings_Tab_Chat + "###tabs-chat";

    private SearchSelector.SelectorPopupOptions WordPopupOptions;

    internal Chat(Plugin plugin, Configuration mutable)
    {
        Plugin = plugin;
        Mutable = mutable;

        WordPopupOptions = RefillSheet();
    }

    private SearchSelector.SelectorPopupOptions RefillSheet()
    {
        return new SearchSelector.SelectorPopupOptions
        {
            FilteredSheet = EmoteCache.SortedCodeArray.Where(w => !Mutable.BlockedEmotes.Contains(w)).ToArray()
        };
    }

    public void Draw(bool changed)
    {
        DrawAutoTellTabsSection();
        ImGui.Spacing();
        DrawBehaviourSection();
        ImGui.Spacing();
        DrawPreviewSection();
        ImGui.Spacing();
        DrawEmotesSection();
    }

    private void DrawAutoTellTabsSection()
    {
        using var tree = ImRaii.TreeNode(HellionStrings.Settings_Chat_AutoTellTabs_Heading);
        if (!tree.Success)
        {
            return;
        }

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

            ImGui.Checkbox(HellionStrings.ChatLog_AutoTellTabs_GreetedToggle_Name, ref Mutable.AutoTellTabsShowGreetedToggle);
            ImGuiUtil.HelpMarker(HellionStrings.ChatLog_AutoTellTabs_GreetedToggle_Description);

            ImGui.Spacing();
            ImGuiUtil.HelpText(HellionStrings.ChatLog_AutoTellTabs_PreloadHint);

            ImGui.Spacing();
            ImGuiUtil.WarningText(HellionStrings.ChatLog_AutoTellTabs_ConflictHint);
        }
    }

    private void DrawBehaviourSection()
    {
        using var tree = ImRaii.TreeNode(HellionStrings.Settings_Chat_Behaviour_Heading);
        if (!tree.Success)
        {
            return;
        }

        using (ImRaii.PushIndent(ImGui.GetStyle().IndentSpacing, false))
        {
            ImGui.Checkbox(Language.Options_CollapseDuplicateMessages_Name, ref Mutable.CollapseDuplicateMessages);
            ImGuiUtil.HelpMarker(Language.Options_CollapseDuplicateMessages_Description);

            if (Mutable.CollapseDuplicateMessages)
            {
                ImGui.Checkbox(Language.Options_CollapseDuplicateMsgUniqueLink_Name, ref Mutable.CollapseKeepUniqueLinks);
                ImGuiUtil.HelpMarker(Language.Options_CollapseDuplicateMsgUniqueLink_Description);
            }
        }
    }

    private void DrawPreviewSection()
    {
        using var tree = ImRaii.TreeNode(HellionStrings.Settings_Chat_Preview_Heading);
        if (!tree.Success)
        {
            return;
        }

        using (ImRaii.PushIndent(ImGui.GetStyle().IndentSpacing, false))
        {
            using (var combo = ImGuiUtil.BeginComboVertical(Language.Options_Preview_Name, Mutable.PreviewPosition.Name()))
            {
                if (combo)
                {
                    foreach (var position in Enum.GetValues<PreviewPosition>())
                    {
                        if (ImGui.Selectable(position.Name(), Mutable.PreviewPosition == position))
                        {
                            Mutable.PreviewPosition = position;
                        }
                    }
                }
            }
            ImGuiUtil.HelpMarker(Language.Options_Preview_Description);

            if (ImGuiUtil.InputIntVertical(Language.Options_PreviewMinimum_Name, Language.Options_PreviewMinimum_Description, ref Mutable.PreviewMinimum))
            {
                Mutable.PreviewMinimum = Math.Clamp(Mutable.PreviewMinimum, 1, 250);
            }

            ImGui.Checkbox(Language.Options_PreviewOnlyIf_Name, ref Mutable.OnlyPreviewIf);
            ImGuiUtil.HelpMarker(Language.Options_PreviewOnlyIf_Description);
        }
    }

    private void DrawEmotesSection()
    {
        using var tree = ImRaii.TreeNode(HellionStrings.Settings_Chat_Emotes_Heading);
        if (!tree.Success)
        {
            return;
        }

        using (ImRaii.PushIndent(ImGui.GetStyle().IndentSpacing, false))
        {
            ImGui.Checkbox(Language.Options_ShowEmotes_Name, ref Mutable.ShowEmotes);
            ImGuiUtil.HelpMarker(Language.Options_ShowEmotes_Desc);

            ImGui.Spacing();
            ImGui.TextUnformatted(Language.Options_Emote_BlockedEmotes);
            ImGui.Spacing();

            if (EmoteCache.State is EmoteCache.LoadingState.Done && WordPopupOptions.FilteredSheet.Length == 0)
            {
                WordPopupOptions = RefillSheet();
            }

            var buttonWidth = ImGui.GetContentRegionAvail().X / 3;
            using (Plugin.FontManager.FontAwesome.Push())
                ImGui.Button(FontAwesomeIcon.Plus.ToIconString(), new Vector2(buttonWidth, 0));

            if (SearchSelector.SelectorPopup("WordAddPopup", out var newWord, WordPopupOptions))
            {
                Mutable.BlockedEmotes.Add(newWord);
            }

            using (var table = ImRaii.Table("##BlockedWords", 2, ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersInner))
            {
                if (table)
                {
                    ImGui.TableSetupColumn(Language.Options_Emote_EmoteTable);
                    ImGui.TableSetupColumn("##Del", ImGuiTableColumnFlags.WidthStretch, 0.07f);

                    ImGui.TableHeadersRow();

                    var copiedList = Mutable.BlockedEmotes.ToArray();
                    foreach (var word in copiedList)
                    {
                        ImGui.TableNextColumn();
                        ImGui.TextUnformatted(word);

                        ImGui.TableNextColumn();
                        if (ImGuiUtil.Button($"##{word}Del", FontAwesomeIcon.Trash, !ImGui.GetIO().KeyCtrl))
                        {
                            Mutable.BlockedEmotes.Remove(word);
                        }
                    }
                }
            }

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            ImGui.TextUnformatted(Language.Options_Emote_EmoteStats);
            ImGui.Spacing();

            if (EmoteCache.State is EmoteCache.LoadingState.Done)
            {
                ImGui.TextColored(ImGuiColors.HealerGreen, Language.Options_Emote_Ready);
            }
            else
            {
                ImGui.TextColored(ImGuiColors.DPSRed, Language.Options_Emote_NotReady);
            }

            ImGui.TextUnformatted($"{Language.Options_Emote_Loaded} {EmoteCache.SortedCodeArray.Length}");
            using (var emoteTable = ImRaii.Table("##LoadedEmotes", 5, ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersInner))
            {
                if (emoteTable)
                {
                    ImGui.TableSetupColumn("##word1");
                    ImGui.TableSetupColumn("##word2");
                    ImGui.TableSetupColumn("##word3");
                    ImGui.TableSetupColumn("##word4");
                    ImGui.TableSetupColumn("##word5");

                    foreach (var word in EmoteCache.SortedCodeArray)
                    {
                        ImGui.TableNextColumn();
                        ImGui.TextUnformatted(word);
                    }
                }
            }
        }
    }
}
