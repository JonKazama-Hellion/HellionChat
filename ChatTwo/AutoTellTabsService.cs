using System;
using System.Linq;
using ChatTwo.Code;
using ChatTwo.Util;

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
            // we deliberately skip. Warn once so we notice future regressions.
            Plugin.Log.Warning("[AutoTellTabs] Could not extract tell partner from message; skipping spawn.");
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
            // Incoming tell: the sender is the conversation partner.
            var fromSender = ChunkUtil.TryGetPlayerPayload(message.Sender);
            if (fromSender != null)
            {
                return (fromSender.PlayerName, fromSender.World.RowId);
            }
            return null;
        }

        // Outgoing tell: the local player is the sender, the partner shows
        // up either as a payload in the content (for tells typed via the
        // Chat 2 input bar) or as the channel's tracked tell target (set by
        // the SetContextTellTarget game hook).
        var fromContent = ChunkUtil.TryGetPlayerPayload(message.Content);
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
        // Stub — implemented in Task 11.
    }

    internal void MarkGreeted(Tab tab)
    {
        // Stub — implemented in Task 12.
    }

    internal void UnmarkGreeted(Tab tab)
    {
        // Stub — implemented in Task 12.
    }

    internal bool IsGreeted(Tab tab)
    {
        return tab.IsGreeted;
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
