using System.Diagnostics;
using HellionChat.Code;
using HellionChat.Export;
using HellionChat.Privacy;
using HellionChat.Resources;
using HellionChat.Util;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface.Colors;
using Dalamud.Interface.ImGuiNotification;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Bindings.ImGui;

namespace HellionChat.Ui.SettingsTabs;

internal sealed class DataManagement : ISettingsTab
{
    private Plugin Plugin { get; }
    private Configuration Mutable { get; }

    public string Name => HellionStrings.Settings_Card_DataManagement_Title + "###tabs-datamanagement";

    // Cleanup state (was in Privacy.cs)
    private Dictionary<int, long>? CleanupCounts;
    private long CleanupKeepCount;
    private long CleanupDeleteCount;
    private bool CleanupRunning;
    private bool CleanupPreviewStale;
    private HashSet<ChatType>? CleanupPreviewSnapshot;
    private bool RetentionRunning => Plugin.RetentionSweepRunning;

    // Export form state (was in Privacy.cs)
    private int ExportRangeDays = 30;
    private string ExportSenderSubstring = string.Empty;
    private readonly HashSet<ChatType> ExportSelectedChannels = [];
    private ExportFormat ExportFormat = ExportFormat.Markdown;
    private bool ExportRunning;

    // DB-Viewer + Advanced state (was in Database.cs)
    private bool ShowAdvanced;
    private long DatabaseLastRefreshTicks;
    private long DatabaseSize;
    private long DatabaseLogSize;
    private int DatabaseMessageCount;

    // Channel groupings shared by Cleanup-Breakdown, Retention and Export
    // sections. Heading is resolved per-frame so a runtime LanguageChanged
    // call updates the labels immediately. 1:1 from Privacy.cs Groups.
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

    internal DataManagement(Plugin plugin, Configuration mutable)
    {
        Plugin = plugin;
        Mutable = mutable;
    }

    public void Draw(bool changed)
    {
        // Shift-on-open keeps the Advanced tools available without a permanent
        // toggle in the UI, mirroring upstream Chat 2 behaviour.
        if (changed)
            ShowAdvanced = ImGui.GetIO().KeyShift;

        DrawStorageSection();
        ImGui.Spacing();
        DrawRetentionSection();
        ImGui.Spacing();
        DrawCleanupSection();
        ImGui.Spacing();
        DrawExportSection();
        ImGui.Spacing();
        DrawDatabaseViewerSection();
        ImGui.Spacing();
        DrawAdvancedSection();
    }

    private void DrawStorageSection()
    {
        using var tree = ImRaii.TreeNode(HellionStrings.Settings_DataManagement_Storage_Heading);
        if (!tree.Success)
            return;

        using (ImRaii.PushIndent(ImGui.GetStyle().IndentSpacing, false))
        {
            ImGui.Checkbox(Language.Options_DatabaseBattleMessages_Name, ref Mutable.DatabaseBattleMessages);
            ImGuiUtil.HelpMarker(Language.Options_DatabaseBattleMessages_Description);

            if (ImGui.Checkbox(Language.Options_LoadPreviousSession_Name, ref Mutable.LoadPreviousSession))
                if (Mutable.LoadPreviousSession)
                    Mutable.FilterIncludePreviousSessions = true;
            ImGuiUtil.HelpMarker(Language.Options_LoadPreviousSession_Description);

            if (ImGui.Checkbox(Language.Options_FilterIncludePreviousSessions_Name, ref Mutable.FilterIncludePreviousSessions))
                if (!Mutable.FilterIncludePreviousSessions)
                    Mutable.LoadPreviousSession = false;
            ImGuiUtil.HelpMarker(Language.Options_FilterIncludePreviousSessions_Description);

            var old = new FileInfo(Path.Join(Plugin.Interface.ConfigDirectory.FullName, "chat.db"));
            var migratedOld = new FileInfo(Path.Join(Plugin.Interface.ConfigDirectory.FullName, "chat-litedb.db"));
            if (old.Exists || migratedOld.Exists)
            {
                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();

                ImGui.TextUnformatted(Language.Options_Database_Old_Heading);
                ImGui.Spacing();

                if (ImGuiUtil.CtrlShiftButton(Language.Options_Database_Old_Delete, Language.Options_Database_Old_Delete_Tooltip))
                {
                    try
                    {
                        if (old.Exists)
                            old.Delete();
                        if (migratedOld.Exists)
                            migratedOld.Delete();
                        WrapperUtil.AddNotification(Language.Options_Database_Old_Delete_Success, NotificationType.Success);
                    }
                    catch (Exception e)
                    {
                        Plugin.Log.Error(e, "Unable to delete old database");
                        WrapperUtil.AddNotification(Language.Options_Database_Old_Delete_Error, NotificationType.Error);
                    }
                }
            }
        }
    }

