using System;
using System.Collections.Generic;
using System.Linq;
using ChatTwo.Code;
using ChatTwo.GameFunctions.Types;
using ChatTwo.Resources;
using ChatTwo.Util;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;

namespace ChatTwo;

// Hellion Chat — Auto-Tell-Tabs.
//
// Spawns a session-only tab per /tell partner so a club greeter can track
// multiple parallel conversations without losing context. Subscribes to
// MessageManager.MessageProcessed for live tells and to ClientState.Logout
// for the cleanup pass; everything else hangs off these two entry points.
//
// See spec: Hellion Chat Auto-Tell-Tabs Spec (Obsidian vault).
internal sealed class AutoTellTabsService : IDisposable
{
    private readonly Plugin _plugin;
    private readonly MessageManager _messageManager;
    private readonly MessageStore _store;
    private readonly object _tempTabsLock = new();

    private bool _initialized;

    internal AutoTellTabsService(Plugin plugin, MessageManager messageManager, MessageStore store)
    {
        _plugin = plugin;
        _messageManager = messageManager;
        _store = store;
    }

    internal int ActiveTempTabCount
    {
        get
        {
            lock (_tempTabsLock)
            {
                return Plugin.Config.Tabs.Count(t => t.IsTempTab);
            }
        }
    }

    internal void Initialize()
    {
        if (_initialized)
        {
            return;
        }

        _messageManager.MessageProcessed += HandleTell;
        Plugin.ClientState.Logout += OnLogout;
        _initialized = true;
    }

    public void Dispose()
    {
        if (!_initialized)
        {
            return;
        }

        Plugin.ClientState.Logout -= OnLogout;
        _messageManager.MessageProcessed -= HandleTell;
        _initialized = false;
    }

    internal void HandleTell(Message message)
    {
        if (!Plugin.Config.EnableAutoTellTabs)
        {
            return;
        }

        if (message.Code.Type != ChatType.TellIncoming && message.Code.Type != ChatType.TellOutgoing)
        {
            return;
        }

        var partner = ExtractTellPartner(message);
        if (partner == null)
        {
            // Real message without a player payload — e.g. GM tells, which
            // we deliberately skip. The diagnostics make future regressions
            // (FFXIV changing tell payload shape, new edge cases) findable
            // without having to crank up debug logging at the source.
            Plugin.Log.Warning(
                $"[AutoTellTabs] Could not extract tell partner. type={message.Code.Type}, " +
                $"senderChunks={message.Sender.Count}, contentChunks={message.Content.Count}, " +
                $"senderSourcePayloads={message.SenderSource?.Payloads?.Count ?? 0}, " +
                $"contentSourcePayloads={message.ContentSource?.Payloads?.Count ?? 0}");
            return;
        }

        lock (_tempTabsLock)
        {
            var existing = FindTempTab(partner.Value.Name, partner.Value.World);
            if (existing != null)
            {
                // Tab already exists; Tab.Matches has already routed this
                // message via the MessageManager pipeline (see Task 2 sender
                // filter).
                return;
            }

            if (ActiveTempTabCount >= Plugin.Config.AutoTellTabsLimit)
            {
                DropOldestTempTab();
            }

            SpawnTempTab(partner.Value, message);
        }
    }

    private (string Name, uint World)? ExtractTellPartner(Message message)
    {
        if (message.Code.Type == ChatType.TellIncoming)
        {
            // Incoming tell: the sender is the conversation partner. The
            // PlayerPayload normally rides on a chunk's Link slot, but for
            // some tell types FFXIV only puts it in the raw SeString —
            // fall back to that before giving up.
            var fromSender = ChunkUtil.TryGetPlayerPayload(message.Sender)
                             ?? ChunkUtil.TryGetPlayerPayload(message.SenderSource);
            if (fromSender != null)
            {
                return (fromSender.PlayerName, fromSender.World.RowId);
            }
            return null;
        }

        // Outgoing tell: the local player is the sender, the partner shows
        // up either as a payload in the content (for tells typed via the
        // Chat 2 input bar) or as the channel's tracked tell target (set by
        // the SetContextTellTarget game hook). Same SeString fallback.
        var fromContent = ChunkUtil.TryGetPlayerPayload(message.Content)
                          ?? ChunkUtil.TryGetPlayerPayload(message.ContentSource)
                          ?? ChunkUtil.TryGetPlayerPayload(message.Sender)
                          ?? ChunkUtil.TryGetPlayerPayload(message.SenderSource);
        if (fromContent != null)
        {
            return (fromContent.PlayerName, fromContent.World.RowId);
        }

        var current = _plugin.CurrentTab.CurrentChannel.TellTarget
                      ?? _plugin.CurrentTab.CurrentChannel.TempTellTarget;
        if (current != null && current.IsSet())
        {
            return (current.Name, current.World);
        }

        return null;
    }

