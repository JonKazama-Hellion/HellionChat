using ChatTwo.Code;
using ChatTwo.Export;
using ChatTwo.Privacy;
using ChatTwo.Resources;
using ChatTwo.Util;
using Dalamud.Interface.ImGuiNotification;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Bindings.ImGui;

namespace ChatTwo.Ui.SettingsTabs;

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

    private Dictionary<int, long>? CleanupCounts;
    private long CleanupKeepCount;
    private long CleanupDeleteCount;
    private bool CleanupRunning;

    private bool RetentionRunning;

    // Export form state
    private int ExportRangeDays = 30;
    private string ExportSenderSubstring = string.Empty;
    private readonly HashSet<ChatType> ExportSelectedChannels = [];
    private ExportFormat ExportFormat = ExportFormat.Markdown;
    private bool ExportRunning;

    public void Draw(bool changed)
    {
        using var _style = HellionStyle.Push();

        if (ImGui.Button(HellionStrings.Wizard_Reopen_Button))
            Plugin.FirstRunWizard.IsOpen = true;
        ImGui.Spacing();

        ImGuiUtil.OptionCheckbox(
            ref Mutable.PrivacyFilterEnabled,
            HellionStrings.Privacy_FilterEnabled_Name,
            HellionStrings.Privacy_FilterEnabled_Description);

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
                using var tree = ImRaii.TreeNode(heading());
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
                HellionStrings.Privacy_PersistUnknown_Name,
                HellionStrings.Privacy_PersistUnknown_Description);
        }

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        DrawRetentionSection();

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        DrawCleanupSection();

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        DrawExportSection();
    }

    private void DrawExportSection()
    {
        ImGui.TextUnformatted(HellionStrings.Export_Heading);
        using (ImRaii.PushIndent(ImGui.GetStyle().IndentSpacing, false))
        {
            ImGuiUtil.HelpText(HellionStrings.Export_Help);

            ImGui.Spacing();

            if (ImGui.InputInt(HellionStrings.Export_Range_Label, ref ExportRangeDays))
                ExportRangeDays = Math.Max(0, ExportRangeDays);

            ImGui.InputText(HellionStrings.Export_Sender_Label, ref ExportSenderSubstring, 256);

            using (var tree = ImRaii.TreeNode(HellionStrings.Export_Channels_Heading))
            {
                if (tree.Success)
                {
                    using (ImRaii.PushIndent(ImGui.GetStyle().IndentSpacing, false))
                    {
                        ImGuiUtil.HelpText(HellionStrings.Export_Channels_AllOff);
                        foreach (var (heading, types) in Groups)
                        {
                            using var subTree = ImRaii.TreeNode($"{heading()}##export-group-{heading()}");
                            if (!subTree.Success)
                                continue;

                            using (ImRaii.PushIndent(ImGui.GetStyle().IndentSpacing, false))
                            foreach (var type in types)
                            {
                                var enabled = ExportSelectedChannels.Contains(type);
                                if (ImGui.Checkbox($"{type}##export-{(int)type}", ref enabled))
                                {
                                    if (enabled)
                                        ExportSelectedChannels.Add(type);
                                    else
                                        ExportSelectedChannels.Remove(type);
                                }
                            }
                        }
                    }
                }
            }

            ImGui.Spacing();
            ImGui.TextUnformatted(HellionStrings.Export_Format_Label);
            ImGui.SameLine();
            var fmt = (int)ExportFormat;
            if (ImGui.RadioButton(HellionStrings.Export_Format_Markdown, ref fmt, (int)ExportFormat.Markdown))
                ExportFormat = ExportFormat.Markdown;
            ImGui.SameLine();
            if (ImGui.RadioButton(HellionStrings.Export_Format_Json, ref fmt, (int)ExportFormat.Json))
                ExportFormat = ExportFormat.Json;
            ImGui.SameLine();
            if (ImGui.RadioButton(HellionStrings.Export_Format_Csv, ref fmt, (int)ExportFormat.Csv))
                ExportFormat = ExportFormat.Csv;

            ImGui.Spacing();

            using (ImRaii.Disabled(ExportRunning))
            {
                if (ImGui.Button(HellionStrings.Export_Button))
                    PromptExport();
            }

            if (ExportRunning)
                ImGuiUtil.HelpText(HellionStrings.Export_Running);
        }
    }

    private void PromptExport()
    {
        var defaultName = $"hellion-chat-export-{DateTimeOffset.Now:yyyyMMdd-HHmm}";
        var ext = ExportFormat.Extension();

        Plugin.FileDialogManager.SaveFileDialog(
            HellionStrings.Export_Dialog_Title,
            ExportFormat.Filter(),
            defaultName,
            ext,
            (success, path) =>
            {
                if (!success || string.IsNullOrWhiteSpace(path))
                    return;
                StartExport(path);
            });
    }

    private void StartExport(string path)
    {
        if (ExportRunning)
            return;
        ExportRunning = true;

        var types = ExportSelectedChannels.Count > 0
            ? ExportSelectedChannels.Select(t => (int)(ushort)t).ToList()
            : null;

        DateTimeOffset? from = ExportRangeDays > 0
            ? DateTimeOffset.UtcNow.AddDays(-ExportRangeDays)
            : null;

        var senderSubstring = string.IsNullOrWhiteSpace(ExportSenderSubstring) ? null : ExportSenderSubstring.Trim();
        var format = ExportFormat;
        var filterDesc = new MessageExporter.FilterDescription(types, from, null, senderSubstring);

        new Thread(() =>
        {
            try
            {
                using var enumerator = Plugin.MessageManager.Store.StreamForExport(types, from, null);
                var written = MessageExporter.ExportToFile(path, format, enumerator, filterDesc);

                if (written > 0)
                    WrapperUtil.AddNotification(string.Format(HellionStrings.Export_Success, written, path), NotificationType.Success);
                else
                    WrapperUtil.AddNotification(HellionStrings.Export_Empty, NotificationType.Info);
            }
            catch (Exception e)
            {
                Plugin.Log.Error(e, "Export failed");
                WrapperUtil.AddNotification(HellionStrings.Export_Error, NotificationType.Error);
            }
            finally
            {
                ExportRunning = false;
            }
        }) { IsBackground = true }.Start();
    }

    private void DrawRetentionSection()
    {
        ImGui.TextUnformatted(HellionStrings.Retention_Heading);
        using (ImRaii.PushIndent(ImGui.GetStyle().IndentSpacing, false))
        {
            ImGuiUtil.OptionCheckbox(
                ref Mutable.RetentionEnabled,
                HellionStrings.Retention_Enabled_Name,
                HellionStrings.Retention_Enabled_Description);

            using (ImRaii.Disabled(!Mutable.RetentionEnabled))
            {
                ImGui.Spacing();

                var defaultDays = Mutable.RetentionDefaultDays;
                if (ImGui.InputInt(HellionStrings.Retention_Default_Label, ref defaultDays))
                    Mutable.RetentionDefaultDays = Math.Max(0, defaultDays);
                ImGuiUtil.HelpText(HellionStrings.Retention_Default_Help);

                ImGui.Spacing();

                if (ImGui.Button(HellionStrings.Retention_Reset_Spec))
                {
                    Mutable.RetentionPerChannelDays =
                        PrivacyDefaults.DefaultRetentionDays.ToDictionary(p => p.Key, p => p.Value);
                }
                ImGui.SameLine();
                if (ImGui.Button(HellionStrings.Retention_Clear_Overrides))
                    Mutable.RetentionPerChannelDays.Clear();

                ImGui.Spacing();

                using (var tree = ImRaii.TreeNode(HellionStrings.Retention_Tree_Heading))
                {
                    if (tree.Success)
                    {
                        using (ImRaii.PushIndent(ImGui.GetStyle().IndentSpacing, false))
                        foreach (var (heading, types) in Groups)
                        {
                            using var subTree = ImRaii.TreeNode(heading());
                            if (!subTree.Success)
                                continue;

                            using (ImRaii.PushIndent(ImGui.GetStyle().IndentSpacing, false))
                            foreach (var type in types)
                            {
                                var hasOverride = Mutable.RetentionPerChannelDays.TryGetValue(type, out var days);
                                var hasSpecDefault = PrivacyDefaults.DefaultRetentionDays.TryGetValue(type, out var specDays);
                                if (!hasOverride)
                                    days = hasSpecDefault ? specDays : Mutable.RetentionDefaultDays;

                                var tag = hasOverride
                                    ? HellionStrings.Retention_Tag_Override
                                    : hasSpecDefault
                                        ? HellionStrings.Retention_Tag_Spec
                                        : HellionStrings.Retention_Tag_Global;
                                if (ImGui.InputInt($"{type} {tag}##retention-{(int)type}", ref days))
                                {
                                    days = Math.Max(0, days);
                                    Mutable.RetentionPerChannelDays[type] = days;
                                }

                                if (hasOverride)
                                {
                                    ImGui.SameLine();
                                    if (ImGui.Button($"{HellionStrings.Retention_Reset_Button}##retention-reset-{(int)type}"))
                                        Mutable.RetentionPerChannelDays.Remove(type);
                                }
                            }
                        }
                    }
                }

                ImGui.Spacing();

                using (ImRaii.Disabled(RetentionRunning))
                {
                    if (ImGuiUtil.CtrlShiftButton(HellionStrings.Retention_Apply_Label, HellionStrings.Retention_Apply_Tooltip))
                        StartRetentionRun();
                }

                if (RetentionRunning)
                    ImGuiUtil.HelpText(HellionStrings.Retention_Running);

                ImGui.Spacing();
                var lastRun = Plugin.Config.RetentionLastRunAt;
                ImGuiUtil.HelpText(lastRun == DateTimeOffset.MinValue
                    ? HellionStrings.Retention_LastRun_Never
                    : string.Format(HellionStrings.Retention_LastRun_At, lastRun.ToLocalTime()));
            }
        }
    }

    private void StartRetentionRun()
    {
        if (RetentionRunning)
            return;

        RetentionRunning = true;
        var policy = Plugin.Config.RetentionPerChannelDays.ToDictionary(p => (int)(ushort)p.Key, p => p.Value);
        var defaultDays = Plugin.Config.RetentionDefaultDays;

        new Thread(() =>
        {
            try
            {
                var deleted = Plugin.MessageManager.Store.DeleteByRetentionPolicy(policy, defaultDays);
                Plugin.Config.RetentionLastRunAt = DateTimeOffset.UtcNow;
                Plugin.SaveConfig();

                Plugin.Log.Information($"Manual retention run deleted {deleted} expired messages.");

                if (deleted > 0)
                {
                    Plugin.Framework.Run(() =>
                    {
                        Plugin.MessageManager.ClearAllTabs();
                        Plugin.MessageManager.FilterAllTabsAsync();
                    }).Wait();
                }

                WrapperUtil.AddNotification(string.Format(HellionStrings.Retention_Success, deleted), NotificationType.Success);
            }
            catch (Exception e)
            {
                Plugin.Log.Error(e, "Manual retention run failed");
                WrapperUtil.AddNotification(HellionStrings.Retention_Error, NotificationType.Error);
            }
            finally
            {
                RetentionRunning = false;
            }
        }) { IsBackground = true }.Start();
    }

    private void DrawCleanupSection()
    {
        ImGui.TextUnformatted(HellionStrings.Cleanup_Heading);
        using (ImRaii.PushIndent(ImGui.GetStyle().IndentSpacing, false))
        {
            ImGuiUtil.HelpText(HellionStrings.Cleanup_Help_Intro);
            ImGuiUtil.HelpText(HellionStrings.Cleanup_Help_SavedNote);

            ImGui.Spacing();

            using (ImRaii.Disabled(CleanupRunning))
            {
                if (ImGui.Button(HellionStrings.Cleanup_RefreshPreview))
                    RefreshCleanupPreview();
            }

            if (CleanupCounts is null)
            {
                ImGuiUtil.HelpText(HellionStrings.Cleanup_NoPreview);
                return;
            }

            ImGui.Spacing();
            ImGuiUtil.HelpText(string.Format(HellionStrings.Cleanup_TotalStored, CleanupKeepCount + CleanupDeleteCount));
            ImGuiUtil.HelpText(string.Format(HellionStrings.Cleanup_WillKeep, CleanupKeepCount));
            ImGuiUtil.HelpText(string.Format(HellionStrings.Cleanup_WillDelete, CleanupDeleteCount));

            using (var tree = ImRaii.TreeNode(HellionStrings.Cleanup_Breakdown))
            {
                if (tree.Success)
                {
                    using (ImRaii.PushIndent(ImGui.GetStyle().IndentSpacing, false))
                    foreach (var (chatType, count) in CleanupCounts.OrderByDescending(p => p.Value))
                    {
                        var name = Enum.IsDefined(typeof(ChatType), (ushort)chatType)
                            ? ((ChatType)(ushort)chatType).ToString()
                            : $"Unknown({chatType})";
                        var keeps = WouldBeKept(chatType);
                        var marker = keeps ? HellionStrings.Cleanup_Marker_Keep : HellionStrings.Cleanup_Marker_Delete;
                        ImGuiUtil.HelpText($"{marker} {name} — {count:N0}");
                    }
                }
            }

            ImGui.Spacing();

            using (ImRaii.Disabled(CleanupRunning || CleanupDeleteCount == 0))
            {
                if (ImGuiUtil.CtrlShiftButton(HellionStrings.Cleanup_Apply_Label,
                        string.Format(HellionStrings.Cleanup_Apply_Tooltip, CleanupDeleteCount)))
                    StartCleanup();
            }

            if (CleanupRunning)
                ImGuiUtil.HelpText(HellionStrings.Cleanup_Running);
        }
    }

    private bool WouldBeKept(int chatType)
    {
        if (!Plugin.Config.PrivacyFilterEnabled)
            return true;
        if (Plugin.Config.PrivacyPersistChannels.Contains((ChatType)(ushort)chatType))
            return true;
        return Plugin.Config.PrivacyPersistUnknownChannels;
    }

    private void RefreshCleanupPreview()
    {
        try
        {
            CleanupCounts = Plugin.MessageManager.Store.GetMessageCountsByChatType();
            CleanupKeepCount = 0;
            CleanupDeleteCount = 0;
            foreach (var (chatType, count) in CleanupCounts)
            {
                if (WouldBeKept(chatType))
                    CleanupKeepCount += count;
                else
                    CleanupDeleteCount += count;
            }
        }
        catch (Exception e)
        {
            Plugin.Log.Error(e, "Failed to compute cleanup preview");
            WrapperUtil.AddNotification(HellionStrings.Cleanup_PreviewError, NotificationType.Error);
        }
    }

    private void StartCleanup()
    {
        if (CleanupRunning)
            return;

        CleanupRunning = true;
        var allowed = Plugin.Config.PrivacyPersistChannels.Select(t => (int)(ushort)t).ToList();

        new Thread(() =>
        {
            try
            {
                var deleted = Plugin.MessageManager.Store.CleanupRetainOnly(allowed);
                Plugin.Log.Information($"Privacy cleanup: deleted {deleted} messages");

                Plugin.Framework.Run(() =>
                {
                    Plugin.MessageManager.ClearAllTabs();
                    Plugin.MessageManager.FilterAllTabsAsync();
                }).Wait();

                WrapperUtil.AddNotification(string.Format(HellionStrings.Cleanup_Success, deleted), NotificationType.Success);
            }
            catch (Exception e)
            {
                Plugin.Log.Error(e, "Privacy cleanup failed");
                WrapperUtil.AddNotification(HellionStrings.Cleanup_Error, NotificationType.Error);
            }
            finally
            {
                CleanupRunning = false;
                CleanupCounts = null;
            }
        }).Start();
    }
}
