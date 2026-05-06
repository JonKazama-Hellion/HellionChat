using System;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using Dalamud.Plugin.Services;
using Newtonsoft.Json;

namespace HellionChat.Integrations;

// We pull Newtonsoft.Json into this single file for IPC compatibility:
// Honorific serialises its TitleData with Newtonsoft (see
// Honorific-master/IpcProvider.cs:9 and CustomTitle.cs:12). Using the
// same library guarantees identical handling of System.Numerics.Vector3?
// and the enum fields we ignore. Newtonsoft is a transitive dependency
// via Dalamud, so no new NuGet entry is needed. The rest of HellionChat
// keeps using System.Text.Json.
internal sealed class HonorificService : IDisposable
{
    private const string IpcNamespace = "Honorific";

    // IPC gates we subscribe to. Keep them as fields so Dispose can
    // unsubscribe the same instances we subscribed in the constructor.
    private readonly ICallGateSubscriber<(uint, uint)> _apiVersion;
    private readonly ICallGateSubscriber<string> _getLocalCharacterTitle;
    private readonly ICallGateSubscriber<string, object> _localCharacterTitleChanged;
    private readonly ICallGateSubscriber<object> _ready;
    private readonly ICallGateSubscriber<object> _disposing;

    private readonly IPluginLog _log;
    private readonly IFramework _framework;
    private bool _versionWarningLogged;

    public HonorificTitleData? CurrentTitle { get; private set; }
    public bool IsAvailable { get; private set; }
    public (uint Major, uint Minor)? DetectedApiVersion { get; private set; }

    public HonorificService(IDalamudPluginInterface pluginInterface, IPluginLog log, IFramework framework)
    {
        _framework = framework;
        _log = log;

        // Dalamud caches gate objects per-name for the lifetime of the
        // plugin interface, so we can register subscribers even when
        // Honorific isn't loaded yet — the gate just won't fire. Calling
        // InvokeFunc before Honorific is up will throw, which is why the
        // initial pull below is wrapped in try-catch.
        //
        // Thread-context: plugin constructors run on Dalamud's plugin-loader
        // thread, NOT the framework thread. Honorific's IPC handlers read
        // ObjectTable.LocalPlayer (Honorific IpcProvider.cs:61), which throws
        // "Not on main thread!" outside the framework thread. If Honorific is
        // already loaded when HellionChat starts, a synchronous InvokeFunc
        // here would surface that exception, the broad catch below would
        // mark IsAvailable=false, and OnTitleChanged's `if (!IsAvailable)`
        // gate would block every subsequent title update. We therefore
        // schedule the initial pull onto the framework thread via
        // IFramework.RunOnFrameworkThread so the IPC call sees the right
        // thread context.
        _apiVersion = pluginInterface
            .GetIpcSubscriber<(uint, uint)>($"{IpcNamespace}.ApiVersion");
        _getLocalCharacterTitle = pluginInterface
            .GetIpcSubscriber<string>($"{IpcNamespace}.GetLocalCharacterTitle");
        _localCharacterTitleChanged = pluginInterface
            .GetIpcSubscriber<string, object>($"{IpcNamespace}.LocalCharacterTitleChanged");
        _ready = pluginInterface
            .GetIpcSubscriber<object>($"{IpcNamespace}.Ready");
        _disposing = pluginInterface
            .GetIpcSubscriber<object>($"{IpcNamespace}.Disposing");

        _localCharacterTitleChanged.Subscribe(OnTitleChanged);
        _ready.Subscribe(OnReady);
        _disposing.Subscribe(OnDisposing);

        _framework.RunOnFrameworkThread(TryInitialPull);
    }

    public void Dispose()
    {
        // Honorific may already be gone by the time we dispose. Wrap each
        // unsubscribe so a missing gate doesn't prevent the others from
        // unsubscribing — leaking even one subscription leaves a callback
        // alive that captures `this`, which keeps the whole service alive
        // and breaks plugin reload.
        TryUnsubscribe(() => _localCharacterTitleChanged.Unsubscribe(OnTitleChanged));
        TryUnsubscribe(() => _ready.Unsubscribe(OnReady));
        TryUnsubscribe(() => _disposing.Unsubscribe(OnDisposing));
    }

    private void TryInitialPull()
    {
        try
        {
            var version = _apiVersion.InvokeFunc();
            DetectedApiVersion = version;

            if (!IsApiVersionCompatible(version))
            {
                if (!_versionWarningLogged)
                {
                    _log.Warning(
                        "Honorific API version mismatch — expected major 3, " +
                        "found {Major}.{Minor}. Disabling Honorific integration.",
                        version.Item1, version.Item2);
                    _versionWarningLogged = true;
                }
                IsAvailable = false;
                return;
            }

            IsAvailable = true;
            _versionWarningLogged = false;
            // Pull the current title once at startup; from here on we rely
            // on LocalCharacterTitleChanged events.
            var json = _getLocalCharacterTitle.InvokeFunc();
            CurrentTitle = ParseTitleJson(json);
        }
        catch (Exception ex)
        {
            // Honorific isn't installed or hasn't initialised yet. The Ready
            // event will give us a second chance later. Log at Debug so
            // users without Honorific don't see noise on every reload.
            _log.Debug(ex, "Honorific not available at HellionChat startup; awaiting Ready.");
            IsAvailable = false;
            CurrentTitle = null;
        }
    }

