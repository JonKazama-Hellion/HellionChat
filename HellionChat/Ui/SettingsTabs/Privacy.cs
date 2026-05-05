using HellionChat.Code;
using HellionChat.Export;
using HellionChat.Privacy;
using HellionChat.Resources;
using HellionChat.Util;
using Dalamud.Interface.Colors;
using Dalamud.Interface.ImGuiNotification;
using Dalamud.Interface.Utility;
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

    private Dictionary<int, long>? CleanupCounts;
    private long CleanupKeepCount;
    private long CleanupDeleteCount;
    private bool CleanupRunning;
    private bool CleanupPreviewStale;
    private HashSet<ChatType>? CleanupPreviewSnapshot;

    // The retention-running state lives on Plugin so the auto-sweep and
    // this manual button see the same flag. UI reads stay lock-free
    // because ImGui is single-threaded and bool reads are atomic in .NET.
    private bool RetentionRunning => Plugin.RetentionSweepRunning;

    // Export form state
    private int ExportRangeDays = 30;
    private string ExportSenderSubstring = string.Empty;
    private readonly HashSet<ChatType> ExportSelectedChannels = [];
    private ExportFormat ExportFormat = ExportFormat.Markdown;
    private bool ExportRunning;

    public void Draw(bool changed)
    {
        if (ImGui.Button(HellionStrings.Wizard_Reopen_Button))
            Plugin.FirstRunWizard.IsOpen = true;
        ImGui.Spacing();

        DrawPrivacyFilterSection();

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

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        DrawAutoTellTabsPreloadSection();
    }

    private void DrawAutoTellTabsPreloadSection()
    {
        using var tree = ImRaii.TreeNode(HellionStrings.Privacy_AutoTellTabs_Section_Title);
        if (!tree.Success)
            return;

        using (ImRaii.PushIndent(ImGui.GetStyle().IndentSpacing, false))
        {
            var preload = Mutable.AutoTellTabsHistoryPreload;
            ImGui.SetNextItemWidth(200f * ImGuiHelpers.GlobalScale);
            if (ImGui.SliderInt(HellionStrings.Privacy_AutoTellTabs_Preload_Name, ref preload, 0, 100))
            {
                Mutable.AutoTellTabsHistoryPreload = preload;
            }
            ImGuiUtil.HelpMarker(HellionStrings.Privacy_AutoTellTabs_Preload_Description);

            ImGui.Spacing();
            ImGuiUtil.HelpText(HellionStrings.Privacy_AutoTellTabs_Preload_Hint);
        }
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
                ImGuiUtil.HelpMarker(HellionStrings.Retention_Default_Help);

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

                ImGuiUtil.HelpText(HellionStrings.Retention_Help_SavedNote);
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
        // Take the shared retention lock so we cannot fight the auto-sweep
        // for the database connection. If the auto-sweep is already in
        // flight we just bail — the user can press the button again once
        // it finishes.
        lock (Plugin.RetentionSweepLock)
        {
            if (Plugin.RetentionSweepRunning)
                return;
            Plugin.RetentionSweepRunning = true;
        }

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
                    // Bound the wait so a hung framework tick can't deadlock
                    // the background retention worker. Five seconds is well
                    // beyond a normal frame; if we time out we log and let
                    // the next FilterAllTabsAsync call recover the state.
                    if (!Plugin.Framework.Run(() =>
                        {
                            Plugin.MessageManager.ClearAllTabs();
                            Plugin.MessageManager.FilterAllTabsAsync();
                        }).Wait(TimeSpan.FromSeconds(5)))
                    {
                        Plugin.Log.Warning("Retention sweep: framework refresh timed out after 5s.");
                    }
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
                lock (Plugin.RetentionSweepLock)
                    Plugin.RetentionSweepRunning = false;
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

            // Drift-detection between the snapshot taken at last refresh
            // and the current Mutable whitelist. Cleanup itself runs on
            // the SAVED policy (Cleanup_Help_SavedNote covers that), but
            // the user usually expects "the preview reflects what I just
            // ticked" — so we surface the divergence instead of silently
            // showing stale numbers.
            if (CleanupPreviewSnapshot is not null
                && !CleanupPreviewSnapshot.SetEquals(Mutable.PrivacyPersistChannels))
            {
                CleanupPreviewStale = true;
            }

            using (var emphasis = CleanupPreviewStale
                       ? ImRaii.PushColor(ImGuiCol.Button, ImGuiColors.HealerGreen with { W = 0.6f })
                       : null)
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

            if (CleanupPreviewStale)
            {
                ImGui.Spacing();
                ImGuiUtil.HelpText(HellionStrings.Cleanup_Preview_Stale);
            }

            ImGui.Spacing();

            using (var staleColor = CleanupPreviewStale
                       ? ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.DalamudGrey)
                       : null)
            {
                ImGuiUtil.HelpText(string.Format(HellionStrings.Cleanup_TotalStored, CleanupKeepCount + CleanupDeleteCount));
                ImGuiUtil.HelpText(string.Format(HellionStrings.Cleanup_WillKeep, CleanupKeepCount));
                ImGuiUtil.HelpText(string.Format(HellionStrings.Cleanup_WillDelete, CleanupDeleteCount));
            }

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

            // Snapshot the whitelist as it stood at preview-time so the
            // render pass can flag the user about subsequent edits. Only
            // updated on success — if the preview throws, the previous
            // snapshot stays in place so stale-detection keeps working.
            CleanupPreviewSnapshot = new HashSet<ChatType>(Mutable.PrivacyPersistChannels);
            CleanupPreviewStale = false;
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

        var thread = new Thread(() =>
        {
            try
            {
                var deleted = Plugin.MessageManager.Store.CleanupRetainOnly(allowed);
                Plugin.Log.Information($"Privacy cleanup: deleted {deleted} messages");

                // Bound the wait so a hung framework tick can't deadlock
                // the background cleanup worker. See the matching comment in
                // the retention path above for rationale.
                // Note: FilterAllTabs() is called synchronously instead of
                // FilterAllTabsAsync() — the async variant fires-and-forgets
                // a Task.Run, so the .Wait() would return before the filter
                // pass actually finishes. See AUDIT-2026-05-05 [QUAL-02].
                if (!Plugin.Framework.Run(() =>
                    {
                        Plugin.MessageManager.ClearAllTabs();
                        Plugin.MessageManager.FilterAllTabs();
                    }).Wait(TimeSpan.FromSeconds(5)))
                {
                    Plugin.Log.Warning("Privacy cleanup: framework refresh timed out after 5s.");
                }

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
        });
        // Background thread so a still-running cleanup doesn't hold up FFXIV exit.
        thread.IsBackground = true;
        thread.Start();
    }
}
