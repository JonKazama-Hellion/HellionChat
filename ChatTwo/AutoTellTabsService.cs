using System;
using System.Linq;

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
        // Stub — implemented in Task 8.
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
        // Stub — implemented in Task 10.
    }
}