    private void DrawRetentionSection()
    {
        using var tree = ImRaii.TreeNode(HellionStrings.Settings_DataManagement_Retention_Heading);
        if (!tree.Success)
            return;

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

                using (var perChannelTree = ImRaii.TreeNode(HellionStrings.Retention_Tree_Heading))
                {
                    if (perChannelTree.Success)
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
        using var tree = ImRaii.TreeNode(HellionStrings.Settings_DataManagement_Cleanup_Heading);
        if (!tree.Success)
            return;

        using (ImRaii.PushIndent(ImGui.GetStyle().IndentSpacing, false))
        {
            ImGuiUtil.HelpText(HellionStrings.Cleanup_Help_Intro);
            ImGuiUtil.HelpText(HellionStrings.Cleanup_Help_SavedNote);

            ImGui.Spacing();

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

            using (var breakdownTree = ImRaii.TreeNode(HellionStrings.Cleanup_Breakdown))
            {
                if (breakdownTree.Success)
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
        thread.IsBackground = true;
        thread.Start();
    }

    private void DrawExportSection()
    {
        using var tree = ImRaii.TreeNode(HellionStrings.Settings_DataManagement_Export_Heading);
        if (!tree.Success)
            return;

        using (ImRaii.PushIndent(ImGui.GetStyle().IndentSpacing, false))
        {
            ImGuiUtil.HelpText(HellionStrings.Export_Help);

            ImGui.Spacing();

            if (ImGui.InputInt(HellionStrings.Export_Range_Label, ref ExportRangeDays))
                ExportRangeDays = Math.Max(0, ExportRangeDays);

            ImGui.InputText(HellionStrings.Export_Sender_Label, ref ExportSenderSubstring, 256);

            using (var channelsTree = ImRaii.TreeNode(HellionStrings.Export_Channels_Heading))
            {
                if (channelsTree.Success)
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

    private void DrawDatabaseViewerSection()
    {
        using var tree = ImRaii.TreeNode(HellionStrings.Settings_DataManagement_DbViewer_Heading);
        if (!tree.Success)
            return;

        using (ImRaii.PushIndent(ImGui.GetStyle().IndentSpacing, false))
        {
            if (DatabaseLastRefreshTicks + 5 * 1000 < Environment.TickCount64)
            {
                DatabaseSize = Plugin.MessageManager.Store.DatabaseSize();
                DatabaseLogSize = Plugin.MessageManager.Store.DatabaseLogSize();
                DatabaseMessageCount = Plugin.MessageManager.Store.MessageCount();
                DatabaseLastRefreshTicks = Environment.TickCount64;
            }

            ImGuiUtil.HelpText(string.Format(Language.Options_Database_Metadata_Path, MessageManager.DatabasePath()));
            if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
            {
                var path = Path.GetDirectoryName(MessageManager.DatabasePath());
                ImGui.SetClipboardText(path);
                WrapperUtil.AddNotification(Language.Options_Database_Metadata_CopyConfigPathNotification, NotificationType.Info);
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                ImGuiUtil.Tooltip(Language.Options_Database_Metadata_CopyConfigPath);
            }

            ImGuiUtil.HelpText(string.Format(Language.Options_Database_Metadata_Size, StringUtil.BytesToString(DatabaseSize)));
            if (ImGui.IsItemHovered())
                ImGuiUtil.Tooltip(StringUtil.BytesToString(DatabaseSize));

            ImGuiUtil.HelpText(string.Format(Language.Options_Database_Metadata_LogSize, StringUtil.BytesToString(DatabaseLogSize)));
            if (ImGui.IsItemHovered())
                ImGuiUtil.Tooltip(StringUtil.BytesToString(DatabaseLogSize));

            ImGuiUtil.HelpText(string.Format(Language.Options_Database_Metadata_MessageCount, DatabaseMessageCount));

            if (ImGuiUtil.CtrlShiftButton(Language.Options_ClearDatabase_Button, Language.Options_ClearDatabase_Tooltip))
            {
                Plugin.Log.Warning("Clearing messages from database");
                Plugin.MessageManager.Store.ClearMessages();
                Plugin.MessageManager.ClearAllTabs();

                DatabaseLastRefreshTicks = 0;
                WrapperUtil.AddNotification(Language.Options_ClearDatabase_Success, NotificationType.Info);
            }
        }
    }

    private void DrawAdvancedSection()
    {
        if (!ShowAdvanced)
            return;

        using var tree = ImRaii.TreeNode(HellionStrings.Settings_DataManagement_Advanced_Heading);
        if (!tree.Success)
            return;

        using (ImRaii.PushIndent(ImGui.GetStyle().IndentSpacing, false))
        {
            using var wrap = ImRaii.TextWrapPos(0.0f);

            ImGuiUtil.WarningText(Language.Options_Database_Advanced_Warning);
            if (ImGuiUtil.CtrlShiftButton("Perform maintenance", "Ctrl+Shift: MessageManager.Store.PerformMaintenance()"))
                Plugin.MessageManager.Store.PerformMaintenance();

            if (ImGuiUtil.CtrlShiftButton("Reload messages from database", "Ctrl+Shift: MessageManager.FilterAllTabs()"))
            {
                Plugin.MessageManager.ClearAllTabs();
                Plugin.MessageManager.FilterAllTabsAsync();
            }

            if (ImGuiUtil.CtrlShiftButton("Inject 10,000 messages", "Ctrl+Shift: creates 10,000 unique messages (async)"))
                new Thread(() => InsertMessages(10_000)).Start();
        }
    }

    private void InsertMessages(int count)
    {
        Plugin.Log.Info($"Inserting {count} messages due to user request");

        var stopwatch = Stopwatch.StartNew();
        var playerName = Plugin.PlayerState.CharacterName;
        var worldId = Plugin.PlayerState.HomeWorld.ValueNullable?.RowId ?? 0;
        var senderSource = new SeStringBuilder()
            .AddText("<")
            .Add(new PlayerPayload(playerName, worldId))
            .AddText("Random Message")
            .Add(RawPayload.LinkTerminator)
            .AddText(">: ")
            .Build();
        var senderChunks = ChunkUtil.ToChunks(senderSource, ChunkSource.Sender, ChatType.Debug).ToList();
        var messages = new List<Message>(count);
        for (var i = 0; i < count; i++)
        {
            var contentSource = new SeStringBuilder()
                .AddText("Random message payload - ")
                .AddItalics(Guid.NewGuid().ToString())
                .Build();
            var contentChunks = ChunkUtil.ToChunks(contentSource, ChunkSource.Content, ChatType.Debug).ToList();

            var chatCode = new ChatCode(XivChatType.Say, 0, 0);
            messages.Add(new Message(
                Guid.NewGuid(),
                Plugin.MessageManager.CurrentContentId,
                Plugin.MessageManager.CurrentContentId,
                DateTimeOffset.UtcNow,
                chatCode,
                senderChunks,
                contentChunks,
                senderSource,
                contentSource,
                Guid.Empty
            ));
        }

        var elapsedTicks = stopwatch.ElapsedTicks;
        stopwatch.Stop();
        Plugin.Log.Info($"Crafted {count} messages in {elapsedTicks} ticks ({elapsedTicks / TimeSpan.TicksPerMillisecond}ms)");

        stopwatch = Stopwatch.StartNew();
        foreach (var message in messages)
            Plugin.MessageManager.Store.UpsertMessage(message);

        elapsedTicks = stopwatch.ElapsedTicks;
        stopwatch.Stop();
        Plugin.Log.Info($"Upserted {count} messages in {elapsedTicks} ticks ({elapsedTicks / TimeSpan.TicksPerMillisecond}ms)");

        Plugin.Framework.Run(() =>
        {
            stopwatch = Stopwatch.StartNew();
            Plugin.MessageManager.ClearAllTabs();
            elapsedTicks = stopwatch.ElapsedTicks;
            stopwatch.Stop();
            Plugin.Log.Info($"Cleared {Plugin.Config.Tabs.Count} tabs in {elapsedTicks} ticks ({elapsedTicks / TimeSpan.TicksPerMillisecond}ms)");
        }).Wait();

        Plugin.Framework.Run(() =>
        {
            stopwatch = Stopwatch.StartNew();
            Plugin.MessageManager.FilterAllTabs();
            elapsedTicks = stopwatch.ElapsedTicks;
            stopwatch.Stop();
            Plugin.Log.Info($"Fetched and filtered all tabs in {elapsedTicks} ticks ({elapsedTicks / TimeSpan.TicksPerMillisecond}ms)");
        }).Wait();
    }
}