    private Tab? FindTempTab(string name, uint world)
    {
        return Plugin.Config.Tabs.FirstOrDefault(t =>
            t.IsTempTab
            && t.TellTarget != null
            && string.Equals(t.TellTarget.Name, name, StringComparison.OrdinalIgnoreCase)
            && t.TellTarget.World == world);
    }

    private void DropOldestTempTab()
    {
        // Greeted tabs are dropped before un-greeted ones (the user said
        // "I'm done with that conversation"), and within each bucket we
        // pick the oldest LastActivity. This protects active conversations
        // and unfinished greetings while still freeing up a slot.
        var victim = Plugin.Config.Tabs
            .Select((tab, idx) => (Tab: tab, Index: idx))
            .Where(t => t.Tab.IsTempTab)
            .OrderByDescending(t => t.Tab.IsGreeted)
            .ThenBy(t => t.Tab.LastActivity)
            .FirstOrDefault();

        if (victim.Tab == null)
        {
            return;
        }

        // v0.6.1 — if the victim is currently popped out, tear down the
        // matching Popout window first. Otherwise the window stays in
        // PopOutWindows + WindowSystem and renders empty / re-spawns on the
        // next AddPopOutsToDraw tick. Latent since pop-outs were introduced;
        // becomes visible with AutoTellTabsOpenAsPopout where dropping a
        // popped tab is now a routine code path.
        if (victim.Tab.PopOut)
        {
            var popout = _plugin.ChatLogWindow.ActivePopouts
                .FirstOrDefault(p => p.TabIdentifier == victim.Tab.Identifier);
            if (popout != null)
            {
                popout.IsOpen = false;
            }
        }

        Plugin.Config.Tabs.RemoveAt(victim.Index);

        // Re-anchor the active tab so the user does not silently end up on
        // a different conversation when their tab gets dropped or shifted.
        if (victim.Index <= _plugin.LastTab)
        {
            _plugin.WantedTab = 0;
        }
    }

    private void SpawnTempTab((string Name, uint World) partner, Message currentMessage)
    {
        var tab = BuildTempTab(partner.Name, partner.World);

        // Preload first so the tab opens with chronological history above
        // the current message — and so a slow DB query never causes a
        // visible "empty tab, then history pops in" effect on screen.
        // The current message is already persisted in the store by the
        // time MessageProcessed fires (see MessageManager.cs: UpsertMessage
        // runs before the event), so we have to exclude it explicitly to
        // avoid the separator landing below the live tell.
        PreloadHistory(tab, partner.Name, partner.World, currentMessage.Id);

        tab.AddMessage(currentMessage, unread: true);

        // Hellion Chat v0.6.1 — opt-in: open new /tell tabs directly as a
        // pop-out window. Set BEFORE Tabs.Add so the next render-tick's
        // AddPopOutsToDraw() sees PopOut=true and spawns the Popout window
        // alongside the tab going into the list. No SaveConfig() because
        // auto-tell tabs are IsTempTab (session-only, never persisted).
        if (Plugin.Config.AutoTellTabsOpenAsPopout)
        {
            tab.PopOut = true;
        }

        Plugin.Config.Tabs.Add(tab);
    }

    private static Tab BuildTempTab(string playerName, uint worldRowId)
    {
        return new Tab
        {
            Name = FormatTabName(playerName, worldRowId),
            IsTempTab = true,
            AllSenderMessages = true,
            TellTarget = new TellTarget(playerName, worldRowId, 0, TellReason.Direct),
            Channel = InputChannel.Tell,
            DisplayTimestamp = true,
            UnreadMode = UnreadMode.Unseen,
            HideWhenInactive = false,
            SelectedChannels = new Dictionary<ChatType, (ChatSource, ChatSource)>
            {
                [ChatType.TellIncoming] = (ChatSourceExt.All, ChatSourceExt.All),
                [ChatType.TellOutgoing] = (ChatSourceExt.All, ChatSourceExt.All),
            },
        };
    }

    private static string FormatTabName(string playerName, uint worldRowId)
    {
        if (Sheets.WorldSheet.TryGetRow(worldRowId, out var worldRow))
        {
            return $"{playerName}@{worldRow.Name}";
        }
        // World sheet lookup miss is rare (only for FFXIV worlds Dalamud has
        // not yet seen). Fall back to the raw RowId so the user still has a
        // unique, readable label.
        return $"{playerName}@World{worldRowId}";
    }

