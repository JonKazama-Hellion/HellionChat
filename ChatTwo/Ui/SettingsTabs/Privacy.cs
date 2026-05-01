using ChatTwo.Code;
using ChatTwo.Privacy;
using ChatTwo.Util;
using Dalamud.Interface.ImGuiNotification;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Bindings.ImGui;

namespace ChatTwo.Ui.SettingsTabs;

internal sealed class Privacy : ISettingsTab
{
    private Plugin Plugin { get; }
    private Configuration Mutable { get; }

    public string Name => "Privacy###tabs-privacy";

    internal Privacy(Plugin plugin, Configuration mutable)
    {
        Plugin = plugin;
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

    // Cleanup preview state. Held in the tab so the user can refresh and
    // inspect before confirming. Resets when the tab is reopened (acceptable —
    // a stale preview against a freshly-edited whitelist would be misleading).
    private Dictionary<int, long>? CleanupCounts;
    private long CleanupKeepCount;
    private long CleanupDeleteCount;
    private bool CleanupRunning;

    private bool RetentionRunning;

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

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        DrawRetentionSection();

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        DrawCleanupSection();
    }

    private void DrawRetentionSection()
    {
        ImGui.TextUnformatted("Message retention");
        using (ImRaii.PushIndent(ImGui.GetStyle().IndentSpacing, false))
        {
            ImGuiUtil.OptionCheckbox(
                ref Mutable.RetentionEnabled,
                "Auto-delete messages after a per-channel retention window",
                "When enabled, messages older than the configured window are deleted on every plugin start (at most once per 24 hours). " +
                "Off by default — the plugin never deletes history without your explicit consent.");

            using (ImRaii.Disabled(!Mutable.RetentionEnabled))
            {
                ImGui.Spacing();

                var defaultDays = Mutable.RetentionDefaultDays;
                if (ImGui.InputInt("Default retention (days, 0 = never)", ref defaultDays))
                    Mutable.RetentionDefaultDays = Math.Max(0, defaultDays);
                ImGuiUtil.HelpText("Applies to channels without an explicit override below.");

                ImGui.Spacing();

                if (ImGui.Button("Reset overrides to spec defaults"))
                {
                    Mutable.RetentionPerChannelDays =
                        PrivacyDefaults.DefaultRetentionDays.ToDictionary(p => p.Key, p => p.Value);
                }
                ImGui.SameLine();
                if (ImGui.Button("Clear all overrides"))
                    Mutable.RetentionPerChannelDays.Clear();

                ImGui.Spacing();

                using (var tree = ImRaii.TreeNode("Per-channel retention overrides"))
                {
                    if (tree.Success)
                    {
                        using (ImRaii.PushIndent(ImGui.GetStyle().IndentSpacing, false))
                        foreach (var (heading, types) in Groups)
                        {
                            using var subTree = ImRaii.TreeNode(heading);
                            if (!subTree.Success)
                                continue;

                            using (ImRaii.PushIndent(ImGui.GetStyle().IndentSpacing, false))
                            foreach (var type in types)
                            {
                                var hasOverride = Mutable.RetentionPerChannelDays.TryGetValue(type, out var days);
                                if (!hasOverride)
                                    days = Mutable.RetentionDefaultDays;

                                if (ImGui.InputInt($"{type}##retention-{(int)type}", ref days))
                                {
                                    days = Math.Max(0, days);
                                    Mutable.RetentionPerChannelDays[type] = days;
                                }

                                if (hasOverride)
                                {
                                    ImGui.SameLine();
                                    if (ImGui.Button($"reset##retention-reset-{(int)type}"))
                                        Mutable.RetentionPerChannelDays.Remove(type);
                                }
                            }
                        }
                    }
                }

                ImGui.Spacing();

                using (ImRaii.Disabled(RetentionRunning))
                {
                    if (ImGuiUtil.CtrlShiftButton("Apply retention policy now",
                            "Ctrl+Shift: runs the retention sweep immediately using the SAVED policy. Save your changes first."))
                        StartRetentionRun();
                }

                if (RetentionRunning)
                    ImGuiUtil.HelpText("Retention sweep running in background…");

                ImGui.Spacing();
                var lastRun = Plugin.Config.RetentionLastRunAt;
                ImGuiUtil.HelpText(lastRun == DateTimeOffset.MinValue
                    ? "Last run: never"
                    : $"Last run: {lastRun.ToLocalTime():yyyy-MM-dd HH:mm}");
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

                WrapperUtil.AddNotification($"Retention sweep complete: {deleted:N0} messages removed.", NotificationType.Success);
            }
            catch (Exception e)
            {
                Plugin.Log.Error(e, "Manual retention run failed");
                WrapperUtil.AddNotification("Retention sweep failed, see /xllog", NotificationType.Error);
            }
            finally
            {
                RetentionRunning = false;
            }
        }) { IsBackground = true }.Start();
    }

    private void DrawCleanupSection()
    {
        ImGui.TextUnformatted("Apply filter to existing database");
        using (ImRaii.PushIndent(ImGui.GetStyle().IndentSpacing, false))
        {
            ImGuiUtil.HelpText(
                "The privacy filter only applies to new messages. " +
                "Use the cleanup below to retroactively remove already-stored messages " +
                "that don't match your saved whitelist.");
            ImGuiUtil.HelpText(
                "Cleanup uses your SAVED whitelist (Plugin.Config), not unsaved edits above. " +
                "Click Save first if you want to apply your current edits.");

            ImGui.Spacing();

            using (ImRaii.Disabled(CleanupRunning))
            {
                if (ImGui.Button("Refresh preview"))
                    RefreshCleanupPreview();
            }

            if (CleanupCounts is null)
            {
                ImGuiUtil.HelpText("No preview yet. Click Refresh to compute the impact.");
                return;
            }

            ImGui.Spacing();
            ImGuiUtil.HelpText($"Total stored messages: {CleanupKeepCount + CleanupDeleteCount:N0}");
            ImGuiUtil.HelpText($"Will keep:    {CleanupKeepCount:N0}");
            ImGuiUtil.HelpText($"Will delete:  {CleanupDeleteCount:N0}");

            using (var tree = ImRaii.TreeNode("Per-channel breakdown"))
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
                        var marker = keeps ? "[KEEP]  " : "[DELETE]";
                        ImGuiUtil.HelpText($"{marker} {name} — {count:N0}");
                    }
                }
            }

            ImGui.Spacing();

            using (ImRaii.Disabled(CleanupRunning || CleanupDeleteCount == 0))
            {
                if (ImGuiUtil.CtrlShiftButton("Apply current filter to database",
                        $"Ctrl+Shift: Hard-deletes {CleanupDeleteCount:N0} messages, then runs VACUUM. Cannot be undone."))
                    StartCleanup();
            }

            if (CleanupRunning)
                ImGuiUtil.HelpText("Cleanup running in background…");
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
            WrapperUtil.AddNotification("Failed to compute cleanup preview, see /xllog", NotificationType.Error);
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

                WrapperUtil.AddNotification($"Privacy cleanup complete: {deleted:N0} messages removed.", NotificationType.Success);
            }
            catch (Exception e)
            {
                Plugin.Log.Error(e, "Privacy cleanup failed");
                WrapperUtil.AddNotification("Privacy cleanup failed, see /xllog", NotificationType.Error);
            }
            finally
            {
                CleanupRunning = false;
                CleanupCounts = null;
            }
        }).Start();
    }
}
