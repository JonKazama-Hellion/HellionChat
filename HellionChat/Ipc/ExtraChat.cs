using Dalamud.Plugin.Ipc;

namespace HellionChat.Ipc;

public sealed class ExtraChat : IDisposable
{
#pragma warning disable CS0649 // Assigned through IPC
    [Serializable]
    private struct OverrideInfo
    {
        public string? Channel;
        public ushort UiColour;
        public uint Rgba;
    }
#pragma warning restore CS0649

    private ICallGateSubscriber<OverrideInfo, object> OverrideChannelGate { get; }
    private ICallGateSubscriber<Dictionary<string, uint>, Dictionary<string, uint>> ChannelCommandColoursGate { get; }
    private ICallGateSubscriber<Dictionary<Guid, string>, Dictionary<Guid, string>> ChannelNamesGate { get; }

    internal (string, uint)? ChannelOverride { get; set; }

    // Volatile reference: IPC callbacks (OnChannelCommandColours/OnChannelNames) fire on a
    // Dalamud-dispatcher thread while the ImGui thread reads the IReadOnlyDictionary projections.
    // Reference assignment is atomic on x64, but the JIT (especially Mono on Wine/Linux) needs
    // the volatile barrier to guarantee visibility across threads. See AUDIT-2026-05-05 [SEC-01].
    private volatile Dictionary<string, uint> ChannelCommandColoursInternal = new();
    internal IReadOnlyDictionary<string, uint> ChannelCommandColours => ChannelCommandColoursInternal;

    private volatile Dictionary<Guid, string> ChannelNamesInternal = new();
    internal IReadOnlyDictionary<Guid, string> ChannelNames => ChannelNamesInternal;

    internal ExtraChat()
    {
        OverrideChannelGate = Plugin.Interface.GetIpcSubscriber<OverrideInfo, object>("ExtraChat.OverrideChannelColour");
        ChannelCommandColoursGate = Plugin.Interface.GetIpcSubscriber<Dictionary<string, uint>, Dictionary<string, uint>>("ExtraChat.ChannelCommandColours");
        ChannelNamesGate = Plugin.Interface.GetIpcSubscriber<Dictionary<Guid, string>, Dictionary<Guid, string>>("ExtraChat.ChannelNames");

        OverrideChannelGate.Subscribe(OnOverrideChannel);
        ChannelCommandColoursGate.Subscribe(OnChannelCommandColours);
        ChannelNamesGate.Subscribe(OnChannelNames);
        try
        {
            ChannelCommandColoursInternal = ChannelCommandColoursGate.InvokeFunc(null!);
            ChannelNamesInternal = ChannelNamesGate.InvokeFunc(null!);
        }
        catch (Exception ex)
        {
            // ExtraChat is optional; missing IPC peer is normal when the plugin isn't loaded.
            Plugin.Log.Verbose(ex, "ExtraChat IPC initial state query failed (peer not loaded?)");
        }
    }

    public void Dispose()
    {
        OverrideChannelGate.Unsubscribe(OnOverrideChannel);
        ChannelCommandColoursGate.Unsubscribe(OnChannelCommandColours);
        ChannelNamesGate.Unsubscribe(OnChannelNames);
    }

    private void OnOverrideChannel(OverrideInfo info)
    {
        if (info.Channel == null)
        {
            ChannelOverride = null;
            return;
        }

        ChannelOverride = (info.Channel, info.Rgba);
    }

    private void OnChannelCommandColours(Dictionary<string, uint> obj)
    {
        ChannelCommandColoursInternal = obj;
    }

    private void OnChannelNames(Dictionary<Guid, string> obj)
    {
        ChannelNamesInternal = obj;
    }
}