    private void PreloadHistory(Tab tab, string senderName, uint senderWorld, Guid currentMessageId)
    {
        var preloadCount = Plugin.Config.AutoTellTabsHistoryPreload;
        if (preloadCount <= 0)
        {
            return;
        }

        try
        {
            // Pull one extra row because the live tell that triggered this
            // spawn is already in the store and would otherwise eat one of
            // the user's preload-budget slots.
            var history = _store.GetTellHistoryWithSender(
                _messageManager.CurrentContentId,
                senderName,
                senderWorld,
                preloadCount + 1);

            var historicMessages = history
                .Where(m => m.Id != currentMessageId)
                .Take(preloadCount)
                .ToList();

            if (historicMessages.Count == 0)
            {
                // No prior tells with this player — leave the tab to start
                // empty so the user does not see a "history loaded" marker
                // sitting alone above the very first message.
                return;
            }

            // The history list is already oldest-first, so a plain AddPrune
            // loop produces the chronological order the user expects to see
            // when the tab opens.
            foreach (var message in historicMessages)
            {
                tab.Messages.AddPrune(message, MessageManager.MessageDisplayLimit);
            }

            // Visible separator between the loaded history and the live
            // tell that triggered this spawn. Goes in last so it sorts
            // after the historical messages but before the current one.
            tab.Messages.AddPrune(
                MakeSystemMarker(HellionStrings.AutoTellTabs_HistorySeparator),
                MessageManager.MessageDisplayLimit);
        }
        catch (Exception ex)
        {
            // Non-fatal: the tab still spawns, but the user gets a visible
            // notice instead of silently missing history. The error logs
            // once with full stack trace for diagnosis.
            Plugin.Log.Error(ex, "[AutoTellTabs] History preload failed");
            tab.Messages.AddPrune(
                MakeSystemMarker(HellionStrings.AutoTellTabs_HistoryLoadError),
                MessageManager.MessageDisplayLimit);
        }
    }

    private static Message MakeSystemMarker(string text)
    {
        var seString = new SeStringBuilder().AddText(text).Build();
        var chunks = ChunkUtil.ToChunks(seString, ChunkSource.Content, ChatType.System).ToList();
        var code = new ChatCode((XivChatType)ChatType.System, 0, 0);
        return Message.FakeMessage(chunks, code);
    }

    internal void MarkGreeted(Tab tab)
    {
        SetGreeted(tab, true);
    }

    internal void UnmarkGreeted(Tab tab)
    {
        SetGreeted(tab, false);
    }

    internal bool IsGreeted(Tab tab)
    {
        return tab.IsGreeted;
    }

    private void SetGreeted(Tab tab, bool greeted)
    {
        if (tab == null)
        {
            return;
        }

        lock (_tempTabsLock)
        {
            // Frame-race guard (E5): the sidebar might still render a tab
            // that has already been removed by LRU drop or logout cleanup.
            // Silently skip the toggle so we don't mutate stale state.
            if (!Plugin.Config.Tabs.Contains(tab))
            {
                return;
            }

            tab.IsGreeted = greeted;
        }
    }

    private void OnLogout(int type, int code)
    {
        lock (_tempTabsLock)
        {
            // Snapshot whether the active tab is about to be removed, BEFORE
            // we mutate the list — index lookups would lie to us afterwards.
            var lastIndex = _plugin.LastTab;
            var lastIndexValid = lastIndex >= 0 && lastIndex < Plugin.Config.Tabs.Count;
            var currentWasTempTab = lastIndexValid && Plugin.Config.Tabs[lastIndex].IsTempTab;

            // v0.6.1 — symmetric to DropOldestTempTab cleanup: tear down any
            // popped-out temp tab windows before removing the tabs themselves,
            // otherwise PopOutWindows + WindowSystem keep ghost entries until
            // the next plugin reload. Especially relevant once Auto-Pop-Out is
            // enabled — every logout would otherwise leak as many ghosts as
            // there were active /tell pop-outs.
            var poppedTempTabIds = Plugin.Config.Tabs
                .Where(t => t.IsTempTab && t.PopOut)
                .Select(t => t.Identifier)
                .ToList();
            if (poppedTempTabIds.Count > 0)
            {
                var poppedSet = poppedTempTabIds.ToHashSet();
                foreach (var popout in _plugin.ChatLogWindow.ActivePopouts
                             .Where(p => poppedSet.Contains(p.TabIdentifier))
                             .ToList())
                {
                    popout.IsOpen = false;
                }
            }

            Plugin.Config.Tabs.RemoveAll(t => t.IsTempTab);

            // Force a switch to tab 0 if the active tab was a temp tab OR
            // if drops before the active index pushed LastTab out of range.
            // Otherwise the user keeps their current persistent tab.
            var stillValid = lastIndex >= 0 && lastIndex < Plugin.Config.Tabs.Count;
            if (currentWasTempTab || !stillValid)
            {
                _plugin.WantedTab = 0;
            }
        }
    }
}