    // Honorific fires LocalCharacterTitleChanged through its nameplate hook
    // (Honorific-master/Plugin.cs:665), which means we get title updates on
    // character switches automatically as soon as the new character is
    // rendered. While the user is in the character-select menu, HellionChat's
    // window is hidden by default via HideWhenNotLoggedIn (Configuration.cs:152),
    // so the stale-title window between logout and login isn't user-visible.
    private void OnTitleChanged(string json)
    {
        // Don't update cached state when we've already decided we can't trust
        // Honorific (e.g. version mismatch). Subscription stays live in case a
        // compatible Honorific reloads, in which case Ready triggers TryInitialPull
        // and sets IsAvailable back to true.
        if (!IsAvailable) return;
        CurrentTitle = ParseTitleJson(json);
    }

    private void OnReady()
    {
        // Honorific loaded after HellionChat; redo the version check and
        // initial pull. Idempotent on purpose — Honorific can fire Ready
        // more than once across reloads.
        //
        // Honorific's NotifyReady may dispatch from any thread, and
        // TryInitialPull eventually calls IPC handlers that read
        // ObjectTable.LocalPlayer — same "Not on main thread!" hazard as
        // the constructor path. Schedule onto the framework thread.
        _framework.RunOnFrameworkThread(TryInitialPull);
    }

    private void OnDisposing()
    {
        // Honorific is unloading. Drop our cached state so the header
        // hides on the next frame; subscriptions stay registered because
        // the gates may come back later (Honorific reload).
        //
        // Race-note: Honorific's NotifyDisposing calls ChangedLocalCharacterTitle(null)
        // BEFORE SendMessage on the Disposing gate (IpcProvider.cs:109-111),
        // so OnTitleChanged is expected to fire first and already null out
        // CurrentTitle. We re-clear here as belt-and-braces; should the
        // ordering ever flip, ShouldRenderSlot would still gate on IsAvailable.
        CurrentTitle = null;
        IsAvailable = false;
        DetectedApiVersion = null;
    }

    private void TryUnsubscribe(Action unsubscribe)
    {
        try
        {
            unsubscribe();
        }
        catch (Exception ex)
        {
            _log.Debug(ex, "Honorific unsubscribe failed (likely already gone).");
        }
    }

    // Threading note: Dalamud fires IPC events on the framework thread and
    // ImGui renders on the framework thread, so OnTitleChanged and the
    // render path that reads CurrentTitle never race — OnTitleChanged is
    // safe to keep direct (no RunOnFrameworkThread wrap needed) because
    // LocalCharacterTitleChanged delivery is framework-thread by Dalamud
    // contract. If a future change moves either side onto a worker thread,
    // switch to volatile/Interlocked for the CurrentTitle field.
    //
    // The constructor's initial pull and OnReady, on the other hand, are
    // explicitly scheduled via IFramework.RunOnFrameworkThread because
    // they run outside that contract: the constructor executes on the
    // plugin-loader thread, and Honorific's NotifyReady can dispatch from
    // any thread. Both call paths eventually invoke IPC handlers that read
    // ObjectTable.LocalPlayer, which throws "Not on main thread!" off the
    // framework thread — see the constructor comment block for context.
    //
    // Divergence from ChatTwo/Ipc/ExtraChat.cs: that file uses `volatile`
    // on its state fields out of caution. We don't, because the framework-
    // thread delivery is the documented Dalamud contract. If the two files
    // ever need to share a threading audit, this is the place to revisit.

    // --- Pure-logic helpers below; tested via HellionChat.Tests/Integrations. ---

    internal static HonorificTitleData? ParseTitleJson(string json)
    {
        if (string.IsNullOrEmpty(json))
            return null;

        try
        {
            return JsonConvert.DeserializeObject<HonorificTitleData>(json);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    internal static bool IsApiVersionCompatible((uint Major, uint Minor) apiVersion)
    {
        return apiVersion.Major == 3;
    }

    internal static bool ShouldRenderSlot(
        bool toggleEnabled,
        bool isAvailable,
        HonorificTitleData? title)
    {
        if (!toggleEnabled) return false;
        if (!isAvailable) return false;
        if (title is null) return false;
        if (title.IsOriginal) return false;
        if (string.IsNullOrEmpty(title.Title)) return false;
        return true;
    }
}
